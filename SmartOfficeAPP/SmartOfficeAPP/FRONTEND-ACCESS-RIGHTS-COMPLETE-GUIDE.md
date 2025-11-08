# ?? Guide Frontend - Gestion des Droits d'Accès

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

La page de gestion des droits d'accès permet aux administrateurs de:
- ? Consulter tous les droits d'accès
- ? Créer de nouveaux droits d'accès (Zone ou Périphérique)
- ? Modifier des droits existants
- ? Supprimer des droits d'accès
- ? Filtrer et rechercher les droits

### Architecture Backend Complète

```
???????????????????????????????????????????????????????????
?      Frontend Blazor (MySmartApp.Ui)         ?
?     /access page      ?
???????????????????????????????????????????????????????????
          ? HTTP/JSON (camelCase)
    ?
???????????????????????????????????????????????????????????
?           DroitAccesController (API Backend)      ?
?         /api/admin/access-rights     ?
???????????????????????????????????????????????????????????
?  - GetDroitsAcces()          ? List all       ?
?  - GetDroitAccesById(id)     ? Get one ?
?  - AddDroitAcces(dto)        ? Create               ?
?  - UpdateDroitAcces(id, dto) ? Update    ?
?  - DeleteDroitAcces(id)      ? Delete   ?
???????????????????????????????????????????????????????????
          ?
   ?
???????????????????????????????????????????????????????????
?              DroitAccesService (Business Logic)     ?
?  - Mapping entités ? DTOs enrichis      ?
?  - Validation métier        ?
???????????????????????????????????????????????????????????
       ?
        ?
???????????????????????????????????????????????????????????
?      DroitAccesRepository (Data Access)        ?
?  - Include User, Zone, PeripheriqueLogique    ?
?  - Gestion timestamps (CreatedAt, UpdatedAt)            ?
???????????????????????????????????????????????????????????
      ?
  ?
???????????????????????????????????????????????????????????
?       PostgreSQL Database         ?
?  Table: DroitsAcces (avec timestamps) ?
???????????????????????????????????????????????????????????
```

---

## ?? APIs Disponibles

### Base URL
```
https://localhost:7144/api
```

### 1. **GET** - Liste de tous les droits d'accès

```http
GET /api/admin/access-rights
Authorization: Bearer {token}
```

**Réponse (200 OK):**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": 1,
      "userId": "abc-123-def",
  "userName": "john.doe",
      "userEmail": "john.doe@example.com",
      "userFullName": "John Doe",
      "zoneId": 5,
      "zoneName": "Salle de réunion A",
      "zoneDescription": null,
    "peripheriqueLogiqueId": null,
      "peripheriqueLogiqueName": null,
      "peripheriqueLogiqueType": null,
"type": "Controle",
      "createdAt": "2025-01-06T10:30:00Z",
      "updatedAt": null
    },
    {
      "id": 2,
      "userId": "xyz-789-ghi",
      "userName": "jane.smith",
      "userEmail": "jane.smith@example.com",
      "userFullName": "Jane Smith",
      "zoneId": null,
      "zoneName": null,
      "zoneDescription": null,
  "peripheriqueLogiqueId": 12,
      "peripheriqueLogiqueName": "Climatisation Bureau 3",
      "peripheriqueLogiqueType": "HVAC",
  "type": "Lecture",
      "createdAt": "2025-01-05T14:20:00Z",
    "updatedAt": "2025-01-06T09:15:00Z"
    }
  ],
  "databaseTimestamp": "2025-01-06T12:00:00Z"
}
```

---

### 2. **GET** - Récupérer un droit d'accès par ID

```http
GET /api/admin/access-rights/{id}
Authorization: Bearer {token}
```

**Réponse (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": 1,
    "userId": "abc-123-def",
    "userName": "john.doe",
 "userEmail": "john.doe@example.com",
    "userFullName": "John Doe",
    "zoneId": 5,
    "zoneName": "Salle de réunion A",
    "zoneDescription": null,
    "peripheriqueLogiqueId": null,
    "peripheriqueLogiqueName": null,
    "peripheriqueLogiqueType": null,
    "type": "Controle",
    "createdAt": "2025-01-06T10:30:00Z",
    "updatedAt": null
  },
  "databaseTimestamp": "2025-01-06T12:00:00Z"
}
```

