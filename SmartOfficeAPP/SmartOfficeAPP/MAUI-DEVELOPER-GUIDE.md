# ?? SmartOffice MAUI - Guide Développeur Complet

## ?? **VUE D'ENSEMBLE**

Ce guide vous permet de créer une application mobile MAUI pour contrôler les périphériques SmartOffice (lumières, climatisation, sécurité, etc.).

### **Ce que vous allez apprendre :**
? Connecter l'app MAUI à l'API backend  
? Lister et afficher les périphériques  
? Envoyer des commandes (ON/OFF, luminosité, etc.)  
? Recevoir les notifications temps réel (SignalR)  
? Gérer les erreurs et la résilience

### **Prérequis :**
- .NET MAUI ou Xamarin.Forms
- Visual Studio 2022
- Téléphone Android/iOS connecté en USB
- PC et téléphone sur le même réseau WiFi

---

## ?? **DOCUMENTATION DISPONIBLE**

### **1. Configuration Réseau (10 min)**

| Document | Objectif |
|----------|----------|
| [MOBILE-README.md](MOBILE-README.md) | **START HERE** - Configuration IP et pare-feu |
| [MOBILE-SETUP-EXPRESS.md](MOBILE-SETUP-EXPRESS.md) | Commandes rapides (copier-coller) |
| [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md) | Guide complet avec troubleshooting |

**Résultat attendu :**
```
? Backend démarré sur http://192.168.1.100:5290
? Pare-feu configuré (ports 5290, 7144)
? Test navigateur mobile OK
```

---

### **2. Contrôle des Périphériques (15 min)**

| Document | Objectif |
|----------|----------|
| [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) | **Code minimal** - Démarrer en 5 min |
| [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) | **Guide complet** - Architecture, SignalR, tests |
| [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) | **Référence** - Toutes les commandes disponibles |

**Résultat attendu :**
```csharp
// Liste des périphériques
var devices = await deviceService.GetAllAsync();

// Contrôle ON/OFF
await deviceService.SetStateAsync("esp32-office-01", "relay1", true);

// Ajuster luminosité
await deviceService.SetBrightnessAsync("esp32-office-01", "led1", 75);
```

---

### **3. Exemples de Code**

| Document | Objectif |
|----------|----------|
| [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md) | Exemples ApiService |
| [MAUI-ApiService-Example.cs](MAUI-ApiService-Example.cs) | Classe complète |

---

### **4. Tests et Validation**

| Fichier | Objectif |
|---------|----------|
| [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) | Tests API de base |
| [Tests/39-Mobile-Devices-Enriched-Tests.http](Tests/39-Mobile-Devices-Enriched-Tests.http) | Tests complets avec validation |
| [Tests/29-Firmware-v2-Complete-Commands.http](Tests/29-Firmware-v2-Complete-Commands.http) | Tests des commandes |

---

## ??? **ARCHITECTURE DE L'APPLICATION**

```
??????????????????????????????????????????
?         APP MOBILE MAUI                ?
?                                        ?
?  ????????????????????????????????????  ?
?  ?         Views (XAML)             ?  ?
?  ?  - DevicesListPage.xaml          ?  ?
?  ?  - DeviceControlPage.xaml        ?  ?
?  ?  - StatsPage.xaml                ?  ?
?  ????????????????????????????????????  ?
?             ?                          ?
?  ????????????????????????????????????  ?
?  ?      ViewModels (MVVM)           ?  ?
?  ?  - DevicesListViewModel          ?  ?
?  ?  - DeviceControlViewModel        ?  ?
?  ????????????????????????????????????  ?
?             ?                          ?
?  ????????????????????????????????????  ?
?  ?         Services                 ?  ?
?  ?  - ApiService (HTTP)             ?  ?
?  ?  - DevicePeripheralService       ?  ?
?  ?  - DeviceCommandService          ?  ?
?  ?  - SignalRService (Temps réel)   ?  ?
?  ????????????????????????????????????  ?
?             ?                          ?
?  ????????????????????????????????????  ?
?  ?         Models                   ?  ?
?  ?  - DevicePeripheralEnriched      ?  ?
?  ?  - Zone                          ?  ?
?  ?  - DeviceCommand                 ?  ?
?  ????????????????????????????????????  ?
??????????????????????????????????????????
             ?
             ? HTTP REST + WebSocket
             ?
??????????????????????????????????????????
?       BACKEND API (.NET 9)             ?
?   https://192.168.1.100:5290           ?
??????????????????????????????????????????
             ?
             ? IoT Hub Commands
             ?
??????????????????????????????????????????
?       FIRMWARE ESP32                   ?
?   - Périphériques physiques            ?
??????????????????????????????????????????
```

