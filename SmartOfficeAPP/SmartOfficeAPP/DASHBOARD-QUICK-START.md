# ?? Quick Start - Intégration Dashboard Frontend

## ?? Démarrage rapide (5 minutes)

### 1?? Backend (Déjà prêt ?)

Le backend expose maintenant **5 endpoints** pour le Dashboard :

```
? GET  /api/User/profile        ? Profil utilisateur
? GET  /api/Dashboard/stats           ? KPI (devices, users, incidents, energy)
? GET  /api/Dashboard/energy?period=X ? Graphique de consommation
? GET  /api/Dashboard/device-activity ? Répartition par type d'appareil
? POST /api/Commande      ? Envoyer une commande IoT
```

### 2?? Frontend (À implémenter)

#### Étape A : Créer les services

```bash
mkdir -p Services/IServices
touch Services/IServices/IAuthService.cs
touch Services/IServices/IDashboardStatsService.cs
touch Services/IServices/ICommandeService.cs
touch Services/AuthService.cs
touch Services/DashboardStatsService.cs
touch Services/CommandeService.cs
```

#### Étape B : Créer les DTOs

```bash
mkdir -p Models
touch Models/CurrentUserDto.cs
touch Models/DashboardStats.cs
touch Models/EnergyDataPoint.cs
touch Models/DeviceActivityData.cs
touch Models/CommandeResult.cs
touch Models/ApiResponse.cs
```

#### Étape C : Enregistrer les services dans `Program.cs`

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardStatsService, DashboardStatsService>();
builder.Services.AddScoped<ICommandeService, CommandeService>();
```

#### Étape D : Configurer le `HttpClient`

```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5210") 
});
```

---

## ?? Fichiers à copier-coller

### ?? `Models/CurrentUserDto.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class CurrentUserDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string> Roles { get; set; } = new();
}
```

### ?? `Models/DashboardStats.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class DashboardStats
{
    public int ActiveDevices { get; set; }
    public int TotalDevices { get; set; }
    public double DevicesChangePercent { get; set; }
    
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }
    public double UsersChangePercent { get; set; }
    
    public int OpenIncidents { get; set; }
    public int TotalIncidents { get; set; }
    public double IncidentsChangePercent { get; set; }
    
    public double EnergyConsumed { get; set; }
    public double EnergyChangePercent { get; set; }
}
```

### ?? `Models/EnergyDataPoint.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class EnergyDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}
```

### ?? `Models/DeviceActivityData.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class DeviceActivityData
{
    public string DeviceType { get; set; } = string.Empty;
    public int Count { get; set; }
}
```

### ?? `Models/ApiResponse.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
```

### ?? `Services/IServices/IDashboardStatsService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IDashboardStatsService
{
    Task<DashboardStats?> GetDashboardStatsAsync();
    Task<List<EnergyDataPoint>> GetEnergyConsumptionDataAsync(string period);
    Task<List<DeviceActivityData>> GetDeviceActivityDataAsync();
}
```

### ?? `Services/DashboardStatsService.cs`

```csharp
using MySmartApp.Ui.Models;
using System.Net.Http.Json;

namespace MySmartApp.Ui.Services;

public class DashboardStatsService : IDashboardStatsService
{
    private readonly HttpClient _httpClient;

    public DashboardStatsService(HttpClient httpClient)
    {
  _httpClient = httpClient;
    }

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
     var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardStats>>("/api/Dashboard/stats");
        return response?.Data;
    }

    public async Task<List<EnergyDataPoint>> GetEnergyConsumptionDataAsync(string period)
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<EnergyDataPoint>>>($"/api/Dashboard/energy?period={period}");
        return response?.Data ?? new();
    }

    public async Task<List<DeviceActivityData>> GetDeviceActivityDataAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<DeviceActivityData>>>("/api/Dashboard/device-activity");
        return response?.Data ?? new();
    }
}
```

---

## ?? Tester le backend

### Option 1 : Avec Visual Studio Code (Extension REST Client)

1. Ouvrir `SmartOfficeAPP/Tests/34-Dashboard-Frontend-API.http`
2. Cliquer sur "Send Request" pour chaque endpoint
3. Vérifier que les réponses sont correctes

### Option 2 : Avec Swagger

1. Démarrer le backend : `dotnet run`
2. Ouvrir `http://localhost:5210/swagger`
3. Tester les endpoints `/api/Dashboard/*`