---

### 3. **POST** - Créer un nouveau droit d'accès

```http
POST /api/admin/access-rights
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "abc-123-def",
  "zoneId": 5,
  "peripheriqueLogiqueId": null,
  "type": "Controle"
}
```

**Paramètres:**

| Champ | Type | Requis | Description | Valeurs possibles |
|-------|------|--------|-------------|-------------------|
| `userId` | string | ? | ID de l'utilisateur | UUID d'Identity |
| `zoneId` | int? | ? | ID de la zone | Si accès à une zone |
| `peripheriqueLogiqueId` | int? | ? | ID du périphérique | Si accès à un périphérique |
| `type` | string | ? | Type de droit | `"Lecture"` ou `"Controle"` |

**?? Important:** Un droit doit avoir **soit** `zoneId`, **soit** `peripheriqueLogiqueId` (pas les deux).

**Réponse (201 Created):**
```json
{
  "isSuccess": true,
  "data": {
    "id": 42,
    "userId": "abc-123-def",
    "userName": "john.doe",
    "userEmail": "john.doe@example.com",
    "userFullName": "John Doe",
    "zoneId": 5,
    "zoneName": "Salle de réunion A",
    "zoneDescription": null,
    "peripheriqueLogiqueId": null,
    "peripheriqueLogiqueName": null,
    "peripheriqueLogiqueType": null,
    "type": "Controle",
    "createdAt": "2025-01-06T12:30:00Z",
    "updatedAt": null
  },
  "databaseTimestamp": "2025-01-06T12:30:00Z"
}
```

---

### 4. **PUT** - Mettre à jour un droit d'accès

```http
PUT /api/admin/access-rights/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "abc-123-def",
  "zoneId": 5,
  "peripheriqueLogiqueId": null,
  "type": "Lecture"
}
```

**Réponse (204 No Content):**
```
(Pas de contenu - succès)
```

**Réponse (404 Not Found):**
```json
{
  "error": "Droit d'accès non trouvé ou impossible de mettre à jour."
}
```

---

### 5. **DELETE** - Supprimer un droit d'accès

```http
DELETE /api/admin/access-rights/{id}
Authorization: Bearer {token}
```

**Réponse (204 No Content):**
```
(Pas de contenu - succès)
```

**Réponse (404 Not Found):**
```json
{
  "error": "Droit d'accès non trouvé."
}
```

---

### 6. **Bonus** - APIs Simplifiées

#### Affecter une Zone à un Utilisateur

```http
POST /api/admin/assign-zone-to-user
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "abc-123-def",
  "zoneId": 5,
  "accessType": 1  // 0 = Lecture, 1 = Controle
}
```

#### Affecter un Périphérique à un Utilisateur

```http
POST /api/admin/assign-device-to-user
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "abc-123-def",
  "peripheriqueLogiqueId": 12,
  "accessType": 0  // 0 = Lecture, 1 = Controle
}
```

---

## ?? Modèles de Données (DTOs)

### DroitAccesDto (Réponse complète)

```csharp
public class DroitAccesDto
{
    public int Id { get; set; }
    
    // ? Utilisateur
    public string UserId { get; set; }
    public string? UserName { get; set; }
 public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    
    // ? Zone
 public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public string? ZoneDescription { get; set; }
    
    // ? Périphérique
    public int? PeripheriqueLogiqueId { get; set; }
    public string? PeripheriqueLogiqueName { get; set; }
    public string? PeripheriqueLogiqueType { get; set; }
    
    // ? Type d'accès
    public string Type { get; set; }  // "Lecture" ou "Controle"
    
 // ? Métadonnées
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### DroitAccesCreationDto (Création/Modification)

```csharp
public class DroitAccesCreationDto
{
    public string UserId { get; set; }          // Requis
    public int? ZoneId { get; set; }  // Optionnel (Zone OU Périphérique)
    public int? PeripheriqueLogiqueId { get; set; }  // Optionnel (Zone OU Périphérique)
    public string Type { get; set; }            // Requis: "Lecture" ou "Controle"
}
```

### TypeScript/C# Équivalent pour Frontend

```typescript
// TypeScript
export interface DroitAccesDto {
  id: number;
  userId: string;
  userName?: string;
  userEmail?: string;
  userFullName?: string;
  zoneId?: number;
  zoneName?: string;
  zoneDescription?: string;
  peripheriqueLogiqueId?: number;
  peripheriqueLogiqueName?: string;
  peripheriqueLogiqueType?: string;
  type: 'Lecture' | 'Controle';
  createdAt: string;  // ISO 8601 date
  updatedAt?: string;
}

