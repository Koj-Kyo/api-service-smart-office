# ?? Quick Start - Analytics Énergétiques Frontend

## ?? Temps estimé: 3-4 heures

---

## ?? Checklist Rapide

- [ ] 1. Créer le service `IConsommationStatsService` (45 min)
- [ ] 2. Remplacer les données mockées par les vraies APIs (1h30)
- [ ] 3. Ajouter la gestion d'erreurs et validation (45 min)
- [ ] 4. Tester tous les endpoints (45 min)

---

## ?? Étape 1: Créer le Service Frontend (45 min)

### 1.1 - Créer l'interface

**Fichier:** `MySmartApp.Ui/Services/IConsommationStatsService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IConsommationStatsService
{
    Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate);
    
    Task<ApiResponse<List<ConsommationHoraireDto>>> GetConsommationHoraireAsync(
        int peripheriqueId, DateTime date);
    
    Task<ApiResponse<List<AnomalieConsommationDto>>> DetectAnomaliesAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate);
    
    Task<ApiResponse<RapportConsommationDto>> GenererRapportConsommationAsync(
        int? zoneId, DateTime startDate, DateTime endDate);
    
    Task<ApiResponse<List<TopConsommateursDto>>> GetTopConsommateursAsync(
        int? zoneId, DateTime startDate, DateTime endDate, int top = 10);
}
```

### 1.2 - Implémenter le service

**Fichier:** `MySmartApp.Ui/Services/ConsommationStatsService.cs`

```csharp
using MySmartApp.Ui.Models;
using System.Net.Http.Json;

namespace MySmartApp.Ui.Services;

public class ConsommationStatsService : IConsommationStatsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConsommationStatsService> _logger;

    public ConsommationStatsService(
        HttpClient httpClient, 
        ILogger<ConsommationStatsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var url = $"api/ConsommationStats/peripherique/{peripheriqueId}" +
                      $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            
            var response = await _httpClient.GetFromJsonAsync<ConsommationStatsDto>(url);
            
            return new ApiResponse<ConsommationStatsDto>
            {
                IsSuccess = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur stats consommation");
            return new ApiResponse<ConsommationStatsDto> { IsSuccess = false };
        }
    }

    public async Task<ApiResponse<List<ConsommationHoraireDto>>> GetConsommationHoraireAsync(
        int peripheriqueId, DateTime date)
    {
        try
        {
            var url = $"api/ConsommationStats/peripherique/{peripheriqueId}/horaire" +
                      $"?date={date:yyyy-MM-dd}";
            
            var response = await _httpClient.GetFromJsonAsync<List<ConsommationHoraireDto>>(url);
            
            return new ApiResponse<List<ConsommationHoraireDto>>
            {
                IsSuccess = true,
                Data = response ?? new List<ConsommationHoraireDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur données horaires");
            return new ApiResponse<List<ConsommationHoraireDto>>
            {
                IsSuccess = false,
                Data = new List<ConsommationHoraireDto>()
            };
        }
    }

    public async Task<ApiResponse<List<AnomalieConsommationDto>>> DetectAnomaliesAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var url = $"api/ConsommationStats/peripherique/{peripheriqueId}/anomalies" +
                      $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            
            var response = await _httpClient.GetFromJsonAsync<List<AnomalieConsommationDto>>(url);
            
            return new ApiResponse<List<AnomalieConsommationDto>>
            {
                IsSuccess = true,
                Data = response ?? new List<AnomalieConsommationDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur détection anomalies");
            return new ApiResponse<List<AnomalieConsommationDto>>
            {
                IsSuccess = false,
                Data = new List<AnomalieConsommationDto>()
            };
        }
    }

    public async Task<ApiResponse<RapportConsommationDto>> GenererRapportConsommationAsync(
        int? zoneId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var url = $"api/ConsommationStats/rapport" +
                      $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            
            if (zoneId.HasValue)
                url += $"&zoneId={zoneId.Value}";
            
            var response = await _httpClient.GetFromJsonAsync<RapportConsommationDto>(url);
            
            return new ApiResponse<RapportConsommationDto>
            {
                IsSuccess = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur génération rapport");
            return new ApiResponse<RapportConsommationDto> { IsSuccess = false };
        }
    }

    public async Task<ApiResponse<List<TopConsommateursDto>>> GetTopConsommateursAsync(
        int? zoneId, DateTime startDate, DateTime endDate, int top = 10)
    {
        try
        {
            var url = $"api/ConsommationStats/top-consommateurs" +
                      $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&top={top}";
            
            if (zoneId.HasValue)
                url += $"&zoneId={zoneId.Value}";
            
            var response = await _httpClient.GetFromJsonAsync<List<TopConsommateursDto>>(url);
            
            return new ApiResponse<List<TopConsommateursDto>>
            {
                IsSuccess = true,
                Data = response ?? new List<TopConsommateursDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur top consommateurs");
            return new ApiResponse<List<TopConsommateursDto>>
            {
                IsSuccess = false,
                Data = new List<TopConsommateursDto>()
            };
        }
    }
}
```

