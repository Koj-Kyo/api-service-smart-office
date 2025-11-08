# ?? Quick Start - Intégration Frontend des Droits d'Accès

## ?? Temps estimé: 2 heures

---

## ?? Checklist Rapide

- [ ] 1. Créer le service `IDroitAccesService` (30 min)
- [ ] 2. Corriger les propriétés dans la page Blazor (30 min)
- [ ] 3. Tester les APIs avec les fichiers HTTP (30 min)
- [ ] 4. Valider l'affichage frontend (30 min)

---

## ?? Étape 1: Créer le Service Frontend (30 min)

### 1.1 - Créer l'interface

**Fichier:** `MySmartApp.Ui/Services/IDroitAccesService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IDroitAccesService
{
    Task<ApiResponse<List<DroitAccesDto>>> GetDroitsAccesAsync();
    Task<ApiResponse<DroitAccesDto>> GetDroitAccesByIdAsync(int id);
    Task<ApiResponse<DroitAccesDto>> AddDroitAccesAsync(DroitAccesCreationDto dto);
 Task<ApiResponse<bool>> UpdateDroitAccesAsync(int id, DroitAccesCreationDto dto);
    Task<ApiResponse<bool>> DeleteDroitAccesAsync(int id);
}
```

### 1.2 - Implémenter le service

**Fichier:** `MySmartApp.Ui/Services/DroitAccesService.cs`

```csharp
using MySmartApp.Ui.Models;
using System.Net.Http.Json;

namespace MySmartApp.Ui.Services;

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
      return await _httpClient.GetFromJsonAsync<ApiResponse<DroitAccesDto>>(
         $"api/admin/access-rights/{id}") 
     ?? new ApiResponse<DroitAccesDto> { IsSuccess = false };
     }
        catch (Exception ex)
        {
  _logger.LogError(ex, "Erreur lors du chargement du droit {Id}", id);
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
       return await response.Content.ReadFromJsonAsync<ApiResponse<DroitAccesDto>>() 
        ?? new ApiResponse<DroitAccesDto> { IsSuccess = false };
            }
        
       return new ApiResponse<DroitAccesDto> { IsSuccess = false };
   }
        catch (Exception ex)
      {
_logger.LogError(ex, "Erreur lors de la création");
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
       _logger.LogError(ex, "Erreur lors de la mise à jour {Id}", id);
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
          _logger.LogError(ex, "Erreur lors de la suppression {Id}", id);
            return new ApiResponse<bool> { IsSuccess = false, Data = false };
        }
    }
}
```

### 1.3 - Enregistrer le service

**Fichier:** `MySmartApp.Ui/Program.cs`

```csharp
// Ajouter après les autres services
builder.Services.AddScoped<IDroitAccesService, DroitAccesService>();
```

---

## ?? Étape 2: Corriger la Page Blazor (30 min)

### 2.1 - Correction #1: Nom du périphérique

```razor
@* ? AVANT *@
<MudText>@context.PeripheriqueName</MudText>

@* ? APRÈS *@
<MudText>@context.PeripheriqueLogiqueName</MudText>
```

### 2.2 - Correction #2: Affichage utilisateur avec fallback

```razor
@* ? AVANT *@
<MudText>@(context.UserName ?? context.UserId)</MudText>

@* ? APRÈS *@
<MudText>@(context.UserFullName ?? context.UserName ?? context.UserId)</MudText>
@if (!string.IsNullOrEmpty(context.UserEmail))
{
    <MudText Typo="Typo.caption" Color="Color.Secondary">@context.UserEmail</MudText>
}
```

### 2.3 - Correction #3: Type d'accès (pas type de ressource)

```csharp
// ? AVANT - Type utilisé pour Zone/Périphérique
private string GetTypeDisplay(string type) => type switch
{
 "Zone" => "?? Zone",
    "Peripherique" => "?? Périphérique",
    _ => type
};

// ? APRÈS - Type pour Lecture/Contrôle
private string GetTypeDisplay(string type) => type switch
{
    "Lecture" => "??? Lecture",
    "Controle" => "??? Contrôle",
    _ => type
};
```

