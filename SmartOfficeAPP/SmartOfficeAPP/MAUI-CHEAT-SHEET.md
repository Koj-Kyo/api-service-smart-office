# ?? MAUI SmartOffice - Cheat Sheet

## ? Configuration Express (2 min)

```bash
# 1. IP
ipconfig | findstr "IPv4"

# 2. Pare-feu (Admin)
.\configure-firewall.ps1

# 3. Backend
dotnet run --launch-profile http

# 4. Test
http://VOTRE_IP:5290/health/live
```

---

## ?? Packages NuGet

```bash
dotnet add package System.Net.Http.Json
dotnet add package Microsoft.AspNetCore.SignalR.Client
```

---

## ?? AppConfig.cs

```csharp
public static class AppConfig
{
    private const string BaseUrl = "http://192.168.1.100:5290"; // ? VOTRE IP
    
    public static string ApiBaseUrl => BaseUrl;
    public static string SignalRHubUrl => $"{BaseUrl}/hubs/notifications";
}
```

---

## ?? ApiService.cs (Minimal)

```csharp
public class ApiService
{
    private readonly HttpClient _http = new() { BaseAddress = new Uri(AppConfig.ApiBaseUrl) };

    public void SetToken(string token) =>
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public async Task<T?> GetAsync<T>(string endpoint) =>
        await _http.GetFromJsonAsync<T>(endpoint);

    public async Task<TRes?> PostAsync<TReq, TRes>(string endpoint, TReq data)
    {
        var response = await _http.PostAsJsonAsync(endpoint, data);
        return await response.Content.ReadFromJsonAsync<TRes>();
    }

    public async Task<bool> PostAsync<T>(string endpoint, T data) =>
        (await _http.PostAsJsonAsync(endpoint, data)).IsSuccessStatusCode;
}
```

---

## ?? Login

```csharp
var response = await apiService.PostAsync<LoginRequest, LoginResponse>(
    "/api/auth/login",
    new LoginRequest { Email = "admin@smartoffice.com", Password = "Admin@123" });

apiService.SetToken(response.Token);
```

---

## ?? Lister Périphériques

```csharp
// GET /api/DevicePeripherals/enriched
var devices = await apiService.GetAsync<List<Device>>("/api/DevicePeripherals/enriched");
```

---

## ?? Commandes

### ON/OFF
```csharp
await apiService.PostAsync(
    "/api/Devices/esp32-office-01/peripherals/relay1/commands/setState",
    new { parameters = new { value = true } });
```

### Luminosité
```csharp
await apiService.PostAsync(
    "/api/Devices/esp32-office-01/peripherals/led1/commands/setBrightness",
    new { parameters = new { value = 75 } });
```

### Couleur
```csharp
await apiService.PostAsync(
    "/api/Devices/esp32-office-01/peripherals/led1/commands/setColor",
    new { parameters = new { value = "#FF5733" } });
```

### Température
```csharp
await apiService.PostAsync(
    "/api/Devices/esp32-office-01/peripherals/hvac1/commands/setTemperature",
    new { parameters = new { value = 22.5 } });
```

### Lock/Unlock
```csharp
await apiService.PostAsync(
    "/api/Devices/esp32-office-01/peripherals/lock1/commands/lock",
    new { parameters = new { } });
```

---

## ?? Statistiques

```csharp
// GET /api/DevicePeripherals/stats
var stats = await apiService.GetAsync<Stats>("/api/DevicePeripherals/stats");
```

---

## ?? Gestion Erreurs

```csharp
try
{
    var result = await apiService.GetAsync<Device>("/api/DevicePeripherals/enriched/1");
}
catch (HttpRequestException)
{
    // Problème réseau
}
catch (TaskCanceledException)
{
    // Timeout
}
catch (Exception ex)
{
    // Autre erreur
}
```

---

## ?? SignalR (Optionnel)

```csharp
var hub = new HubConnectionBuilder()
    .WithUrl(AppConfig.SignalRHubUrl)
    .WithAutomaticReconnect()
    .Build();

hub.On<string>("ReceiveTelemetry", (data) => Console.WriteLine(data));
hub.On<string>("ReceiveCommandResult", (data) => Console.WriteLine(data));
hub.On<string>("ReceiveDeviceStateUpdate", (data) => Console.WriteLine(data));

await hub.StartAsync();
```

---

## ?? Modèles

```csharp
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string IotDeviceId { get; set; }
    public bool IsActive { get; set; }
    public bool IsActionable { get; set; }
    public List<string> Capabilities { get; set; }
    public Zone? Zone { get; set; }
}

public class Zone
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

---

## ? Checklist

- [ ] IP configurée dans `AppConfig`
- [ ] Backend démarré
- [ ] Pare-feu OK
- [ ] Test navigateur mobile OK
- [ ] Packages installés
- [ ] Login fonctionne
- [ ] Liste périphériques OK
- [ ] Commandes fonctionnent

---

## ?? Documentation

| Guide | Lien |
|-------|------|
| **Démarrage rapide** | [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) |
| **Guide complet** | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) |
| **Toutes les commandes** | [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) |
| **Guide développeur** | [MAUI-DEVELOPER-GUIDE.md](MAUI-DEVELOPER-GUIDE.md) |
| **Index complet** | [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md) |

---

## ?? Dépannage

| Erreur | Fix |
|--------|-----|
| Cannot connect | IP, pare-feu, backend |
| 401 Unauthorized | Token expiré, login |
| 404 Not Found | Endpoint incorrect |
| Timeout | Réseau lent, timeout |

---

**?? Temps: 5 min | Niveau: Débutant**
