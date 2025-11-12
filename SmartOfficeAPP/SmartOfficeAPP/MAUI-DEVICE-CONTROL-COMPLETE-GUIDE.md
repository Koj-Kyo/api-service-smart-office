# ?? Guide Complet - Contrôle des Périphériques depuis MAUI

## ?? **OBJECTIF**

Permettre à votre application mobile MAUI de :
- ? Lister les périphériques avec état temps réel
- ? Afficher les détails complets (DB + IoT Hub fusionnés)
- ? Envoyer des commandes (ON/OFF, ajuster luminosité, etc.)
- ? Recevoir les notifications temps réel (SignalR)
- ? Gérer les statistiques et l'énergie

---

## ?? **TABLE DES MATIÈRES**

1. [Architecture de Communication](#architecture)
2. [Configuration Initiale](#configuration)
3. [Récupérer les Périphériques](#recuperer-peripheriques)
4. [Envoyer des Commandes](#envoyer-commandes)
5. [Recevoir les Notifications Temps Réel](#notifications-temps-reel)
6. [Exemples de Code Complets](#exemples-code)
7. [Gestion des Erreurs](#gestion-erreurs)
8. [Tests et Validation](#tests-validation)

---

## ??? **1. ARCHITECTURE DE COMMUNICATION** {#architecture}

```
???????????????????????????????????
?   APP MOBILE MAUI (XAMARIN)    ?
?   - Xamarin.Forms / .NET MAUI   ?
?   - HttpClient (REST API)       ?
?   - SignalR Client (Temps réel) ?
???????????????????????????????????
             ?
             ? HTTP/HTTPS + WebSocket
             ?
???????????????????????????????????
?   BACKEND API (.NET 9)          ?
?   https://192.168.1.100:5290    ?
?                                 ?
?   ???????????????????????????   ?
?   ? DevicePeripherals       ?   ?
?   ? Controller (REST)       ?   ?
?   ???????????????????????????   ?
?               ?                 ?
?   ???????????????????????????   ?
?   ? DevicePeripheral        ?   ?
?   ? Service (Business)      ?   ?
?   ???????????????????????????   ?
?           ?          ?           ?
?   ???????????????? ??????????   ?
?   ? PostgreSQL   ? ?IoT Hub ?   ?
?   ? (DevicePeri- ? ?(Twin)  ?   ?
?   ?  pherals)    ? ?        ?   ?
?   ???????????????? ??????????   ?
?                                 ?
?   ???????????????????????????   ?
?   ? NotificationHub         ?   ?
?   ? (SignalR)               ?   ?
?   ???????????????????????????   ?
???????????????????????????????????
             ?
             ? IoT Hub Commands
             ?
???????????????????????????????????
?   FIRMWARE ESP32                ?
?   - Exécute les commandes       ?
?   - Reporte l'état (Twin)       ?
???????????????????????????????????
```

---

## ?? **2. CONFIGURATION INITIALE** {#configuration}

### **2.1 Installer les Packages NuGet**

Dans votre projet MAUI, installez :

```xml
<ItemGroup>
  <!-- HTTP Client pour REST API -->
  <PackageReference Include="System.Net.Http.Json" Version="9.0.0" />
  
  <!-- SignalR pour temps réel -->
  <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="9.0.0" />
  
  <!-- JSON Serialization -->
  <PackageReference Include="System.Text.Json" Version="9.0.0" />
</ItemGroup>
```

### **2.2 Configuration de la Connexion**

Créez un fichier `AppConfig.cs` :

```csharp
namespace SmartOfficeMobile.Configuration;

public static class AppConfig
{
    // ? IMPORTANT: Remplacer par VOTRE adresse IP locale
    // Trouvez votre IP avec: ipconfig | findstr "IPv4"
    private const string BaseUrl = "http://192.168.1.100:5290"; // ? CHANGEZ ICI
    
    public static string ApiBaseUrl => BaseUrl;
    public static string SignalRHubUrl => $"{BaseUrl}/hubs/notifications";
    
    // Timeout recommandés
    public static TimeSpan HttpTimeout => TimeSpan.FromSeconds(30);
    public static TimeSpan SignalRTimeout => TimeSpan.FromSeconds(15);
}
```

### **2.3 Service API de Base**

Créez `Services/ApiService.cs` :

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartOfficeMobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl),
            Timeout = AppConfig.HttpTimeout
        };
        
        // Options de sérialisation JSON (camelCase)
        JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public JsonSerializerOptions JsonSerializerOptions { get; }

    public void SetAuthToken(string token)
    {
        _authToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GET Error: {ex.Message}");
            throw;
        }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonSerializerOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"POST Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> PostAsync<TRequest>(string endpoint, TRequest data)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data, JsonSerializerOptions);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"POST Error: {ex.Message}");
            return false;
        }
    }
}
```

---

## ?? **3. RÉCUPÉRER LES PÉRIPHÉRIQUES** {#recuperer-peripheriques}

### **3.1 Modèles de Données**

Créez `Models/DevicePeripheralEnriched.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class DevicePeripheralEnriched
{
    // =============================
    // 1. IDENTITÉ / BASE
    // =============================
    public int Id { get; set; }
    public string? Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Lighting, Climate, Security, etc.
    public Zone? Zone { get; set; }
    public int? ZoneId { get; set; }
    public bool IsActive { get; set; }
    public List<string> Capabilities { get; set; } = new();

    // =============================
    // 2. ÉTAT TEMPS RÉEL
    // =============================
    public string ConnectivityStatus { get; set; } = "Unknown"; // Online, Offline, Degraded
    public DateTime? LastUpdatedUtc { get; set; }
    public string HealthStatus { get; set; } = "Unknown"; // OK, Warning, Error
    public string? HealthMessage { get; set; }

    // =============================
    // 3. MÉTADONNÉES HARDWARE
    // =============================
    public string IotDeviceId { get; set; } = string.Empty;
    public int Gpio { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public DateTime? InstalledAt { get; set; }

    // =============================
    // 4. ÉTAT SPÉCIFIQUE PAR TYPE (dynamique)
    // =============================
    public object? TypeSpecificState { get; set; }

    // =============================
    // 5. DONNÉES ÉNERGÉTIQUES
    // =============================
    public string? Metering { get; set; }
    public float? RatedWatt { get; set; }
    public double? EnergyTodayWh { get; set; }
    public float? CurrentPowerW { get; set; }

    // =============================
    // 6. MÉTADONNÉES ADMINISTRATIVES
    // =============================
    public string Status { get; set; } = "active";
    public bool IsActionable { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DevicePeripheralsStats
{
    public int TotalDevices { get; set; }
    public int ActiveDevicesCount { get; set; }
    public int OfflineDevicesCount { get; set; }
    public int ErrorDevicesCount { get; set; }
    public int ZonesCount { get; set; }
    public Dictionary<string, int> CountsByType { get; set; } = new();
    public int ActionableDevicesCount { get; set; }
    public double TotalCurrentPowerW { get; set; }
    public double TotalEnergyTodayWh { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
}
```

### **3.2 Service de Périphériques**

Créez `Services/DevicePeripheralService.cs` :

```csharp
namespace SmartOfficeMobile.Services;

public class DevicePeripheralService
{
    private readonly ApiService _apiService;

    public DevicePeripheralService(ApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Récupère tous les périphériques enrichis
    /// </summary>
    public async Task<List<DevicePeripheralEnriched>?> GetAllAsync(bool fresh = false)
    {
        var endpoint = fresh 
            ? "/api/DevicePeripherals/enriched?fresh=true" 
            : "/api/DevicePeripherals/enriched";
            
        return await _apiService.GetAsync<List<DevicePeripheralEnriched>>(endpoint);
    }

    /// <summary>
    /// Récupère un périphérique par ID
    /// </summary>
    public async Task<DevicePeripheralEnriched?> GetByIdAsync(int id)
    {
        return await _apiService.GetAsync<DevicePeripheralEnriched>(
            $"/api/DevicePeripherals/enriched/{id}");
    }

    /// <summary>
    /// Récupère les périphériques d'une zone
    /// </summary>
    public async Task<List<DevicePeripheralEnriched>?> GetByZoneAsync(int zoneId)
    {
        return await _apiService.GetAsync<List<DevicePeripheralEnriched>>(
            $"/api/DevicePeripherals/enriched/zone/{zoneId}");
    }

    /// <summary>
    /// Récupère les périphériques d'un device IoT Hub
    /// </summary>
    public async Task<List<DevicePeripheralEnriched>?> GetByDeviceAsync(string iotDeviceId)
    {
        return await _apiService.GetAsync<List<DevicePeripheralEnriched>>(
            $"/api/DevicePeripherals/enriched/device/{iotDeviceId}");
    }

    /// <summary>
    /// Récupère les statistiques globales
    /// </summary>
    public async Task<DevicePeripheralsStats?> GetStatsAsync()
    {
        return await _apiService.GetAsync<DevicePeripheralsStats>(
            "/api/DevicePeripherals/stats");
    }
}
```

---

## ?? **4. ENVOYER DES COMMANDES** {#envoyer-commandes}

### **4.1 Modèles de Commandes**

Créez `Models/DeviceCommand.cs` :

```csharp
namespace SmartOfficeMobile.Models;

public class DeviceCommand
{
    public string DeviceId { get; set; } = string.Empty;
    public string PeripheralName { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

public class CommandResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
```

### **4.2 Service de Commandes**

Créez `Services/DeviceCommandService.cs` :

```csharp
namespace SmartOfficeMobile.Services;

public class DeviceCommandService
{
    private readonly ApiService _apiService;

    public DeviceCommandService(ApiService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Envoie une commande à un périphérique
    /// Endpoint: POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/{command}
    /// </summary>
    public async Task<bool> SendCommandAsync(
        string iotDeviceId, 
        string peripheralName, 
        string command, 
        Dictionary<string, object>? parameters = null)
    {
        var endpoint = $"/api/Devices/{iotDeviceId}/peripherals/{peripheralName}/commands/{command}";
        
        var payload = new { parameters = parameters ?? new Dictionary<string, object>() };
        
        return await _apiService.PostAsync(endpoint, payload);
    }

    // =============================
    // COMMANDES SPÉCIFIQUES
    // =============================

    /// <summary>
    /// Allumer/Éteindre un périphérique (relay, light)
    /// </summary>
    public async Task<bool> SetStateAsync(string iotDeviceId, string peripheralName, bool on)
    {
        var command = "setState";
        var parameters = new Dictionary<string, object>
        {
            { "value", on }
        };
        
        return await SendCommandAsync(iotDeviceId, peripheralName, command, parameters);
    }

    /// <summary>
    /// Ajuster la luminosité (0-100)
    /// </summary>
    public async Task<bool> SetBrightnessAsync(string iotDeviceId, string peripheralName, int brightness)
    {
        var command = "setBrightness";
        var parameters = new Dictionary<string, object>
        {
            { "value", Math.Clamp(brightness, 0, 100) }
        };
        
        return await SendCommandAsync(iotDeviceId, peripheralName, command, parameters);
    }

    /// <summary>
    /// Changer la couleur (hex #RRGGBB)
    /// </summary>
    public async Task<bool> SetColorAsync(string iotDeviceId, string peripheralName, string hexColor)
    {
        var command = "setColor";
        var parameters = new Dictionary<string, object>
        {
            { "value", hexColor }
        };
        
        return await SendCommandAsync(iotDeviceId, peripheralName, command, parameters);
    }

    /// <summary>
    /// Ajuster la température cible (Climate)
    /// </summary>
    public async Task<bool> SetTemperatureAsync(string iotDeviceId, string peripheralName, double temperature)
    {
        var command = "setTemperature";
        var parameters = new Dictionary<string, object>
        {
            { "value", temperature }
        };
        
        return await SendCommandAsync(iotDeviceId, peripheralName, command, parameters);
    }

    /// <summary>
    /// Verrouiller/Déverrouiller une porte
    /// </summary>
    public async Task<bool> SetLockAsync(string iotDeviceId, string peripheralName, bool locked)
    {
        var command = locked ? "lock" : "unlock";
        
        return await SendCommandAsync(iotDeviceId, peripheralName, command);
    }

    /// <summary>
    /// Réinitialiser les compteurs énergétiques
    /// </summary>
    public async Task<bool> ResetEnergyAsync(string iotDeviceId)
    {
        var endpoint = $"/api/Devices/{iotDeviceId}/peripherals/energy/reset";
        
        return await _apiService.PostAsync(endpoint, new { });
    }
}
```

---

## ?? **5. RECEVOIR LES NOTIFICATIONS TEMPS RÉEL (SignalR)** {#notifications-temps-reel}

### **5.1 Service SignalR**

Créez `Services/SignalRService.cs` :

```csharp
using Microsoft.AspNetCore.SignalR.Client;

namespace SmartOfficeMobile.Services;

public class SignalRService
{
    private HubConnection? _connection;
    private bool _isConnected;

    // Événements pour notifier l'UI
    public event EventHandler<string>? TelemetryReceived;
    public event EventHandler<string>? CommandExecuted;
    public event EventHandler<string>? DeviceStateChanged;

    public bool IsConnected => _isConnected;

    /// <summary>
    /// Démarre la connexion SignalR
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(AppConfig.SignalRHubUrl)
                .WithAutomaticReconnect()
                .Build();

            // Écouter les notifications de télémétrie
            _connection.On<string>("ReceiveTelemetry", (data) =>
            {
                Console.WriteLine($"Télémétrie reçue: {data}");
                TelemetryReceived?.Invoke(this, data);
            });

            // Écouter les notifications de commandes
            _connection.On<string>("ReceiveCommandResult", (data) =>
            {
                Console.WriteLine($"Résultat commande: {data}");
                CommandExecuted?.Invoke(this, data);
            });

            // Écouter les changements d'état
            _connection.On<string>("ReceiveDeviceStateUpdate", (data) =>
            {
                Console.WriteLine($"État périphérique changé: {data}");
                DeviceStateChanged?.Invoke(this, data);
            });

            await _connection.StartAsync();
            _isConnected = true;
            
            Console.WriteLine("? SignalR connecté");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur SignalR: {ex.Message}");
            _isConnected = false;
        }
    }

    /// <summary>
    /// Arrête la connexion SignalR
    /// </summary>
    public async Task StopAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
            _isConnected = false;
            
            Console.WriteLine("? SignalR déconnecté");
        }
    }
}
```

---

## ?? **6. EXEMPLES DE CODE COMPLETS** {#exemples-code}

### **6.1 ViewModel pour la Liste de Périphériques**

Créez `ViewModels/DevicesListViewModel.cs` :

```csharp
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SmartOfficeMobile.ViewModels;

public class DevicesListViewModel : BaseViewModel
{
    private readonly DevicePeripheralService _deviceService;
    private readonly SignalRService _signalRService;
    
    public ObservableCollection<DevicePeripheralEnriched> Devices { get; }
    public ICommand RefreshCommand { get; }
    public ICommand DeviceSelectedCommand { get; }

    private bool _isRefreshing;
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public DevicesListViewModel(
        DevicePeripheralService deviceService,
        SignalRService signalRService)
    {
        _deviceService = deviceService;
        _signalRService = signalRService;
        
        Devices = new ObservableCollection<DevicePeripheralEnriched>();
        
        RefreshCommand = new Command(async () => await LoadDevicesAsync());
        DeviceSelectedCommand = new Command<DevicePeripheralEnriched>(OnDeviceSelected);
        
        // Écouter les mises à jour temps réel
        _signalRService.DeviceStateChanged += OnDeviceStateChanged;
    }

    private async Task LoadDevicesAsync()
    {
        IsRefreshing = true;
        
        try
        {
            var devices = await _deviceService.GetAllAsync(fresh: false);
            
            if (devices != null)
            {
                Devices.Clear();
                foreach (var device in devices)
                {
                    Devices.Add(device);
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur", 
                $"Impossible de charger les périphériques: {ex.Message}", 
                "OK");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void OnDeviceSelected(DevicePeripheralEnriched device)
    {
        // Navigation vers la page de détails
        // await Shell.Current.GoToAsync($"devicedetails?id={device.Id}");
    }

    private void OnDeviceStateChanged(object sender, string data)
    {
        // Rafraîchir l'état en temps réel
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await LoadDevicesAsync();
        });
    }
}
```

### **6.2 ViewModel pour le Contrôle d'un Périphérique**

Créez `ViewModels/DeviceControlViewModel.cs` :

```csharp
namespace SmartOfficeMobile.ViewModels;

public class DeviceControlViewModel : BaseViewModel
{
    private readonly DevicePeripheralService _deviceService;
    private readonly DeviceCommandService _commandService;
    
    private DevicePeripheralEnriched? _device;
    public DevicePeripheralEnriched? Device
    {
        get => _device;
        set => SetProperty(ref _device, value);
    }

    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (SetProperty(ref _isOn, value))
            {
                _ = ToggleStateAsync();
            }
        }
    }

    private int _brightness;
    public int Brightness
    {
        get => _brightness;
        set
        {
            if (SetProperty(ref _brightness, value))
            {
                _ = UpdateBrightnessAsync();
            }
        }
    }

    public ICommand RefreshCommand { get; }

    public DeviceControlViewModel(
        DevicePeripheralService deviceService,
        DeviceCommandService commandService)
    {
        _deviceService = deviceService;
        _commandService = commandService;
        
        RefreshCommand = new Command(async () => await LoadDeviceAsync());
    }

    public async Task InitializeAsync(int deviceId)
    {
        await LoadDeviceAsync(deviceId);
    }

    private async Task LoadDeviceAsync(int? deviceId = null)
    {
        try
        {
            if (deviceId.HasValue)
            {
                Device = await _deviceService.GetByIdAsync(deviceId.Value);
            }
            else if (Device != null)
            {
                Device = await _deviceService.GetByIdAsync(Device.Id);
            }

            // Initialiser les contrôles selon l'état actuel
            if (Device?.TypeSpecificState is JsonElement state)
            {
                if (state.ValueKind == JsonValueKind.String)
                {
                    IsOn = state.GetString()?.ToLower() == "on";
                }
                else if (state.ValueKind == JsonValueKind.Object)
                {
                    if (state.TryGetProperty("brightness", out var brightnessElem))
                    {
                        Brightness = brightnessElem.GetInt32();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur", 
                $"Impossible de charger le périphérique: {ex.Message}", 
                "OK");
        }
    }

    private async Task ToggleStateAsync()
    {
        if (Device == null) return;

        try
        {
            var success = await _commandService.SetStateAsync(
                Device.IotDeviceId,
                Device.Name,
                IsOn);

            if (!success)
            {
                // Revenir à l'état précédent si échec
                IsOn = !IsOn;
                
                await Application.Current.MainPage.DisplayAlert(
                    "Erreur", 
                    "Impossible de changer l'état du périphérique", 
                    "OK");
            }
        }
        catch (Exception ex)
        {
            IsOn = !IsOn;
            
            await Application.Current.MainPage.DisplayAlert(
                "Erreur", 
                $"Erreur lors du changement d'état: {ex.Message}", 
                "OK");
        }
    }

    private async Task UpdateBrightnessAsync()
    {
        if (Device == null) return;

        try
        {
            await _commandService.SetBrightnessAsync(
                Device.IotDeviceId,
                Device.Name,
                Brightness);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Erreur", 
                $"Erreur lors de l'ajustement de la luminosité: {ex.Message}", 
                "OK");
        }
    }
}
```

### **6.3 Page XAML - Liste de Périphériques**

Créez `Views/DevicesListPage.xaml` :

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SmartOfficeMobile.Views.DevicesListPage"
             Title="Périphériques">
    
    <RefreshView IsRefreshing="{Binding IsRefreshing}"
                 Command="{Binding RefreshCommand}">
        <CollectionView ItemsSource="{Binding Devices}"
                        SelectionMode="Single"
                        SelectionChangedCommand="{Binding DeviceSelectedCommand}">
            
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="10,5" Padding="15" HasShadow="True">
                        <Grid ColumnDefinitions="Auto,*,Auto">
                            
                            <!-- Icône selon le type -->
                            <Image Grid.Column="0" 
                                   Source="{Binding Type, Converter={StaticResource TypeToIconConverter}}"
                                   WidthRequest="40" HeightRequest="40"
                                   Margin="0,0,15,0"/>
                            
                            <!-- Informations -->
                            <StackLayout Grid.Column="1">
                                <Label Text="{Binding Name}" 
                                       FontSize="18" FontAttributes="Bold"/>
                                <Label Text="{Binding Zone.Name}" 
                                       FontSize="14" TextColor="Gray"/>
                                <Label Text="{Binding ConnectivityStatus}"
                                       FontSize="12"
                                       TextColor="{Binding ConnectivityStatus, Converter={StaticResource StatusToColorConverter}}"/>
                            </StackLayout>
                            
                            <!-- Indicateur d'état -->
                            <BoxView Grid.Column="2"
                                     WidthRequest="20" HeightRequest="20"
                                     CornerRadius="10"
                                     BackgroundColor="{Binding IsActive, Converter={StaticResource BoolToColorConverter}}"/>
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
            
            <CollectionView.EmptyView>
                <StackLayout HorizontalOptions="Center" VerticalOptions="Center">
                    <Label Text="Aucun périphérique disponible" 
                           FontSize="18" TextColor="Gray"/>
                </StackLayout>
            </CollectionView.EmptyView>
            
        </CollectionView>
    </RefreshView>
    
</ContentPage>
```

### **6.4 Page XAML - Contrôle de Périphérique**

Créez `Views/DeviceControlPage.xaml` :

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SmartOfficeMobile.Views.DeviceControlPage"
             Title="{Binding Device.Name}">
    
    <ScrollView>
        <StackLayout Padding="20" Spacing="20">
            
            <!-- En-tête -->
            <Frame HasShadow="True" Padding="15">
                <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto,Auto">
                    <Image Grid.Row="0" Grid.Column="0" Grid.RowSpan="3"
                           Source="{Binding Device.Type, Converter={StaticResource TypeToIconConverter}}"
                           WidthRequest="60" HeightRequest="60"
                           Margin="0,0,15,0"/>
                    
                    <Label Grid.Row="0" Grid.Column="1"
                           Text="{Binding Device.Name}"
                           FontSize="24" FontAttributes="Bold"/>
                    
                    <Label Grid.Row="1" Grid.Column="1"
                           Text="{Binding Device.Zone.Name}"
                           FontSize="16" TextColor="Gray"/>
                    
                    <Label Grid.Row="2" Grid.Column="1"
                           Text="{Binding Device.ConnectivityStatus}"
                           FontSize="14"
                           TextColor="{Binding Device.ConnectivityStatus, Converter={StaticResource StatusToColorConverter}}"/>
                </Grid>
            </Frame>
            
            <!-- Contrôle ON/OFF -->
            <Frame HasShadow="True" Padding="15" IsVisible="{Binding Device.IsActionable}">
                <Grid ColumnDefinitions="*,Auto">
                    <Label Grid.Column="0" Text="État" FontSize="18" VerticalOptions="Center"/>
                    <Switch Grid.Column="1" IsToggled="{Binding IsOn}"/>
                </Grid>
            </Frame>
            
            <!-- Contrôle Luminosité (si Lighting) -->
            <Frame HasShadow="True" Padding="15" IsVisible="{Binding Device.Capabilities, Converter={StaticResource HasCapabilityConverter}, ConverterParameter='Brightness'}">
                <StackLayout Spacing="10">
                    <Label Text="Luminosité" FontSize="18"/>
                    <Slider Value="{Binding Brightness}" Minimum="0" Maximum="100"/>
                    <Label Text="{Binding Brightness, StringFormat='{0}%'}" 
                           HorizontalOptions="Center" FontSize="16"/>
                </StackLayout>
            </Frame>
            
            <!-- Informations Énergétiques -->
            <Frame HasShadow="True" Padding="15" IsVisible="{Binding Device.Metering, Converter={StaticResource IsNotNullConverter}}">
                <StackLayout Spacing="10">
                    <Label Text="Consommation Énergétique" FontSize="18" FontAttributes="Bold"/>
                    
                    <Grid ColumnDefinitions="*,Auto" RowDefinitions="Auto,Auto,Auto">
                        <Label Grid.Row="0" Grid.Column="0" Text="Puissance Actuelle"/>
                        <Label Grid.Row="0" Grid.Column="1" 
                               Text="{Binding Device.CurrentPowerW, StringFormat='{0:F2} W'}"
                               FontAttributes="Bold"/>
                        
                        <Label Grid.Row="1" Grid.Column="0" Text="Énergie Aujourd'hui"/>
                        <Label Grid.Row="1" Grid.Column="1"
                               Text="{Binding Device.EnergyTodayWh, StringFormat='{0:F2} Wh'}"
                               FontAttributes="Bold"/>
                        
                        <Label Grid.Row="2" Grid.Column="0" Text="Puissance Nominale"/>
                        <Label Grid.Row="2" Grid.Column="1"
                               Text="{Binding Device.RatedWatt, StringFormat='{0:F0} W'}"
                               FontAttributes="Bold"/>
                    </Grid>
                </StackLayout>
            </Frame>
            
            <!-- Bouton Rafraîchir -->
            <Button Text="Rafraîchir" Command="{Binding RefreshCommand}"/>
            
        </StackLayout>
    </ScrollView>
    
</ContentPage>
```

---

## ?? **7. GESTION DES ERREURS** {#gestion-erreurs}

### **7.1 Gestion des Erreurs Réseau**

```csharp
public class NetworkErrorHandler
{
    public static async Task<T?> HandleAsync<T>(Func<Task<T>> operation, string errorTitle = "Erreur")
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex)
        {
            await ShowErrorAsync(errorTitle, "Impossible de contacter le serveur. Vérifiez votre connexion.");
            Console.WriteLine($"Network error: {ex.Message}");
            return default;
        }
        catch (TaskCanceledException ex)
        {
            await ShowErrorAsync(errorTitle, "La requête a pris trop de temps. Réessayez.");
            Console.WriteLine($"Timeout: {ex.Message}");
            return default;
        }
        catch (Exception ex)
        {
            await ShowErrorAsync(errorTitle, $"Une erreur inattendue s'est produite: {ex.Message}");
            Console.WriteLine($"Error: {ex.Message}");
            return default;
        }
    }

    private static async Task ShowErrorAsync(string title, string message)
    {
        await Application.Current.MainPage.DisplayAlert(title, message, "OK");
    }
}
```

### **7.2 Retry Policy**

```csharp
public class RetryPolicy
{
    public static async Task<T?> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int delayMs = 1000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"Retry {i + 1}/{maxRetries} après erreur: {ex.Message}");
                await Task.Delay(delayMs * (i + 1)); // Exponential backoff
            }
        }

        // Dernière tentative sans catch
        return await operation();
    }
}
```

---

## ? **8. TESTS ET VALIDATION** {#tests-validation}

### **8.1 Checklist de Tests**

```
? Configuration
[ ] IP Backend correcte dans AppConfig
[ ] Pare-feu configuré (configure-firewall.ps1)
[ ] Backend démarré (dotnet run --launch-profile http)
[ ] Test connexion navigateur mobile (http://IP:5290/health/live)

? Authentification
[ ] Login fonctionne
[ ] Token JWT stocké
[ ] Token ajouté aux headers HTTP

? Récupération de Données
[ ] Liste des périphériques affichée
[ ] Détails d'un périphérique OK
[ ] Statistiques affichées
[ ] Filtrage par zone fonctionne

? Commandes
[ ] ON/OFF fonctionne (relay, light)
[ ] Ajustement luminosité fonctionne (si capable)
[ ] Ajustement température fonctionne (si Climate)
[ ] Commandes reçues par ESP32
[ ] État mis à jour après commande

? Temps Réel (SignalR)
[ ] Connexion SignalR établie
[ ] Notifications de télémétrie reçues
[ ] Notifications de commandes reçues
[ ] UI se rafraîchit automatiquement

? Gestion d'Erreurs
[ ] Erreur réseau gérée proprement
[ ] Timeout géré
[ ] Retry fonctionne
[ ] Messages d'erreur affichés
```

### **8.2 Tests Manuels**

```csharp
// Dans votre page de tests ou debug
public class DebugTestsPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly DevicePeripheralService _deviceService;
    private readonly DeviceCommandService _commandService;
    private readonly SignalRService _signalRService;

    public async Task RunAllTestsAsync()
    {
        // Test 1: Connexion API
        await TestApiConnectionAsync();
        
        // Test 2: Récupération périphériques
        await TestGetDevicesAsync();
        
        // Test 3: Envoi commande
        await TestSendCommandAsync();
        
        // Test 4: SignalR
        await TestSignalRAsync();
    }

    private async Task TestApiConnectionAsync()
    {
        Console.WriteLine("?? Test: Connexion API...");
        
        try
        {
            var response = await _apiService.GetAsync<object>("/health/live");
            Console.WriteLine("? API accessible");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur API: {ex.Message}");
        }
    }

    private async Task TestGetDevicesAsync()
    {
        Console.WriteLine("?? Test: Récupération périphériques...");
        
        try
        {
            var devices = await _deviceService.GetAllAsync();
            Console.WriteLine($"? {devices?.Count ?? 0} périphériques récupérés");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur: {ex.Message}");
        }
    }

    private async Task TestSendCommandAsync()
    {
        Console.WriteLine("?? Test: Envoi commande...");
        
        try
        {
            var success = await _commandService.SetStateAsync("esp32-office-01", "relay1", true);
            Console.WriteLine(success ? "? Commande envoyée" : "? Commande échouée");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur: {ex.Message}");
        }
    }

    private async Task TestSignalRAsync()
    {
        Console.WriteLine("?? Test: Connexion SignalR...");
        
        try
        {
            await _signalRService.StartAsync();
            Console.WriteLine(_signalRService.IsConnected ? "? SignalR connecté" : "? SignalR non connecté");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Erreur: {ex.Message}");
        }
    }
}
```

---

## ?? **RESSOURCES ADDITIONNELLES**

| Document | Description |
|----------|-------------|
| [MOBILE-README.md](MOBILE-README.md) | Guide de démarrage rapide |
| [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md) | Configuration USB détaillée |
| [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md) | Exemples de code API |
| [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) | Documentation API périphériques |
| [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) | Tests HTTP des endpoints |

---

## ?? **DÉPANNAGE**

### **Problème: "Cannot connect to API"**
? Vérifiez que le backend est démarré (`dotnet run --launch-profile http`)  
? Vérifiez l'IP dans `AppConfig.cs`  
? Testez dans le navigateur mobile: `http://IP:5290/health/live`  
? Vérifiez le pare-feu (`configure-firewall.ps1`)

### **Problème: "Unauthorized" (401)**
? Vérifiez que le token JWT est valide  
? Vérifiez que le token est ajouté aux headers (`Authorization: Bearer ...`)  
? Reconnectez-vous si le token a expiré (15 min par défaut)

### **Problème: "SignalR not connecting"**
? Vérifiez que l'URL SignalR est correcte (`http://IP:5290/hubs/notifications`)  
? Vérifiez que le backend supporte WebSocket  
? Testez la connexion HTTP normale d'abord

### **Problème: "Commands not working"**
? Vérifiez que le périphérique est `isActionable: true`  
? Vérifiez que l'ESP32 est en ligne (`connectivityStatus: Online`)  
? Vérifiez les logs backend pour voir si la commande est reçue  
? Testez avec les fichiers HTTP (Tests/29-Firmware-v2-Complete-Commands.http)

---

**? Vous êtes maintenant prêt à contrôler vos périphériques depuis MAUI !** ??

**Temps total estimé:** 2-3 heures pour l'implémentation complète  
**Niveau:** Intermédiaire  
**Support:** Consultez les fichiers de documentation dans `SmartOfficeAPP/`
