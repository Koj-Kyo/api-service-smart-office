# ?? Guide Frontend - Analytics Énergétiques

## ?? Table des Matières

1. [Vue d'ensemble](#vue-densemble)
2. [APIs Disponibles](#apis-disponibles)
3. [Modèles de Données (DTOs)](#modèles-de-données-dtos)
4. [Implémentation du Service Frontend](#implémentation-du-service-frontend)
5. [Corrections de la Page Blazor](#corrections-de-la-page-blazor)
6. [Exemples de Requêtes](#exemples-de-requêtes)
7. [Graphiques et Visualisations](#graphiques-et-visualisations)
8. [Gestion des Erreurs](#gestion-des-erreurs)
9. [Tests et Validation](#tests-et-validation)

---

## ?? Vue d'ensemble

La page Analytics permet aux utilisateurs de:
- ? Consulter les statistiques de consommation énergétique
- ? Analyser la consommation par périphérique ou zone
- ? Visualiser les tendances horaires et quotidiennes
- ? Identifier les top consommateurs
- ? Détecter les anomalies de consommation
- ? Générer des rapports détaillés
- ? Obtenir des recommandations d'optimisation

### Architecture Backend Complète

```
???????????????????????????????????????????????????????????
?      Frontend Blazor (MySmartApp.Ui)          ?
?     /analytics page                           ?
???????????????????????????????????????????????????????????
                       ? HTTP/JSON (camelCase)
                       ?
???????????????????????????????????????????????????????????
?     ConsommationStatsController (API Backend)   ?
?     /api/ConsommationStats/*                    ?
???????????????????????????????????????????????????????????
?  - GetConsommationStats()       ? Stats périphérique    ?
?  - GetConsommationHoraire()     ? Données horaires      ?
?  - DetectAnomalies()            ? Anomalies détectées   ?
?  - GenererRapportConsommation() ? Rapport complet       ?
?  - GetTopConsommateurs()        ? Top consommateurs     ?
???????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????
?      ConsommationStatsService (Business Logic)  ?
?  - Calculs statistiques                         ?
?  - Détection d'anomalies                        ?
?  - Génération de recommandations                ?
???????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????
?       TelemetrieRepository (Data Access)        ?
?  - Requêtes SQL optimisées                      ?
?  - Agrégations temporelles                      ?
???????????????????????????????????????????????????????????
                       ?
                       ?
???????????????????????????????????????????????????????????
?       PostgreSQL Database + TimescaleDB         ?
?  Table: Telemetries (séries temporelles)       ?
???????????????????????????????????????????????????????????
```

---

## ?? APIs Disponibles

### Base URL
```
https://localhost:7144/api/ConsommationStats
```

### 1. **GET** - Statistiques de consommation par périphérique

```http
GET /api/ConsommationStats/peripherique/{peripheriqueId}?startDate={date}&endDate={date}
Authorization: Bearer {token}
```

**Paramètres:**
- `peripheriqueId` (int, path) - ID du périphérique
- `startDate` (DateTime, query) - Date de début
- `endDate` (DateTime, query) - Date de fin

**Réponse (200 OK):**
```json
{
  "peripheriqueId": 1,
  "peripheriqueName": "Climatiseur Salon",
  "zoneId": 1,
  "zoneName": "Salon",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-01-07T00:00:00Z",
  "consommationTotaleMwh": 145.7,
  "puissanceMoyenneMw": 6.2,
  "puissanceMaximaleMw": 8.5,
  "puissanceMinimaleMw": 3.1,
  "tempsFonctionnementHeures": 168.5,
  "efficaciteEnergetique": 0.78,
  "nombreCycles": 24,
  "tempsMoyenCycleMinutes": 45.2
}
```

---

### 2. **GET** - Statistiques par zone

```http
GET /api/ConsommationStats/zone/{zoneId}?startDate={date}&endDate={date}
Authorization: Bearer {token}
```

**Réponse:** Même structure que par périphérique, agrégée pour toute la zone.

---

### 3. **GET** - Consommation horaire

```http
GET /api/ConsommationStats/peripherique/{peripheriqueId}/horaire?date={date}
Authorization: Bearer {token}
```

**Paramètres:**
- `peripheriqueId` (int, path) - ID du périphérique
- `date` (DateTime, query) - Date pour laquelle obtenir les données horaires

**Réponse (200 OK):**
```json
[
  {
    "heure": "2025-01-06T00:00:00Z",
    "consommationMwh": 5.2,
    "puissanceMoyenneMw": 6.1,
    "etatActif": true,
    "nombreActivations": 2
  },
  {
    "heure": "2025-01-06T01:00:00Z",
    "consommationMwh": 4.8,
    "puissanceMoyenneMw": 5.9,
    "etatActif": true,
    "nombreActivations": 1
  }
  // ... 24 heures total
]
```

---

### 4. **GET** - Tendances de consommation

```http
GET /api/ConsommationStats/peripherique/{peripheriqueId}/tendances?nombreJours=30
Authorization: Bearer {token}
```

**Paramètres:**
- `peripheriqueId` (int, path) - ID du périphérique
- `nombreJours` (int, query, default: 30) - Nombre de jours pour l'analyse (max 365)

**Réponse (200 OK):**
```json
{
  "peripheriqueId": 1,
  "peripheriqueName": "Climatiseur Salon",
  "nombreJours": 30,
  "consommationQuotidienneMoyenne": 20.8,
  "tendancePourcentage": 5.2,
  "consommationPrediteJour": 21.5,
  "donneesQuotidiennes": [
    {
      "date": "2025-01-01T00:00:00Z",
      "consommationMwh": 19.5,
      "tempsFonctionnementHeures": 22.3,
      "puissanceMoyenne": 6.4
    }
    // ... 30 jours
  ]
}
```

---

### 5. **GET** - Détection d'anomalies

```http
GET /api/ConsommationStats/peripherique/{peripheriqueId}/anomalies?startDate={date}&endDate={date}
Authorization: Bearer {token}
```

**Réponse (200 OK):**
```json
[
  {
    "peripheriqueId": 1,
    "peripheriqueName": "Climatiseur Salon",
    "dateDetection": "2025-01-05T14:30:00Z",
    "typeAnomalie": "ConsommationElevee",
    "description": "Consommation 35% supérieure à la moyenne habituelle",
    "valeurMesuree": 9.2,
    "valeurAttendue": 6.8,
    "ecartPourcentage": 35.3,
    "severite": "High"
  }
]
```

---

### 6. **GET** - Génération de rapport

```http
GET /api/ConsommationStats/rapport?zoneId={zoneId}&startDate={date}&endDate={date}
Authorization: Bearer {token}
```

**Paramètres:**
- `zoneId` (int?, query, optional) - Si fourni, rapport pour cette zone uniquement
- `startDate` (DateTime, query) - Date de début
- `endDate` (DateTime, query) - Date de fin

**Réponse (200 OK):**
```json
{
  "zoneId": 1,
  "zoneName": "Salon",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-01-07T00:00:00Z",
  "dateGeneration": "2025-01-07T10:30:00Z",
  "consommationTotaleMwh": 145.7,
  "coutTotalEstime": 18.95,
  "economiesPotentiellesMwh": 12.3,
  "economiesPotentiellesEuros": 3.75,
  "statistiquesParPeripherique": [
    {
      "peripheriqueId": 1,
      "peripheriqueName": "Climatiseur Salon",
      "consommationTotaleMwh": 85.2,
      // ... autres stats
    }
  ],
  "recommandations": [
    {
      "peripheriqueId": 1,
      "peripheriqueName": "Climatiseur Salon",
      "typeRecommandation": "Optimisation température",
      "description": "Réduire la température de 1°C permettrait...",
      "economieEstimeeMwh": 3.6,
      "economieEstimeeEuros": 1.52,
      "priorite": "High"
    }
  ],
  "comparaisonPeriodePrecedente": {
    "consommationPrecedenteMwh": 138.2,
    "consommationActuelleMwh": 145.7,
    "variationMwh": 7.5,
    "variationPourcentage": 5.4,
    "tendance": "Dégradation"
  }
}
```

---

### 7. **GET** - Calcul du coût énergétique

```http
GET /api/ConsommationStats/peripherique/{peripheriqueId}/cout?startDate={date}&endDate={date}&tarifKwh={tarif}
Authorization: Bearer {token}
```

**Paramètres:**
- `peripheriqueId` (int, path) - ID du périphérique
- `startDate` (DateTime, query) - Date de début
- `endDate` (DateTime, query) - Date de fin
- `tarifKwh` (decimal, query) - Tarif par kWh (ex: 0.13 pour 0,13€/kWh)

**Réponse (200 OK):**
```json
{
  "peripheriqueId": 1,
  "peripheriqueName": "Climatiseur Salon",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2025-01-07T00:00:00Z",
  "consommationMwh": 145.7,
  "tarifKwh": 0.13,
  "coutTotal": 18.94,
  "coutMoyen": 2.71,
  "coutJournalier": 2.71,
  "coutsHoraires": [
    {
      "heure": "2025-01-06T00:00:00Z",
      "consommationMwh": 5.2,
      "tarifKwh": 0.13,
      "cout": 0.68
    }
    // ... heures
  ]
}
```

---

### 8. **GET** - Top des consommateurs

```http
GET /api/ConsommationStats/top-consommateurs?zoneId={zoneId}&startDate={date}&endDate={date}&top=10
Authorization: Bearer {token}
```

**Paramètres:**
- `zoneId` (int?, query, optional) - Filtrer par zone
- `startDate` (DateTime, query) - Date de début
- `endDate` (DateTime, query) - Date de fin
- `top` (int, query, default: 10) - Nombre de résultats (max 100)

**Réponse (200 OK):**
```json
[
  {
    "peripheriqueId": 1,
    "peripheriqueName": "Climatiseur Salon",
    "peripheriqueType": "HVAC",
    "zoneId": 1,
    "zoneName": "Salon",
    "consommationMwh": 45.2,
    "pourcentageTotal": 31.0,
    "coutEstime": 5.89,
    "rang": 1
  },
  {
    "peripheriqueId": 2,
    "peripheriqueName": "Éclairage Bureau",
    "peripheriqueType": "LED",
    "zoneId": 2,
    "zoneName": "Bureau",
    "consommationMwh": 28.7,
    "pourcentageTotal": 19.7,
    "coutEstime": 3.74,
    "rang": 2
  }
  // ... jusqu'à 'top' résultats
]
```

---

### 9. **GET** - Comparaison entre périodes

```http
GET /api/ConsommationStats/peripherique/{peripheriqueId}/comparaison
    ?startDate1={date}&endDate1={date}
    &startDate2={date}&endDate2={date}
Authorization: Bearer {token}
```

**Réponse (200 OK):**
```json
{
  "peripheriqueId": 1,
  "peripheriqueName": "Climatiseur Salon",
  "startDate1": "2024-12-01T00:00:00Z",
  "endDate1": "2024-12-31T00:00:00Z",
  "consommationPeriode1Mwh": 138.2,
  "startDate2": "2025-01-01T00:00:00Z",
  "endDate2": "2025-01-31T00:00:00Z",
  "consommationPeriode2Mwh": 145.7,
  "differenceMwh": 7.5,
  "differencePourcentage": 5.4,
  "tendance": "Augmentation",
  "interpretation": "La consommation a augmenté de 5.4% par rapport à la période précédente"
}
```

---

## ?? Modèles de Données (DTOs)

### ConsommationStatsDto

```csharp
public class ConsommationStatsDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Métriques de consommation
    public double ConsommationTotaleMwh { get; set; }
    public double PuissanceMoyenneMw { get; set; }
    public double PuissanceMaximaleMw { get; set; }
    public double PuissanceMinimaleMw { get; set; }
    public double TempsFonctionnementHeures { get; set; }
    public double EfficaciteEnergetique { get; set; }
    public int NombreCycles { get; set; }
    public double TempsMoyenCycleMinutes { get; set; }
}
```

### ConsommationHoraireDto

```csharp
public class ConsommationHoraireDto
{
    public DateTime Heure { get; set; }
    public double ConsommationMwh { get; set; }
    public double PuissanceMoyenneMw { get; set; }
    public bool EtatActif { get; set; }
    public int NombreActivations { get; set; }
}
```

### AnomalieConsommationDto

```csharp
public class AnomalieConsommationDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    public DateTime DateDetection { get; set; }
    public string TypeAnomalie { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ValeurMesuree { get; set; }
    public double ValeurAttendue { get; set; }
    public double EcartPourcentage { get; set; }
    public string Severite { get; set; } = "Medium"; // Low, Medium, High
}
```

### TopConsommateursDto

```csharp
public class TopConsommateursDto
{
    public int PeripheriqueId { get; set; }
    public string PeripheriqueName { get; set; } = string.Empty;
    public string PeripheriqueType { get; set; } = string.Empty;
    public int ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public double ConsommationMwh { get; set; }
    public double PourcentageTotal { get; set; }
    public decimal CoutEstime { get; set; }
    public int Rang { get; set; }
}
```

### RapportConsommationDto

```csharp
public class RapportConsommationDto
{
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DateGeneration { get; set; }
    
    public double ConsommationTotaleMwh { get; set; }
    public decimal CoutTotalEstime { get; set; }
    public double EconomiesPotentiellesMwh { get; set; }
    public decimal EconomiesPotentiellesEuros { get; set; }
    
    public List<ConsommationStatsDto> StatistiquesParPeripherique { get; set; } = new();
    public List<RecommandationOptimisationDto> Recommandations { get; set; } = new();
    public ComparaisonPeriodeDto? ComparaisonPeriodePrecedente { get; set; }
}
```

---

## ?? Implémentation du Service Frontend

### Interface IConsommationStatsService

```csharp
public interface IConsommationStatsService
{
    // Statistiques de base
    Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate);
    
    Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsByZoneAsync(
        int zoneId, DateTime startDate, DateTime endDate);
    
    // Données temporelles
    Task<ApiResponse<List<ConsommationHoraireDto>>> GetConsommationHoraireAsync(
        int peripheriqueId, DateTime date);
    
    Task<ApiResponse<ConsommationTrendDto>> GetConsommationTrendAsync(
        int peripheriqueId, int nombreJours = 30);
    
    // Analyses avancées
    Task<ApiResponse<List<AnomalieConsommationDto>>> DetectAnomaliesAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate);
    
    Task<ApiResponse<List<TopConsommateursDto>>> GetTopConsommateursAsync(
        int? zoneId, DateTime startDate, DateTime endDate, int top = 10);
    
    // Rapports
    Task<ApiResponse<RapportConsommationDto>> GenererRapportConsommationAsync(
        int? zoneId, DateTime startDate, DateTime endDate);
    
    Task<ApiResponse<CoutEnergetiqueDto>> CalculerCoutEnergetiqueAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate, decimal tarifKwh);
    
    Task<ApiResponse<ComparaisonConsommationDto>> ComparerConsommationAsync(
        int peripheriqueId, 
        DateTime startDate1, DateTime endDate1,
        DateTime startDate2, DateTime endDate2);
}
```

### Implémentation ConsommationStatsService

```csharp
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
            _logger.LogError(ex, "Erreur lors du chargement des stats de consommation");
            return new ApiResponse<ConsommationStatsDto>
            {
                IsSuccess = false,
                Data = null
            };
        }
    }

    public async Task<ApiResponse<ConsommationStatsDto>> GetConsommationStatsByZoneAsync(
        int zoneId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var url = $"api/ConsommationStats/zone/{zoneId}" +
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
            _logger.LogError(ex, "Erreur lors du chargement des stats de zone");
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
            _logger.LogError(ex, "Erreur lors du chargement des données horaires");
            return new ApiResponse<List<ConsommationHoraireDto>>
            {
                IsSuccess = false,
                Data = new List<ConsommationHoraireDto>()
            };
        }
    }

    public async Task<ApiResponse<ConsommationTrendDto>> GetConsommationTrendAsync(
        int peripheriqueId, int nombreJours = 30)
    {
        try
        {
            var url = $"api/ConsommationStats/peripherique/{peripheriqueId}/tendances" +
                      $"?nombreJours={nombreJours}";
            
            var response = await _httpClient.GetFromJsonAsync<ConsommationTrendDto>(url);
            
            return new ApiResponse<ConsommationTrendDto>
            {
                IsSuccess = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement des tendances");
            return new ApiResponse<ConsommationTrendDto> { IsSuccess = false };
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
            _logger.LogError(ex, "Erreur lors de la détection d'anomalies");
            return new ApiResponse<List<AnomalieConsommationDto>>
            {
                IsSuccess = false,
                Data = new List<AnomalieConsommationDto>()
            };
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
            {
                url += $"&zoneId={zoneId.Value}";
            }
            
            var response = await _httpClient.GetFromJsonAsync<List<TopConsommateursDto>>(url);
            
            return new ApiResponse<List<TopConsommateursDto>>
            {
                IsSuccess = true,
                Data = response ?? new List<TopConsommateursDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement du top consommateurs");
            return new ApiResponse<List<TopConsommateursDto>>
            {
                IsSuccess = false,
                Data = new List<TopConsommateursDto>()
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
            {
                url += $"&zoneId={zoneId.Value}";
            }
            
            var response = await _httpClient.GetFromJsonAsync<RapportConsommationDto>(url);
            
            return new ApiResponse<RapportConsommationDto>
            {
                IsSuccess = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du rapport");
            return new ApiResponse<RapportConsommationDto> { IsSuccess = false };
        }
    }

    public async Task<ApiResponse<CoutEnergetiqueDto>> CalculerCoutEnergetiqueAsync(
        int peripheriqueId, DateTime startDate, DateTime endDate, decimal tarifKwh)
    {
        try
        {
            var url = $"api/ConsommationStats/peripherique/{peripheriqueId}/cout" +
                      $"?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}" +
                      $"&tarifKwh={tarifKwh}";
            
            var response = await _httpClient.GetFromJsonAsync<CoutEnergetiqueDto>(url);
            
            return new ApiResponse<CoutEnergetiqueDto>
            {
                IsSuccess = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul du coût énergétique");
            return new ApiResponse<CoutEnergetiqueDto> { IsSuccess = false };
        }
    }

    public async Task<ApiResponse<ComparaisonConsommationDto>> ComparerConsommationAsync(
        int peripheriqueId, 
        DateTime startDate1, DateTime endDate1,
        DateTime startDate2, DateTime endDate2)
    {
        try
        {
            var url = $"api/ConsommationStats/peripherique/{peripheriqueId}/comparaison" +
                      $"?startDate1={startDate1:yyyy-MM-dd}&endDate1={endDate1:yyyy-MM-dd}" +
                      $"&startDate2={startDate2:yyyy-MM-dd}&endDate2={endDate2:yyyy-MM-dd}";
            
            var response = await _httpClient.GetFromJsonAsync<ComparaisonConsommationDto>(url);
            
            return new ApiResponse<ComparaisonConsommationDto>
            {
                IsSuccess = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la comparaison de consommation");
            return new ApiResponse<ComparaisonConsommationDto> { IsSuccess = false };
        }
    }
}
```

### Enregistrement du Service

```csharp
// Program.cs
builder.Services.AddScoped<IConsommationStatsService, ConsommationStatsService>();
```

---

## ?? Corrections de la Page Blazor

### Problèmes Identifiés dans Votre Page

1. **Données mockées en dur** ? Utiliser les vraies APIs
2. **Pas de gestion d'erreurs** ? Ajouter try/catch et messages utilisateur
3. **Graphiques non connectés aux APIs** ? Utiliser les données réelles
4. **Filtres incomplets** ? Ajouter validation et feedback

### Code Corrigé

#### **1. Remplacement des méthodes mockées**

```csharp
// ? AVANT (données mockées)
private async Task LoadMockData()
{
    await Task.Delay(1000);
    consommationStats = new ConsommationStatsDto(/* ... hardcoded data ... */);
}

// ? APRÈS (données réelles)
private async Task LoadRealData()
{
    if (!selectedPeripheriqueId.HasValue || !startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Veuillez sélectionner un périphérique et une période", Severity.Warning);
        return;
    }

    isLoading = true;
    StateHasChanged();

    try
    {
        var response = await ConsommationService.GetConsommationStatsAsync(
            selectedPeripheriqueId.Value, 
            startDate.Value, 
            endDate.Value);

        if (response.IsSuccess && response.Data != null)
        {
            consommationStats = response.Data;
            Logger.LogInformation("? Statistiques chargées avec succès");
        }
        else
        {
            Snackbar.Add("? Erreur lors du chargement des statistiques", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Exception lors du chargement des données");
        Snackbar.Add("? Une erreur est survenue", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

#### **2. Chargement des données horaires**

```csharp
// ? Version corrigée avec vraies données
private async Task LoadConsommationHoraire()
{
    if (!selectedPeripheriqueId.HasValue) 
    {
        Logger.LogWarning("Aucun périphérique sélectionné");
        return;
    }

    try
    {
        var response = await ConsommationService.GetConsommationHoraireAsync(
            selectedPeripheriqueId.Value, 
            DateTime.Today);

        if (response.IsSuccess && response.Data != null)
        {
            consommationHoraire = response.Data;
            Logger.LogInformation("? {Count} heures de données chargées", consommationHoraire.Count);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Erreur lors du chargement des données horaires");
    }
}
```

#### **3. Top consommateurs avec vraies données**

```csharp
// ? Version corrigée
private async Task LoadTopConsommateurs()
{
    if (!startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Veuillez sélectionner une période", Severity.Warning);
        return;
    }

    isLoading = true;
    StateHasChanged();

    try
    {
        var response = await ConsommationService.GetTopConsommateursAsync(
            selectedZoneId, 
            startDate.Value, 
            endDate.Value, 
            top: 10);

        if (response.IsSuccess && response.Data != null)
        {
            topConsommateurs = response.Data;
            Snackbar.Add($"? {topConsommateurs.Count} consommateurs chargés", Severity.Success);
        }
        else
        {
            Snackbar.Add("? Aucune donnée disponible", Severity.Warning);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Erreur lors du chargement du top consommateurs");
        Snackbar.Add("? Erreur de chargement", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

#### **4. Détection d'anomalies**

```csharp
// ? Version corrigée
private async Task DetectAnomalies()
{
    if (!selectedPeripheriqueId.HasValue || !startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Paramètres manquants pour détecter les anomalies", Severity.Warning);
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
                Snackbar.Add($"?? {anomalies.Count} anomalies détectées", Severity.Warning);
            }
            else
            {
                Snackbar.Add("? Aucune anomalie détectée", Severity.Success);
            }
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Erreur lors de la détection d'anomalies");
        Snackbar.Add("? Erreur de détection", Severity.Error);
    }
    finally
    {
        isLoading = false;
        StateHasChanged();
    }
}
```

#### **5. Génération de rapport**

```csharp
// ? Version corrigée
private async Task GenererRapport()
{
    if (!startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Veuillez sélectionner une période", Severity.Warning);
        return;
    }

    isLoading = true;
    StateHasChanged();

    try
    {
        var response = await ConsommationService.GenererRapportConsommationAsync(
            selectedZoneId, 
            startDate.Value, 
            endDate.Value);

        if (response.IsSuccess && response.Data != null)
        {
            rapport = response.Data;
            Snackbar.Add("? Rapport généré avec succès", Severity.Success);
            
            // Optionnel: télécharger en PDF
            // await DownloadRapportPdf(rapport);
        }
        else
        {
            Snackbar.Add("? Impossible de générer le rapport", Severity.Error);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "? Erreur lors de la génération du rapport");
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

## ?? Graphiques et Visualisations

### Configuration MudChart pour consommation horaire

```csharp
private List<ChartSeries> GetConsommationHoraireSeries()
{
    if (!consommationHoraire.Any()) 
    {
        Logger.LogWarning("Aucune donnée horaire disponible pour le graphique");
        return new List<ChartSeries>();
    }

    return new List<ChartSeries>
    {
        new ChartSeries
        {
            Name = "Consommation (mWh)",
            Data = consommationHoraire.Select(x => x.ConsommationMwh).ToArray()
        },
        new ChartSeries
        {
            Name = "Puissance moyenne (mW)",
            Data = consommationHoraire.Select(x => x.PuissanceMoyenneMw).ToArray()
        }
    };
}

private string[] GetHeuresLabels()
{
    if (!consommationHoraire.Any()) 
        return Array.Empty<string>();
    
    return consommationHoraire.Select(x => x.Heure.ToString("HH:mm")).ToArray();
}
```

### Options de graphique personnalisées

```razor
<MudChart ChartType="ChartType.Line"
          ChartSeries="@GetConsommationHoraireSeries()"
          XAxisLabels="@GetHeuresLabels()"
          Width="100%"
          Height="300"
          ChartOptions="@chartOptions" />

@code {
    private ChartOptions chartOptions = new()
    {
        YAxisTicks = 5,
        MaxNumYAxisTicks = 10,
        YAxisFormat = "0.0",
        LineStrokeWidth = 2,
        DisableLegend = false,
        InterpolationOption = InterpolationOption.Straight
    };
}
```

---

## ?? Gestion des Erreurs

### Codes d'erreur HTTP

| Code | Signification | Action Frontend |
|------|---------------|-----------------|
| `200` | OK | Traiter les données |
| `400` | Bad Request | Valider les paramètres (dates, IDs) |
| `401` | Unauthorized | Rediriger vers login |
| `404` | Not Found | Périphérique/Zone inexistant |
| `500` | Internal Server Error | Message d'erreur générique |

### Validation des paramètres

```csharp
private bool ValidateParameters()
{
    if (!startDate.HasValue || !endDate.HasValue)
    {
        Snackbar.Add("?? Veuillez sélectionner une période", Severity.Warning);
        return false;
    }

    if (startDate.Value >= endDate.Value)
    {
        Snackbar.Add("?? La date de début doit être antérieure à la date de fin", Severity.Warning);
        return false;
    }

    var maxPeriod = TimeSpan.FromDays(365);
    if ((endDate.Value - startDate.Value) > maxPeriod)
    {
        Snackbar.Add("?? La période ne peut pas dépasser 365 jours", Severity.Warning);
        return false;
    }

    return true;
}
```

---

## ? Tests et Validation

### Checklist de Validation

- [ ] ? GET /api/ConsommationStats/peripherique/{id} retourne des données
- [ ] ? Les statistiques principales s'affichent correctement
- [ ] ? Le graphique horaire se charge avec les vraies données
- [ ] ? Le top consommateurs s'affiche
- [ ] ? Les anomalies sont détectées et affichées
- [ ] ? Le rapport se génère correctement
- [ ] ? Les filtres fonctionnent (périphérique, zone, dates)
- [ ] ? La validation des dates fonctionne
- [ ] ? Les messages d'erreur sont clairs
- [ ] ? Les graphiques sont responsive

### Scénarios de Test

#### Test 1: Charger les statistiques d'un périphérique

```
1. Sélectionner "Climatiseur Salon"
2. Choisir période: 7 derniers jours
3. Cliquer "Analyser"
? Les 4 cartes de stats s'affichent
? Les valeurs sont cohérentes (consommation > 0)
```

#### Test 2: Visualiser les données horaires

```
1. Sélectionner un périphérique
2. Analyser
? Le graphique "Consommation horaire" apparaît
? 24 points de données sont affichés
? Les courbes sont lisses et continues
```

#### Test 3: Détecter les anomalies

```
1. Sélectionner un périphérique
2. Choisir une période de 30 jours
3. Cliquer "Détecter Anomalies"
? Le tableau des anomalies s'affiche
? Chaque anomalie a: type, description, écart%, sévérité
```

#### Test 4: Générer un rapport

```
1. Choisir une période
2. Optionnel: sélectionner une zone
3. Cliquer "Générer Rapport"
? Le rapport s'affiche avec:
   - Consommation totale
   - Coût estimé
   - Économies potentielles
   - Recommandations (3 minimum)
```

---

## ?? Ressources Supplémentaires

### Documentation Backend

- [Controller: ConsommationStatsController.cs](../Controllers/ConsommationStatsController.cs)
- [Service: ConsommationStatsService.cs](../Services/ServicesImpl/ConsommationStatsService.cs)
- [DTOs: ConsommationStatsDto.cs](../DTOs/ConsommationStatsDto.cs)

### Exemple de fichier .http pour tests

```http
### Variables
@baseUrl = https://localhost:7144
@token = your-jwt-token

### 1. Stats périphérique
GET {{baseUrl}}/api/ConsommationStats/peripherique/1
    ?startDate=2025-01-01
    &endDate=2025-01-07
Authorization: Bearer {{token}}

### 2. Données horaires
GET {{baseUrl}}/api/ConsommationStats/peripherique/1/horaire
    ?date=2025-01-06
Authorization: Bearer {{token}}

### 3. Top consommateurs
GET {{baseUrl}}/api/ConsommationStats/top-consommateurs
    ?startDate=2025-01-01
    &endDate=2025-01-07
    &top=10
Authorization: Bearer {{token}}

### 4. Anomalies
GET {{baseUrl}}/api/ConsommationStats/peripherique/1/anomalies
    ?startDate=2025-01-01
    &endDate=2025-01-07
Authorization: Bearer {{token}}

### 5. Rapport
GET {{baseUrl}}/api/ConsommationStats/rapport
    ?startDate=2025-01-01
    &endDate=2025-01-07
Authorization: Bearer {{token}}
```

---

## ?? Résumé

### ? Ce qui est prêt (Backend)

1. **9 endpoints API** fonctionnels
2. **Service de calcul statistique** complet
3. **Détection d'anomalies** automatique
4. **Génération de rapports** avec recommandations
5. **Support multi-zones et multi-périphériques**

### ?? À corriger (Frontend)

1. **Remplacer** les données mockées par les vraies APIs
2. **Ajouter** la gestion d'erreurs avec Snackbar
3. **Valider** les paramètres avant les appels API
4. **Connecter** les graphiques aux données réelles
5. **Tester** tous les scénarios utilisateur

### ?? Métriques clés

- **9 APIs** disponibles
- **8 DTOs** complets
- **4 types de graphiques** supportés
- **3 niveaux de sévérité** pour anomalies
- **2-3 recommandations** par rapport

---

**Date:** 6 janvier 2025  
**Status:** ? Backend complet, Frontend à connecter  
**Temps estimé:** 3-4 heures d'implémentation frontend
