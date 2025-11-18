# ?? Référence Rapide - Commandes Périphériques MAUI

## ?? **TOUTES LES COMMANDES DISPONIBLES**

### **?? Commandes Génériques**

#### **1. Set State (ON/OFF)**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setState

// Code MAUI
await deviceService.SetStateAsync("esp32-office-01", "relay1", true);

// Payload
{
  "parameters": {
    "value": true  // true = ON, false = OFF
  }
}

// Types supportés: relay, light, actuator, motor
```

---

### **?? Commandes Éclairage (Lighting)**

#### **2. Set Brightness**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setBrightness

// Code MAUI
await deviceService.SetBrightnessAsync("esp32-office-01", "led1", 75);

// Payload
{
  "parameters": {
    "value": 75  // 0-100
  }
}

// Requis: Capability "Brightness"
```

#### **3. Set Color**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setColor

// Code MAUI
await deviceService.SetColorAsync("esp32-office-01", "led1", "#FF5733");

// Payload
{
  "parameters": {
    "value": "#FF5733"  // Hex color
  }
}

// Requis: Capability "Color"
```

#### **4. Set Color Temperature**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setColorTemperature

// Code MAUI
await deviceService.SetColorTemperatureAsync("esp32-office-01", "led1", 3000);

// Payload
{
  "parameters": {
    "value": 3000  // Kelvin (2700-6500)
  }
}

// Requis: Capability "ColorTemperature"
```

---

### **??? Commandes Climatisation (Climate)**

#### **5. Set Temperature**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setTemperature

// Code MAUI
await deviceService.SetTemperatureAsync("esp32-office-01", "hvac1", 22.5);

// Payload
{
  "parameters": {
    "value": 22.5  // Celsius
  }
}

// Type: Climate
```

#### **6. Set Fan Speed**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setFanSpeed

// Code MAUI
await deviceService.SetFanSpeedAsync("esp32-office-01", "hvac1", 3);

// Payload
{
  "parameters": {
    "value": 3  // 1-5
  }
}

// Requis: Capability "FanSpeed"
```

#### **7. Set Climate Mode**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setMode

// Code MAUI
await deviceService.SetModeAsync("esp32-office-01", "hvac1", "cool");

// Payload
{
  "parameters": {
    "value": "cool"  // cool, heat, auto, fan, dry
  }
}

// Modes disponibles:
// - cool: Refroidissement
// - heat: Chauffage
// - auto: Automatique
// - fan: Ventilation seule
// - dry: Déshumidification
```

---

### **?? Commandes Sécurité (Security & Door)**

#### **8. Lock**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/lock

// Code MAUI
await deviceService.LockAsync("esp32-office-01", "lock1");

// Payload
{
  "parameters": {}
}

// Type: Door, Security
```

#### **9. Unlock**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/unlock

// Code MAUI
await deviceService.UnlockAsync("esp32-office-01", "lock1");

// Payload
{
  "parameters": {}
}

// ?? Vérifier les permissions utilisateur
```

#### **10. Arm Security**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/arm

// Code MAUI
await deviceService.ArmAsync("esp32-office-01", "alarm1", "full");

// Payload
{
  "parameters": {
    "mode": "full"  // full, partial, away, home
  }
}

// Modes:
// - full: Armé complet
// - partial: Armé partiel
// - away: Mode absent
// - home: Mode présent
```

#### **11. Disarm Security**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/disarm

// Code MAUI
await deviceService.DisarmAsync("esp32-office-01", "alarm1", "1234");

// Payload
{
  "parameters": {
    "code": "1234"  // Code de sécurité
  }
}

// ?? Requiert code PIN
```

---

### **?? Commandes Multimédia (Media)**

#### **12. Set Volume**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setVolume

// Code MAUI
await deviceService.SetVolumeAsync("esp32-office-01", "speaker1", 60);

// Payload
{
  "parameters": {
    "value": 60  // 0-100
  }
}

// Type: Media
```

#### **13. Play/Pause**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/playPause

// Code MAUI
await deviceService.PlayPauseAsync("esp32-office-01", "speaker1");

// Payload
{
  "parameters": {}
}
```

#### **14. Mute/Unmute**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/mute

// Code MAUI
await deviceService.MuteAsync("esp32-office-01", "speaker1", true);

// Payload
{
  "parameters": {
    "value": true  // true = mute, false = unmute
  }
}
```

---

