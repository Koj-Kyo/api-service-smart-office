# ? Démarrage Rapide - Contrôle Périphériques MAUI

## ?? En 5 Minutes

### **1. Configuration IP (1 min)**

```csharp
// AppConfig.cs
public static class AppConfig
{
    private const string BaseUrl = "http://192.168.1.100:5290"; // ? VOTRE IP ICI
    
    public static string ApiBaseUrl => BaseUrl;
    public static string SignalRHubUrl => $"{BaseUrl}/hubs/notifications";
}
```

**Trouver votre IP:**
```powershell
ipconfig | findstr "IPv4"
```

---

### **2. Packages NuGet (1 min)**

```bash
dotnet add package System.Net.Http.Json
dotnet add package Microsoft.AspNetCore.SignalR.Client
```

---

### **3. Code Minimal (3 min)**

#### **Services/ApiService.cs**
```csharp
public class ApiService
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(AppConfig.ApiBaseUrl)
    };

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint) =>
        await _httpClient.GetFromJsonAsync<T>(endpoint);

    public async Task<bool> PostAsync<T>(string endpoint, T data)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        return response.IsSuccessStatusCode;
    }
}
```

#### **Services/DeviceService.cs**
```csharp
public class DeviceService
{
    private readonly ApiService _api;

    public DeviceService(ApiService api) => _api = api;

    // Récupérer tous les périphériques
    public async Task<List<Device>?> GetAllAsync() =>
        await _api.GetAsync<List<Device>>("/api/DevicePeripherals/enriched");

    // Allumer/Éteindre
    public async Task<bool> SetStateAsync(string deviceId, string name, bool on)
    {
        var endpoint = $"/api/Devices/{deviceId}/peripherals/{name}/commands/setState";
        return await _api.PostAsync(endpoint, new { parameters = new { value = on } });
    }

    // Ajuster luminosité
    public async Task<bool> SetBrightnessAsync(string deviceId, string name, int brightness)
    {
        var endpoint = $"/api/Devices/{deviceId}/peripherals/{name}/commands/setBrightness";
        return await _api.PostAsync(endpoint, new { parameters = new { value = brightness } });
    }
}
```

#### **Models/Device.cs**
```csharp
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string IotDeviceId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsActionable { get; set; }
    public List<string> Capabilities { get; set; } = new();
    public object? TypeSpecificState { get; set; }
}
```

#### **ViewModel**
```csharp
public class DevicesViewModel
{
    private readonly DeviceService _deviceService;
    public ObservableCollection<Device> Devices { get; } = new();

    public DevicesViewModel(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    public async Task LoadAsync()
    {
        var devices = await _deviceService.GetAllAsync();
        Devices.Clear();
        foreach (var device in devices ?? [])
        {
            Devices.Add(device);
        }
    }

    public async Task ToggleAsync(Device device)
    {
        var currentState = device.TypeSpecificState?.ToString() == "on";
        await _deviceService.SetStateAsync(
            device.IotDeviceId, 
            device.Name, 
            !currentState);
    }
}
```

#### **XAML - Liste Simple**
```xml
<CollectionView ItemsSource="{Binding Devices}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Frame Margin="10">
                <Grid ColumnDefinitions="*,Auto">
                    <Label Grid.Column="0" Text="{Binding Name}" FontSize="18"/>
                    <Switch Grid.Column="1" 
                            IsToggled="{Binding IsActive}"
                            Toggled="OnToggled"/>
                </Grid>
            </Frame>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

---

## ? Tester Immédiatement

### **1. Démarrer le Backend**
```bash
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
dotnet run --launch-profile http
```

### **2. Tester dans Navigateur Mobile**
```
http://192.168.1.100:5290/health/live
```

### **3. Dans votre App MAUI**
```csharp
// App.xaml.cs - Lors du démarrage
var apiService = new ApiService();
var deviceService = new DeviceService(apiService);

// Login d'abord
var loginResponse = await apiService.PostAsync<LoginRequest, LoginResponse>(
    "/api/auth/login",
    new LoginRequest { Email = "admin@smartoffice.com", Password = "Admin@123" });

apiService.SetToken(loginResponse.Token);

// Charger les devices
var devices = await deviceService.GetAllAsync();
```

---

## ?? Commandes Disponibles

| Action | Endpoint | Payload |
|--------|----------|---------|
| **ON/OFF** | `/api/Devices/{id}/peripherals/{name}/commands/setState` | `{ "parameters": { "value": true } }` |
| **Luminosité** | `/api/Devices/{id}/peripherals/{name}/commands/setBrightness` | `{ "parameters": { "value": 75 } }` |
| **Couleur** | `/api/Devices/{id}/peripherals/{name}/commands/setColor` | `{ "parameters": { "value": "#FF5733" } }` |
| **Température** | `/api/Devices/{id}/peripherals/{name}/commands/setTemperature` | `{ "parameters": { "value": 22.5 } }` |
| **Lock/Unlock** | `/api/Devices/{id}/peripherals/{name}/commands/lock` | `{}` |

---

## ?? Exemples d'Utilisation

### **Allumer une lumière**
```csharp
await deviceService.SetStateAsync("esp32-office-01", "relay1", true);
```

### **Régler luminosité à 80%**
```csharp
await deviceService.SetBrightnessAsync("esp32-office-01", "led1", 80);
```

### **Verrouiller une porte**
```csharp
var endpoint = "/api/Devices/esp32-office-01/peripherals/lock1/commands/lock";
await apiService.PostAsync(endpoint, new { });
```

---

## ?? Dépannage Express

| Problème | Solution |
|----------|----------|
| "Cannot connect" | Vérifier IP dans `AppConfig`, backend démarré, pare-feu OK |
| "Unauthorized" | Token JWT expiré, relancer le login |
| "Command not working" | Vérifier `isActionable: true`, ESP32 en ligne |

---

## ?? Documentation Complète

Pour plus de détails, consultez :
- [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Guide complet avec SignalR, gestion d'erreurs, etc.
- [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) ? Documentation API complète
- [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) ? Tests HTTP

---

**?? C'est tout ! Vous pouvez maintenant contrôler vos périphériques depuis MAUI !**

**Temps total:** 5 minutes ??
