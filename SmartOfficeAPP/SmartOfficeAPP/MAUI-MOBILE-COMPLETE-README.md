# ?? Documentation Complète - SmartOffice Mobile (MAUI)

## ?? **OBJECTIF**

Cette documentation vous permet de créer une application mobile **MAUI/Xamarin** pour contrôler tous les périphériques SmartOffice (lumières, climatisation, sécurité, robots, etc.) avec des notifications temps réel.

---

## ?? **DÉMARRAGE ULTRA-RAPIDE (5 MIN)**

### **1. Configuration Réseau**
```bash
ipconfig | findstr "IPv4"  # Noter votre IP
.\configure-firewall.ps1   # Configurer pare-feu (Admin)
dotnet run --launch-profile http  # Démarrer backend
```

### **2. Test Connexion**
Navigateur mobile : `http://VOTRE_IP:5290/health/live`

### **3. Code MAUI Minimal**
```csharp
// AppConfig.cs
private const string BaseUrl = "http://192.168.1.100:5290"; // ? VOTRE IP

// Login
var response = await apiService.PostAsync<LoginRequest, LoginResponse>(
    "/api/auth/login",
    new { Email = "admin@smartoffice.com", Password = "Admin@123" });

// Lister périphériques
var devices = await apiService.GetAsync<List<Device>>("/api/DevicePeripherals/enriched");

// Contrôler (ON/OFF)
await apiService.PostAsync(
    "/api/Devices/esp32-office-01/peripherals/relay1/commands/setState",
    new { parameters = new { value = true } });
```

**?? Détails :** [MAUI-CHEAT-SHEET.md](MAUI-CHEAT-SHEET.md)

---

## ?? **DOCUMENTATION PAR NIVEAU**

### **?? DÉBUTANT - Je veux juste que ça marche (10 min)**

| Ordre | Document | Temps | Description |
|-------|----------|-------|-------------|
| 1 | [MOBILE-README.md](MOBILE-README.md) | 5 min | Configuration réseau |
| 2 | [MAUI-CHEAT-SHEET.md](MAUI-CHEAT-SHEET.md) | 2 min | Code minimal copier-coller |
| 3 | [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) | 5 min | Contrôle périphériques |

**? Résultat :** App mobile fonctionnelle avec liste + contrôle ON/OFF

---

### **?? INTERMÉDIAIRE - Je veux tout comprendre (1h)**

| Ordre | Document | Temps | Description |
|-------|----------|-------|-------------|
| 1 | [MAUI-DEVELOPER-GUIDE.md](MAUI-DEVELOPER-GUIDE.md) | 30 min | Guide complet avec architecture |
| 2 | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) | 20 min | SignalR, gestion erreurs, tests |
| 3 | [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) | 10 min | Toutes les commandes disponibles |

**? Résultat :** App complète avec temps réel, retry, cache, etc.

---

### **?? AVANCÉ - Je veux personnaliser (2h+)**

| Document | Description |
|----------|-------------|
| [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) | Architecture, patterns, best practices |
| [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) | Documentation API complète |
| [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) | Tests API pour validation |
| [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) | Référence complète des commandes |

**? Résultat :** App production-ready avec toutes les features

---

## ?? **INDEX COMPLET DE LA DOCUMENTATION**

### **Configuration & Connexion**

| Document | Description | Temps |
|----------|-------------|-------|
| [MOBILE-README.md](MOBILE-README.md) | **START HERE** - Vue d'ensemble | 5 min |
| [MOBILE-SETUP-EXPRESS.md](MOBILE-SETUP-EXPRESS.md) | Commandes rapides (copier-coller) | 2 min |
| [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md) | Guide complet avec troubleshooting | 15 min |
| [MOBILE-SETUP-VISUAL-GUIDE.md](MOBILE-SETUP-VISUAL-GUIDE.md) | Guide visuel avec captures | 10 min |
| [MOBILE-CONFIGURATION-INDEX.md](MOBILE-CONFIGURATION-INDEX.md) | Index configuration réseau | - |

### **Développement MAUI**

| Document | Description | Temps |
|----------|-------------|-------|
| [MAUI-CHEAT-SHEET.md](MAUI-CHEAT-SHEET.md) | **Référence ultra-rapide** | 2 min |
| [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) | **Démarrage rapide** | 5 min |
| [MAUI-DEVELOPER-GUIDE.md](MAUI-DEVELOPER-GUIDE.md) | **Guide développeur complet** | 30 min |
| [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) | Guide technique avancé | 60 min |
| [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) | Référence complète des commandes | 10 min |

### **Exemples de Code**

| Document | Description |
|----------|-------------|
| [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md) | Exemples ApiService |
| [MAUI-ApiService-Example.cs](MAUI-ApiService-Example.cs) | Classe complète |

### **Tests & Validation**