### 1.3 - Enregistrer le service

**Fichier:** `MySmartApp.Ui/Program.cs`

```csharp
// Ajouter après les autres services
builder.Services.AddScoped<IConsommationStatsService, ConsommationStatsService>();
```

---

## ?? Étape 2: Remplacer les Données Mockées (1h30)

### 2.1 - Correction #1: LoadData au démarrage

```csharp
// ? AVANT
protected override async Task OnInitializedAsync()
{
    await LoadMockData();
}

// ? APRÈS
protected override async Task OnInitializedAsync()
{
    // Charger avec des valeurs par défaut pour afficher quelque chose
    await LoadDefaultData();
}

private async Task LoadDefaultData()
{
    // Optionnel: charger les stats pour le premier périphérique disponible
    // ou simplement afficher un message invitant à sélectionner
    Snackbar.Add("?? Sélectionnez un périphérique et une période pour commencer", 
        Severity.Info);
}
```

### 2.2 - Correction #2: LoadConsommationStats avec vraies données

```csharp
// ? AVANT (mockée)
private async Task LoadConsommationStats()
{
    // ... code mocké
}

// ? APRÈS (vraies données)
private async Task LoadConsommationStats()
{
    if (!ValidateParameters()) return;

    isLoading = true;
    StateHasChanged();

    try
    {
        // 1. Charger les stats principales
        var statsResponse = await ConsommationService.GetConsommationStatsAsync(
            selectedPeripheriqueId!.Value, 
            startDate!.Value, 
            endDate!.Value);

        if (statsResponse.IsSuccess && statsResponse.Data != null)
        {
            consommationStats = statsResponse.Data;
            
            // 2. Charger les données horaires
            await LoadConsommationHoraire();
            
            Snackbar.Add("? Données chargées avec succès", Severity.Success);
        }
        else
        {
            Snackbar.Add("? Aucune donnée disponible", Severity.Warning);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur chargement stats");
        Snackbar.Add("? Erreur lors du chargement", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

### 2.3 - Correction #3: LoadConsommationHoraire

```csharp
// ? AVANT (mockée avec GenerateMockConsommationHoraire)
private List<ConsommationHoraireDto> GenerateMockConsommationHoraire() { /* ... */ }