export interface DroitAccesCreationDto {
  userId: string;
  zoneId?: number;
  peripheriqueLogiqueId?: number;
  type: 'Lecture' | 'Controle';
}
```

```csharp
// C# (Blazor)
public class DroitAccesDto
{
    public int Id { get; set; }
 public string UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public string? ZoneDescription { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
    public string? PeripheriqueLogiqueName { get; set; }
 public string? PeripheriqueLogiqueType { get; set; }
    public string Type { get; set; }
public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record DroitAccesCreationDto(
    string UserId,
    int? ZoneId,
    int? PeripheriqueLogiqueId,
    string Type
);
```

---

## ?? Implémentation du Service Frontend

### Interface IDroitAccesService

```csharp
public interface IDroitAccesService
{
    Task<ApiResponse<List<DroitAccesDto>>> GetDroitsAccesAsync();
    Task<ApiResponse<DroitAccesDto>> GetDroitAccesByIdAsync(int id);
    Task<ApiResponse<DroitAccesDto>> AddDroitAccesAsync(DroitAccesCreationDto dto);
    Task<ApiResponse<bool>> UpdateDroitAccesAsync(int id, DroitAccesCreationDto dto);
    Task<ApiResponse<bool>> DeleteDroitAccesAsync(int id);
}
```

### Implémentation DroitAccesService

```csharp
public class DroitAccesService : IDroitAccesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DroitAccesService> _logger;

    public DroitAccesService(HttpClient httpClient, ILogger<DroitAccesService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<List<DroitAccesDto>>> GetDroitsAccesAsync()
    {
   try
        {
    var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<DroitAccesDto>>>(
 "api/admin/access-rights");
      
    return response ?? new ApiResponse<List<DroitAccesDto>>
            {
        IsSuccess = false,
              Data = new List<DroitAccesDto>()
   };
        }
        catch (Exception ex)
     {
        _logger.LogError(ex, "Erreur lors du chargement des droits d'accès");
     return new ApiResponse<List<DroitAccesDto>>
            {
 IsSuccess = false,
           Data = new List<DroitAccesDto>()
            };
        }
    }

    public async Task<ApiResponse<DroitAccesDto>> GetDroitAccesByIdAsync(int id)
    {
   try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<DroitAccesDto>>(
           $"api/admin/access-rights/{id}");
         
            return response ?? new ApiResponse<DroitAccesDto> { IsSuccess = false };
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Erreur lors du chargement du droit d'accès {Id}", id);
       return new ApiResponse<DroitAccesDto> { IsSuccess = false };
}
    }

    public async Task<ApiResponse<DroitAccesDto>> AddDroitAccesAsync(DroitAccesCreationDto dto)
    {
  try
        {
       var response = await _httpClient.PostAsJsonAsync("api/admin/access-rights", dto);
     
   if (response.IsSuccessStatusCode)
         {
    var result = await response.Content.ReadFromJsonAsync<ApiResponse<DroitAccesDto>>();
        return result ?? new ApiResponse<DroitAccesDto> { IsSuccess = false };
        }
    
            return new ApiResponse<DroitAccesDto> { IsSuccess = false };
        }
        catch (Exception ex)
        {
       _logger.LogError(ex, "Erreur lors de la création du droit d'accès");
return new ApiResponse<DroitAccesDto> { IsSuccess = false };
    }
    }

    public async Task<ApiResponse<bool>> UpdateDroitAccesAsync(int id, DroitAccesCreationDto dto)
{
        try
        {
     var response = await _httpClient.PutAsJsonAsync($"api/admin/access-rights/{id}", dto);
    
            return new ApiResponse<bool>
            {
           IsSuccess = response.IsSuccessStatusCode,
   Data = response.IsSuccessStatusCode
     };
    }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Erreur lors de la mise à jour du droit d'accès {Id}", id);
       return new ApiResponse<bool> { IsSuccess = false, Data = false };
    }
    }

