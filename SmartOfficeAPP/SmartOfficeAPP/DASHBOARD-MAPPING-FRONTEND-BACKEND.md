# ?? Mapping Frontend-Backend - Dashboard

## Vue d'ensemble

Ce document établit la **correspondance exacte** entre les appels du frontend Blazor et les endpoints backend disponibles.

---

## ?? Correspondances Services Frontend ? Endpoints Backend

### 1. **IAuthService**

| Méthode Frontend | Endpoint Backend | HTTP | Réponse |
|------------------|------------------|------|---------|
| `GetCurrentUserAsync()` | `/api/User/profile` | GET | `CurrentUserDto` |
| `LoginAsync(email, password)` | `/api/Auth/login` | POST | `AuthResponseDto` |
| `LogoutAsync()` | _(client-side uniquement)_ | - | - |

---

### 2. **IDashboardStatsService**

| Méthode Frontend | Endpoint Backend | HTTP | Paramètres | Réponse |
|------------------|------------------|------|------------|---------|
| `GetDashboardStatsAsync()` | `/api/Dashboard/stats` | GET | - | `DashboardStats` |
| `GetEnergyConsumptionDataAsync(period)` | `/api/Dashboard/energy` | GET | `?period={period}` | `List<EnergyDataPoint>` |
| `GetDeviceActivityDataAsync()` | `/api/Dashboard/device-activity` | GET | - | `List<DeviceActivityData>` |

---

### 3. **ICommandeService**

| Méthode Frontend | Endpoint Backend | HTTP | Body | Réponse |
|------------------|------------------|------|------|---------|
| `SendCommandAsync(id, cmd, val)` | `/api/Commande` | POST | `{ peripheriqueLogiqueId, nomCommande, valeur }` | `CommandeResult` |

---

## ?? Correspondances DTOs Frontend ? Backend

### ? Identiques (même structure)

| DTO Frontend | DTO Backend | Utilisation |
|--------------|-------------|-------------|
| `CurrentUserDto` | `UserProfileDto` | Profil utilisateur |
| `DashboardStats` | `DashboardStatsDto` | Statistiques KPI |
| `EnergyDataPoint` | `EnergyDataPointDto` | Points de données énergie |
| `DeviceActivityData` | `DeviceActivityDataDto` | Activité des appareils |

### ?? Différences mineures

| DTO Frontend | DTO Backend | Différence |
|--------------|-------------|------------|
| `CommandeResult` | `CommandeResponseDto` | Noms de propriétés identiques, namespace différent |
| `ApiResponse<T>` | `ApiResponse<T>` | Structure identique (wrapper générique) |

---

## ?? Headers HTTP requis

### Authentification JWT

**Toutes les requêtes** nécessitent un header `Authorization` :

```http
Authorization: Bearer {accessToken}
```

### Configuration HttpClient (Frontend)

```csharp
_httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", _token);
```

---

## ?? Format des réponses API

### Wrapper générique `ApiResponse<T>`

Toutes les réponses du backend Dashboard sont wrappées :

```json
{
  "data": { /* Données réelles */ },
  "source": "Database" | "Cache",
  "timestamp": "2025-01-13T10:30:00Z"
}
```

### Extraction des données (Frontend)

```csharp
var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardStats>>("/api/Dashboard/stats");
var stats = response?.Data; // ?? Extraire Data
```

---

## ?? Utilisation dans Dashboard.razor

### Code actuel de votre Dashboard

```csharp
@code {
    // === Variables d'état ===
    private string? _currentUserName;
    private string? _currentUserRole;
    private DashboardStats? _stats;
    private List<ChartSeries> _energySeries = new();
    private string[] _energyLabels = Array.Empty<string>();
    private double[] _deviceActivityData = Array.Empty<double>();
    private string[] _deviceActivityLabels = Array.Empty<string>();
    
    // === Chargement des données ===
    protected override async Task OnInitializedAsync()
    {
        await LoadUserDataAsync();
      await LoadDashboardStatsAsync();
 await LoadEnergyDataAsync();
        await LoadDeviceActivityAsync();
    }
}
```

### ? Mapping exact avec les services backend

```csharp
private async Task LoadUserDataAsync()
{
    // ? Appel : IAuthService.GetCurrentUserAsync()
    // ? Backend : GET /api/User/profile
    var currentUser = await AuthService.GetCurrentUserAsync();
    if (currentUser != null)
    {
        _currentUserName = currentUser.FirstName ?? currentUser.Email?.Split('@')[0];
      _currentUserRole = currentUser.Roles.FirstOrDefault();
    }
}

private async Task LoadDashboardStatsAsync()
{
    // ? Appel : IDashboardStatsService.GetDashboardStatsAsync()
    // ? Backend : GET /api/Dashboard/stats
    _stats = await DashboardStatsService.GetDashboardStatsAsync();
}

private async Task LoadEnergyDataAsync()
{
    // ? Appel : IDashboardStatsService.GetEnergyConsumptionDataAsync("month")
    // ? Backend : GET /api/Dashboard/energy?period=month
    var energyData = await DashboardStatsService.GetEnergyConsumptionDataAsync(_selectedPeriod);
    
    _energyLabels = energyData.Select(d => d.Label).ToArray();
    _energySeries = new List<ChartSeries>
    {
    new ChartSeries
   {
        Name = "Consommation (kWh)",
Data = energyData.Select(d => d.Value).ToArray()
        }
    };
}

private async Task LoadDeviceActivityAsync()
{
    // ? Appel : IDashboardStatsService.GetDeviceActivityDataAsync()
    // ? Backend : GET /api/Dashboard/device-activity
    var activityData = await DashboardStatsService.GetDeviceActivityDataAsync();
    
    _deviceActivityLabels = activityData.Select(d => d.DeviceType).ToArray();
    _deviceActivityData = activityData.Select(d => (double)d.Count).ToArray();
}

private async Task SendDeviceCommand(int peripheriqueId, string nomCommande, string? valeur = null)
{
    // ? Appel : ICommandeService.SendCommandAsync(id, cmd, val)
    // ? Backend : POST /api/Commande
    var result = await CommandeService.SendCommandAsync(peripheriqueId, nomCommande, valeur);
    
    if (result.IsSuccess)
    {
        Snackbar.Add("Commande envoyée avec succès", Severity.Success);
    }
    else
    {
        Snackbar.Add($"Échec: {result.Message}", Severity.Warning);
    }
}
```

