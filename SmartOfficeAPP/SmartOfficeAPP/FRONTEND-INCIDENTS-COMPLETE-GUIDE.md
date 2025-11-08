# ?? Guide Frontend - Gestion des Incidents

## ?? Table des Matières

1. [Vue d'ensemble](#vue-densemble)
2. [APIs Disponibles](#apis-disponibles)
3. [Modèles de Données (DTOs)](#modèles-de-données-dtos)
4. [Implémentation du Service Frontend](#implémentation-du-service-frontend)
5. [Corrections de la Page Blazor](#corrections-de-la-page-blazor)
6. [Exemples de Requêtes](#exemples-de-requêtes)
7. [Gestion des Erreurs](#gestion-des-erreurs)
8. [Tests et Validation](#tests-et-validation)

---

## ?? Vue d'ensemble

La page de gestion des incidents permet aux utilisateurs de:
- ? Consulter tous les tickets d'incidents
- ? Créer de nouveaux tickets
- ? Modifier le statut des tickets
- ? Assigner des tickets aux utilisateurs (Manager/Admin)
- ? Supprimer des tickets (Admin uniquement)
- ? Filtrer par statut et rechercher
- ? Voir les détails complets d'un ticket

### Architecture Backend Complète

```
???????????????????????????????????????????????????????????
?      Frontend Blazor (MySmartApp.Ui)          ?
?     /incidents page                           ?
???????????????????????????????????????????????????????????
                       ? HTTP/JSON (camelCase)
                       ?
???????????????????????????????????????????????????????????
?     IncidentTicketsController (API Backend)     ?
?     /api/IncidentTickets                        ?
???????????????????????????????????????????????????????????
?  - CreateTicket()         ? Créer un ticket             ?
?  - GetTicketById()        ? Détails d'un ticket         ?
?  - GetTicketsByStatus()   ? Filtrer par statut          ?
?  - GetTicketsByUserId()   ? Tickets d'un utilisateur    ?
?  - UpdateTicketStatus()   ? Changer le statut           ?
?  - AssignTicket()         ? Assigner (Manager/Admin)    ?
?  - DeleteTicket()         ? Supprimer (Admin)           ?
???????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????
?      IncidentTicketService (Business Logic)     ?
?  - Gestion des tickets                          ?
?  - Validation métier                            ?
?  - Notifications automatiques                   ?
???????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????
?      IncidentTicketRepository (Data Access)     ?
?  - Include CreatedBy, AssignedTo               ?
?  - Include Zone, Peripherique                   ?
?  - Gestion timestamps                           ?
???????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????
?       PostgreSQL Database                       ?
?  Table: IncidentTickets                        ?
???????????????????????????????????????????????????????????
```

---

## ?? APIs Disponibles

### Base URL
```
https://localhost:7144/api/IncidentTickets
```

### 1. **POST** - Créer un nouveau ticket

```http
POST /api/IncidentTickets
Authorization: Bearer {token}
Content-Type: application/json

{
  "sujet": "Capteur de température défectueux",
  "description": "Le capteur dans la salle de réunion A ne répond plus",
  "statut": "Ouvert",
  "zoneId": 5,
  "peripheriqueLogiqueId": null
}
```

**Paramètres (IncidentTicketCreateDto):**

| Champ | Type | Requis | Description | Contraintes |
|-------|------|--------|-------------|-------------|
| `sujet` | string | ? | Titre du ticket | Max 100 caractères |
| `description` | string | ? | Description détaillée | Max 500 caractères |
| `statut` | string | ? | Statut initial | "Ouvert", "En cours", "En attente" |
| `zoneId` | int? | ? | ID de la zone concernée | Optionnel |
| `peripheriqueLogiqueId` | int? | ? | ID du périphérique | Optionnel |

**Réponse (201 Created):**
```json
{
  "id": 42,
  "sujet": "Capteur de température défectueux",
  "description": "Le capteur dans la salle de réunion A ne répond plus",
  "statut": "Ouvert",
  "createdById": "user-123",
  "createdByName": "John Doe",
  "assignedToId": null,
  "assignedToName": null,
  "zoneId": 5,
  "zoneName": "Salle de réunion A",
  "peripheriqueLogiqueId": null,
  "peripheriqueName": null,
  "createdAt": "2025-01-06T14:30:00Z",
  "updatedAt": null
}
```

**Location Header:**
```
Location: https://localhost:7144/api/IncidentTickets/42
```

---

### 2. **GET** - Récupérer un ticket par ID

```http
GET /api/IncidentTickets/{id}
Authorization: Bearer {token}
```

**Paramètres:**
- `id` (int, path) - ID du ticket

**Réponse (200 OK):**
```json
{
  "id": 1,
  "sujet": "Capteur de température défectueux",
  "description": "Le capteur dans la salle de réunion A ne répond plus depuis ce matin.",
  "statut": "Ouvert",
  "createdById": "user-123",
  "createdByName": "John Doe",
  "assignedToId": null,
  "assignedToName": null,
  "zoneId": 5,
  "zoneName": "Salle de réunion A",
  "peripheriqueLogiqueId": null,
  "peripheriqueName": null,
  "createdAt": "2025-01-06T10:30:00Z",
  "updatedAt": null
}
```

**Réponse (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

---

### 3. **GET** - Récupérer les tickets par statut

```http
GET /api/IncidentTickets/status/{status}
Authorization: Bearer {token}
```

**Paramètres:**
- `status` (string, path) - Statut à filtrer

**Statuts possibles:**
- `"Ouvert"` - Tickets nouvellement créés
- `"En cours"` - Tickets en cours de traitement
- `"En attente"` - En attente de retour/information
- `"Résolu"` - Problème résolu
- `"Fermé"` - Ticket fermé
- `"Annulé"` - Ticket annulé

**Réponse (200 OK):**
```json
[
  {
    "id": 1,
    "sujet": "Capteur défectueux",
    "description": "...",
    "statut": "Ouvert",
    "createdById": "user-123",
    "createdByName": "John Doe",
    "assignedToId": null,
    "assignedToName": null,
    "zoneId": 5,
    "zoneName": "Salle A",
    "peripheriqueLogiqueId": null,
    "peripheriqueName": null,
    "createdAt": "2025-01-06T10:30:00Z",
    "updatedAt": null
  },
  {
    "id": 2,
    "sujet": "Problème réseau",
    "description": "...",
    "statut": "Ouvert",
    "createdById": "user-456",
    "createdByName": "Jane Smith",
    "assignedToId": "tech-789",
    "assignedToName": "Bob Wilson",
    "zoneId": null,
    "zoneName": null,
    "peripheriqueLogiqueId": 12,
    "peripheriqueName": "Routeur WiFi",
    "createdAt": "2025-01-05T14:20:00Z",
    "updatedAt": "2025-01-06T09:15:00Z"
  }
]
```

---

### 4. **GET** - Récupérer les tickets d'un utilisateur

```http
GET /api/IncidentTickets/user/{userId}
Authorization: Bearer {token}
```

**Paramètres:**
- `userId` (string, path) - ID de l'utilisateur

**Description:**
Retourne tous les tickets créés par OU assignés à l'utilisateur spécifié.

**Réponse (200 OK):**
```json
[
  {
    "id": 1,
    "sujet": "Ticket créé par moi",
    "statut": "Ouvert",
    "createdById": "user-123",
    "assignedToId": null,
    // ...
  },
  {
    "id": 5,
    "sujet": "Ticket assigné à moi",
    "statut": "En cours",
    "createdById": "other-user",
    "assignedToId": "user-123",
    // ...
  }
]
```

---

### 5. **PUT** - Mettre à jour le statut d'un ticket

```http
PUT /api/IncidentTickets/{id}/status
Authorization: Bearer {token}
Content-Type: application/json

"Résolu"
```

**Paramètres:**
- `id` (int, path) - ID du ticket
- `newStatus` (string, body) - Nouveau statut (passé directement comme string)

**Statuts valides:**
- `"Ouvert"`
- `"En cours"`
- `"En attente"`
- `"Résolu"`
- `"Fermé"`
- `"Annulé"`

**Réponse (204 No Content):**
```
(Pas de contenu - succès)
```

**Réponse (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

---

### 6. **PUT** - Assigner un ticket à un utilisateur (Manager/Admin)

```http
PUT /api/IncidentTickets/{id}/assign
Authorization: Bearer {token}
Content-Type: application/json

"tech-user-789"
```

**Authorization:** `[Authorize(Roles = "Manager,Admin")]`

**Paramètres:**
- `id` (int, path) - ID du ticket
- `assignedToId` (string, body) - ID de l'utilisateur à qui assigner (passé directement comme string)

**Réponse (204 No Content):**
```
(Pas de contenu - succès)
```

**Réponse (403 Forbidden):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "User does not have the required role"
}
```

**Réponse (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

---

### 7. **DELETE** - Supprimer un ticket (Admin uniquement)

```http
DELETE /api/IncidentTickets/{id}
Authorization: Bearer {token}
```

**Authorization:** `[Authorize(Roles = "Admin")]`

**Paramètres:**
- `id` (int, path) - ID du ticket à supprimer

**Réponse (204 No Content):**
```
(Pas de contenu - succès)
```

**Réponse (403 Forbidden):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "User does not have the required role"
}
```

**Réponse (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404
}
```

---

## ?? Modèles de Données (DTOs)

### IncidentTicketDto (Réponse complète)

```csharp
public class IncidentTicketDto
{
    public int Id { get; set; }
    
    // ?? Informations du ticket
    public string Sujet { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    
    // ?? Créateur
    public string CreatedById { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    
    // ?? Assigné à
    public string? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    
    // ?? Zone concernée
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    
    // ?? Périphérique concerné
    public int? PeripheriqueLogiqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    
    // ?? Métadonnées
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### IncidentTicketCreateDto (Création)

```csharp
public class IncidentTicketCreateDto
{
    [Required(ErrorMessage = "Le sujet est obligatoire")]
    [MaxLength(100, ErrorMessage = "Le sujet ne peut pas dépasser 100 caractères")]
    public string Sujet { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La description est obligatoire")]
    [MaxLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Le statut est obligatoire")]
    public string Statut { get; set; } = "Ouvert";
    
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}
```

---

## ?? Implémentation du Service Frontend

### Interface IIncidentTicketService

```csharp
public interface IIncidentTicketService
{
    // CRUD de base
    Task<ApiResponse<IncidentTicketDto>> CreateTicketAsync(IncidentTicketCreateDto dto);
    Task<ApiResponse<IncidentTicketDto>> GetTicketByIdAsync(int id);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByStatusAsync(string status);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByUserIdAsync(string userId);
    Task<ApiResponse<bool>> UpdateTicketStatusAsync(int id, string newStatus);
    Task<ApiResponse<bool>> AssignTicketAsync(int id, string assignedToId);
    Task<ApiResponse<bool>> DeleteTicketAsync(int id);
    
    // Méthodes helper frontend
    Task<List<string>> GetAvailableStatusesAsync();
    Task<ApiResponse<List<IncidentTicketDto>>> GetMyTicketsAsync(); // Tickets de l'utilisateur courant
}
```

### Implémentation IncidentTicketService

```csharp
public class IncidentTicketService : IIncidentTicketService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IncidentTicketService> _logger;
    private readonly AuthenticationStateProvider _authStateProvider;

    public IncidentTicketService(
        HttpClient httpClient, 
        ILogger<IncidentTicketService> logger,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _logger = logger;
        _authStateProvider = authStateProvider;
    }

    public async Task<ApiResponse<IncidentTicketDto>> CreateTicketAsync(IncidentTicketCreateDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/IncidentTickets", dto);
            
            if (response.IsSuccessStatusCode)
            {
                var ticket = await response.Content.ReadFromJsonAsync<IncidentTicketDto>();
                return new ApiResponse<IncidentTicketDto>
                {
                    IsSuccess = true,
                    Data = ticket
                };
            }
            
            _logger.LogWarning("Erreur lors de la création du ticket: {StatusCode}", response.StatusCode);
            return new ApiResponse<IncidentTicketDto> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception lors de la création du ticket");
            return new ApiResponse<IncidentTicketDto> { IsSuccess = false };
        }
    }

    public async Task<ApiResponse<IncidentTicketDto>> GetTicketByIdAsync(int id)
    {
        try
        {
            var ticket = await _httpClient.GetFromJsonAsync<IncidentTicketDto>($"api/IncidentTickets/{id}");
            
            return new ApiResponse<IncidentTicketDto>
            {
                IsSuccess = ticket != null,
                Data = ticket
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Ticket {Id} non trouvé", id);
            return new ApiResponse<IncidentTicketDto> { IsSuccess = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement du ticket {Id}", id);
            return new ApiResponse<IncidentTicketDto> { IsSuccess = false };
        }
    }

    public async Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByStatusAsync(string status)
    {
        try
        {
            var tickets = await _httpClient.GetFromJsonAsync<List<IncidentTicketDto>>(
                $"api/IncidentTickets/status/{Uri.EscapeDataString(status)}");
            
            return new ApiResponse<List<IncidentTicketDto>>
            {
                IsSuccess = true,
                Data = tickets ?? new List<IncidentTicketDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement des tickets par statut {Status}", status);
            return new ApiResponse<List<IncidentTicketDto>>
            {
                IsSuccess = false,
                Data = new List<IncidentTicketDto>()
            };
        }
    }

    public async Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByUserIdAsync(string userId)
    {
        try
        {
            var tickets = await _httpClient.GetFromJsonAsync<List<IncidentTicketDto>>(
                $"api/IncidentTickets/user/{userId}");
            
            return new ApiResponse<List<IncidentTicketDto>>
            {
                IsSuccess = true,
                Data = tickets ?? new List<IncidentTicketDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement des tickets de l'utilisateur {UserId}", userId);
            return new ApiResponse<List<IncidentTicketDto>>
            {
                IsSuccess = false,
                Data = new List<IncidentTicketDto>()
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateTicketStatusAsync(int id, string newStatus)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/IncidentTickets/{id}/status", newStatus);
            
            return new ApiResponse<bool>
            {
                IsSuccess = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du statut du ticket {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
    }

    public async Task<ApiResponse<bool>> AssignTicketAsync(int id, string assignedToId)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/IncidentTickets/{id}/assign", assignedToId);
            
            return new ApiResponse<bool>
            {
                IsSuccess = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Accès refusé pour l'assignation du ticket {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'assignation du ticket {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
    }

    public async Task<ApiResponse<bool>> DeleteTicketAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/IncidentTickets/{id}");
            
            return new ApiResponse<bool>
            {
                IsSuccess = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Accès refusé pour la suppression du ticket {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du ticket {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
    }

    public Task<List<string>> GetAvailableStatusesAsync()
    {
        // Statuts hardcodés côté frontend
        return Task.FromResult(new List<string>
        {
            "Ouvert",
            "En cours",
            "En attente",
            "Résolu",
            "Fermé",
            "Annulé"
        });
    }

    public async Task<ApiResponse<List<IncidentTicketDto>>> GetMyTicketsAsync()
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Impossible d'obtenir l'ID utilisateur courant");
                return new ApiResponse<List<IncidentTicketDto>>
                {
                    IsSuccess = false,
                    Data = new List<IncidentTicketDto>()
                };
            }
            
            return await GetTicketsByUserIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement des tickets de l'utilisateur courant");
            return new ApiResponse<List<IncidentTicketDto>>
            {
                IsSuccess = false,
                Data = new List<IncidentTicketDto>()
            };
        }
    }
}
```

### Enregistrement du Service

```csharp
// Program.cs
builder.Services.AddScoped<IIncidentTicketService, IncidentTicketService>();
```

---

## ?? Corrections de la Page Blazor

### Problèmes Identifiés

1. **Pas de vraie API connectée** ? Connecter le service
2. **Méthode `IsMyTicket` hardcodée** ? Utiliser les vraies données
3. **Pas de statistiques backend** ? Calculer côté frontend
4. **Assignation simplifiée** ? Améliorer avec liste d'utilisateurs
5. **Pas de Snackbar** ? Ajouter feedback utilisateur

### Code Corrigé

#### **1. Injection des dépendances**

```csharp
@inject IIncidentTicketService IncidentTicketService
@inject ISnackbar Snackbar
@inject ILogger<Incidents> Logger
@inject IDialogService DialogService
@inject AuthenticationStateProvider AuthStateProvider
```

#### **2. Chargement des données**

```csharp
private string? currentUserId;

protected override async Task OnInitializedAsync()
{
    // Obtenir l'ID de l'utilisateur courant
    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
    currentUserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    
    // Charger les statuts disponibles
    availableStatuses = await IncidentTicketService.GetAvailableStatusesAsync();
    
    // Charger les données
    await LoadData();
}

private async Task LoadData()
{
    isLoading = true;
    StateHasChanged();
    
    try
    {
        // Charger tous les tickets (on filtrera côté frontend)
        var allTicketsResponse = await LoadAllTickets();
        
        if (allTicketsResponse.IsSuccess && allTicketsResponse.Data != null)
        {
            tickets = allTicketsResponse.Data;
            
            // Calculer les statistiques côté frontend
            statistics = CalculateStatistics(tickets);
            
            Snackbar.Add($"? {tickets.Count} tickets chargés", Severity.Success);
            Logger.LogInformation("? {Count} tickets chargés", tickets.Count);
        }
        else
        {
            Snackbar.Add("? Erreur lors du chargement des tickets", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Erreur lors du chargement des données");
        Snackbar.Add("? Une erreur est survenue", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}

// Helper pour charger tous les tickets en combinant les différents statuts
private async Task<ApiResponse<List<IncidentTicketDto>>> LoadAllTickets()
{
    var allTickets = new List<IncidentTicketDto>();
    
    foreach (var status in availableStatuses)
    {
        var response = await IncidentTicketService.GetTicketsByStatusAsync(status);
        if (response.IsSuccess && response.Data != null)
        {
            allTickets.AddRange(response.Data);
        }
    }
    
    return new ApiResponse<List<IncidentTicketDto>>
    {
        IsSuccess = true,
        Data = allTickets
    };
}

// Calculer les statistiques côté frontend
private Dictionary<string, int> CalculateStatistics(List<IncidentTicketDto> ticketList)
{
    return new Dictionary<string, int>
    {
        ["Total"] = ticketList.Count,
        ["Ouvert"] = ticketList.Count(t => t.Statut == "Ouvert"),
        ["En cours"] = ticketList.Count(t => t.Statut == "En cours"),
        ["Résolu"] = ticketList.Count(t => t.Statut == "Résolu")
    };
}
```

#### **3. Corriger IsMyTicket**

```csharp
private bool IsMyTicket(IncidentTicketDto ticket)
{
    if (string.IsNullOrEmpty(currentUserId))
        return false;
    
    return ticket.CreatedById == currentUserId || ticket.AssignedToId == currentUserId;
}
```

#### **4. Création de ticket avec gestion d'erreurs**

```csharp
private async Task CreateTicket()
{
    if (createForm != null)
    {
        await createForm.Validate();
        if (!createForm.IsValid)
        {
            Snackbar.Add("?? Veuillez remplir tous les champs obligatoires", Severity.Warning);
            return;
        }
    }

    isCreating = true;
    StateHasChanged();

    try
    {
        var result = await IncidentTicketService.CreateTicketAsync(newTicket);
        
        if (result.IsSuccess)
        {
            Snackbar.Add("? Ticket créé avec succès", Severity.Success);
            await LoadData();
            CloseCreateDialog();
        }
        else
        {
            Snackbar.Add("? Erreur lors de la création du ticket", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Exception lors de la création");
        Snackbar.Add("? Une erreur est survenue", Severity.Error);
    }
    finally
    {
        isCreating = false;
        StateHasChanged();
    }
}
```

#### **5. Marquer comme résolu**

```csharp
private async Task SetTicketResolved(IncidentTicketDto ticket)
{
    if (ticket.Statut == "Résolu")
    {
        Snackbar.Add("?? Ce ticket est déjà résolu", Severity.Info);
        return;
    }

    bool? confirm = await DialogService.ShowMessageBox(
        "? Marquer comme résolu",
        $"Voulez-vous marquer le ticket #{ticket.Id} comme résolu ?\n\nSujet: {ticket.Sujet}",
        yesText: "Oui, résoudre",
        cancelText: "Annuler");

    if (confirm == true)
    {
        await SetTicketStatus(ticket, "Résolu");
    }
}

private async Task SetTicketStatus(IncidentTicketDto ticket, string newStatus)
{
    try
    {
        var updateResult = await IncidentTicketService.UpdateTicketStatusAsync(ticket.Id, newStatus);
        
        if (updateResult.IsSuccess)
        {
            Snackbar.Add($"? Statut mis à jour: {newStatus}", Severity.Success);
            await LoadData();
            
            // Mettre à jour le dialog de détails si ouvert
            if (isDetailsDialogOpen && selectedTicket?.Id == ticket.Id)
            {
                var refreshed = await IncidentTicketService.GetTicketByIdAsync(ticket.Id);
                if (refreshed.IsSuccess && refreshed.Data != null)
                {
                    selectedTicket = refreshed.Data;
                    StateHasChanged();
                }
            }
        }
        else
        {
            Snackbar.Add("? Erreur lors de la mise à jour", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Erreur mise à jour statut ticket {Id}", ticket.Id);
        Snackbar.Add("? Une erreur est survenue", Severity.Error);
    }
}
```

#### **6. Assignation améliorée**

```csharp
private async Task AssignTicket(IncidentTicketDto ticket)
{
    var result = await DialogService.ShowMessageBox(
        $"?? Assigner le ticket #{ticket.Id}",
        $"Voulez-vous vous assigner ce ticket ?\n\nSujet: {ticket.Sujet}",
        yesText: "M'assigner",
        cancelText: "Annuler");

    if (result == true)
    {
        try
        {
            if (string.IsNullOrEmpty(currentUserId))
            {
                Snackbar.Add("? Impossible d'obtenir l'utilisateur courant", Severity.Error);
                return;
            }

            var assignResult = await IncidentTicketService.AssignTicketAsync(ticket.Id, currentUserId);
            
            if (assignResult.IsSuccess)
            {
                Snackbar.Add("? Ticket assigné avec succès", Severity.Success);
                await LoadData();
            }
            else
            {
                Snackbar.Add("? Erreur lors de l'assignation (vérifiez vos permissions)", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Erreur assignation ticket {Id}", ticket.Id);
            Snackbar.Add("? Une erreur est survenue", Severity.Error);
        }
    }
}
```

#### **7. Suppression avec confirmation**

```csharp
private async Task DeleteTicket(IncidentTicketDto ticket)
{
    bool? result = await DialogService.ShowMessageBox(
        "??? Confirmation de suppression",
        $"Êtes-vous sûr de vouloir supprimer le ticket #{ticket.Id} ?\n\n" +
        $"Sujet: {ticket.Sujet}\n" +
        $"Statut: {ticket.Statut}\n\n" +
        $"?? Cette action est irréversible.",
        yesText: "Oui, supprimer",
        cancelText: "Annuler");

    if (result == true)
    {
        try
        {
            var deleteResult = await IncidentTicketService.DeleteTicketAsync(ticket.Id);
            
            if (deleteResult.IsSuccess)
            {
                Snackbar.Add("? Ticket supprimé avec succès", Severity.Success);
                await LoadData();
                
                if (isDetailsDialogOpen && selectedTicket?.Id == ticket.Id)
                {
                    CloseDetailsDialog();
                }
            }
            else
            {
                Snackbar.Add("? Erreur lors de la suppression (vérifiez vos permissions)", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Erreur suppression ticket {Id}", ticket.Id);
            Snackbar.Add("? Une erreur est survenue", Severity.Error);
        }
    }
}
```

---

## ?? Exemples de Requêtes (Tests)

### Fichier: `37-Incidents-Complete.http`

```http
### Variables
@baseUrl = https://localhost:7144
@token = your-jwt-token-here

### 1. Créer un ticket (Zone)
POST {{baseUrl}}/api/IncidentTickets
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "sujet": "Capteur défectueux",
  "description": "Le capteur de température ne répond plus",
  "statut": "Ouvert",
  "zoneId": 5,
  "peripheriqueLogiqueId": null
}

### 2. Créer un ticket (Périphérique)
POST {{baseUrl}}/api/IncidentTickets
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "sujet": "Climatisation hors service",
  "description": "Le climatiseur ne démarre pas",
  "statut": "Ouvert",
  "zoneId": null,
  "peripheriqueLogiqueId": 12
}

### 3. Créer un ticket (Général)
POST {{baseUrl}}/api/IncidentTickets
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "sujet": "Problème réseau",
  "description": "Perte de connexion intermittente",
  "statut": "Ouvert"
}

### 4. Récupérer un ticket par ID
GET {{baseUrl}}/api/IncidentTickets/1
Authorization: Bearer {{token}}

### 5. Récupérer les tickets "Ouvert"
GET {{baseUrl}}/api/IncidentTickets/status/Ouvert
Authorization: Bearer {{token}}

### 6. Récupérer les tickets d'un utilisateur
GET {{baseUrl}}/api/IncidentTickets/user/user-123
Authorization: Bearer {{token}}

### 7. Mettre à jour le statut
PUT {{baseUrl}}/api/IncidentTickets/1/status
Authorization: Bearer {{token}}
Content-Type: application/json

"En cours"

### 8. Marquer comme résolu
PUT {{baseUrl}}/api/IncidentTickets/1/status
Authorization: Bearer {{token}}
Content-Type: application/json

"Résolu"

### 9. Assigner un ticket (Manager/Admin)
PUT {{baseUrl}}/api/IncidentTickets/1/assign
Authorization: Bearer {{token}}
Content-Type: application/json

"tech-user-789"

### 10. Supprimer un ticket (Admin)
DELETE {{baseUrl}}/api/IncidentTickets/1
Authorization: Bearer {{token}}
```

---

## ?? Gestion des Erreurs

### Codes d'erreur HTTP

| Code | Signification | Action Frontend |
|------|---------------|-----------------|
| `200` | OK | Traiter les données |
| `201` | Created | Afficher succès + recharger |
| `204` | No Content | Afficher succès + recharger |
| `400` | Bad Request | Afficher erreur de validation |
| `401` | Unauthorized | Rediriger vers login |
| `403` | Forbidden | Message "Accès refusé" |
| `404` | Not Found | Message "Ticket introuvable" |
| `500` | Internal Server Error | Message d'erreur générique |

### Exemple complet de gestion d'erreur

```csharp
try
{
    var response = await IncidentTicketService.CreateTicketAsync(newTicket);
    
    if (response.IsSuccess)
    {
        Snackbar.Add("? Ticket créé avec succès", Severity.Success);
        await LoadData();
        CloseCreateDialog();
    }
    else
    {
        Snackbar.Add("? Erreur lors de la création", Severity.Error);
    }
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
{
    Snackbar.Add("?? Session expirée, veuillez vous reconnecter", Severity.Warning);
    NavigationManager.NavigateTo("/login");
}
catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
{
    Snackbar.Add("? Vous n'avez pas l'autorisation requise", Severity.Error);
}
catch (Exception ex)
{
    Logger.LogError(ex, "Erreur inattendue");
    Snackbar.Add("? Une erreur est survenue", Severity.Error);
}
```

---

## ? Tests et Validation

### Checklist de Validation

- [ ] ? POST /api/IncidentTickets crée un ticket
- [ ] ? GET /api/IncidentTickets/{id} retourne les détails
- [ ] ? GET /api/IncidentTickets/status/{status} filtre correctement
- [ ] ? PUT /api/IncidentTickets/{id}/status met à jour
- [ ] ? PUT /api/IncidentTickets/{id}/assign assigne (Manager/Admin)
- [ ] ? DELETE /api/IncidentTickets/{id} supprime (Admin)
- [ ] ? Les statistiques s'affichent
- [ ] ? Les filtres fonctionnent
- [ ] ? Le switch "Mes tickets" fonctionne
- [ ] ? Les dialogs fonctionnent

### Scénarios de Test

#### Test 1: Créer un ticket

```
1. Cliquer "Créer un ticket"
2. Remplir: Sujet, Description, Statut
3. Sélectionner Zone ou Périphérique
4. Cliquer "Créer le ticket"
? Le ticket apparaît dans la liste
```

#### Test 2: Marquer comme résolu

```
1. Cliquer sur ? d'un ticket "Ouvert"
2. Confirmer
? Statut passe à "Résolu"
? Chip devient vert
```

#### Test 3: Assigner (Manager/Admin)

```
1. Se connecter en Manager/Admin
2. Cliquer sur ??
3. Confirmer l'auto-assignation
? Le ticket est assigné
```

#### Test 4: Filtrer par statut

```
1. Sélectionner "Résolu" dans le filtre
? Seuls les tickets résolus s'affichent
```

---

## ?? Résumé

### ? Ce qui est prêt (Backend)

1. **7 endpoints API** fonctionnels
2. **Gestion complète CRUD** des tickets
3. **Autorisation** par rôle (Manager, Admin)
4. **Relations** avec Zones et Périphériques
5. **Timestamps** automatiques

### ?? À corriger (Frontend)

1. **Implémenter** `IIncidentTicketService`
2. **Connecter** toutes les méthodes aux APIs
3. **Ajouter** Snackbar pour feedback
4. **Corriger** `IsMyTicket`
5. **Calculer** les statistiques côté frontend
6. **Tester** tous les scénarios

---

**Date:** 6 janvier 2025  
**Status:** ? Backend complet, Frontend à connecter  
**Temps estimé:** 2-3 heures d'implémentation frontend