    public async Task<ApiResponse<bool>> DeleteDroitAccesAsync(int id)
    {
     try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/access-rights/{id}");
            
    return new ApiResponse<bool>
 {
       IsSuccess = response.IsSuccessStatusCode,
        Data = response.IsSuccessStatusCode
        };
        }
     catch (Exception ex)
     {
            _logger.LogError(ex, "Erreur lors de la suppression du droit d'accès {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
  }
}
```

### Enregistrement du Service

```csharp
// Program.cs ou Startup.cs
builder.Services.AddScoped<IDroitAccesService, DroitAccesService>();
```

---

## ?? Corrections de la Page Blazor

### Problèmes Identifiés dans Votre Page

1. **Propriété `PeripheriqueName` inexistante** ? Utiliser `PeripheriqueLogiqueName`
2. **Propriété `Type` mal gérée** ? `Type` est `"Lecture"` ou `"Controle"`, pas `"Zone"` ou `"Peripherique"`
3. **Filtre par type incorrect** ? Filtrer par ZoneId vs PeripheriqueLogiqueId

### Code Corrigé

#### **1. Affichage du nom du périphérique**

```razor
@* ? AVANT (incorrect) *@
@(context.PeripheriqueName ?? $"Périphérique {context.PeripheriqueLogiqueId}")

@* ? APRÈS (correct) *@
@(context.PeripheriqueLogiqueName ?? $"Périphérique {context.PeripheriqueLogiqueId}")
```

#### **2. Affichage du nom d'utilisateur**

```razor
@* ? AVANT (UserName peut être null) *@
@(context.UserName ?? context.UserId)

@* ? APRÈS (avec fallback complet) *@
@(context.UserFullName ?? context.UserName ?? context.UserId)
```

#### **3. Filtre par type de ressource (Zone vs Périphérique)**

```razor
<MudSelect T="string?" @bind-Value="selectedResourceTypeFilter" Label="Ressource" Clearable Style="min-width: 150px;">
    <MudSelectItem T="string?" Value="@((string?)"Zone")">Zone</MudSelectItem>
    <MudSelectItem T="string?" Value="@((string?)"Peripherique")">Périphérique</MudSelectItem>
</MudSelect>
```

```csharp
private IEnumerable<DroitAccesDto> filteredDroits =>
    droitsAcces.Where(d => 
        (string.IsNullOrEmpty(searchString) || 
       (d.UserFullName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) ||
   (d.UserName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) ||
   d.UserId.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
         (d.ZoneName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) ||
       (d.PeripheriqueLogiqueName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)) &&
        (string.IsNullOrEmpty(selectedResourceTypeFilter) || 
         (selectedResourceTypeFilter == "Zone" && d.ZoneId.HasValue) ||
      (selectedResourceTypeFilter == "Peripherique" && d.PeripheriqueLogiqueId.HasValue))
  );
```

#### **4. Affichage du type d'accès**

```csharp
private string GetTypeDisplay(string type) => type switch
{
    "Lecture" => "??? Lecture",
    "Controle" => "??? Contrôle",
    _ => type
};

private Color GetTypeColor(string type) => type switch
{
    "Lecture" => Color.Info,
    "Controle" => Color.Warning,
  _ => Color.Default
};
```

#### **5. Sélection du type de ressource dans le formulaire**

```csharp
private void OnTypeChanged()
{
 // Réinitialiser les champs selon la sélection
    if (editingDroit.ResourceType == "Zone")
    {
        editingDroit.PeripheriqueLogiqueId = null;
    }
    else if (editingDroit.ResourceType == "Peripherique")
    {
        editingDroit.ZoneId = null;
    }
}
```

#### **6. Modèle d'édition complet**

```csharp
public class DroitAccesEditModel
{
    public int Id { get; set; }
public string UserId { get; set; } = "";
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
    public string AccessType { get; set; } = "Controle";  // "Lecture" ou "Controle"
    public string ResourceType { get; set; } = "Zone";     // "Zone" ou "Peripherique" (pour l'UI uniquement)
}
```

---

## ?? Exemple Complet de Page Corrigée

```razor
@page "/access"
@using MySmartApp.Ui.Services
@using MySmartApp.Ui.Models
@inject IDialogService DialogService
@inject IDroitAccesService DroitAccesService
@inject ISnackbar Snackbar
@inject ILogger<Access> Logger
@attribute [Authorize(Roles = "Admin")]

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="my-8">
    <MudText Typo="Typo.h4" Class="mb-4">?? Gestion des Droits d'Accès</MudText>
    