### Option 3 : Avec Postman

1. Importer la collection depuis `Tests/34-Dashboard-Frontend-API.http`
2. Ajouter le token JWT dans les headers
3. Exécuter les requêtes

---

## ?? Authentification

### Obtenir un token JWT

```http
POST http://localhost:5210/api/Auth/login
Content-Type: application/json

{
"email": "admin@example.com",
  "password": "Admin@123"
}
```

**Réponse** :
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "guid-refresh-token",
  "userId": "user-guid"
}
```

### Utiliser le token dans les requêtes

```csharp
_httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", accessToken);
```

---

## ?? Utilisation dans la page Dashboard

### Code existant de votre Dashboard.razor

```csharp
@code {
    private DashboardStats? _stats;
    private List<EnergyDataPoint> _energyData = new();
    private List<DeviceActivityData> _deviceActivity = new();
    
    protected override async Task OnInitializedAsync()
    {
        // ? Charger les statistiques
  _stats = await DashboardStatsService.GetDashboardStatsAsync();
        
   // ? Charger les données énergétiques
        _energyData = await DashboardStatsService.GetEnergyConsumptionDataAsync("month");
        
        // ? Charger l'activité des appareils
  _deviceActivity = await DashboardStatsService.GetDeviceActivityDataAsync();
    }
    
    // ? Envoyer une commande IoT
    private async Task SendDeviceCommand(int peripheriqueId, string nomCommande)
    {
        var result = await CommandeService.SendCommandAsync(peripheriqueId, nomCommande);
        
        if (result.IsSuccess)
        {
          Snackbar.Add("Commande envoyée avec succès", Severity.Success);
        }
        else
 {
            Snackbar.Add($"Erreur: {result.Message}", Severity.Error);
      }
}
}
```

---

## ?? Checklist de validation

### Backend ? (Déjà fait)

- [x] Contrôleur `DashboardController` créé
- [x] Endpoints fonctionnels
- [x] DTOs backend créés
- [x] Tests HTTP disponibles
- [x] Documentation complète

### Frontend ?? (À faire)

- [ ] Services créés (`IAuthService`, `IDashboardStatsService`, `ICommandeService`)
- [ ] DTOs frontend créés
- [ ] Services enregistrés dans `Program.cs`
- [ ] Token JWT stocké et envoyé dans les headers
- [ ] Gestion des erreurs HTTP (401, 403, 500)
- [ ] Affichage des données dans les composants MudBlazor
- [ ] Tests d'intégration frontend

---

## ?? Documentation complète

Pour une documentation détaillée avec exemples complets, consulter :

?? **`DASHBOARD-FRONTEND-INTEGRATION-GUIDE.md`**

Ce guide contient :
- Configuration complète des services
- Implémentation détaillée avec gestion d'erreurs
- Tous les DTOs nécessaires
- Exemples de réponses API
- Gestion des erreurs HTTP
- Checklist de validation

---

## ?? Problèmes fréquents

### ? Erreur 401 Unauthorized

**Cause** : Token JWT manquant ou expiré

**Solution** :
```csharp
// Vérifier que le token est bien envoyé
_httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", _token);
```

### ? Erreur CORS

**Cause** : Backend ne permet pas les requêtes depuis le frontend

**Solution** : Vérifier la configuration CORS dans `Program.cs` du backend

### ? Données null

**Cause** : Aucune donnée dans la base de données

**Solution** : Utiliser le service de seeding ou créer des données de test

---

## ?? Prochaines étapes

1. ? **Implémenter les services frontend** (copier-coller le code ci-dessus)
2. ? **Tester avec le backend** (démarrer les deux projets)
3. ? **Afficher les données** dans les composants MudBlazor
4. ? **Gérer les erreurs** (toast/snackbar)
5. ? **Optimiser** (cache, retry, loading states)

---

**Temps estimé d'intégration** : 2-3 heures  
**Difficulté** : ????? (Facile)  
**Support** : Consulter `DASHBOARD-FRONTEND-INTEGRATION-GUIDE.md`
