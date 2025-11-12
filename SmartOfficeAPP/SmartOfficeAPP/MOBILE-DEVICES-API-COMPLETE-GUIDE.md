# ?? MOBILE APP - DEVICE PERIPHERALS API COMPLETE GUIDE

## ?? **OBJECTIF**
Fournir toutes les données nécessaires pour alimenter la **page Devices** de l'app mobile MAUI avec :
- ? Liste des périphériques avec état temps réel
- ? Détails complets d'un périphérique
- ? Actions/commandes disponibles
- ? Statistiques agrégées globales

---

## ?? **ENDPOINTS DISPONIBLES**

### **1?? Liste complète des périphériques enrichis**
```http
GET /api/DevicePeripherals/enriched
Authorization: Bearer {token}
```

**Réponse:** Tableau de `DevicePeripheralEnrichedDto`

**Utilisation:** 
- Affichage de la grille/liste des périphériques
- Page d'accueil de l'app mobile
- Support `?fresh=true` pour bypass cache

---

### **2?? Détails d'un périphérique enrichi**
```http
GET /api/DevicePeripherals/enriched/{id}
Authorization: Bearer {token}
```

**Paramètres:**
- `id` (int): ID du périphérique en base de données

**Réponse:** `DevicePeripheralEnrichedDto`

**Utilisation:**
- Page de détails d'un périphérique
- Affichage des paramètres complets
- Actions disponibles

---

### **3?? Périphériques d'un device IoT Hub**
```http
GET /api/DevicePeripherals/enriched/device/{iotDeviceId}
Authorization: Bearer {token}
```

**Paramètres:**
- `iotDeviceId` (string): ID du device IoT Hub (ex: `esp32-office-01`)

**Réponse:** Tableau de `DevicePeripheralEnrichedDto`

**Utilisation:**
- Vue par ESP32 physique
- Gestion d'un device spécifique

---

### **4?? Périphériques d'une zone**
```http
GET /api/DevicePeripherals/enriched/zone/{zoneId}
Authorization: Bearer {token}
```

**Paramètres:**
- `zoneId` (int): ID de la zone/pièce

**Réponse:** Tableau de `DevicePeripheralEnrichedDto`

**Utilisation:**
- Vue par pièce (Bureau, Salle de réunion, etc.)
- Contrôle d'une zone spécifique

---

### **5?? Statistiques agrégées**
```http
GET /api/DevicePeripherals/stats
Authorization: Bearer {token}
```

**Réponse:** `DevicePeripheralsStatsDto`

**Utilisation:**
- Dashboard global
- Compteurs de l'app mobile
- Graphiques d'agrégation

---

## ?? **STRUCTURE DES DONNÉES**

### **DevicePeripheralEnrichedDto** (Périphérique enrichi)

#### **1. IDENTITÉ / BASE**
```json
{
  "id": 1,                      // ID en base de données (int)
  "guid": "uuid-here",          // GUID unique du firmware (string, optionnel)
  "name": "relay4",             // Nom affiché (string)
  "type": "Lighting",           // Type normalisé (Lighting|Climate|Security|Media|Cleaner|Door|Sensor|Actuator)
  "zone": {                     // Zone/pièce (ZoneDto, optionnel)
    "id": 1,
    "name": "Bureau",
    "description": "Bureau principal"
  },
  "zoneId": 1,                  // ID de la zone (int, optionnel)
  "isActive": true,             // État global: actif/en ligne (bool)
  "capabilities": [             // Fonctionnalités disponibles (array<string>)
    "State",
    "Brightness",
    "Color",
    "Energy"
  ]
}
```

**Types disponibles:**
- `Lighting` ? Éclairage
- `Climate` ? Climatisation/Chauffage
- `Security` ? Sécurité (alarme, détecteur)
- `Media` ? Multimédia (speaker, TV)
- `Cleaner` ? Robot aspirateur
- `Door` ? Porte/Serrure connectée
- `Sensor` ? Capteur générique
- `Actuator` ? Actionneur générique