    <div class="d-flex align-center justify-space-between mb-4">
   <MudButton OnClick="OpenAddDialog" Variant="Variant.Filled" Color="Color.Primary" StartIcon="@Icons.Material.Filled.Add">
          Ajouter un droit d'accès
     </MudButton>
     
        <div class="d-flex gap-2">
            <MudTextField T="string" 
             @bind-Value="searchString"
           Placeholder="Rechercher..." 
         Adornment="Adornment.Start" 
    AdornmentIcon="@Icons.Material.Filled.Search" 
     Style="max-width: 300px;" />
       
            <MudSelect T="string?" @bind-Value="selectedResourceTypeFilter" Label="Ressource" Clearable Style="min-width: 150px;">
    <MudSelectItem T="string?" Value="@((string?)"Zone")">?? Zone</MudSelectItem>
          <MudSelectItem T="string?" Value="@((string?)"Peripherique")">?? Périphérique</MudSelectItem>
            </MudSelect>
  
            <MudSelect T="string?" @bind-Value="selectedAccessTypeFilter" Label="Type d'accès" Clearable Style="min-width: 150px;">
<MudSelectItem T="string?" Value="@((string?)"Lecture")">??? Lecture</MudSelectItem>
       <MudSelectItem T="string?" Value="@((string?)"Controle")">??? Contrôle</MudSelectItem>
  </MudSelect>
            
     <MudButton OnClick="RefreshData" 
      Variant="Variant.Outlined" 
        StartIcon="@Icons.Material.Filled.Refresh"
      Disabled="@isLoading">
   Actualiser
     </MudButton>
        </div>
    </div>

