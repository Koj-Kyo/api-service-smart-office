# Guide de Synchronisation Azure IoT Hub ? Base de Données

## Vue d'ensemble

Ce système permet de synchroniser automatiquement les **devices IoT** (ESP32, ESP8266, etc.) et leurs **périphériques physiques** (relais, capteurs, etc.) depuis Azure IoT Hub vers la base de données locale.

## Architecture

```
Azure IoT Hub (brainoffice)
    ?
    ?? Devices (ESP32, ESP8266, ...)
    ?   ?? Device Twin
    ?       ?? desired.peripherals
    ?       ?? reported.peripherals
    ?           ?? relay1 (GPIO 26)
    ?           ?? relay2 (GPIO 27)
    ?           ?? mq2 (GPIO 34)
    ?           ?? dht22 (GPIO 25)
    ?
Base de Données SQL Server
    ?? Table: PeripheriqueLogique (devices IoT)
    ?? Table: DevicePeripheral (périphériques physiques)
```

## Concepts Clés

### 1. Device IoT (ESP32)
- **Un** device IoT = **un** ESP32/ESP8266 enregistré dans Azure IoT Hub
- Stocké dans la table `PeripheriqueLogique`
- Identifié par `IotDeviceId` (exemple: `"esp32-office-001"`)

### 2. Périphérique Physique
- Capteur ou actionneur branché sur un ESP32
- Exemples: relais, capteur de fumée MQ2, capteur DHT22, LED
- Stocké dans la table `DevicePeripheral`
- Identifié par `IotDeviceId` + `Name` (exemple: `esp32-office-001` + `relay1`)

### 3. Device Twin Properties
- `reported.peripherals`: Ce que l'ESP32 **reporte** comme périphériques connectés
- `desired.peripherals`: La configuration **souhaitée** par le backend

## Endpoints API

### Synchronisation des Devices

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `GET` | `/api/IoTHub/devices?max=100` | Liste les devices depuis Azure IoT Hub |
| `POST` | `/api/IoTHub/devices/sync` | Synchronise les devices vers `PeripheriqueLogique` |
| `GET` | `/api/IoTHub/device/{deviceId}/twin` | Récupère le twin d'un device |
| `GET` | `/api/Devices` | Liste les devices depuis la base de données |

### Synchronisation des Périphériques Physiques

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| `POST` | `/api/IoTHub/device/{deviceId}/peripherals/sync` | Synchronise les périphériques d'un device |
| `POST` | `/api/IoTHub/peripherals/sync-all` | Synchronise TOUS les périphériques de TOUS les devices |
| `GET` | `/api/DevicePeripherals/device/{deviceId}` | Liste les périphériques d'un device depuis la DB |
| `GET` | `/api/DevicePeripherals` | Liste tous les périphériques depuis la DB |
| `GET` | `/api/Devices/{deviceId}/peripherals` | Lit les périphériques depuis le twin (reported/desired) |

## Workflow de Synchronisation

### Étape 1: Synchroniser les Devices

```http
POST https://localhost:7144/api/IoTHub/devices/sync
Authorization: Bearer {admin_token}
```

**Ce qui se passe:**
1. Récupère tous les devices depuis Azure IoT Hub (via `RegistryManager.GetDevicesAsync`)
2. Pour chaque device:
   - Vérifie s'il existe déjà dans `PeripheriqueLogique` (par `IotDeviceId`)
   - Si nouveau ? crée une entrée avec:
     - `Name`: l'ID du device
     - `Type`: extrait depuis le twin (`deviceType` ou `type`)
     - `IotDeviceId`: l'ID Azure IoT Hub
     - `ZoneId`: première zone disponible dans la DB

**Réponse:**
```json
{
  "success": true,
  "syncedCount": 3,
  "message": "3 devices synchronisés avec succès depuis Azure IoT Hub"
}
```

### Étape 2: Synchroniser les Périphériques Physiques

#### Option A: Un device spécifique

```http
POST https://localhost:7144/api/IoTHub/device/esp32-office-001/peripherals/sync
Authorization: Bearer {admin_token}
```