| Document | Description |
|----------|-------------|
| [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) | Tests API de base |
| [Tests/39-Mobile-Devices-Enriched-Tests.http](Tests/39-Mobile-Devices-Enriched-Tests.http) | Tests complets avec validation |
| [Tests/40-Mobile-Device-Connection-Tests.http](Tests/40-Mobile-Device-Connection-Tests.http) | Tests connexion mobile |
| [Tests/29-Firmware-v2-Complete-Commands.http](Tests/29-Firmware-v2-Complete-Commands.http) | Tests commandes firmware |

### **API Backend**

| Document | Description |
|----------|-------------|
| [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) | Documentation API périphériques |
| [API-COMPLETE-DOCUMENTATION.md](API-COMPLETE-DOCUMENTATION.md) | Documentation API complète |

### **Index & Navigation**

| Document | Description |
|----------|-------------|
| [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md) | **Index complet mobile** |
| [MOBILE-CONFIGURATION-INDEX.md](MOBILE-CONFIGURATION-INDEX.md) | Index configuration |
| **?? CE FICHIER** | Documentation complète |

---

## ?? **PARCOURS RECOMMANDÉS**

### **Parcours 1 : Je démarre de zéro (15 min)**
1. [MOBILE-README.md](MOBILE-README.md) ? Configuration réseau
2. [MAUI-CHEAT-SHEET.md](MAUI-CHEAT-SHEET.md) ? Code minimal
3. Tester l'app
4. ? App mobile fonctionnelle

### **Parcours 2 : Je veux une app complète (1h30)**
1. [MOBILE-README.md](MOBILE-README.md) ? Configuration
2. [MAUI-DEVELOPER-GUIDE.md](MAUI-DEVELOPER-GUIDE.md) ? Développement
3. [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? SignalR + Avancé
4. [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) ? Toutes les commandes
5. ? App production-ready

### **Parcours 3 : J'ai un problème (10 min)**
1. [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Section "Dépannage"
2. Checklist de validation
3. [Tests/40-Mobile-Device-Connection-Tests.http](Tests/40-Mobile-Device-Connection-Tests.http) ? Tests connexion
4. ? Problème résolu

---

## ??? **ARCHITECTURE GLOBALE**

```
???????????????????????????????????????????????????????????????
?                    APP MOBILE MAUI                          ?
?                                                             ?
?  ??????????????????  ??????????????????  ????????????????  ?
?  ?  Views (XAML)  ?  ?  ViewModels    ?  ?   Services   ?  ?
?  ?  - Liste       ????  - MVVM        ????  - ApiService?  ?
?  ?  - Contrôle    ?  ?  - Commands    ?  ?  - DeviceCmd ?  ?
?  ?  - Stats       ?  ?  - Binding     ?  ?  - SignalR   ?  ?
?  ??????????????????  ??????????????????  ????????????????  ?
?                                                  ?          ?
???????????????????????????????????????????????????????????????
                                                   ?
                       HTTP REST + WebSocket (SignalR)
                                                   ?
???????????????????????????????????????????????????????????????
?                   BACKEND API (.NET 9)                      ?
?              https://192.168.1.100:5290                     ?
?                                                             ?
?  ????????????????????????  ????????????????????????????    ?
?  ? DevicePeripherals    ?  ? NotificationHub          ?    ?
?  ? Controller           ?  ? (SignalR)                ?    ?
?  ? - GET /enriched      ?  ? - ReceiveTelemetry       ?    ?
?  ? - POST /commands     ?  ? - ReceiveCommandResult   ?    ?
?  ????????????????????????  ????????????????????????????    ?
?             ?                                               ?
?  ????????????????????????  ????????????????????????????    ?
?  ? PostgreSQL           ?  ? Azure IoT Hub            ?    ?
?  ? - DevicePeripherals  ?  ? - Device Twins           ?    ?
?  ? - Zones              ?  ? - Commands ? ESP32       ?    ?
?  ????????????????????????  ????????????????????????????    ?
???????????????????????????????????????????????????????????????
                                       ?
                                       ? MQTT / IoT Hub Protocol
                                       ?
???????????????????????????????????????????????????????????????
?                   FIRMWARE ESP32                            ?
?  - Relays (ON/OFF)                                          ?
?  - LEDs (Brightness, Color)                                 ?
?  - HVAC (Temperature, Fan)                                  ?
?  - Locks (Lock/Unlock)                                      ?
?  - Sensors (Temperature, Motion, etc.)                      ?
???????????????????????????????????????????????????????????????
```

---

## ?? **ENDPOINTS API DISPONIBLES**

### **Lecture (GET)**
```
GET /api/DevicePeripherals/enriched
GET /api/DevicePeripherals/enriched/{id}
GET /api/DevicePeripherals/enriched/device/{iotDeviceId}
GET /api/DevicePeripherals/enriched/zone/{zoneId}
GET /api/DevicePeripherals/stats
```

### **Commandes (POST)**
```
POST /api/Devices/{deviceId}/peripherals/{name}/commands/setState
POST /api/Devices/{deviceId}/peripherals/{name}/commands/setBrightness
POST /api/Devices/{deviceId}/peripherals/{name}/commands/setColor
POST /api/Devices/{deviceId}/peripherals/{name}/commands/setTemperature
POST /api/Devices/{deviceId}/peripherals/{name}/commands/setFanSpeed
POST /api/Devices/{deviceId}/peripherals/{name}/commands/setMode
POST /api/Devices/{deviceId}/peripherals/{name}/commands/lock
POST /api/Devices/{deviceId}/peripherals/{name}/commands/unlock
POST /api/Devices/{deviceId}/peripherals/{name}/commands/arm
POST /api/Devices/{deviceId}/peripherals/{name}/commands/disarm
POST /api/Devices/{deviceId}/peripherals/{name}/commands/setVolume
POST /api/Devices/{deviceId}/peripherals/{name}/commands/playPause
POST /api/Devices/{deviceId}/peripherals/{name}/commands/mute
POST /api/Devices/{deviceId}/peripherals/{name}/commands/start
POST /api/Devices/{deviceId}/peripherals/{name}/commands/stop
POST /api/Devices/{deviceId}/peripherals/{name}/commands/dock
POST /api/Devices/{deviceId}/peripherals/energy/reset
```

**?? Voir toutes les commandes :** [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md)

---

## ? **CHECKLIST DE VALIDATION**

### **Configuration (5 min)**
- [ ] IP Backend trouvée (`ipconfig`)
- [ ] Pare-feu configuré (`configure-firewall.ps1`)
- [ ] Backend démarré (`dotnet run --launch-profile http`)
- [ ] Test navigateur mobile OK (`http://IP:5290/health/live`)
- [ ] IP dans code MAUI (`AppConfig.BaseUrl`)

### **Code MAUI (15 min)**
- [ ] Packages NuGet installés
- [ ] `AppConfig.cs` créé
- [ ] `ApiService.cs` créé
- [ ] `AuthService.cs` créé
- [ ] `DeviceService.cs` créé
- [ ] `DeviceCommandService.cs` créé
- [ ] Modèles créés (`Device`, `Zone`)

### **Fonctionnalités de Base (10 min)**
- [ ] Login fonctionne
- [ ] Token JWT stocké
- [ ] Liste périphériques affichée
- [ ] Détails périphérique affichés
- [ ] Commande ON/OFF fonctionne
- [ ] Gestion d'erreurs implémentée

### **Fonctionnalités Avancées (30 min)**
- [ ] Commande luminosité fonctionne
- [ ] Commande couleur fonctionne
- [ ] Commande température fonctionne
- [ ] SignalR connecté
- [ ] Notifications temps réel reçues
- [ ] UI se rafraîchit automatiquement

---

## ?? **DÉPANNAGE RAPIDE**

| Problème | Solution | Document |
|----------|----------|----------|
| **Cannot connect to backend** | Vérifier IP, pare-feu, backend démarré | [MOBILE-README.md](MOBILE-README.md) |
| **Unauthorized (401)** | Token expiré, relancer login | [MAUI-DEVELOPER-GUIDE.md](MAUI-DEVELOPER-GUIDE.md) |
| **Commands not working** | Vérifier `isActionable`, ESP32 en ligne | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) |
| **SignalR not connecting** | Vérifier WebSocket, URL correcte | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) |
| **Liste vide** | Vérifier authentification, données en DB | [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) |