    @if (isLoading)
    {
        <MudProgressLinear Indeterminate="true" />
    }
    else
    {
      <MudTable Items="@filteredDroits" Dense="true" Hover="true" Elevation="2">
            <HeaderContent>
       <MudTh>ID</MudTh>
     <MudTh>Utilisateur</MudTh>
      <MudTh>Type d'accès</MudTh>
     <MudTh>Ressource</MudTh>
     <MudTh>Créé le</MudTh>
      <MudTh>Actions</MudTh>
   </HeaderContent>
            <RowTemplate>
       <MudTd DataLabel="ID">@context.Id</MudTd>
  <MudTd DataLabel="User">
    <div class="d-flex flex-column">
             <MudText Typo="Typo.body2">
 @(context.UserFullName ?? context.UserName ?? context.UserId)
               </MudText>
       @if (!string.IsNullOrEmpty(context.UserEmail))
      {
        <MudText Typo="Typo.caption" Color="Color.Secondary">@context.UserEmail</MudText>
     }
           </div>
      </MudTd>
            <MudTd DataLabel="AccessType">
          <MudChip Size="Size.Small" Color="@GetTypeColor(context.Type)">
     @GetTypeDisplay(context.Type)
     </MudChip>
        </MudTd>
        <MudTd DataLabel="Resource">
       @if (context.ZoneId.HasValue)
         {
 <div class="d-flex align-center">
          <MudIcon Icon="@Icons.Material.Filled.LocationOn" Class="mr-2" />
          <div>
              <MudText Typo="Typo.body2">@(context.ZoneName ?? $"Zone {context.ZoneId}")</MudText>
      <MudText Typo="Typo.caption" Color="Color.Secondary">Zone #{context.ZoneId}</MudText>
        </div>
   </div>
         }
             else if (context.PeripheriqueLogiqueId.HasValue)
      {
    <div class="d-flex align-center">
   <MudIcon Icon="@Icons.Material.Filled.DeviceHub" Class="mr-2" />
  <div>
    <MudText Typo="Typo.body2">@(context.PeripheriqueLogiqueName ?? $"Périphérique {context.PeripheriqueLogiqueId}")</MudText>
                 @if (!string.IsNullOrEmpty(context.PeripheriqueLogiqueType))
       {
     <MudText Typo="Typo.caption" Color="Color.Secondary">@context.PeripheriqueLogiqueType</MudText>
  }
         </div>
 </div>
         }
       </MudTd>
                <MudTd DataLabel="CreatedAt">
 <MudText Typo="Typo.body2">@context.CreatedAt.ToString("dd/MM/yyyy HH:mm")</MudText>
          @if (context.UpdatedAt.HasValue)
           {
       <MudText Typo="Typo.caption" Color="Color.Secondary">
      Modifié: @context.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm")
     </MudText>
               }
    </MudTd>
        <MudTd DataLabel="Actions">
    <MudIconButton Icon="@Icons.Material.Filled.Edit" 
 Color="Color.Primary" 
  Size="Size.Small"
           OnClick="@(() => OpenEditDialog(context))" />
        
  <MudIconButton Icon="@Icons.Material.Filled.Delete" 
   Color="Color.Error" 
      Size="Size.Small"
                   OnClick="@(() => DeleteDroitAcces(context))" />
           </MudTd>
   </RowTemplate>
            
  <NoRecordsContent>
  <MudText Align="Align.Center" Color="Color.Secondary" Class="py-8">
       Aucun droit d'accès trouvé
           </MudText>
  </NoRecordsContent>
        </MudTable>
 }
</MudContainer>

@code {
    private bool isLoading = true;
    private string searchString = "";
    private string? selectedResourceTypeFilter;
    private string? selectedAccessTypeFilter;
    
    private List<DroitAccesDto> droitsAcces = new();
    
  private IEnumerable<DroitAccesDto> filteredDroits =>
        droitsAcces.Where(d => 
   (string.IsNullOrEmpty(searchString) || 
      (d.UserFullName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) ||
   (d.UserName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) ||
             d.UserId.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
   (d.ZoneName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true) ||
     (d.PeripheriqueLogiqueName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true)) &&
            (string.IsNullOrEmpty(selectedResourceTypeFilter) || 
             (selectedResourceTypeFilter == "Zone" && d.ZoneId.HasValue) ||
             (selectedResourceTypeFilter == "Peripherique" && d.PeripheriqueLogiqueId.HasValue)) &&
            (string.IsNullOrEmpty(selectedAccessTypeFilter) || d.Type == selectedAccessTypeFilter)
        );

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        StateHasChanged();
        
        try
        {
            var response = await DroitAccesService.GetDroitsAccesAsync();
       
            if (response.IsSuccess && response.Data != null)
         {
     droitsAcces = response.Data;
       Logger.LogInformation("? {Count} droits d'accès chargés", droitsAcces.Count);
            }
      else
    {
   Snackbar.Add("Erreur lors du chargement des droits d'accès", Severity.Error);
      }
     }
        catch (Exception ex)
      {
  Logger.LogError(ex, "? Erreur lors du chargement des données");
            Snackbar.Add("Erreur lors du chargement des droits d'accès", Severity.Error);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData() => await LoadData();

    private void OpenAddDialog()
    {
        // TODO: Implémenter le dialog d'ajout
    }

    private void OpenEditDialog(DroitAccesDto droit)
    {
        // TODO: Implémenter le dialog d'édition
    }

 private async Task DeleteDroitAcces(DroitAccesDto droit)
    {
        bool? result = await DialogService.ShowMessageBox(
       "?? Confirmation de suppression",
        $"Supprimer le droit d'accès de {droit.UserFullName ?? droit.UserName ?? droit.UserId} ?",
            yesText: "Supprimer",
       cancelText: "Annuler");

        if (result == true)
   {
    var deleteResult = await DroitAccesService.DeleteDroitAccesAsync(droit.Id);
            if (deleteResult.IsSuccess)
    {
         Snackbar.Add("Droit d'accès supprimé avec succès", Severity.Success);
                await LoadData();
        }
 else
            {
          Snackbar.Add("Erreur lors de la suppression", Severity.Error);
            }
   }
    }

    private string GetTypeDisplay(string type) => type switch
    {
        "Lecture" => "??? Lecture",
    "Controle" => "??? Contrôle",
        _ => type
    };

  private Color GetTypeColor(string type) => type switch
 {
        "Lecture" => Color.Info,
        "Controle" => Color.Warning,
        _ => Color.Default
    };
}
```

---

## ?? Exemples de Requêtes (Tests)

### Fichier: `access-rights-tests.http`

```http
### Variables
@baseUrl = https://localhost:5001
@token = your-jwt-token-here

### 1. Liste tous les droits d'accès
GET {{baseUrl}}/api/admin/access-rights
Authorization: Bearer {{token}}

### 2. Récupérer un droit par ID
GET {{baseUrl}}/api/admin/access-rights/1
Authorization: Bearer {{token}}

### 3. Créer un droit d'accès ZONE
POST {{baseUrl}}/api/admin/access-rights
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "userId": "abc-123-def-456",
  "zoneId": 5,
  "peripheriqueLogiqueId": null,
  "type": "Controle"
}

### 4. Créer un droit d'accès PÉRIPHÉRIQUE
POST {{baseUrl}}/api/admin/access-rights
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "userId": "abc-123-def-456",
  "zoneId": null,
  "peripheriqueLogiqueId": 12,
  "type": "Lecture"
}