**Ce qui se passe:**
1. Récupère le Device Twin de `esp32-office-001`
2. Parse `reported.peripherals` (ou `desired.peripherals` si reported vide)
3. Pour chaque périphérique:
   - Vérifie s'il existe dans `DevicePeripheral` (par `IotDeviceId` + `Name`)
   - Si nouveau ? crée une entrée avec:
     - `Name`: nom du périphérique (ex: `relay1`)
     - `Type`: type (ex: `relay`, `smoke`, `temp`)
     - `Gpio`: numéro GPIO (ex: `26`)
     - `IotDeviceId`: l'ID du device parent
     - `LastKnownState`: état sérialisé JSON
     - `Status`: `active`
   - Si existant ? met à jour `LastKnownState` et `LastUpdateReceived`

**Exemple de twin:**
```json
{
  "deviceId": "esp32-office-001",
  "properties": {
    "reported": {
      "peripherals": [
        { "name": "relay1", "type": "relay", "gpio": 26, "state": "off" },
        { "name": "relay2", "type": "relay", "gpio": 27, "state": "on" },
        { "name": "mq2", "type": "smoke", "gpio": 34, "value": 250 },
        { "name": "dht22", "type": "temp_humidity", "gpio": 25, "temp": 22.5, "humidity": 60 }
      ]
    }
  }
}
```

**Réponse:**
```json
{
  "success": true,
  "syncedCount": 4,
  "deviceId": "esp32-office-001",
  "message": "4 périphériques physiques synchronisés depuis IoT Hub"
}
```

#### Option B: Tous les devices

```http
POST https://localhost:7144/api/IoTHub/peripherals/sync-all
Authorization: Bearer {admin_token}
```

**Ce qui se passe:**
1. Liste tous les devices IoT Hub
2. Pour chaque device ? appelle la synchronisation des périphériques
3. Agrège les résultats

**Réponse:**
```json
{
  "success": true,
  "totalSynced": 12,
  "message": "12 périphériques physiques synchronisés depuis IoT Hub"
}
```

### Étape 3: Vérifier les Résultats

```http
# Voir les devices synchronisés
GET https://localhost:7144/api/Devices
Authorization: Bearer {admin_token}

# Voir les périphériques d'un device
GET https://localhost:7144/api/DevicePeripherals/device/esp32-office-001
Authorization: Bearer {admin_token}

# Voir l'état fusionné (desired + reported)
GET https://localhost:7144/api/Devices/esp32-office-001/peripherals/relay1/merged
Authorization: Bearer {admin_token}
```

## Structure des Tables

### Table: PeripheriqueLogique

| Colonne | Type | Description |
|---------|------|-------------|
| `Id` | int | PK auto-incrémenté |
| `Name` | string | Nom du device (par défaut = IotDeviceId) |
| `Type` | string | Type extrait du twin |
| `IotDeviceId` | string | ID unique dans Azure IoT Hub |
| `ZoneId` | int | FK vers Zone |

### Table: DevicePeripheral

| Colonne | Type | Description |
|---------|------|-------------|
| `Id` | int | PK auto-incrémenté |
| `Name` | string | Nom du périphérique (ex: `relay1`) |
| `Type` | string | Type (ex: `relay`, `smoke`) |
| `Gpio` | int | Numéro GPIO |
| `IotDeviceId` | string | ID du device parent |
| `Unit` | string | Unité de mesure (°C, %, etc.) |
| `LastKnownState` | string | État JSON sérialisé |
| `LastUpdateReceived` | DateTime | Dernière mise à jour |
| `Status` | string | `active`, `error`, `offline` |
| `PeripheriqueLogiqueId` | int? | Lien optionnel vers PeripheriqueLogique |

## Gestion des Doublons

### Devices
- Vérification par `IotDeviceId`
- Si existe déjà ? skip
- Si nouveau ? create

### Périphériques
- Vérification par `IotDeviceId` + `Name`
- Si existe déjà ? mise à jour de `LastKnownState` et `LastUpdateReceived`
- Si nouveau ? create

## Cas d'Usage

### 1. Premier Déploiement
```bash
# 1. Synchroniser tous les devices
POST /api/IoTHub/devices/sync

# 2. Synchroniser tous les périphériques
POST /api/IoTHub/peripherals/sync-all
```

### 2. Nouveau ESP32 Ajouté
```bash
# Option 1: Sync global
POST /api/IoTHub/devices/sync
POST /api/IoTHub/peripherals/sync-all

# Option 2: Sync ciblé
POST /api/IoTHub/devices/sync
POST /api/IoTHub/device/new-esp32-id/peripherals/sync
```