### **?? Commandes Robot Aspirateur (Cleaner)**

#### **15. Start Cleaning**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/start

// Code MAUI
await deviceService.StartCleaningAsync("esp32-office-01", "vacuum1", "full");

// Payload
{
  "parameters": {
    "mode": "full"  // full, quick, spot, edge, mapping
  }
}

// Modes:
// - full: Nettoyage complet
// - quick: Nettoyage rapide
// - spot: Nettoyage localisé
// - edge: Nettoyage bordures
// - mapping: Cartographie
```

#### **16. Stop Cleaning**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/stop

// Code MAUI
await deviceService.StopCleaningAsync("esp32-office-01", "vacuum1");

// Payload
{
  "parameters": {}
}
```

#### **17. Return to Dock**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/dock

// Code MAUI
await deviceService.ReturnToDockAsync("esp32-office-01", "vacuum1");

// Payload
{
  "parameters": {}
}
```

---

### **? Commandes Énergétiques**

#### **18. Reset Energy Counters**
```csharp
// Endpoint
POST /api/Devices/{iotDeviceId}/peripherals/energy/reset

// Code MAUI
await deviceService.ResetEnergyAsync("esp32-office-01");

// Payload
{
  "parameters": {}
}

// ?? Réinitialise tous les compteurs énergétiques du device
```

---

## ?? **IMPLÉMENTATION DANS MAUI**

### **Service Complet avec Toutes les Commandes**

```csharp
namespace SmartOfficeMobile.Services;

public class DeviceCommandService
{
    private readonly ApiService _api;

    public DeviceCommandService(ApiService api) => _api = api;

    // Helper pour envoyer une commande
    private async Task<bool> SendAsync(string deviceId, string name, string cmd, object? parameters = null)
    {
        var endpoint = $"/api/Devices/{deviceId}/peripherals/{name}/commands/{cmd}";
        return await _api.PostAsync(endpoint, new { parameters = parameters ?? new { } });
    }

    // =============================
    // COMMANDES GÉNÉRIQUES
    // =============================
    public async Task<bool> SetStateAsync(string deviceId, string name, bool on) =>
        await SendAsync(deviceId, name, "setState", new { value = on });

    // =============================
    // ÉCLAIRAGE
    // =============================
    public async Task<bool> SetBrightnessAsync(string deviceId, string name, int brightness) =>
        await SendAsync(deviceId, name, "setBrightness", new { value = Math.Clamp(brightness, 0, 100) });

    public async Task<bool> SetColorAsync(string deviceId, string name, string hexColor) =>
        await SendAsync(deviceId, name, "setColor", new { value = hexColor });

    public async Task<bool> SetColorTemperatureAsync(string deviceId, string name, int kelvin) =>
        await SendAsync(deviceId, name, "setColorTemperature", new { value = Math.Clamp(kelvin, 2700, 6500) });

    // =============================
    // CLIMATISATION
    // =============================
    public async Task<bool> SetTemperatureAsync(string deviceId, string name, double temp) =>
        await SendAsync(deviceId, name, "setTemperature", new { value = temp });

    public async Task<bool> SetFanSpeedAsync(string deviceId, string name, int speed) =>
        await SendAsync(deviceId, name, "setFanSpeed", new { value = Math.Clamp(speed, 1, 5) });

    public async Task<bool> SetModeAsync(string deviceId, string name, string mode) =>
        await SendAsync(deviceId, name, "setMode", new { value = mode });

    // =============================
    // SÉCURITÉ
    // =============================
    public async Task<bool> LockAsync(string deviceId, string name) =>
        await SendAsync(deviceId, name, "lock");

    public async Task<bool> UnlockAsync(string deviceId, string name) =>
        await SendAsync(deviceId, name, "unlock");

    public async Task<bool> ArmAsync(string deviceId, string name, string mode) =>
        await SendAsync(deviceId, name, "arm", new { mode });

    public async Task<bool> DisarmAsync(string deviceId, string name, string code) =>
        await SendAsync(deviceId, name, "disarm", new { code });

    // =============================
    // MULTIMÉDIA
    // =============================
    public async Task<bool> SetVolumeAsync(string deviceId, string name, int volume) =>
        await SendAsync(deviceId, name, "setVolume", new { value = Math.Clamp(volume, 0, 100) });

    public async Task<bool> PlayPauseAsync(string deviceId, string name) =>
        await SendAsync(deviceId, name, "playPause");