### 5. Modifier un droit d'accès
PUT {{baseUrl}}/api/admin/access-rights/1
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "userId": "abc-123-def-456",
  "zoneId": 5,
  "peripheriqueLogiqueId": null,
  "type": "Lecture"
}

### 6. Supprimer un droit d'accès
DELETE {{baseUrl}}/api/admin/access-rights/1
Authorization: Bearer {{token}}

### 7. Affecter une zone (API simplifiée)
POST {{baseUrl}}/api/admin/assign-zone-to-user
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "userId": "abc-123-def-456",
  "zoneId": 3,
  "accessType": 1
}

### 8. Affecter un périphérique (API simplifiée)
POST {{baseUrl}}/api/admin/assign-device-to-user
Authorization: Bearer {{token}}
Content-Type: application/json

{
  "userId": "abc-123-def-456",
  "peripheriqueLogiqueId": 8,
  "accessType": 0
}
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
| `404` | Not Found | Message "Ressource introuvable" |
| `500` | Internal Server Error | Message d'erreur générique |

### Exemple de gestion d'erreur

```csharp
try
{
    var response = await DroitAccesService.AddDroitAccesAsync(dto);
    
    if (response.IsSuccess)
    {
        Snackbar.Add("? Droit d'accès créé avec succès", Severity.Success);
     await LoadData();
      CloseDialog();
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
Snackbar.Add("?? Vous n'avez pas l'autorisation requise", Severity.Error);
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

- [ ] ? GET /api/admin/access-rights retourne toutes les données
- [ ] ? Les propriétés `userName`, `userEmail`, `userFullName` sont présentes
- [ ] ? Les propriétés `createdAt` et `updatedAt` sont présentes
- [ ] ? POST crée un nouveau droit correctement
- [ ] ? PUT met à jour un droit existant
- [ ] ? DELETE supprime un droit
- [ ] ? Les filtres de recherche fonctionnent
- [ ] ? Le tri par date fonctionne
- [ ] ? Les types d'accès ("Lecture", "Controle") s'affichent correctement
- [ ] ? La distinction Zone/Périphérique fonctionne
- [ ] ? Les erreurs 404/401/403/500 sont gérées

### Scénarios de Test

#### Test 1: Créer un droit d'accès ZONE

```
1. Cliquer sur "Ajouter"
2. Remplir UserId: "test-user-123"
3. Sélectionner ResourceType: "Zone"
4. Remplir ZoneId: 5
5. Sélectionner AccessType: "Controle"
6. Valider
? Un nouveau droit apparaît dans la liste avec zoneName affiché
```

#### Test 2: Créer un droit d'accès PÉRIPHÉRIQUE

```
1. Cliquer sur "Ajouter"
2. Remplir UserId: "test-user-123"
3. Sélectionner ResourceType: "Peripherique"
4. Remplir PeripheriqueLogiqueId: 12
5. Sélectionner AccessType: "Lecture"
6. Valider
? Un nouveau droit apparaît avec peripheriqueLogiqueName et type affiché
```

#### Test 3: Filtrer par Zone

```
1. Sélectionner filtre "Ressource": Zone
? Seuls les droits avec ZoneId != null sont affichés
```

#### Test 4: Filtrer par Type d'accès

```
1. Sélectionner filtre "Type d'accès": Lecture
? Seuls les droits avec Type="Lecture" sont affichés
```

#### Test 5: Rechercher par nom d'utilisateur

```
1. Entrer "john" dans le champ de recherche
? Seuls les droits de John Doe sont affichés
```

---

## ?? Ressources Supplémentaires

### Documentation Backend

- [Model: DroitAcces.cs](../Models/DroitAcces.cs)
- [DTO: DroitAccesDto.cs](../DTOs/DroitAccesDto.cs)
- [Controller: DroitAccesController.cs](../Controllers/DroitAccesController.cs)
- [Service: DroitAccesService.cs](../Services/ServicesImpl/DroitAccesService.cs)
- [Repository: DroitAccesRepository.cs](../Repositories/Repos/DroitAccesRepository.cs)

### Types d'accès (Enum)

```csharp
public enum DroitAccesType
{
    Lecture = 0,    // L'utilisateur peut consulter l'entité
    Controle = 1  // L'utilisateur peut contrôler l'entité
}
```

**Conversion:**
- Backend ? Frontend: `DroitAccesType.Controle` ? `"Controle"` (string)
- Frontend ? Backend: `"Controle"` ? `DroitAccesType.Controle` (enum)

---

## ?? Résumé des Changements Backend

### ? Ce qui a été modifié

1. **Modèle `DroitAcces`:**
   - Ajout de `CreatedAt` (DateTime)
   - Ajout de `UpdatedAt` (DateTime?)

2. **DTO `DroitAccesDto`:**
   - Ajout de `UserName` (string?)
   - Ajout de `UserEmail` (string?)
   - Ajout de `UserFullName` (string?)
   - Ajout de `PeripheriqueLogiqueType` (string?)
   - Ajout de `CreatedAt` (DateTime)
   - Ajout de `UpdatedAt` (DateTime?)

3. **Repository `DroitAccesRepository`:**
   - Chargement automatique de la relation `User` via `.Include(da => da.User)`
   - Tri par `CreatedAt` descendant
   - Gestion automatique des timestamps

4. **Service `DroitAccesService`:**
   - Mapping enrichi avec toutes les propriétés utilisateur
   - Mapping du type de périphérique
   - Mapping des timestamps

5. **Migration:**
   - Création de `AddTimestampsToDroitAcces` appliquée à la base de données

### ? Toutes les APIs sont prêtes

- ? `GET /api/admin/access-rights`
- ? `GET /api/admin/access-rights/{id}`
- ? `POST /api/admin/access-rights`
- ? `PUT /api/admin/access-rights/{id}`
- ? `DELETE /api/admin/access-rights/{id}`
- ? `POST /api/admin/assign-zone-to-user` (bonus)
- ? `POST /api/admin/assign-device-to-user` (bonus)

---

## ?? Prochaines Étapes

1. **Implémenter le service frontend** (`IDroitAccesService`)
2. **Corriger les noms de propriétés** dans la page Blazor
3. **Tester les APIs** avec le fichier `.http`
4. **Implémenter les dialogs** d'ajout/modification
5. **Valider les scénarios** de test

---

**Date:** 6 janvier 2025  
**Status:** ? Backend complet et prêt  
**Prochaine étape:** Implémentation frontend