---

## ?? Flux de données complet

### Au chargement de la page Dashboard

```
1. Frontend appelle GetCurrentUserAsync()
   ??> Backend : GET /api/User/profile
       ??> Réponse : { id, email, firstName, lastName, roles }

2. Frontend appelle GetDashboardStatsAsync()
   ??> Backend : GET /api/Dashboard/stats
       ??> Réponse : { activeDevices, totalUsers, openIncidents, energyConsumed, ... }

3. Frontend appelle GetEnergyConsumptionDataAsync("month")
   ??> Backend : GET /api/Dashboard/energy?period=month
       ??> Réponse : [{ label: "01/12", value: 45.2 }, ...]

4. Frontend appelle GetDeviceActivityDataAsync()
   ??> Backend : GET /api/Dashboard/device-activity
       ??> Réponse : [{ deviceType: "Climatisation", count: 5 }, ...]
```

### Lors d'un clic sur un DeviceCard

```
1. Utilisateur clique sur le bouton "Toggle" du DeviceCard
   ??> Frontend appelle SendCommandAsync(1, "TurnOn")
       ??> Backend : POST /api/Commande { peripheriqueLogiqueId: 1, nomCommande: "TurnOn" }
           ??> Backend vérifie les droits d'accès (zone/device)
           ??> Backend envoie la commande via Azure IoT Hub
           ??> Backend enregistre la commande en DB
        ??> Backend notifie via SignalR (optionnel)
    ??> Réponse : { isSuccess: true, message: "Commande envoyée", commandeId: 42 }
```

---

## ?? Configuration requise

### Frontend (Program.cs)

```csharp
// HttpClient avec BaseAddress
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5210") 
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardStatsService, DashboardStatsService>();
builder.Services.AddScoped<ICommandeService, CommandeService>();
```

### Backend (Program.cs)

```csharp
// CORS (si frontend sur un port différent)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5xxx") // Port du frontend
  .AllowAnyMethod()
             .AllowAnyHeader()
  .AllowCredentials();
    });
});

// Output Caching
builder.Services.AddOutputCache();

// Controllers
builder.Services.AddControllers();

// ...

app.UseCors();
app.UseOutputCache();
```

---

## ??? Gestion des erreurs

### Codes HTTP

| Code | Cause | Action Frontend |
|------|-------|-----------------|
| 200 | Succès | Afficher les données |
| 401 | Token JWT manquant/expiré | Rediriger vers `/login` |
| 403 | Accès refusé | Afficher "Accès refusé" |
| 404 | Ressource introuvable | Afficher "Données non disponibles" |
| 500 | Erreur serveur | Afficher "Erreur serveur" + retry |

### Exemple de gestion dans un service

```csharp
public async Task<DashboardStats?> GetDashboardStatsAsync()
{
    try
    {
  var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardStats>>("/api/Dashboard/stats");
        return response?.Data;
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
    {
        // Token expiré ? redirection login
        _navigationManager.NavigateTo("/login");
    return null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération des statistiques");
     return null;
    }
}
```

---

## ? Checklist de validation

### Backend (Déjà fait ?)

- [x] Contrôleur `DashboardController` créé
- [x] Endpoints testés et fonctionnels
- [x] DTOs backend créés
- [x] `UserProfileDto` enrichi avec FirstName, LastName, Roles
- [x] Cache configuré (OutputCache)
- [x] Logs ajoutés
- [x] Documentation complète

### Frontend (À faire ??)

- [ ] Services `IAuthService`, `IDashboardStatsService`, `ICommandeService` implémentés
- [ ] DTOs frontend créés
- [ ] Services enregistrés dans `Program.cs`
- [ ] HttpClient configuré avec BaseAddress
- [ ] Token JWT stocké et envoyé dans headers
- [ ] Gestion des erreurs HTTP (401, 403, 500)
- [ ] Données affichées dans Dashboard.razor
- [ ] Tests d'intégration frontend

---

## ?? Documentation complémentaire

| Fichier | Contenu |
|---------|---------|
| `DASHBOARD-FRONTEND-INTEGRATION-GUIDE.md` | Guide complet avec exemples de code détaillés |
| `DASHBOARD-BACKEND-IMPLEMENTATION-SUMMARY.md` | Résumé de l'implémentation backend |
| `DASHBOARD-QUICK-START.md` | Démarrage rapide (5 minutes) |
| `Tests/34-Dashboard-Frontend-API.http` | Tests HTTP des endpoints |

---

## ?? Temps d'intégration estimé

| Tâche | Temps estimé |
|-------|--------------|
| Créer les services frontend | 30 min |
| Créer les DTOs frontend | 15 min |
| Configurer Program.cs | 5 min |
| Implémenter l'authentification JWT | 20 min |
| Intégrer dans Dashboard.razor | 30 min |
| Tester et debugger | 30 min |
| **TOTAL** | **~2h** |

---

**Dernière mise à jour** : 13 janvier 2025  
**Version** : 1.0.0  
**Statut** : ? Backend prêt pour intégration frontend
