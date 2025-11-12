# ?? Configuration API Client - Application Mobile MAUI

## ?? Configuration du Service API

Voici comment configurer votre client API dans votre application MAUI pour se connecter au backend sur votre réseau local.

---

## ?? Exemple : ApiService.cs

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace SmartOfficeMobile.Services
{
    public class ApiService
    {
        // ?? CONFIGURATION - Remplacez par votre adresse IP
        #if DEBUG
        // Mode développement - Appareil physique USB
        private const string BaseUrl = "http://192.168.1.100:5290"; // ?? CHANGEZ ICI
        
        // Alternative pour émulateur Android
        // private const string BaseUrl = "http://10.0.2.2:5290";
        
        // Alternative pour simulateur iOS
        // private const string BaseUrl = "http://localhost:5290";
        #else
        // Mode production
        private const string BaseUrl = "https://api.smartoffice.com";
        #endif

        private readonly HttpClient _httpClient;
        private string? _jwtToken;

        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                // ?? DÉVELOPPEMENT UNIQUEMENT - Accepter tous les certificats SSL
                #if DEBUG
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                #endif
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Headers par défaut
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "SmartOffice-Mobile/1.0");
        }

        /// <summary>
        /// Définir le token JWT après connexion
        /// </summary>
        public void SetAuthToken(string token)
        {
            _jwtToken = token;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        /// <summary>
        /// Supprimer le token (déconnexion)
        /// </summary>
        public void ClearAuthToken()
        {
            _jwtToken = null;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }

        /// <summary>
        /// Test de connectivité - Health Check
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health/live");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Connexion utilisateur
        /// </summary>
        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new
                {
                    email = email,
                    password = password
                };

                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    
                    if (result?.Token != null)
                    {
                        SetAuthToken(result.Token);
                    }
                    
                    return result;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupérer le profil utilisateur
        /// </summary>
        public async Task<UserProfile?> GetProfileAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserProfile>("/api/Auth/profile");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get profile failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupérer toutes les zones
        /// </summary>
        public async Task<List<Zone>?> GetZonesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Zone");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Zone>>();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get zones failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupérer tous les périphériques
        /// </summary>
        public async Task<List<Device>?> GetDevicesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Device>>("/api/PeripheriqueLogique");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get devices failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupérer les statistiques du dashboard
        /// </summary>
        public async Task<DashboardStats?> GetDashboardStatsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DashboardStats>("/api/Dashboard/stats");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get dashboard stats failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Envoyer une commande à un périphérique
        /// </summary>
        public async Task<bool> SendCommandAsync(int peripheriqueId, string action, object? parameters = null)
        {
            try
            {
                var command = new
                {
                    peripheriqueId = peripheriqueId,
                    action = action,
                    parameters = parameters
                };

                var response = await _httpClient.PostAsJsonAsync("/api/Commande", command);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send command failed: {ex.Message}");
                return false;
            }
        }
    }

    // ============================================
    // DTOs - Adaptez selon votre backend
    // ============================================

    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? UserId { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class UserProfile
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Prenom { get; set; }
        public string? Nom { get; set; }
        public List<string>? Roles { get; set; }
    }

    public class Zone
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Description { get; set; }
    }

    public class Device
    {
        public int Id { get; set; }
        public string? Nom { get; set; }
        public string? Type { get; set; }
        public bool EstActif { get; set; }
        public int? ZoneId { get; set; }
    }

    public class DashboardStats
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int TotalZones { get; set; }
        public double TotalEnergyConsumption { get; set; }
    }
}
```

---

## ?? Utilisation dans votre Application

### 1?? Enregistrer le Service (MauiProgram.cs)

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Enregistrer le service API
        builder.Services.AddSingleton<ApiService>();

        return builder.Build();
    }
}
```

---

### 2?? Utiliser dans une Page (MainPage.xaml.cs)

```csharp
public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;

    public MainPage(ApiService apiService)
    {
        InitializeComponent();
        _apiService = apiService;
    }

    private async void OnTestConnectionClicked(object sender, EventArgs e)
    {
        var isConnected = await _apiService.TestConnectionAsync();
        
        if (isConnected)
        {
            await DisplayAlert("Succès", "Connexion au serveur réussie !", "OK");
        }
        else
        {
            await DisplayAlert("Erreur", "Impossible de se connecter au serveur", "OK");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await _apiService.LoginAsync("admin@smartoffice.com", "Admin@123");
            
            if (result?.Token != null)
            {
                await DisplayAlert("Succès", $"Connecté en tant que {result.Email}", "OK");
                
                // Naviguer vers la page principale
                await Navigation.PushAsync(new DashboardPage(_apiService));
            }
            else
            {
                await DisplayAlert("Erreur", "Identifiants invalides", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Erreur de connexion: {ex.Message}", "OK");
        }
    }
}
```

---

## ?? Configuration SignalR (Notifications en Temps Réel)

Si vous utilisez SignalR pour les notifications :

```csharp
using Microsoft.AspNetCore.SignalR.Client;

public class SignalRService
{
    private HubConnection? _hubConnection;
    private const string HubUrl = "http://192.168.1.100:5290/notificationhub"; // ?? CHANGEZ ICI

    public async Task ConnectAsync(string jwtToken)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(HubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(jwtToken);
            })
            .WithAutomaticReconnect()
            .Build();

        // Écouter les notifications
        _hubConnection.On<string>("ReceiveNotification", (message) =>
        {
            Console.WriteLine($"Notification reçue: {message}");
            // Afficher une alerte ou mettre à jour l'UI
        });

        await _hubConnection.StartAsync();
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
```

---

## ?? Test de Configuration

### Étape 1 : Vérifier l'adresse IP

```powershell
ipconfig
```

### Étape 2 : Remplacer dans le code

```csharp
private const string BaseUrl = "http://192.168.1.100:5290"; // ?? Votre IP
```

### Étape 3 : Lancer le backend

```bash
cd SmartOfficeAPP
dotnet run --launch-profile http
```

### Étape 4 : Tester depuis l'app mobile

1. Déployer l'app sur le téléphone
2. Cliquer sur "Test Connection"
3. Résultat : ? Succès

---

## ?? Checklist de Configuration

- [ ] Remplacez `192.168.1.100` par votre IP dans `ApiService.cs`
- [ ] Backend démarré sur le PC
- [ ] Pare-feu configuré (ports 5290, 7144)
- [ ] Téléphone et PC sur le même WiFi
- [ ] Test de connexion réussi depuis l'app

---

## ?? Problèmes Courants

### Erreur : "Connection refused"
```csharp
// Vérifiez que l'URL est correcte
private const string BaseUrl = "http://192.168.1.100:5290"; // Pas de / à la fin
```

### Erreur : "SSL/TLS error"
```csharp
// En développement, désactivez la validation SSL
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};
```

### Erreur : "Timeout"
```csharp
// Augmentez le timeout
_httpClient.Timeout = TimeSpan.FromSeconds(60);
```

---

## ?? Prochaines Étapes

1. ? Configurer l'ApiService
2. ? Tester la connexion
3. ? Implémenter l'authentification
4. ? Ajouter SignalR pour les notifications
5. ? Développer les fonctionnalités métier

Bon développement ! ??