    public async Task<bool> MuteAsync(string deviceId, string name, bool mute) =>
        await SendAsync(deviceId, name, "mute", new { value = mute });

    // =============================
    // ROBOT ASPIRATEUR
    // =============================
    public async Task<bool> StartCleaningAsync(string deviceId, string name, string mode) =>
        await SendAsync(deviceId, name, "start", new { mode });

    public async Task<bool> StopCleaningAsync(string deviceId, string name) =>
        await SendAsync(deviceId, name, "stop");

    public async Task<bool> ReturnToDockAsync(string deviceId, string name) =>
        await SendAsync(deviceId, name, "dock");

    // =============================
    // ÉNERGÉTIQUE
    // =============================
    public async Task<bool> ResetEnergyAsync(string deviceId)
    {
        var endpoint = $"/api/Devices/{deviceId}/peripherals/energy/reset";
        return await _api.PostAsync(endpoint, new { });
    }
}
```

---

## ?? **EXEMPLES D'UTILISATION PAR SCÉNARIO**

### **Scénario 1: Contrôle Lumière Simple**
```csharp
var service = new DeviceCommandService(apiService);

// Allumer
await service.SetStateAsync("esp32-office-01", "relay1", true);

// Régler luminosité à 75%
await service.SetBrightnessAsync("esp32-office-01", "relay1", 75);

// Éteindre
await service.SetStateAsync("esp32-office-01", "relay1", false);
```

### **Scénario 2: Contrôle Climatisation**
```csharp
// Mettre en mode refroidissement à 22°C
await service.SetModeAsync("esp32-office-01", "hvac1", "cool");
await service.SetTemperatureAsync("esp32-office-01", "hvac1", 22.0);
await service.SetFanSpeedAsync("esp32-office-01", "hvac1", 3);
```

### **Scénario 3: Verrouillage Sécurisé**
```csharp
// Verrouiller la porte
await service.LockAsync("esp32-office-01", "lock1");

// Armer l'alarme
await service.ArmAsync("esp32-office-01", "alarm1", "full");
```

### **Scénario 4: Contrôle Robot Aspirateur**
```csharp
// Démarrer nettoyage complet
await service.StartCleaningAsync("esp32-office-01", "vacuum1", "full");

// Attendre...

// Retour à la base
await service.ReturnToDockAsync("esp32-office-01", "vacuum1");
```

---

## ? **VALIDATION DES COMMANDES**

### **Avant d'envoyer une commande, vérifier:**

```csharp
public static bool CanExecuteCommand(Device device, string command)
{
    // 1. Device doit être actif
    if (!device.IsActive) return false;
    
    // 2. Device doit être actionnable
    if (!device.IsActionable) return false;
    
    // 3. Device doit être en ligne
    if (device.ConnectivityStatus != "Online") return false;
    
    // 4. Capability spécifique si nécessaire
    switch (command)
    {
        case "setBrightness":
            return device.Capabilities.Contains("Brightness");
        case "setColor":
            return device.Capabilities.Contains("Color");
        case "setTemperature":
            return device.Type == "Climate";
        case "lock":
        case "unlock":
            return device.Capabilities.Contains("Lock");
        default:
            return true;
    }
}
```

---

## ?? **GESTION D'ERREURS**

```csharp
public async Task<bool> SafeSendCommandAsync(string deviceId, string name, string command)
{
    try
    {
        // Vérifier la connexion
        if (!await CheckConnectionAsync())
        {
            await ShowErrorAsync("Pas de connexion au serveur");
            return false;
        }

        // Envoyer la commande
        var success = await _commandService.SetStateAsync(deviceId, name, true);

        if (!success)
        {
            await ShowErrorAsync("La commande a échoué");
            return false;
        }

        // Attendre confirmation (optionnel)
        await Task.Delay(1000);
        await RefreshDeviceStateAsync(deviceId, name);

        return true;
    }
    catch (Exception ex)
    {
        await ShowErrorAsync($"Erreur: {ex.Message}");
        return false;
    }
}
```

---

## ?? **RESSOURCES**

- [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Guide complet
- [Tests/29-Firmware-v2-Complete-Commands.http](Tests/29-Firmware-v2-Complete-Commands.http) ? Tests HTTP
- [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) ? Documentation API

---

**?? Référence complète des commandes SmartOffice Mobile**  
**Dernière mise à jour:** 2025-01-15  
**Version:** v1.0
