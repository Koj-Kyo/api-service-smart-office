# ?? Guide d'Intégration API Dashboard - Frontend Blazor

## ?? Vue d'ensemble

Ce guide détaille l'intégration complète de la page **Dashboard** (`/dashboard`) du frontend Blazor avec les APIs backend .NET 9.

---

## ?? Table des matières

1. [Configuration des services frontend](#1-configuration-des-services-frontend)
2. [Interfaces des services](#2-interfaces-des-services)
3. [Implémentation des services](#3-implémentation-des-services)
4. [DTOs Frontend](#4-dtos-frontend)
5. [Endpoints API disponibles](#5-endpoints-api-disponibles)
6. [Gestion des erreurs](#6-gestion-des-erreurs)
7. [Tests et validation](#7-tests-et-validation)

---

## 1. Configuration des services frontend

### ?? `Program.cs` (Frontend Blazor)

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MySmartApp.Ui;
using MySmartApp.Ui.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configuration HttpClient avec BaseAddress
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5210") // ?? URL de votre backend
});

// ? Enregistrement des services pour le Dashboard
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IDashboardStatsService, DashboardStatsService>();
builder.Services.AddScoped<ICommandeService, CommandeService>();

// MudBlazor
builder.Services.AddMudServices();

await builder.Build().RunAsync();
```

---

## 2. Interfaces des services

### ?? `Services/IServices/IAuthService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IAuthService
{
    /// <summary>
    /// Récupère le profil de l'utilisateur connecté
    /// </summary>
    Task<CurrentUserDto?> GetCurrentUserAsync();
    
    /// <summary>
    /// Connexion utilisateur
    /// </summary>
    Task<AuthResult> LoginAsync(string email, string password);
    
    /// <summary>
    /// Déconnexion utilisateur
    /// </summary>
    Task LogoutAsync();
}
```

### ?? `Services/IServices/IDashboardStatsService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface IDashboardStatsService
{
    /// <summary>
 /// Récupère les statistiques KPI du Dashboard
  /// </summary>
  Task<DashboardStats?> GetDashboardStatsAsync();
    
    /// <summary>
    /// Récupère les données de consommation énergétique
    /// </summary>
    /// <param name="period">"day", "week" ou "month"</param>
    Task<List<EnergyDataPoint>> GetEnergyConsumptionDataAsync(string period);
    
    /// <summary>
    /// Récupère l'activité des appareils par type
    /// </summary>
    Task<List<DeviceActivityData>> GetDeviceActivityDataAsync();
}
```

### ?? `Services/IServices/ICommandeService.cs`

```csharp
using MySmartApp.Ui.Models;

namespace MySmartApp.Ui.Services;

public interface ICommandeService
{
    /// <summary>
    /// Envoie une commande à un périphérique IoT
    /// </summary>
    /// <param name="peripheriqueId">ID du périphérique</param>
    /// <param name="nomCommande">Nom de la commande (TurnOn, TurnOff, etc.)</param>
    /// <param name="valeur">Valeur optionnelle (pour SetTemperature, etc.)</param>
    Task<CommandeResult> SendCommandAsync(int peripheriqueId, string nomCommande, string? valeur = null);
}
```

---

## 3. Implémentation des services

### ?? `Services/AuthService.cs`

```csharp
using MySmartApp.Ui.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace MySmartApp.Ui.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private string? _token;

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
    {
      _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        try
  {
  // ? Récupérer le token stocké (localStorage, sessionStorage, etc.)
            if (!string.IsNullOrEmpty(_token))
    {
   _httpClient.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", _token);
      }

            // ? Appeler l'endpoint backend
  var response = await _httpClient.GetAsync("/api/User/profile");
            
     if (!response.IsSuccessStatusCode)
       {
           _logger.LogWarning("Échec de récupération du profil utilisateur: {StatusCode}", 
          response.StatusCode);
     return null;
        }

    var user = await response.Content.ReadFromJsonAsync<CurrentUserDto>();
            return user;
        }
        catch (Exception ex)
      {
        _logger.LogError(ex, "Erreur lors de la récupération du profil utilisateur");
            return null;
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
  var loginRequest = new { email, password };
 var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginRequest);
            
 if (!response.IsSuccessStatusCode)
    {
           return new AuthResult { IsSuccess = false, Message = "Identifiants invalides" };
      }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (authResponse != null)
            {
 _token = authResponse.AccessToken;
         _httpClient.DefaultRequestHeaders.Authorization = 
      new AuthenticationHeaderValue("Bearer", _token);
        
        // ?? TODO: Stocker le token dans localStorage ou sessionStorage
                
      return new AuthResult { IsSuccess = true, Token = _token };
        }

            return new AuthResult { IsSuccess = false, Message = "Erreur de connexion" };
        }
   catch (Exception ex)
        {
       _logger.LogError(ex, "Erreur lors de la connexion");
 return new AuthResult { IsSuccess = false, Message = ex.Message };
        }
    }

    public Task LogoutAsync()
    {
     _token = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
        // ?? TODO: Supprimer le token du localStorage
     return Task.CompletedTask;
    }
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
    private readonly ILogger<DashboardStatsService> _logger;

    public DashboardStatsService(HttpClient httpClient, ILogger<DashboardStatsService> logger)
    {
     _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DashboardStats?> GetDashboardStatsAsync()
    {
        try
        {
// ? Appeler l'endpoint backend
   var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardStats>>(
 "/api/Dashboard/stats");
            
  return response?.Data;
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "Erreur lors de la récupération des statistiques du Dashboard");
            return null;
        }
    }

    public async Task<List<EnergyDataPoint>> GetEnergyConsumptionDataAsync(string period)
    {
        try
        {
    // ? Appeler l'endpoint backend avec paramètre de période
      var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<EnergyDataPoint>>>(
 $"/api/Dashboard/energy?period={period}");
            
            return response?.Data ?? new List<EnergyDataPoint>();
        }
        catch (Exception ex)
    {
      _logger.LogError(ex, "Erreur lors de la récupération des données énergétiques");
        return new List<EnergyDataPoint>();
     }
    }

    public async Task<List<DeviceActivityData>> GetDeviceActivityDataAsync()
    {
        try
        {
      // ? Appeler l'endpoint backend
       var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<DeviceActivityData>>>(
           "/api/Dashboard/device-activity");
            
            return response?.Data ?? new List<DeviceActivityData>();
  }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Erreur lors de la récupération de l'activité des appareils");
       return new List<DeviceActivityData>();
        }
    }
}
```

### ?? `Services/CommandeService.cs`

```csharp
using MySmartApp.Ui.Models;
using System.Net.Http.Json;

namespace MySmartApp.Ui.Services;

public class CommandeService : ICommandeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CommandeService> _logger;

    public CommandeService(HttpClient httpClient, ILogger<CommandeService> logger)
    {
     _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CommandeResult> SendCommandAsync(
      int peripheriqueId, 
        string nomCommande, 
        string? valeur = null)
    {
        try
        {
        var request = new
            {
                peripheriqueLogiqueId = peripheriqueId,
      nomCommande,
       valeur
};

            // ? Appeler l'endpoint backend
            var response = await _httpClient.PostAsJsonAsync("/api/Commande", request);
            
            if (!response.IsSuccessStatusCode)
{
         return new CommandeResult 
       { 
       IsSuccess = false, 
      Message = $"Erreur HTTP {response.StatusCode}" 
           };
   }

     var result = await response.Content.ReadFromJsonAsync<CommandeResponseDto>();
    
         return new CommandeResult
    {
        IsSuccess = result?.IsSuccess ?? false,
             Message = result?.Message ?? "Erreur inconnue",
        CommandeId = result?.CommandeId
            };
        }
        catch (Exception ex)
        {
 _logger.LogError(ex, "Erreur lors de l'envoi de la commande {Command} au périphérique {Id}", 
           nomCommande, peripheriqueId);
        
 return new CommandeResult
   {
     IsSuccess = false,
  Message = ex.Message
            };
        }
    }
}
```

---

## 4. DTOs Frontend

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
    // Appareils
    public int ActiveDevices { get; set; }
    public int TotalDevices { get; set; }
    public double DevicesChangePercent { get; set; }
    
    // Utilisateurs
    public int ActiveUsers { get; set; }
  public int TotalUsers { get; set; }
 public double UsersChangePercent { get; set; }
    
    // Incidents
    public int OpenIncidents { get; set; }
    public int TotalIncidents { get; set; }
    public double IncidentsChangePercent { get; set; }
    
    // Énergie
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

### ?? `Models/CommandeResult.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class CommandeResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CommandeId { get; set; }
}
```

### ?? `Models/ApiResponse.cs`

```csharp
namespace MySmartApp.Ui.Models;

/// <summary>
/// Wrapper générique pour les réponses API backend
/// </summary>
public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
```

### ?? `Models/AuthResponseDto.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
```

### ?? `Models/CommandeResponseDto.cs`

```csharp
namespace MySmartApp.Ui.Models;

public class CommandeResponseDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? CommandeId { get; set; }
}
```

---

## 5. Endpoints API disponibles

### ?? Authentification requise

Tous les endpoints nécessitent un **JWT Bearer token** dans les headers :

```http
Authorization: Bearer {votre_token}
```

### ?? Liste des endpoints

| Endpoint | Méthode | Description | Service Frontend |
|----------|---------|-------------|------------------|
| `/api/User/profile` | GET | Profil utilisateur connecté | `IAuthService.GetCurrentUserAsync()` |
| `/api/Dashboard/stats` | GET | Statistiques KPI | `IDashboardStatsService.GetDashboardStatsAsync()` |
| `/api/Dashboard/energy?period={period}` | GET | Consommation énergie | `IDashboardStatsService.GetEnergyConsumptionDataAsync(period)` |
| `/api/Dashboard/device-activity` | GET | Activité appareils | `IDashboardStatsService.GetDeviceActivityDataAsync()` |
| `/api/Commande` | POST | Envoyer commande IoT | `ICommandeService.SendCommandAsync()` |

### ?? Exemples de réponses

#### `GET /api/User/profile`

```json
{
  "id": "user-guid-123",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "firstName": "John",
  "lastName": "Doe",
  "roles": ["Admin", "User"]
}
```

#### `GET /api/Dashboard/stats`

```json
{
  "data": {
    "activeDevices": 12,
    "totalDevices": 15,
    "devicesChangePercent": 5.2,
    "activeUsers": 8,
 "totalUsers": 10,
    "usersChangePercent": 3.1,
    "openIncidents": 3,
    "totalIncidents": 25,
    "incidentsChangePercent": -2.4,
    "energyConsumed": 1245.5,
    "energyChangePercent": -8.5
  },
  "source": "Database",
  "timestamp": "2025-01-13T10:30:00Z"
}
```

#### `GET /api/Dashboard/energy?period=month`

```json
{
  "data": [
    { "label": "01/12", "value": 45.2 },
    { "label": "02/12", "value": 48.7 },
    { "label": "03/12", "value": 42.1 },
    { "label": "04/12", "value": 39.8 }
  ],
  "source": "Database",
  "timestamp": "2025-01-13T10:30:00Z"
}
```

#### `GET /api/Dashboard/device-activity`

```json
{
  "data": [
  { "deviceType": "Climatisation", "count": 5 },
    { "deviceType": "Éclairage", "count": 8 },
    { "deviceType": "Capteur", "count": 2 }
  ],
  "source": "Database",
  "timestamp": "2025-01-13T10:30:00Z"
}
```

#### `POST /api/Commande`

**Request Body:**
```json
{
  "peripheriqueLogiqueId": 1,
  "nomCommande": "TurnOn",
  "valeur": null
}
```

**Response:**
```json
{
  "isSuccess": true,
  "message": "Commande envoyée avec succès",
  "commandeId": 42
}
```

---

## 6. Gestion des erreurs

### ?? Codes d'erreur HTTP

| Code | Signification | Action frontend |
|------|---------------|-----------------|
| 200 | Succès | Traiter les données normalement |
| 401 | Non authentifié | Rediriger vers `/login` |
| 403 | Accès refusé | Afficher message d'erreur |
| 404 | Ressource introuvable | Afficher "Données non disponibles" |
| 500 | Erreur serveur | Afficher "Erreur serveur, réessayez plus tard" |

### ??? Gestion des erreurs dans les services

```csharp
public async Task<DashboardStats?> GetDashboardStatsAsync()
{
    try
    {
        var response = await _httpClient.GetFromJsonAsync<ApiResponse<DashboardStats>>(
            "/api/Dashboard/stats");
        
        return response?.Data;
    }
    catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
     _logger.LogWarning("Session expirée, redirection vers login");
        // TODO: Rediriger vers /login
        return null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erreur lors de la récupération des statistiques");
        // Afficher un Snackbar d'erreur
 return null;
    }
}
```

---

## 7. Tests et validation

### ?? Fichier de tests HTTP

Le fichier `Tests/34-Dashboard-Frontend-API.http` contient tous les tests des endpoints.

### ? Checklist de validation

#### Backend

- [ ] Le contrôleur `DashboardController` est créé
- [ ] Les DTOs `DashboardStatsDto`, `EnergyDataPointDto`, `DeviceActivityDataDto` existent
- [ ] `UserProfileDto` contient `FirstName`, `LastName`, `Roles`
- [ ] `UserController.GetProfile()` retourne les rôles
- [ ] Les endpoints répondent avec des codes HTTP 200
- [ ] L'authentification JWT fonctionne
- [ ] Le cache OutputCache est configuré

#### Frontend

- [ ] Les services sont enregistrés dans `Program.cs`
- [ ] Les interfaces `IAuthService`, `IDashboardStatsService`, `ICommandeService` sont implémentées
- [ ] Les DTOs frontend correspondent aux DTOs backend
- [ ] Le `HttpClient` a la bonne `BaseAddress`
- [ ] Le token JWT est stocké et envoyé dans les headers
- [ ] Les erreurs HTTP sont gérées (401, 403, 500)
- [ ] Les données null sont gérées (tableau vide au lieu de null)
- [ ] Les composants MudBlazor affichent correctement les données

---

## ?? Support et Contact

Pour toute question ou problème d'intégration :

1. **Vérifier les logs backend** : `SmartOfficeAPP/logs/`
2. **Consulter les tests HTTP** : `SmartOfficeAPP/Tests/34-Dashboard-Frontend-API.http`
3. **Vérifier la console du navigateur** (F12) pour les erreurs JavaScript
4. **Vérifier que le backend est démarré** : `http://localhost:5210/swagger`

---

## ?? Démarrage rapide

### Backend

```bash
cd SmartOfficeAPP
dotnet run
```

Le backend sera accessible sur `http://localhost:5210`

### Frontend

```bash
cd MySmartApp.Ui
dotnet run
```

Le frontend sera accessible sur `http://localhost:5xxx` (port généré)

### Tests

1. Lancer le backend
2. Ouvrir `Tests/01-Authentication.http`
3. Exécuter la requête de login pour obtenir un token
4. Copier le token dans la variable `@token` de `Tests/34-Dashboard-Frontend-API.http`
5. Exécuter les tests des endpoints Dashboard

---

## ?? Ressources additionnelles

- [Documentation MudBlazor](https://mudblazor.com/)
- [Documentation Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/)
- [Documentation ASP.NET Core API](https://learn.microsoft.com/aspnet/core/web-api/)
- [Documentation Azure IoT Hub](https://learn.microsoft.com/azure/iot-hub/)

---

**Version du guide** : 1.0.0  
**Date de mise à jour** : 13 janvier 2025  
**Auteur** : Backend Development Team
