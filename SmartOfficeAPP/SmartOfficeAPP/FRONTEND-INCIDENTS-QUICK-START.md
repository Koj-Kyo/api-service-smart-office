# ?? Quick Start - Incidents (Frontend)

## ?? Temps d'implémentation: 2-3 heures

---

## ?? Checklist Rapide (4 étapes)

### ? Étape 1: Créer le Service (45 min)

**Créer:** `MySmartApp.Ui/Services/IIncidentTicketService.cs`

```csharp
public interface IIncidentTicketService
{
    Task<ApiResponse<IncidentTicketDto>> CreateTicketAsync(IncidentTicketCreateDto dto);
    Task<ApiResponse<IncidentTicketDto>> GetTicketByIdAsync(int id);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByStatusAsync(string status);
    Task<ApiResponse<List<IncidentTicketDto>>> GetTicketsByUserIdAsync(string userId);
    Task<ApiResponse<bool>> UpdateTicketStatusAsync(int id, string newStatus);
    Task<ApiResponse<bool>> AssignTicketAsync(int id, string assignedToId);
    Task<ApiResponse<bool>> DeleteTicketAsync(int id);
    Task<List<string>> GetAvailableStatusesAsync();
    Task<ApiResponse<List<IncidentTicketDto>>> GetMyTicketsAsync();
}
```

**Créer:** `MySmartApp.Ui/Services/IncidentTicketService.cs`

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
                return new ApiResponse<IncidentTicketDto> { IsSuccess = true, Data = ticket };
            }
            
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
            return new ApiResponse<IncidentTicketDto> { IsSuccess = ticket != null, Data = ticket };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur chargement ticket {Id}", id);
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
            _logger.LogError(ex, "Erreur chargement par statut {Status}", status);
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
            _logger.LogError(ex, "Erreur chargement tickets utilisateur {UserId}", userId);
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
            var response = await _httpClient.PutAsJsonAsync($"api/IncidentTickets/{id}/status", newStatus);
            return new ApiResponse<bool>
            {
                IsSuccess = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur mise à jour statut {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
    }

    public async Task<ApiResponse<bool>> AssignTicketAsync(int id, string assignedToId)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/IncidentTickets/{id}/assign", assignedToId);
            return new ApiResponse<bool>
            {
                IsSuccess = response.IsSuccessStatusCode,
                Data = response.IsSuccessStatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur assignation {Id}", id);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur suppression {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
    }

    public Task<List<string>> GetAvailableStatusesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "Ouvert", "En cours", "En attente", "Résolu", "Fermé", "Annulé"
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
            _logger.LogError(ex, "Erreur chargement mes tickets");
            return new ApiResponse<List<IncidentTicketDto>>
            {
                IsSuccess = false,
                Data = new List<IncidentTicketDto>()
            };
        }
    }
}
```

**Enregistrer dans `Program.cs`:**

```csharp
builder.Services.AddScoped<IIncidentTicketService, IncidentTicketService>();
```

---

### ? Étape 2: Connecter la Page Blazor (1h)

**Dans `Incidents.razor`:**

```csharp
@inject IIncidentTicketService IncidentTicketService
@inject ISnackbar Snackbar
@inject AuthenticationStateProvider AuthStateProvider

private string? currentUserId;

protected override async Task OnInitializedAsync()
{
    var authState = await AuthStateProvider.GetAuthenticationStateAsync();
    currentUserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    
    availableStatuses = await IncidentTicketService.GetAvailableStatusesAsync();
    await LoadData();
}