**Capabilities possibles:**
- `State` ? État on/off
- `Brightness` ? Luminosité
- `Color` ? Couleur RGB
- `ColorTemperature` ? Température de couleur
- `Temperature` ? Température cible
- `FanSpeed` ? Vitesse ventilateur
- `Mode` ? Mode de fonctionnement
- `Armed` ? État armé/désarmé
- `Lock` ? Verrouillage
- `Battery` ? Niveau batterie
- `Volume` ? Volume sonore
- `PlayPause` ? Lecture/pause
- `Mute` ? Sourdine
- `Dock` ? Station d'accueil
- `DoorSensor` ? Capteur d'ouverture
- `Energy` ? Mesure énergétique

---

#### **2. ÉTAT TEMPS RÉEL GÉNÉRIQUE**
```json
{
  "connectivityStatus": "Online",      // Online | Offline | Degraded
  "lastUpdatedUtc": "2025-01-15T10:30:00Z",  // Horodatage UTC (datetime)
  "healthStatus": "OK",                // OK | Warning | Error | Unknown
  "healthMessage": null                // Message d'erreur si HealthStatus != OK (string, optionnel)
}
```

---

#### **3. MÉTADONNÉES HARDWARE**
```json
{
  "iotDeviceId": "esp32-office-01",    // ID du device IoT Hub parent (string)
  "gpio": 33,                          // Numéro GPIO (int)
  "unit": "W",                         // Unité de mesure (string, optionnel)
  "notes": "Éclairage principal",      // Notes de câblage (string, optionnel)
  "installedAt": "2025-01-01T00:00:00Z" // Date d'installation (datetime, optionnel)
}
```

---

#### **4. ÉTAT SPÉCIFIQUE PAR TYPE** (objet dynamique)
```json
{
  "typeSpecificState": {
    // Varie selon le type de périphérique (voir ci-dessous)
  }
}
```

**Exemples par type:**

**Lighting (Éclairage)**
```json
{
  "typeSpecificState": {
    "brightness": 75,              // 0-100
    "lightColor": "#FF5733",       // Hex color
    "colorTemperature": 3000,      // Kelvin (optionnel)
    "powerConsumptionW": 12.5,     // Puissance instantanée (W)
    "energyTodayWh": 150.0         // Énergie consommée aujourd'hui (Wh)
  }
}
```

**Climate (Climatisation)**
```json
{
  "typeSpecificState": {
    "targetTemperature": 22.0,     // Température cible (°C)
    "currentTemperature": 21.5,    // Température actuelle (°C)
    "climateMode": "Cool",         // Cool | Heat | Auto | Fan | Dry
    "fanSpeed": 3,                 // Vitesse ventilateur (1-5)
    "humidityPercent": 55          // Humidité (%, optionnel)
  }
}
```

**Security (Sécurité)**
```json
{
  "typeSpecificState": {
    "armedState": "Armed",         // Armed | Disarmed | Partial
    "lastAlarmUtc": "2025-01-15T08:00:00Z",  // Dernière alarme (optionnel)
    "batteryPercent": 85           // Niveau batterie (%, optionnel)
  }
}
```

**Media (Multimédia)**
```json
{
  "typeSpecificState": {
    "volume": 60,                  // Volume 0-100
    "isPlaying": true,             // Lecture en cours (bool)
    "currentMediaTitle": "Song",   // Titre actuel (string, optionnel)
    "mute": false                  // Sourdine (bool)
  }
}
```

**Cleaner (Robot aspirateur)**
```json
{
  "typeSpecificState": {
    "cleanerMode": "Full",         // Standby | Quick | Charge | Full | Mapping
    "batteryPercent": 70,          // Niveau batterie (%)
    "docked": false,               // À la station (bool)
    "progressPercent": 45          // Progression cycle (%, optionnel)
  }
}
```

