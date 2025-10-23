# Résumé: Synchronisation IoT Hub ? Base de Données

## Nouveaux Endpoints Ajoutés

### 1. Synchronisation des Devices IoT

**`GET /api/IoTHub/devices`**
- Liste tous les devices depuis Azure IoT Hub
- Query param: `max` (défaut: 100, max: 1000)
- Rôle: User ou Admin

**`POST /api/IoTHub/devices/sync`**
- Synchronise les devices IoT Hub ? table `PeripheriqueLogique`
- Ne duplique pas (vérification par `IotDeviceId`)
- Rôle: Admin uniquement

### 2. Synchronisation des Périphériques Physiques

**`POST /api/IoTHub/device/{iotDeviceId}/peripherals/sync`**
- Synchronise les périphériques d'un device spécifique
- Lit depuis `reported.peripherals` (ou `desired.peripherals`)
- Ajoute dans table `DevicePeripheral`
- Rôle: Admin uniquement

**`POST /api/IoTHub/peripherals/sync-all`**
- Synchronise TOUS les périphériques de TOUS les devices
- Parcourt tous les devices IoT Hub
- Rôle: Admin uniquement

## Services Modifiés

### `IIotHubService`
- ? Ajout: `Task<IEnumerable<IoTHubDeviceSummary>> ListDevicesAsync(int maxCount = 100)`

### `IPeripheriqueLogiqueService`
- ? Ajout: `Task<int> SyncDevicesFromIoTHubAsync()`

### `IDevicePeripheralService`
- ? Ajout: `Task<int> SyncPeripheralsFromIoTHubAsync(string iotDeviceId)`
- ? Ajout: `Task<int> SyncAllPeripheralsFromIoTHubAsync()`

## Nouveaux DTOs

### `IoTHubDeviceSummary`
```csharp
public class IoTHubDeviceSummary
{
    public string DeviceId { get; set; }
    public string Status { get; set; }
    public string ConnectionState { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public DateTime? StatusUpdatedTime { get; set; }
    public DateTime? ConnectionStateUpdatedTime { get; set; }
}
```

## Workflow Typique

```bash
# 1. Se connecter en tant qu'Admin
POST /api/Auth/login
Body: { "userName": "admin", "password": "Admin@123" }
? Token

# 2. Synchroniser les devices IoT Hub
POST /api/IoTHub/devices/sync
Authorization: Bearer {token}
? { "syncedCount": 3 }

# 3. Synchroniser tous les périphériques physiques
POST /api/IoTHub/peripherals/sync-all
Authorization: Bearer {token}
? { "totalSynced": 12 }

# 4. Vérifier les résultats
GET /api/DevicePeripherals
Authorization: Bearer {token}
? Liste de tous les périphériques physiques
```

## Ce qui est Synchronisé

### Devices (PeripheriqueLogique)
- ? `DeviceId` depuis IoT Hub
- ? `Status` (Enabled/Disabled)
- ? `Type` extrait du twin
- ? Assignation automatique à une zone

### Périphériques Physiques (DevicePeripheral)
- ? `Name` (ex: relay1, mq2, dht22)
- ? `Type` (ex: relay, smoke, temp_humidity)
- ? `Gpio` (numéro de broche)
- ? `Unit` (°C, %, etc.)
- ? `LastKnownState` (état JSON)
- ? `Status` (active, error, offline)

## Gestion des Doublons

? **Devices**: Vérification par `IotDeviceId`
- Existe ? Skip
- Nouveau ? Create

? **Périphériques**: Vérification par `IotDeviceId` + `Name`
- Existe ? Mise à jour de l'état
- Nouveau ? Create

## Notifications SignalR

Chaque synchronisation envoie une notification:
- "IoT Hub Sync" pour les devices
- "Peripherals Sync" pour les périphériques individuels
- "Peripherals Global Sync" pour la synchronisation globale

## Structure Twin Attendue

```json
{
  "deviceId": "esp32-office-001",
  "properties": {
    "reported": {
      "peripherals": [
        {
          "name": "relay1",
          "type": "relay",
          "gpio": 26,
          "state": "off"
        },
        {
          "name": "mq2",
          "type": "smoke",
          "gpio": 34,
          "value": 250,
          "unit": "ppm"
        }
      ]
    },
    "desired": {
      "peripherals": [
        {
          "name": "relay1",
          "type": "relay",
          "gpio": 26
        }
      ]
    }
  }
}
```

## Documentation

- ?? Guide complet: `IoT-Hub-Synchronization-Guide.md`
- ?? Tests HTTP: `Tests\14-IoT-Hub-Sync.http`

## Exemple de Réponse

### POST /api/IoTHub/devices/sync
```json
{
  "success": true,
  "syncedCount": 3,
  "message": "3 devices synchronisés avec succès depuis Azure IoT Hub"
}
```

### POST /api/IoTHub/peripherals/sync-all
```json
{
  "success": true,
  "totalSynced": 12,
  "message": "12 périphériques physiques synchronisés depuis IoT Hub"
}
```

## Prochaines Étapes

1. ? Tester avec votre IoT Hub "brainoffice"
2. ? Vérifier que vos ESP32 reportent bien les périphériques
3. ? Lancer la synchronisation
4. ? Vérifier les données dans la base de données
5. ?? Optionnel: Créer un job récurrent (Hangfire) pour synchroniser automatiquement toutes les heures