private async Task LoadData()
{
    isLoading = true;
    StateHasChanged();
    
    try
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
        
        tickets = allTickets;
        statistics = CalculateStatistics(tickets);
        
        Snackbar.Add($"? {tickets.Count} tickets chargés", Severity.Success);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur chargement données");
        Snackbar.Add("? Une erreur est survenue", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}

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

private bool IsMyTicket(IncidentTicketDto ticket)
{
    return ticket.CreatedById == currentUserId || ticket.AssignedToId == currentUserId;
}
```

---

### ? Étape 3: Ajouter les Actions (30 min)

```csharp
private async Task CreateTicket()
{
    if (createForm != null)
    {
        await createForm.Validate();
        if (!createForm.IsValid)
        {
            Snackbar.Add("?? Veuillez remplir tous les champs", Severity.Warning);
            return;
        }
    }

    isCreating = true;
    
    try
    {
        var result = await IncidentTicketService.CreateTicketAsync(newTicket);
        
        if (result.IsSuccess)
        {
            Snackbar.Add("? Ticket créé", Severity.Success);
            await LoadData();
            CloseCreateDialog();
        }
        else
        {
            Snackbar.Add("? Erreur création", Severity.Error);
        }
    }
    finally
    {
        isCreating = false;
        StateHasChanged();
    }
}

private async Task SetTicketResolved(IncidentTicketDto ticket)
{
    if (ticket.Statut == "Résolu")
    {
        Snackbar.Add("?? Déjà résolu", Severity.Info);
        return;
    }

    bool? confirm = await DialogService.ShowMessageBox(
        "? Marquer résolu",
        $"Marquer #{ticket.Id} comme résolu ?",
        yesText: "Oui",
        cancelText: "Non");

    if (confirm == true)
    {
        var result = await IncidentTicketService.UpdateTicketStatusAsync(ticket.Id, "Résolu");
        if (result.IsSuccess)
        {
            Snackbar.Add("? Ticket résolu", Severity.Success);
            await LoadData();
        }
    }
}

private async Task AssignTicket(IncidentTicketDto ticket)
{
    if (string.IsNullOrEmpty(currentUserId))
    {
        Snackbar.Add("? Utilisateur non identifié", Severity.Error);
        return;
    }

    var result = await IncidentTicketService.AssignTicketAsync(ticket.Id, currentUserId);
    
    if (result.IsSuccess)
    {
        Snackbar.Add("? Ticket assigné", Severity.Success);
        await LoadData();
    }
    else
    {
        Snackbar.Add("? Erreur (permissions?)", Severity.Error);
    }
}

private async Task DeleteTicket(IncidentTicketDto ticket)
{
    bool? confirm = await DialogService.ShowMessageBox(
        "??? Supprimer",
        $"Supprimer #{ticket.Id} ?\n\n?? Irréversible",
        yesText: "Supprimer",
        cancelText: "Annuler");

    if (confirm == true)
    {
        var result = await IncidentTicketService.DeleteTicketAsync(ticket.Id);
        
        if (result.IsSuccess)
        {
            Snackbar.Add("? Ticket supprimé", Severity.Success);
            await LoadData();
        }
        else
        {
            Snackbar.Add("? Erreur (permissions?)", Severity.Error);
        }
    }
}
```

---

### ? Étape 4: Tester (30 min)

**Créer:** `Tests/37-Incidents-Complete.http`

```http
### Variables
@baseUrl = https://localhost:7144
@token = {{$dotenv LOGIN_TOKEN}}

### 1. Créer un ticket
POST {{baseUrl}}/api/IncidentTickets
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "sujet": "Test ticket",
  "description": "Test description",
  "statut": "Ouvert",
  "zoneId": 1
}

### 2. Lister les tickets "Ouvert"
GET {{baseUrl}}/api/IncidentTickets/status/Ouvert
Authorization: Bearer {{token}}

### 3. Marquer résolu
PUT {{baseUrl}}/api/IncidentTickets/1/status
Authorization: Bearer {{token}}
Content-Type: application/json

"Résolu"

### 4. Assigner
PUT {{baseUrl}}/api/IncidentTickets/1/assign
Authorization: Bearer {{token}}
Content-Type: application/json

"user-id-here"

### 5. Supprimer
DELETE {{baseUrl}}/api/IncidentTickets/1
Authorization: Bearer {{token}}
```

---

## ? Validation Finale

- [ ] Le service est enregistré dans `Program.cs`
- [ ] Les tickets se chargent
- [ ] La création fonctionne
- [ ] Le changement de statut fonctionne
- [ ] L'assignation fonctionne (Manager/Admin)
- [ ] La suppression fonctionne (Admin)
- [ ] Les filtres fonctionnent
- [ ] Les statistiques s'affichent

---

## ?? Problèmes Fréquents

### Problème 1: "Aucun ticket ne s'affiche"
**Solution:** Vérifier qu'il y a des tickets en base avec les statuts existants

### Problème 2: "Erreur 403 sur assignation"
**Solution:** Vérifier que l'utilisateur a le rôle Manager ou Admin

### Problème 3: "Erreur 403 sur suppression"
**Solution:** Seuls les Admin peuvent supprimer

### Problème 4: "currentUserId est null"
**Solution:** Vérifier l'authentification et les claims JWT

---

## ?? Résultat Attendu

? Page Incidents fonctionnelle avec:
- Liste des tickets avec filtres
- Création de tickets
- Changement de statut
- Assignation (Manager/Admin)
- Suppression (Admin)
- Statistiques en temps réel
- Feedback utilisateur (Snackbar)

**Temps total:** 2-3 heures ??