### 3. Nouveau Capteur Branché sur ESP32 Existant
```bash
# L'ESP32 doit d'abord reporter le nouveau capteur dans le twin
# Puis synchroniser
POST /api/IoTHub/device/esp32-office-001/peripherals/sync
```

### 4. Vérification Quotidienne
```bash
# Re-sync pour capturer les changements
POST /api/IoTHub/peripherals/sync-all
```

## Notifications SignalR

Lors de chaque synchronisation, une notification est envoyée via SignalR:

```javascript
// Frontend: écouter les notifications
connection.on("ReceiveNotification", (notification) => {
  if (notification.titre === "IoT Hub Sync" || 
      notification.titre === "Peripherals Sync") {
    console.log(notification.message);
    // Rafraîchir l'UI
  }
});
```

## Logs

Les logs sont disponibles dans la console:

```
[INFO] Démarrage de la synchronisation des devices depuis Azure IoT Hub
[INFO] Device esp32-office-001 synchronisé avec succès (Type: Generic IoT Device, Zone: Bureau Principal)
[INFO] Synchronisation terminée : 3 devices ajoutés sur 5 dans IoT Hub
[INFO] Démarrage de la synchronisation des périphériques pour esp32-office-001
[INFO] Périphérique relay1 (Type: relay, GPIO: 26) synchronisé pour esp32-office-001
[INFO] Périphérique mq2 (Type: smoke, GPIO: 34) synchronisé pour esp32-office-001
[INFO] Synchronisation terminée pour esp32-office-001: 4 nouveaux périphériques ajoutés
```

## Dépannage

### Aucun device trouvé
- Vérifier la connection string IoT Hub dans `appsettings.json` ou User Secrets
- Vérifier que l'IoT Hub s'appelle bien "brainoffice"
- Vérifier les devices dans le portail Azure

### Aucun périphérique trouvé
- Vérifier que le firmware ESP32 reporte bien `peripherals` dans `reported properties`
- Vérifier le format JSON: `{ "peripherals": [ { "name": "...", "type": "...", "gpio": N } ] }`
- Essayer de voir le twin: `GET /api/Devices/{deviceId}/twin/full`

### Erreur de synchronisation
- Vérifier les logs backend
- Vérifier les permissions (token Admin requis)
- Vérifier la connection string IoT Hub

## Exemple Complet

```bash
# 1. Login Admin
POST /api/Auth/login
{
  "userName": "admin",
  "password": "Admin@123"
}
# ? Token: eyJhbGci...

# 2. Lister les devices IoT Hub
GET /api/IoTHub/devices?max=100
Authorization: Bearer eyJhbGci...
# ? [ { "deviceId": "esp32-office-001", "status": "Enabled", ... } ]

# 3. Synchroniser les devices
POST /api/IoTHub/devices/sync
Authorization: Bearer eyJhbGci...
# ? { "syncedCount": 3 }

# 4. Synchroniser tous les périphériques
POST /api/IoTHub/peripherals/sync-all
Authorization: Bearer eyJhbGci...
# ? { "totalSynced": 12 }

# 5. Vérifier
GET /api/DevicePeripherals
Authorization: Bearer eyJhbGci...
# ? [ { "name": "relay1", "type": "relay", "gpio": 26, ... }, ... ]
```

## Intégration Frontend

```typescript
// Synchroniser les devices et périphériques
async function syncFromIoTHub() {
  try {
    // 1. Devices
    const devicesResponse = await fetch('/api/IoTHub/devices/sync', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    const devicesResult = await devicesResponse.json();
    console.log(`${devicesResult.syncedCount} devices synchronisés`);

    // 2. Périphériques
    const peripheralsResponse = await fetch('/api/IoTHub/peripherals/sync-all', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    const peripheralsResult = await peripheralsResponse.json();
    console.log(`${peripheralsResult.totalSynced} périphériques synchronisés`);

    // 3. Rafraîchir l'UI
    await loadDevices();
    await loadPeripherals();
  } catch (error) {
    console.error('Erreur de synchronisation:', error);
  }
}
```

## Conclusion

Ce système permet de maintenir automatiquement une copie locale des devices IoT et de leurs périphériques physiques, facilitant:
- La gestion centralisée
- Les requêtes rapides (pas besoin d'interroger IoT Hub à chaque fois)
- L'historisation
- Les associations avec d'autres entités (Zones, Commandes, etc.)
