# ?? Index Complet - Documentation Mobile MAUI

## ?? **DÉMARRAGE RAPIDE**

| Document | Temps | Description |
|----------|-------|-------------|
| [MOBILE-README.md](MOBILE-README.md) | 5 min | **START HERE** - Configuration réseau et connexion |
| [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) | 5 min | Contrôle périphériques en 5 minutes |
| [MAUI-TICKETS-QUICK-START.md](MAUI-TICKETS-QUICK-START.md) | 5 min | Gestion tickets en 5 minutes |
| [MAUI-HISTORY-QUICK-START.md](MAUI-HISTORY-QUICK-START.md) | 5 min | ?? **À CORRIGER** - Voir document de correction |

---

## ?? **GUIDES COMPLETS**

### **Configuration Réseau et Connexion**

| Document | Description |
|----------|-------------|
| [MOBILE-SETUP-EXPRESS.md](MOBILE-SETUP-EXPRESS.md) | ?? Juste les commandes (copier-coller) |
| [MOBILE-SETUP-VISUAL-GUIDE.md](MOBILE-SETUP-VISUAL-GUIDE.md) | ?? Guide visuel avec captures d'écran |
| [MOBILE-USB-DEVICE-SETUP.md](MOBILE-USB-DEVICE-SETUP.md) | ?? Guide complet pas-à-pas |
| [MOBILE-CONFIGURATION-INDEX.md](MOBILE-CONFIGURATION-INDEX.md) | ?? Index de la configuration |

### **Contrôle des Périphériques**

| Document | Description |
|----------|-------------|
| [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) | ?? Guide complet (architecture, code, tests) |
| [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) | ? Démarrage rapide (5 min) |
| [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) | ?? Documentation API périphériques |
| [MAUI-COMMANDS-REFERENCE.md](MAUI-COMMANDS-REFERENCE.md) | ?? Référence complète des commandes |

### **Gestion des Tickets & Incidents**

| Document | Description |
|----------|-------------|
| [MAUI-TICKETS-COMPLETE-GUIDE.md](MAUI-TICKETS-COMPLETE-GUIDE.md) | ?? Guide complet tickets & incidents |
| [MAUI-TICKETS-QUICK-START.md](MAUI-TICKETS-QUICK-START.md) | ? Démarrage rapide (5 min) |
| [FRONTEND-INCIDENTS-README.md](FRONTEND-INCIDENTS-README.md) | ?? Documentation frontend incidents |

### **Historique des Activités** ??

| Document | Description |
|----------|-------------|
| [MAUI-HISTORY-BACKEND-CORRECTION.md](MAUI-HISTORY-BACKEND-CORRECTION.md) | ?? **UTILISEZ CE DOCUMENT** - Correction basée sur API réelle |
| ~~[MAUI-HISTORY-COMPLETE-GUIDE.md](MAUI-HISTORY-COMPLETE-GUIDE.md)~~ | ? **NE PAS UTILISER** - Endpoints incorrects |
| ~~[MAUI-HISTORY-QUICK-START.md](MAUI-HISTORY-QUICK-START.md)~~ | ? **NE PAS UTILISER** - Modèles incorrects |
| [Authorization-Activity-Complete-Guide.md](Authorization-Activity-Complete-Guide.md) | ?? Documentation backend activités (référence) |

> ?? **IMPORTANT:** Les guides originaux de l'historique (`MAUI-HISTORY-COMPLETE-GUIDE.md` et `MAUI-HISTORY-QUICK-START.md`) contiennent des **erreurs critiques**. Les endpoints et modèles documentés ne correspondent PAS à l'API réelle du backend. **Utilisez uniquement** [MAUI-HISTORY-BACKEND-CORRECTION.md](MAUI-HISTORY-BACKEND-CORRECTION.md) qui fournit la documentation correcte basée sur l'implémentation réelle.

### **Exemples de Code**

| Document | Description |
|----------|-------------|
| [MOBILE-API-CLIENT-EXAMPLE.md](MOBILE-API-CLIENT-EXAMPLE.md) | ?? Exemples de code ApiService |
| [MAUI-ApiService-Example.cs](MAUI-ApiService-Example.cs) | ?? Classe ApiService complète |