**Door (Porte/Serrure)**
```json
{
  "typeSpecificState": {
    "isLocked": true,              // Verrouillé (bool)
    "doorOpen": false,             // Porte ouverte (bool, optionnel)
    "lockBatteryPercent": 90       // Niveau batterie serrure (%, optionnel)
  }
}
```

---

#### **5. DONNÉES ÉNERGÉTIQUES**
```json
{
  "metering": "measured",          // none | estimated | measured
  "ratedWatt": 15.0,               // Puissance nominale (W, optionnel)
  "energyTodayWh": 150.0,          // Énergie consommée aujourd'hui (Wh, optionnel)
  "currentPowerW": 12.5            // Puissance instantanée (W, optionnel)
}
```

---

#### **6. MÉTADONNÉES ADMINISTRATIVES**
```json
{
  "status": "active",              // active | error | offline | maintenance
  "isActionable": true,            // Peut recevoir des commandes (bool)
  "createdAt": "2025-01-01T00:00:00Z",    // Date création DB
  "updatedAt": "2025-01-15T10:30:00Z"     // Dernière modification DB (optionnel)
}
```

---

### **DevicePeripheralsStatsDto** (Statistiques agrégées)

```json
{
  "totalDevices": 42,                  // Nombre total de périphériques
  "activeDevicesCount": 38,            // Périphériques actifs (en ligne)
  "offlineDevicesCount": 3,            // Périphériques hors ligne
  "errorDevicesCount": 1,              // Périphériques en erreur
  "zonesCount": 5,                     // Nombre de zones distinctes
  "countsByType": {                    // Comptage par type
    "Lighting": 15,
    "Climate": 5,
    "Security": 8,
    "Sensor": 10,
    "Actuator": 4
  },
  "actionableDevicesCount": 24,        // Périphériques pouvant recevoir des commandes
  "totalCurrentPowerW": 450.75,        // Puissance instantanée totale (W)
  "totalEnergyTodayWh": 5420.5,        // Énergie totale consommée aujourd'hui (Wh)
  "generatedAtUtc": "2025-01-15T10:30:00Z"  // Date de génération des stats
}
```

---

## ?? **CACHE ET FRAÎCHEUR DES DONNÉES**

### **Politique de cache par défaut**
- **Durée:** 2 minutes
- **Policy:** `DeviceData`
- **Invalide automatiquement:** Après modification, création, suppression

### **Forcer refresh (?fresh=true)**
```http
GET /api/DevicePeripherals/enriched?fresh=true
```

Bypass le cache et récupère les données directement depuis :
- Base de données (DB)
- Azure IoT Hub (Twin reported properties)

---

## ?? **EXEMPLES D'UTILISATION MOBILE**

### **Scénario 1: Liste des périphériques**
```csharp
// Dans votre ViewModel MAUI
public async Task LoadDevicesAsync()
{
    var response = await _httpClient.GetAsync(
        "/api/DevicePeripherals/enriched");
    
    var devices = await response.Content
        .ReadFromJsonAsync<List<DevicePeripheralEnrichedDto>>();
    
    // Afficher dans une CollectionView/ListView
    DevicesList = new ObservableCollection<DevicePeripheralEnrichedDto>(devices);
}
```

### **Scénario 2: Détails d'un périphérique**
```csharp
public async Task LoadDeviceDetailsAsync(int deviceId)
{
    var device = await _httpClient.GetFromJsonAsync<DevicePeripheralEnrichedDto>(
        $"/api/DevicePeripherals/enriched/{deviceId}");
    
    // Afficher les détails
    SelectedDevice = device;
    
    // Adapter l'UI selon le type
    switch (device.Type)
    {
        case "Lighting":
            ShowLightingControls(device.TypeSpecificState);
            break;
        case "Climate":
            ShowClimateControls(device.TypeSpecificState);
            break;
        // ...
    }
}
```