### 2.4 - Correction #4: Filtre par ressource

```csharp
// ? AVANT
private string? selectedTypeFilter;

<MudSelect T="string?" @bind-Value="selectedTypeFilter">
    <MudSelectItem Value="@((string?)"Zone")">Zone</MudSelectItem>
    <MudSelectItem Value="@((string?)"Peripherique")">Périphérique</MudSelectItem>
</MudSelect>

filteredDroits.Where(d => selectedTypeFilter == null || d.Type == selectedTypeFilter)

// ? APRÈS
private string? selectedResourceTypeFilter;
private string? selectedAccessTypeFilter;

<MudSelect T="string?" @bind-Value="selectedResourceTypeFilter" Label="Ressource">
    <MudSelectItem Value="@((string?)"Zone")">?? Zone</MudSelectItem>
    <MudSelectItem Value="@((string?)"Peripherique")">?? Périphérique</MudSelectItem>
</MudSelect>

<MudSelect T="string?" @bind-Value="selectedAccessTypeFilter" Label="Type d'accès">
    <MudSelectItem Value="@((string?)"Lecture")">??? Lecture</MudSelectItem>
    <MudSelectItem Value="@((string?)"Controle")">??? Contrôle</MudSelectItem>
</MudSelect>

filteredDroits.Where(d => 
    (selectedResourceTypeFilter == null || 
     (selectedResourceTypeFilter == "Zone" && d.ZoneId.HasValue) ||
  (selectedResourceTypeFilter == "Peripherique" && d.PeripheriqueLogiqueId.HasValue)) &&
    (selectedAccessTypeFilter == null || d.Type == selectedAccessTypeFilter)
)
```

### 2.5 - Correction #5: Modèle d'édition

```csharp
// ? AVANT
public class DroitAccesEditModel
{
    public int Id { get; set; }
public string UserId { get; set; } = "";
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
    public string Type { get; set; } = "";  // Confusion: Zone/Périphérique OU Lecture/Contrôle ?
}

// ? APRÈS
public class DroitAccesEditModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
    public string AccessType { get; set; } = "Controle";  // "Lecture" ou "Controle"
  public string ResourceType { get; set; } = "Zone";    // "Zone" ou "Peripherique" (UI uniquement)
}
```

### 2.6 - Correction #6: Logique du formulaire

```csharp
// ? Quand l'utilisateur change le type de ressource
private void OnResourceTypeChanged()
{
    if (editingDroit.ResourceType == "Zone")
    {
        editingDroit.PeripheriqueLogiqueId = null;
  }
    else if (editingDroit.ResourceType == "Peripherique")
    {
        editingDroit.ZoneId = null;
    }
}

// ? Conversion pour l'envoi à l'API
private async Task SaveDroitAcces()
{
    var creationDto = new DroitAccesCreationDto(
        editingDroit.UserId,
        editingDroit.ZoneId,
        editingDroit.PeripheriqueLogiqueId,
    editingDroit.AccessType  // "Lecture" ou "Controle"
    );

    if (isAdding)
    {
        var result = await DroitAccesService.AddDroitAccesAsync(creationDto);
        // ...
}
}
```

---

## ?? Étape 3: Tester les APIs (30 min)

### 3.1 - Obtenir un token admin

```http
### 1. Login Admin
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin@123"
}

# Copier le token JWT retourné
```

### 3.2 - Tester GET (Liste)

```http
GET https://localhost:5001/api/admin/access-rights
Authorization: Bearer {votre-token}
```

**? Vérifier:**
- `userName`, `userEmail`, `userFullName` présents
- `createdAt` présent
- `peripheriqueLogiqueType` présent (si périphérique)
- Données triées par `createdAt` descendant

### 3.3 - Tester POST (Création Zone)

```http
POST https://localhost:5001/api/admin/access-rights
Authorization: Bearer {votre-token}
Content-Type: application/json

{
  "userId": "votre-user-id",
  "zoneId": 1,
  "peripheriqueLogiqueId": null,
  "type": "Controle"
}
```

**? Vérifier:**
- Réponse 201 Created
- `zoneName` présent dans la réponse
- `createdAt` automatiquement ajouté