---

## ?? **TESTS**

| Fichier | Description |
|---------|-------------|
| [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http) | Tests API périphériques enrichis |
| [Tests/39-Mobile-Devices-Enriched-Tests.http](Tests/39-Mobile-Devices-Enriched-Tests.http) | Tests complets avec validation |
| [Tests/40-Mobile-Device-Connection-Tests.http](Tests/40-Mobile-Device-Connection-Tests.http) | Tests de connexion mobile |
| [Tests/29-Firmware-v2-Complete-Commands.http](Tests/29-Firmware-v2-Complete-Commands.http) | Tests commandes firmware |
| [Tests/37-Incidents-Complete.http](Tests/37-Incidents-Complete.http) | Tests API tickets & incidents |
| [Tests/13-User-Activity-Authorization.http](Tests/13-User-Activity-Authorization.http) | Tests API activités utilisateur |

---

## ?? **ARCHITECTURE ET FLUX**

### **Flux de Communication**

```
???????????????????
?   APP MOBILE    ?  ? Votre app MAUI
?     (MAUI)      ?
???????????????????
         ?
         ? 1. HTTP REST API (Lecture/Commandes)
         ? 2. SignalR WebSocket (Temps réel)
         ?
???????????????????
?   BACKEND API   ?  ? SmartOfficeAPP (.NET 9)
?   (.NET 9)      ?
???????????????????
         ?
         ? 3. Azure IoT Hub (Commandes ? ESP32)
         ? 4. PostgreSQL (Métadonnées)
         ?
???????????????????
?  FIRMWARE ESP32 ?  ? Périphériques physiques
???????????????????
```

---

## ?? **PARCOURS PAR OBJECTIF**

### **Je veux juste connecter mon téléphone**
1. [MOBILE-README.md](MOBILE-README.md) ? Configuration réseau
2. [MOBILE-SETUP-EXPRESS.md](MOBILE-SETUP-EXPRESS.md) ? Commandes rapides
3. Test : `http://VOTRE_IP:5290/health/live`

### **Je veux lister les périphériques**
1. [MAUI-DEVICE-CONTROL-QUICK-START.md](MAUI-DEVICE-CONTROL-QUICK-START.md) ? Code minimal
2. [MOBILE-DEVICES-API-COMPLETE-GUIDE.md](MOBILE-DEVICES-API-COMPLETE-GUIDE.md) ? Structure données
3. Test : [Tests/38-Mobile-Devices-Enriched.http](Tests/38-Mobile-Devices-Enriched.http)