---

## ?? **DÉMARRAGE RAPIDE (30 MIN)**

### **ÉTAPE 1: Configuration Réseau (5 min)**

```bash
# 1. Trouver votre IP
ipconfig | findstr "IPv4"
# Résultat exemple: 192.168.1.100

# 2. Configurer le pare-feu (PowerShell Admin)
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
.\configure-firewall.ps1

# 3. Démarrer le backend
dotnet run --launch-profile http

# 4. Tester dans navigateur mobile
# http://192.168.1.100:5290/health/live
```

**? Checkpoint :** Le navigateur mobile affiche `{"status":"Healthy"}`

---

### **ÉTAPE 2: Configuration MAUI (10 min)**

#### **2.1 Installer les packages**

```bash
dotnet add package System.Net.Http.Json
dotnet add package Microsoft.AspNetCore.SignalR.Client
```

#### **2.2 Créer AppConfig.cs**

```csharp
namespace SmartOfficeMobile.Configuration;

public static class AppConfig
{
    // ? CHANGEZ CETTE IP
    private const string BaseUrl = "http://192.168.1.100:5290";
    
    public static string ApiBaseUrl => BaseUrl;
    public static string SignalRHubUrl => $"{BaseUrl}/hubs/notifications";
}
```

#### **2.3 Créer ApiService.cs**

```csharp
namespace SmartOfficeMobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(AppConfig.ApiBaseUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<bool> PostAsync<T>(string endpoint, T data)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, data);
        return response.IsSuccessStatusCode;
    }
}
```

**? Checkpoint :** ApiService compile sans erreur

---

### **ÉTAPE 3: Authentification (5 min)**

#### **3.1 Modèle de Login**

```csharp
namespace SmartOfficeMobile.Models;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

#### **3.2 Service d'Authentification**

```csharp
namespace SmartOfficeMobile.Services;

public class AuthService
{
    private readonly ApiService _api;

    public AuthService(ApiService api) => _api = api;

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };
        
        var response = await _api.PostAsync<LoginRequest, LoginResponse>(
            "/api/auth/login", 
            request);

        if (response != null)
        {
            _api.SetToken(response.Token);
            
            // Sauvegarder le token (Preferences.Set)
            Preferences.Set("auth_token", response.Token);
        }

        return response;
    }
}
```

#### **3.3 Test Login**

```csharp
// Dans votre page de login
var authService = new AuthService(apiService);

var result = await authService.LoginAsync(
    "admin@smartoffice.com", 
    "Admin@123");

if (result != null)
{
    // Login réussi
    await Shell.Current.GoToAsync("//devices");
}
```

**? Checkpoint :** Login fonctionne, token reçu

---

### **ÉTAPE 4: Lister les Périphériques (10 min)**

#### **4.1 Modèle Device**

```csharp
namespace SmartOfficeMobile.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string IotDeviceId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsActionable { get; set; }
    public string ConnectivityStatus { get; set; } = "Unknown";
    public List<string> Capabilities { get; set; } = new();
    public Zone? Zone { get; set; }
    public object? TypeSpecificState { get; set; }
}

public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

#### **4.2 Service Device**

```csharp
namespace SmartOfficeMobile.Services;

public class DeviceService
{
    private readonly ApiService _api;

    public DeviceService(ApiService api) => _api = api;

    public async Task<List<Device>?> GetAllAsync() =>
        await _api.GetAsync<List<Device>>("/api/DevicePeripherals/enriched");

    public async Task<Device?> GetByIdAsync(int id) =>
        await _api.GetAsync<Device>($"/api/DevicePeripherals/enriched/{id}");

    public async Task<List<Device>?> GetByZoneAsync(int zoneId) =>
        await _api.GetAsync<List<Device>>($"/api/DevicePeripherals/enriched/zone/{zoneId}");
}
```

#### **4.3 ViewModel**

```csharp
namespace SmartOfficeMobile.ViewModels;

public class DevicesViewModel : INotifyPropertyChanged
{
    private readonly DeviceService _deviceService;
    
    public ObservableCollection<Device> Devices { get; } = new();
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand LoadCommand { get; }

    public DevicesViewModel(DeviceService deviceService)
    {
        _deviceService = deviceService;
        LoadCommand = new Command(async () => await LoadAsync());
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        
        try
        {
            var devices = await _deviceService.GetAllAsync();
            
            Devices.Clear();
            foreach (var device in devices ?? [])
            {
                Devices.Add(device);
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
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
```