### 3.4 - Tester POST (Création Périphérique)

```http
POST https://localhost:5001/api/admin/access-rights
Authorization: Bearer {votre-token}
Content-Type: application/json

{
  "userId": "votre-user-id",
  "zoneId": null,
  "peripheriqueLogiqueId": 1,
  "type": "Lecture"
}
```

**? Vérifier:**
- `peripheriqueLogiqueName` présent
- `peripheriqueLogiqueType` présent

### 3.5 - Tester PUT (Modification)

```http
PUT https://localhost:5001/api/admin/access-rights/1
Authorization: Bearer {votre-token}
Content-Type: application/json

{
  "userId": "votre-user-id",
  "zoneId": 1,
  "type": "Lecture"
}
```

**? Vérifier:**
- Réponse 204 No Content
- Après un GET, `updatedAt` est maintenant défini

### 3.6 - Tester DELETE

```http
DELETE https://localhost:5001/api/admin/access-rights/1
Authorization: Bearer {votre-token}
```

**? Vérifier:**
- Réponse 204 No Content
- Après un GET, le droit n'existe plus

---

## ? Étape 4: Valider l'Affichage (30 min)

### 4.1 - Vérifier le tableau

- [ ] Les noms d'utilisateurs complets s'affichent
- [ ] Les emails s'affichent sous les noms
- [ ] Les icônes Zone (??) et Périphérique (??) sont correctes
- [ ] Les types d'accès (??? Lecture / ??? Contrôle) s'affichent
- [ ] Les dates de création s'affichent
- [ ] Le tri par date fonctionne (plus récent en premier)

### 4.2 - Vérifier les filtres

- [ ] Filtre "Ressource: Zone" affiche uniquement les zones
- [ ] Filtre "Ressource: Périphérique" affiche uniquement les périphériques
- [ ] Filtre "Type d'accès: Lecture" affiche uniquement les droits en lecture
- [ ] Filtre "Type d'accès: Contrôle" affiche uniquement les droits en contrôle
- [ ] Recherche par nom d'utilisateur fonctionne
- [ ] Recherche par nom de zone fonctionne
- [ ] Recherche par nom de périphérique fonctionne

### 4.3 - Vérifier la création

- [ ] Le formulaire s'ouvre correctement
- [ ] Le champ "Type de ressource" permet de basculer Zone/Périphérique
- [ ] Le champ approprié s'affiche (ZoneId OU PeripheriqueLogiqueId)
- [ ] Le type d'accès peut être sélectionné (Lecture/Contrôle)
- [ ] La création réussit
- [ ] Le nouveau droit apparaît en tête de liste
- [ ] Toutes les propriétés sont remplies (nom utilisateur, nom zone/périphérique)

### 4.4 - Vérifier la modification

- [ ] Le formulaire se pré-remplit avec les valeurs existantes
- [ ] La modification du type fonctionne (Lecture ? Contrôle)
- [ ] La modification de la ressource fonctionne (Zone ? Périphérique)
- [ ] La sauvegarde réussit
- [ ] Les modifications s'affichent dans le tableau

### 4.5 - Vérifier la suppression

- [ ] Le dialog de confirmation s'affiche
- [ ] Le nom de l'utilisateur est affiché dans la confirmation
- [ ] La suppression réussit
- [ ] Le droit disparaît du tableau

---

## ?? Problèmes Fréquents

### Problème 1: `PeripheriqueName` undefined

**Symptôme:** Erreur TypeScript/C# "Property does not exist"

**Solution:**
```razor
@* Remplacer *@
@context.PeripheriqueName

@* Par *@
@context.PeripheriqueLogiqueName
```

### Problème 2: Type "Zone" dans les données

**Symptôme:** Les types affichés sont "Zone"/"Peripherique" au lieu de "Lecture"/"Controle"

**Solution:** Le backend renvoie `type: "Lecture"` ou `type: "Controle"`. Ne pas confondre avec le type de ressource.