### **Je veux contrôler les périphériques**
1. [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Guide complet
2. Section "Envoyer des Commandes"
3. Test : [Tests/29-Firmware-v2-Complete-Commands.http](Tests/29-Firmware-v2-Complete-Commands.http)

### **Je veux gérer les tickets**
1. [MAUI-TICKETS-QUICK-START.md](MAUI-TICKETS-QUICK-START.md) ? Démarrage rapide
2. [MAUI-TICKETS-COMPLETE-GUIDE.md](MAUI-TICKETS-COMPLETE-GUIDE.md) ? Guide complet
3. Test : [Tests/37-Incidents-Complete.http](Tests/37-Incidents-Complete.http)

### **Je veux voir l'historique des activités** ??
1. ?? **IMPORTANT:** [MAUI-HISTORY-BACKEND-CORRECTION.md](MAUI-HISTORY-BACKEND-CORRECTION.md) ? **Utilisez CE guide corrigé**
2. ? **N'utilisez PAS** les guides originaux (endpoints incorrects)
3. Test : [Tests/13-User-Activity-Authorization.http](Tests/13-User-Activity-Authorization.http)

### **Je veux les notifications temps réel**
1. [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md)
2. Section "Recevoir les Notifications Temps Réel (SignalR)"
3. Installer : `Microsoft.AspNetCore.SignalR.Client`

### **J'ai un problème**
1. [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md)
2. Section "Dépannage"
3. Vérifier checklist : Section "Tests et Validation"

---

## ?? **ENDPOINTS API DISPONIBLES**

### **Périphériques (Lecture)**

```
GET /api/DevicePeripherals/enriched
GET /api/DevicePeripherals/enriched/{id}
GET /api/DevicePeripherals/enriched/device/{iotDeviceId}
GET /api/DevicePeripherals/enriched/zone/{zoneId}
GET /api/DevicePeripherals/stats
```

### **Commandes (Écriture)**

```
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setState
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setBrightness
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setColor
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setTemperature
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setFanSpeed
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setMode
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/lock
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/unlock
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/arm
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/disarm
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/setVolume
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/playPause
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/mute
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/start
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/stop
POST /api/Devices/{iotDeviceId}/peripherals/{name}/commands/dock
POST /api/Devices/{iotDeviceId}/peripherals/energy/reset
```

### **Tickets & Incidents**

```
POST /api/IncidentTickets                  - Créer un ticket
GET  /api/IncidentTickets/user/{email}     - Mes tickets
GET  /api/IncidentTickets/{id}             - Détails d'un ticket
GET  /api/IncidentTickets/status/{status}  - Filtrer par statut
GET  /api/IncidentTickets/type/{type}      - Filtrer par type
GET  /api/IncidentTickets/priority/{priority} - Filtrer par priorité
GET  /api/IncidentTickets/stats            - Statistiques
PUT  /api/IncidentTickets/{id}             - Mettre à jour (Admin)
PUT  /api/IncidentTickets/{id}/assign      - Assigner (Admin)
PUT  /api/IncidentTickets/{id}/resolve     - Résoudre (Admin)
```

### **Historique des Activités** ? (Corrigé)

```
GET /api/user/activity/history              - Historique paginé (authentification automatique)
GET /api/user/activity/stats                - Statistiques utilisateur
GET /api/user/activity/recent               - Activités récentes (24h)
```

> ?? **Note:** Contrairement aux autres endpoints, l'historique utilise la route `/api/user/activity/*` (pas `/api/UserActivity/*`) et récupère automatiquement l'User ID depuis le token JWT. Ne pas inclure l'User ID en paramètre !

### **Authentification**

```
POST /api/auth/login
POST /api/auth/register
GET /api/auth/profile
```

### **Temps Réel (SignalR)**

```
WebSocket: ws://IP:5290/hubs/notifications

Events:
- ReceiveTelemetry
- ReceiveCommandResult
- ReceiveDeviceStateUpdate
```

---

## ?? **CONFIGURATION REQUISE**

### **Backend**

- ? .NET 9
- ? PostgreSQL
- ? Azure IoT Hub
- ? Redis (cache)

### **Mobile**

- ? .NET MAUI / Xamarin.Forms
- ? `System.Net.Http.Json` (REST)
- ? `Microsoft.AspNetCore.SignalR.Client` (Temps réel)
- ? Connexion réseau local (même WiFi que PC)

### **Réseau**

- ? PC et téléphone sur même WiFi
- ? Pare-feu Windows configuré (ports 5290, 7144)
- ? Backend démarré avec `dotnet run --launch-profile http`

---

## ? **CHECKLIST DE VALIDATION**

### **Configuration Initiale**

- [ ] IP trouvée (`ipconfig | findstr "IPv4"`)
- [ ] Pare-feu configuré (`configure-firewall.ps1`)
- [ ] Backend démarré (`dotnet run --launch-profile http`)
- [ ] Test navigateur OK (`http://IP:5290/health/live`)
- [ ] IP dans code MAUI (`AppConfig.BaseUrl`)

### **Code MAUI**

- [ ] Packages NuGet installés
- [ ] `ApiService` créé
- [ ] `DeviceService` créé
- [ ] `TicketService` créé
- [ ] `UserActivityService` créé **(version corrigée)**
- [ ] Modèles créés (`Device`, `Ticket`, `UserActivity`, `Zone`, etc.)
- [ ] Token JWT géré (login + header)

### **Fonctionnalités**

- [ ] Login fonctionne
- [ ] Liste périphériques affichée
- [ ] Détails périphérique OK
- [ ] Commande ON/OFF fonctionne
- [ ] Commande luminosité fonctionne
- [ ] Liste tickets affichée
- [ ] Création ticket fonctionne
- [ ] Filtrage tickets fonctionne
- [ ] Historique activités affiché **(avec routes corrigées)**
- [ ] Filtrage historique fonctionne
- [ ] Pagination historique fonctionne
- [ ] SignalR connecté (optionnel)
- [ ] Notifications temps réel (optionnel)

---

## ?? **SUPPORT RAPIDE**

| Problème | Document |
|----------|----------|
| Cannot connect to backend | [MOBILE-README.md](MOBILE-README.md) ? Section "Dépannage" |
| Unauthorized (401) | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Section "Gestion des Erreurs" |
| Commands not working | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Section "Dépannage" |
| SignalR not connecting | [MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md](MAUI-DEVICE-CONTROL-COMPLETE-GUIDE.md) ? Section "Notifications Temps Réel" |
| Cannot create ticket | [MAUI-TICKETS-COMPLETE-GUIDE.md](MAUI-TICKETS-COMPLETE-GUIDE.md) ? Section "Dépannage" |
| Tickets list empty | [MAUI-TICKETS-COMPLETE-GUIDE.md](MAUI-TICKETS-COMPLETE-GUIDE.md) ? Section "Dépannage" |
| **Activities endpoints 404** | ?? [MAUI-HISTORY-BACKEND-CORRECTION.md](MAUI-HISTORY-BACKEND-CORRECTION.md) ? **Utiliser routes corrigées** |
| Activities list empty | [MAUI-HISTORY-BACKEND-CORRECTION.md](MAUI-HISTORY-BACKEND-CORRECTION.md) ? Section "Dépannage" |
| Filter not working | [MAUI-HISTORY-BACKEND-CORRECTION.md](MAUI-HISTORY-BACKEND-CORRECTION.md) ? Section "Filtrage Local" |

---

## ?? **RESSOURCES EXTERNES**

- [Documentation .NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- [SignalR Client Documentation](https://learn.microsoft.com/aspnet/core/signalr/client)
- [HttpClient Best Practices](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient-guidelines)

---

## ?? **QUICK START COMPLET (10 MIN)**

```bash
# 1. Configuration réseau (3 min)
ipconfig | findstr "IPv4"  # Noter votre IP
cd D:\Memoire\ApiSmart\SmartOfficeAPP\SmartOfficeAPP
.\configure-firewall.ps1   # Configurer pare-feu
dotnet run --launch-profile http  # Démarrer backend

# 2. Test navigateur mobile (1 min)
# Ouvrir: http://VOTRE_IP:5290/health/live

# 3. Code MAUI minimal (5 min)
# Copier AppConfig.cs depuis MAUI-DEVICE-CONTROL-QUICK-START.md
# Copier ApiService.cs depuis MAUI-DEVICE-CONTROL-QUICK-START.md
# Copier DeviceService.cs depuis MAUI-DEVICE-CONTROL-QUICK-START.md
# Copier TicketService.cs depuis MAUI-TICKETS-QUICK-START.md
# ?? Copier UserActivityService.cs depuis MAUI-HISTORY-BACKEND-CORRECTION.md (VERSION CORRIGÉE)

# 4. Test (1 min)
# Lancer l'app MAUI
# Login + GetAllAsync() + CreateTicketAsync() + GetUserActivitiesAsync()
```

---

**?? Documentation complète pour SmartOffice Mobile**  
**Dernière mise à jour:** 2025-01-15  
**Version:** v1.3 ?? **(Correction Historique des Activités)**

**?? Commencez par [MOBILE-README.md](MOBILE-README.md) !**

**?? ATTENTION:** Pour l'historique des activités, utilisez **uniquement** [MAUI-HISTORY-BACKEND-CORRECTION.md](MAUI-HISTORY-BACKEND-CORRECTION.md) !