---

## ?? **CONFIGURATION REQUISE**

### **Backend**
- ? .NET 9
- ? PostgreSQL
- ? Azure IoT Hub
- ? Redis (cache)

### **Mobile**
- ? .NET MAUI ou Xamarin.Forms
- ? Visual Studio 2022
- ? Android/iOS SDK
- ? Téléphone physique (recommandé)

### **Réseau**
- ? PC et téléphone sur même WiFi
- ? Ports ouverts (5290, 7144)

---

## ?? **RESSOURCES EXTERNES**

- [Documentation .NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [SignalR Client Documentation](https://learn.microsoft.com/aspnet/core/signalr/client)
- [HttpClient Best Practices](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)
- [Azure IoT Hub Documentation](https://learn.microsoft.com/azure/iot-hub/)

---

## ?? **QUICK START FINAL**

```bash
# 1. Configuration (2 min)
ipconfig | findstr "IPv4"
.\configure-firewall.ps1
dotnet run --launch-profile http

# 2. Code MAUI (5 min)
# Copier AppConfig.cs depuis MAUI-CHEAT-SHEET.md
# Copier ApiService.cs depuis MAUI-CHEAT-SHEET.md

# 3. Test (1 min)
# Login ? GetAllAsync() ? SetStateAsync()
```

**?? Total : 10 minutes**

---

**?? Documentation Complète SmartOffice Mobile**  
**Dernière mise à jour :** 2025-01-15  
**Version :** v1.0  
**Auteur :** SmartOffice Team

**?? Commencez par [MOBILE-README.md](MOBILE-README.md) ou [MAUI-CHEAT-SHEET.md](MAUI-CHEAT-SHEET.md) !**