### **Scénario 3: Dashboard avec stats**
```csharp
public async Task LoadDashboardAsync()
{
    var stats = await _httpClient.GetFromJsonAsync<DevicePeripheralsStatsDto>(
        "/api/DevicePeripherals/stats");
    
    TotalDevices = stats.TotalDevices;
    ActiveDevices = stats.ActiveDevicesCount;
    TotalEnergyToday = stats.TotalEnergyTodayWh;
    
    // Graphique par type
    DevicesByTypeChart = CreateChart(stats.CountsByType);
}
```

### **Scénario 4: Filtrage par zone**
```csharp
public async Task LoadDevicesByZoneAsync(int zoneId)
{
    var devices = await _httpClient.GetFromJsonAsync<List<DevicePeripheralEnrichedDto>>(
        $"/api/DevicePeripherals/enriched/zone/{zoneId}");
    
    DevicesInZone = new ObservableCollection<DevicePeripheralEnrichedDto>(devices);
}
```

---

## ? **ACTIONS SUR LES PÉRIPHÉRIQUES**

Pour envoyer des commandes aux périphériques, utiliser les endpoints existants :

### **Commande simple**
```http
POST /api/Devices/{iotDeviceId}/peripherals/{peripheralName}/commands/{command}
Authorization: Bearer {token}
Content-Type: application/json

{
  "parameters": {
    "value": true
  }
}
```

### **Exemples de commandes:**
- `setState` ? Changer l'état on/off
- `setBrightness` ? Ajuster luminosité
- `setColor` ? Changer couleur
- `setTemperature` ? Ajuster température cible
- `lock` / `unlock` ? Verrouiller/déverrouiller
- `start` / `stop` ? Démarrer/arrêter

---

## ?? **AUTHENTIFICATION**

Tous les endpoints enrichis nécessitent une authentification JWT :

```http
Authorization: Bearer {token}
```

**Rôles acceptés:**
- Utilisateurs authentifiés (pas besoin d'être Admin)
- Les endpoints de création/modification/suppression nécessitent le rôle Admin

---

## ?? **NOTES IMPORTANTES**

### **Performance**
- Les endpoints enrichis combinent données DB + IoT Hub
- Privilégier le cache (pas de `?fresh=true`) pour de meilleures performances
- Utiliser `/enriched/device/{iotDeviceId}` plutôt que `/enriched` si possible (plus rapide)

### **Disponibilité des données**
- `Zone` peut être `null` si le périphérique n'est pas assigné à une zone
- `TypeSpecificState` peut être `null` si le device est offline
- `Guid` peut être `null` si le firmware ne l'a pas encore reporté
- Les propriétés énergétiques sont `null` si `metering` = `"none"`

### **Types normalisés**
Le backend normalise automatiquement les types :
- `relay`, `light`, `lamp` ? `Lighting`
- `temp`, `climate`, `hvac` ? `Climate`
- `motion`, `smoke`, `alarm` ? `Security`
- etc.

---

## ?? **TESTS**

Fichier de tests HTTP complet disponible :
- `SmartOfficeAPP/Tests/38-Mobile-Devices-Enriched.http`

---

## ?? **RESSOURCES ADDITIONNELLES**

- [API Complete Documentation](./API-COMPLETE-DOCUMENTATION.md)
- [Device Controller Documentation](./Controllers/DeviceController.cs)
- [IoT Hub Service Documentation](./Services/IServices/IIotHubService.cs)

---

## ?? **SUPPORT**

En cas de problème :
1. Vérifier le token JWT (`Authorization: Bearer ...`)
2. Vérifier que le device IoT Hub existe et est en ligne
3. Consulter les logs backend (`GET /api/DeviceLogs/device/{iotDeviceId}`)
4. Tester avec `?fresh=true` pour éliminer les problèmes de cache

---

**Dernière mise à jour:** 2025-01-15  
**Version API:** v1.0  
**Contact:** equipe-backend@smartoffice.com