#### **4.4 XAML - Liste**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SmartOfficeMobile.Views.DevicesPage"
             Title="Périphériques">
    
    <RefreshView IsRefreshing="{Binding IsLoading}"
                 Command="{Binding LoadCommand}">
        <CollectionView ItemsSource="{Binding Devices}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="10" Padding="15" HasShadow="True">
                        <Grid ColumnDefinitions="*,Auto">
                            <StackLayout Grid.Column="0">
                                <Label Text="{Binding Name}" 
                                       FontSize="18" 
                                       FontAttributes="Bold"/>
                                <Label Text="{Binding Zone.Name}" 
                                       FontSize="14" 
                                       TextColor="Gray"/>
                                <Label Text="{Binding Type}" 
                                       FontSize="12"/>
                            </StackLayout>
                            
                            <BoxView Grid.Column="1"
                                     WidthRequest="20" 
                                     HeightRequest="20"
                                     CornerRadius="10"
                                     BackgroundColor="{Binding IsActive, Converter={StaticResource BoolToColorConverter}}"/>
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </RefreshView>
    
</ContentPage>
```

**? Checkpoint :** Liste des périphériques affichée

---

### **ÉTAPE 5: Contrôler un Périphérique (10 min)**

#### **5.1 Service de Commandes**

```csharp
namespace SmartOfficeMobile.Services;

public class DeviceCommandService
{
    private readonly ApiService _api;

    public DeviceCommandService(ApiService api) => _api = api;

    public async Task<bool> SetStateAsync(string deviceId, string name, bool on)
    {
        var endpoint = $"/api/Devices/{deviceId}/peripherals/{name}/commands/setState";
        var payload = new { parameters = new { value = on } };
        return await _api.PostAsync(endpoint, payload);
    }

    public async Task<bool> SetBrightnessAsync(string deviceId, string name, int brightness)
    {
        var endpoint = $"/api/Devices/{deviceId}/peripherals/{name}/commands/setBrightness";
        var payload = new { parameters = new { value = brightness } };
        return await _api.PostAsync(endpoint, payload);
    }
}
```

#### **5.2 Utilisation**

```csharp
var commandService = new DeviceCommandService(apiService);

// Allumer
await commandService.SetStateAsync("esp32-office-01", "relay1", true);

// Régler luminosité
await commandService.SetBrightnessAsync("esp32-office-01", "led1", 75);

// Éteindre
await commandService.SetStateAsync("esp32-office-01", "relay1", false);
```

**? Checkpoint :** Commandes ON/OFF fonctionnent

---

## ? **CHECKLIST COMPLÈTE**

### **Configuration**
- [ ] IP Backend trouvée
- [ ] Pare-feu configuré
- [ ] Backend démarré (`dotnet run --launch-profile http`)
- [ ] Test navigateur mobile OK (`http://IP:5290/health/live`)
- [ ] IP dans `AppConfig.BaseUrl`

### **Code MAUI**
- [ ] Packages NuGet installés
- [ ] `AppConfig.cs` créé
- [ ] `ApiService.cs` créé
- [ ] `AuthService.cs` créé
- [ ] `DeviceService.cs` créé
- [ ] `DeviceCommandService.cs` créé
- [ ] Modèles créés (`Device`, `Zone`, etc.)

### **Fonctionnalités**
- [ ] Login fonctionne
- [ ] Token JWT stocké
- [ ] Liste périphériques affichée
- [ ] Détails périphérique affichés
- [ ] Commande ON/OFF fonctionne
- [ ] Commande luminosité fonctionne (si capable)
- [ ] Gestion d'erreurs implémentée

### **Optionnel (Avancé)**
- [ ] SignalR connecté
- [ ] Notifications temps réel reçues
- [ ] UI se rafraîchit automatiquement
- [ ] Retry policy implémentée
- [ ] Cache local implémenté

---

## ?? **PROCHAINES ÉTAPES**

1. **Ajouter SignalR pour le temps réel**
   ? [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) - Section 5

2. **Implémenter toutes les commandes**
   ? [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md)

3. **Ajouter les statistiques**
   ? Endpoint: `GET /api/DevicePeripherals/stats`

4. **Implémenter la gestion d'erreurs robuste**
   ? [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) - Section 7

---

## ?? **SUPPORT**

| Problème | Solution |
|----------|----------|
| Cannot connect to backend | Vérifier IP, pare-feu, backend démarré |
| Unauthorized (401) | Token expiré, relancer login |
| Commands not working | Vérifier `isActionable`, ESP32 en ligne |
| Liste vide | Vérifier authentification, données en DB |

**Documentation complète :** [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md)

---

**?? Vous êtes maintenant prêt à développer votre app mobile SmartOffice !**

**Temps total estimé :** 30-60 minutes  
**Niveau :** Intermédiaire  
**Technologies :** .NET MAUI, REST API, SignalR, MVVM