```csharp
// ? INCORRECT
<MudChip>@context.Type</MudChip>  // Affiche "Lecture" ou "Controle"

// ? CORRECT
@if (context.ZoneId.HasValue) 
{
    <MudText>Zone: @context.ZoneName</MudText>
}
<MudChip>@context.Type</MudChip>  // Affiche "Lecture" ou "Controle"
```

### Problème 3: Filtre ne fonctionne pas

**Symptôme:** Le filtre par "Zone" n'affiche rien

**Solution:**
```csharp
// ? INCORRECT
.Where(d => d.Type == "Zone")  // Type est "Lecture" ou "Controle", pas "Zone"

// ? CORRECT
.Where(d => d.ZoneId.HasValue)  // Vérifier si ZoneId n'est pas null
```

### Problème 4: Nom utilisateur toujours vide

**Symptôme:** Affiche toujours l'ID au lieu du nom

**Solution:** Vérifier que le backend charge la relation `User`
```csharp
// Dans le Repository
.Include(da => da.User)  // ? Doit être présent
```

### Problème 5: 401 Unauthorized

**Symptôme:** Toutes les requêtes échouent avec 401

**Solution:**
1. Vérifier que le token est valide
2. Vérifier que le token est dans le header: `Authorization: Bearer {token}`
3. Vérifier que l'utilisateur a le rôle "Admin"

---

## ?? Checklist Finale

### Backend
- [x] ? Modèle `DroitAcces` avec timestamps
- [x] ? DTO `DroitAccesDto` enrichi
- [x] ? Repository charge relations
- [x] ? Service mappe correctement
- [x] ? Migration appliquée
- [x] ? APIs fonctionnelles

### Frontend
- [ ] Service `IDroitAccesService` créé
- [ ] Service enregistré dans DI
- [ ] Page Blazor corrigée (propriétés)
- [ ] Filtres corrigés (Zone/Périphérique + Lecture/Contrôle)
- [ ] Formulaire corrigé (ResourceType + AccessType)
- [ ] Tests réussis

### Tests
- [ ] GET liste fonctionne
- [ ] GET détail fonctionne
- [ ] POST création zone fonctionne
- [ ] POST création périphérique fonctionne
- [ ] PUT modification fonctionne
- [ ] DELETE suppression fonctionne
- [ ] Filtres fonctionnent
- [ ] Recherche fonctionne
- [ ] Affichage correct

---

## ?? Besoin d'Aide ?

### Documentation Complète
- **Guide détaillé:** `FRONTEND-ACCESS-RIGHTS-COMPLETE-GUIDE.md`
- **Tests HTTP:** `Tests\35-Access-Rights-Complete.http`
- **Résumé backend:** `ACCESS-RIGHTS-BACKEND-SUMMARY.md`

### Structure des Propriétés

**Backend renvoie:**
```json
{
  "userName": "john.doe",     // Nom du compte
  "userEmail": "john@...",      // Email
  "userFullName": "John Doe",   // Nom complet
  "zoneName": "Salle A",      // SI zone
  "peripheriqueLogiqueName": "Clim", // SI périphérique
  "peripheriqueLogiqueType": "HVAC", // SI périphérique
  "type": "Controle", // Type d'ACCÈS (pas ressource)
  "createdAt": "2025-01-06...", // Date création
  "updatedAt": null // Date modification
}
```

**Frontend doit utiliser:**
- `UserFullName ?? UserName ?? UserId` pour l'affichage utilisateur
- `PeripheriqueLogiqueName` (pas `PeripheriqueName`)
- `Type` pour "Lecture"/"Controle" (pas "Zone"/"Peripherique")
- `ZoneId.HasValue` pour filtrer les zones
- `PeripheriqueLogiqueId.HasValue` pour filtrer les périphériques

---

## ?? Résumé en 3 Points

1. **Service:** Créer `DroitAccesService` avec les 5 méthodes CRUD
2. **Corrections:** Remplacer `PeripheriqueName` ? `PeripheriqueLogiqueName` partout
3. **Tests:** Utiliser `Tests\35-Access-Rights-Complete.http`

**Temps total estimé:** 2 heures

---

**Date:** 6 janvier 2025  
**Status:** ? Backend prêt, Frontend à implémenter  
**Durée estimée:** 2 heures