// ? APRÈS (vraies données)
private async Task LoadConsommationHoraire()
{
    if (!selectedPeripheriqueId.HasValue) return;

    try
    {
        var response = await ConsommationService.GetConsommationHoraireAsync(
            selectedPeripheriqueId.Value, 
            DateTime.Today);

        if (response.IsSuccess && response.Data != null)
        {
            consommationHoraire = response.Data;
            Logger.LogInformation("? {Count} heures chargées", consommationHoraire.Count);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur données horaires");
    }
}
```

### 2.4 - Correction #4: LoadTopConsommateurs

```csharp
// ? AVANT (données mockées en dur)
topConsommateurs = new List<TopConsommateursDto> { /* hardcoded */ };

// ? APRÈS (vraies données)
private async Task LoadTopConsommateurs()
{
    if (!ValidateParameters()) return;

    isLoading = true;
    StateHasChanged();

    try
    {
        var response = await ConsommationService.GetTopConsommateursAsync(
            selectedZoneId, 
            startDate!.Value, 
            endDate!.Value, 
            top: 10);

        if (response.IsSuccess && response.Data != null)
        {
            topConsommateurs = response.Data;
            Snackbar.Add($"? {topConsommateurs.Count} consommateurs chargés", 
                Severity.Success);
        }
        else
        {
            topConsommateurs = new List<TopConsommateursDto>();
            Snackbar.Add("?? Aucun consommateur trouvé", Severity.Info);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur top consommateurs");
        Snackbar.Add("? Erreur de chargement", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

### 2.5 - Correction #5: DetectAnomalies

```csharp
// ? Version corrigée complète
private async Task DetectAnomalies()
{
    if (!selectedPeripheriqueId.HasValue || !startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Paramètres manquants", Severity.Warning);
        return;
    }

    isLoading = true;
    StateHasChanged();

    try
    {
        var response = await ConsommationService.DetectAnomaliesAsync(
            selectedPeripheriqueId.Value, 
            startDate.Value, 
            endDate.Value);

        if (response.IsSuccess && response.Data != null)
        {
            anomalies = response.Data;
            
            if (anomalies.Any())
            {
                var highSeverity = anomalies.Count(a => a.Severite == "High");
                Snackbar.Add($"?? {anomalies.Count} anomalies détectées " +
                           $"({highSeverity} critiques)", 
                           Severity.Warning);
            }
            else
            {
                Snackbar.Add("? Aucune anomalie détectée", Severity.Success);
            }
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur détection anomalies");
        Snackbar.Add("? Erreur de détection", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

### 2.6 - Correction #6: GenererRapport

```csharp
// ? Version corrigée complète
private async Task GenererRapport()
{
    if (!ValidateParameters()) return;

    isLoading = true;
    StateHasChanged();

    try
    {
        var response = await ConsommationService.GenererRapportConsommationAsync(
            selectedZoneId, 
            startDate!.Value, 
            endDate!.Value);

        if (response.IsSuccess && response.Data != null)
        {
            rapport = response.Data;
            Snackbar.Add("? Rapport généré avec succès", Severity.Success);
            
            // Optionnel: scroll vers le rapport
            // await JSRuntime.InvokeVoidAsync("scrollToElement", "rapport-section");
        }
        else
        {
            Snackbar.Add("? Impossible de générer le rapport", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Erreur génération rapport");
        Snackbar.Add("? Erreur de génération", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

---

## ? Étape 3: Ajouter Validation et Gestion d'Erreurs (45 min)

### 3.1 - Méthode de validation centralisée

```csharp
private bool ValidateParameters()
{
    if (!selectedPeripheriqueId.HasValue && !selectedZoneId.HasValue)
    {
        Snackbar.Add("?? Veuillez sélectionner un périphérique ou une zone", 
            Severity.Warning);
        return false;
    }

    if (!startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Veuillez sélectionner une période", 
            Severity.Warning);
        return false;
    }

    if (startDate.Value >= endDate.Value)
    {
        Snackbar.Add("?? La date de début doit être antérieure à la date de fin", 
            Severity.Warning);
        return false;
    }

    var maxPeriod = TimeSpan.FromDays(365);
    if ((endDate.Value - startDate.Value) > maxPeriod)
    {
        Snackbar.Add("?? La période ne peut pas dépasser 365 jours", 
            Severity.Warning);
        return false;
    }

    return true;
}
```

### 3.2 - Ajouter ISnackbar dans la page

```razor
@inject ISnackbar Snackbar
```

### 3.3 - Messages de feedback utilisateur

```csharp
// Exemples de messages contextuels

// Succès
Snackbar.Add("? Données chargées avec succès", Severity.Success);
Snackbar.Add($"? {count} éléments trouvés", Severity.Success);

// Avertissement
Snackbar.Add("?? Aucune donnée disponible pour cette période", Severity.Warning);
Snackbar.Add("?? Sélectionnez un périphérique pour commencer", Severity.Info);

// Erreur
Snackbar.Add("? Erreur lors du chargement", Severity.Error);
Snackbar.Add("? Impossible de se connecter au serveur", Severity.Error);
```

---

## ?? Étape 4: Tests et Validation (45 min)

### 4.1 - Créer un fichier de tests HTTP

**Fichier:** `analytics-tests.http`

```http
### Variables
@baseUrl = https://localhost:7144
@token = {{$dotenv LOGIN_TOKEN}}
@peripheriqueId = 1
@zoneId = 1

### 1. Authentification
POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "email": "admin@smartoffice.com",
  "password": "Admin@123"
}

### 2. Stats périphérique (7 jours)
GET {{baseUrl}}/api/ConsommationStats/peripherique/{{peripheriqueId}}
    ?startDate=2025-01-01
    &endDate=2025-01-07
Authorization: Bearer {{token}}

### 3. Données horaires (aujourd'hui)
GET {{baseUrl}}/api/ConsommationStats/peripherique/{{peripheriqueId}}/horaire
    ?date=2025-01-06
Authorization: Bearer {{token}}

### 4. Top consommateurs
GET {{baseUrl}}/api/ConsommationStats/top-consommateurs
    ?startDate=2025-01-01
    &endDate=2025-01-07
    &top=10
Authorization: Bearer {{token}}

### 5. Détection anomalies
GET {{baseUrl}}/api/ConsommationStats/peripherique/{{peripheriqueId}}/anomalies
    ?startDate=2025-01-01
    &endDate=2025-01-07
Authorization: Bearer {{token}}

### 6. Génération rapport
GET {{baseUrl}}/api/ConsommationStats/rapport
    ?startDate=2025-01-01
    &endDate=2025-01-07
Authorization: Bearer {{token}}

### 7. Rapport par zone
GET {{baseUrl}}/api/ConsommationStats/rapport
    ?zoneId={{zoneId}}
    &startDate=2025-01-01
    &endDate=2025-01-07
Authorization: Bearer {{token}}
```

### 4.2 - Checklist de tests frontend

```
Tests de Base:
- [ ] La page se charge sans erreur
- [ ] Les filtres (dates, périphérique, zone) s'affichent
- [ ] Le bouton "Analyser" est cliquable

Tests de Chargement:
- [ ] Cliquer "Analyser" charge les stats
- [ ] Les 4 cartes de stats affichent des valeurs > 0
- [ ] Le graphique horaire se remplit avec des données
- [ ] Le loader s'affiche pendant le chargement

Tests Top Consommateurs:
- [ ] Cliquer "Top Consommateurs" charge la liste
- [ ] Au moins 1 consommateur s'affiche
- [ ] Les barres de progression fonctionnent
- [ ] Les pourcentages sont cohérents (somme ? 100%)

Tests Anomalies:
- [ ] Cliquer "Détecter Anomalies" fonctionne
- [ ] Si anomalies: le tableau s'affiche avec toutes les colonnes
- [ ] Si pas d'anomalies: message "Aucune anomalie détectée"
- [ ] Les sévérités sont colorées (High=rouge, Medium=orange, Low=bleu)

Tests Rapport:
- [ ] Cliquer "Générer Rapport" fonctionne
- [ ] Les 3 métriques principales s'affichent
- [ ] Les recommandations s'affichent (si disponibles)
- [ ] La section comparaison s'affiche (si données disponibles)

Tests d'Erreur:
- [ ] Cliquer "Analyser" sans sélection ? message d'avertissement
- [ ] Date début > date fin ? message d'erreur
- [ ] Période > 365 jours ? message d'erreur
- [ ] Erreur serveur (500) ? message "Erreur de chargement"
```

### 4.3 - Tests de validation

```csharp
// Test 1: Validation période vide
startDate = null;
endDate = null;
await LoadConsommationStats();
// ? Attendu: Message "Veuillez sélectionner une période"

// Test 2: Validation dates inversées
startDate = DateTime.Today;
endDate = DateTime.Today.AddDays(-7);
await LoadConsommationStats();
// ? Attendu: Message "date de début doit être antérieure"

// Test 3: Validation période trop longue
startDate = DateTime.Today.AddDays(-400);
endDate = DateTime.Today;
await LoadConsommationStats();
// ? Attendu: Message "ne peut pas dépasser 365 jours"

// Test 4: Chargement réussi
startDate = DateTime.Today.AddDays(-7);
endDate = DateTime.Today;
selectedPeripheriqueId = 1;
await LoadConsommationStats();
// ? Attendu: consommationStats != null && consommationHoraire.Count > 0
```

---

## ?? Problèmes Fréquents

### Problème 1: "Aucune donnée disponible"

**Symptôme:** Les APIs retournent des listes vides

**Solutions possibles:**
1. Vérifier que des télémétries existent en base pour la période
2. Vérifier que le périphéqueId existe
3. Vérifier les dates (format: yyyy-MM-dd)

```csharp
// Debug: logger les paramètres
Logger.LogInformation("Chargement stats: peripheriqueId={Id}, start={Start}, end={End}", 
    selectedPeripheriqueId, startDate, endDate);
```

### Problème 2: Graphique vide

**Symptôme:** Le graphique ne s'affiche pas

**Solution:**
```csharp
// Vérifier que consommationHoraire contient des données
private List<ChartSeries> GetConsommationHoraireSeries()
{
    if (!consommationHoraire.Any())
    {
        Logger.LogWarning("Aucune donnée horaire pour le graphique");
        return new List<ChartSeries>();
    }
    
    // ... reste du code
}
```

### Problème 3: Erreur 401 Unauthorized

**Symptôme:** Toutes les requêtes échouent avec 401

**Solution:**
1. Vérifier que le token JWT est valide
2. Vérifier que l'utilisateur est authentifié
3. Vérifier que le token est dans les headers

```csharp
// Vérifier le token dans Program.cs
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://localhost:7144"),
    DefaultRequestHeaders = 
    { 
        { "Authorization", $"Bearer {token}" } 
    }
});
```

### Problème 4: Période invalide

**Symptôme:** Erreur 400 Bad Request

**Solution:** Valider les dates avant l'appel
```csharp
if (!ValidateParameters()) 
{
    return; // Ne pas appeler l'API
}
```

---

## ?? Checklist Finale

### Backend
- [x] ? 9 endpoints API fonctionnels
- [x] ? Service de calcul statistique
- [x] ? Détection d'anomalies
- [x] ? Génération de rapports

### Frontend
- [ ] Service `IConsommationStatsService` créé
- [ ] Service enregistré dans DI
- [ ] Méthodes mockées remplacées par vraies APIs
- [ ] Validation des paramètres implémentée
- [ ] Gestion d'erreurs avec Snackbar
- [ ] Tests réussis

### Graphiques
- [ ] Consommation horaire connecté aux données réelles
- [ ] Top consommateurs affiche les vraies données
- [ ] Anomalies colorées selon sévérité
- [ ] Rapport complet avec recommandations

---

## ?? Résumé en 3 Points

1. **Service:** Créer `ConsommationStatsService` avec 5 méthodes principales
2. **Données:** Remplacer toutes les fonctions `LoadMock*()` par des appels API
3. **Validation:** Ajouter `ValidateParameters()` et gestion d'erreurs

**Temps total estimé:** 3-4 heures

---

## ?? Besoin d'Aide ?

### Documentation Complète
- **Guide détaillé:** `FRONTEND-ANALYTICS-COMPLETE-GUIDE.md`
- **Tests HTTP:** `analytics-tests.http`

### Propriétés Importantes

**ConsommationStatsDto:**
- `ConsommationTotaleMwh` - Consommation totale
- `PuissanceMoyenneMw` - Puissance moyenne
- `TempsFonctionnementHeures` - Temps de fonctionnement
- `EfficaciteEnergetique` - Efficacité (0-1)

**ConsommationHoraireDto:**
- `Heure` - DateTime de l'heure
- `ConsommationMwh` - Consommation pour cette heure
- `PuissanceMoyenneMw` - Puissance moyenne
- `EtatActif` - Si le périphérique était actif

**TopConsommateursDto:**
- `PeripheriqueName` - Nom du périphérique
- `ConsommationMwh` - Consommation totale
- `PourcentageTotal` - % par rapport au total
- `CoutEstime` - Coût estimé en €

---

**Date:** 6 janvier 2025  
**Status:** ? Backend prêt, Frontend à implémenter  
**Durée estimée:** 3-4 heures
