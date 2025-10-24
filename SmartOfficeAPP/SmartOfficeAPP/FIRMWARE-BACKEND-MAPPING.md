# ?? Mapping Firmware ESP32 ? Backend ASP.NET

## ?? Table de Correspondance Complète

| # | Fonctionnalité Firmware | Fonction Firmware | Endpoint Backend | Service Backend | Modèle DB |
|---|-------------------------|-------------------|------------------|-----------------|-----------|
| **1** | **Métriques Système** | | | | |
| 1.1 | Rapport métriques | `reportMetrics()` | `GET /api/DeviceStatus/{id}/metrics/latest` | `DeviceStatusService.GetLatestMetricsAsync()` | `DeviceMetrics` |
| 1.2 | Heartbeat | `sendTelemetry("heartbeat")` | Auto via Background Worker | `TelemetrieIngestionWorker` | `Telemetrie` |
| 1.3 | RSSI WiFi | `WiFi.RSSI()` | `POST /api/DeviceStatus/{id}/metrics` | `DeviceStatusService.RecordMetricsAsync()` | `DeviceMetrics.Rssi` |
| 1.4 | Heap libre | `ESP.getFreeHeap()` | `POST /api/DeviceStatus/{id}/metrics` | `DeviceStatusService.RecordMetricsAsync()` | `DeviceMetrics.HeapFree` |
| 1.5 | Queue NVS | `nvsQueue.size()` | `POST /api/DeviceStatus/{id}/metrics` | `DeviceStatusService.RecordMetricsAsync()` | `DeviceMetrics.NvsQueueSize` |
| 1.6 | Uptime | `millis() / 1000` | `POST /api/DeviceStatus/{id}/metrics` | `DeviceStatusService.RecordMetricsAsync()` | `DeviceMetrics.UptimeSeconds` |
| 1.7 | Version firmware | `FIRMWARE_VERSION` | `POST /api/DeviceStatus/{id}/metrics` | `DeviceStatusService.RecordMetricsAsync()` | `DeviceMetrics.FirmwareVersion` |
| **2** | **RFID Access Control** | | | | |
| 2.1 | Scan carte autorisé | `sendTelemetryString("rfid", "last_scan_uid")` | `POST /api/DeviceStatus/{id}/rfid-events` | `DeviceStatusService.RecordRfidEventAsync()` | `RfidEvent` |
| 2.2 | Scan carte refusé | `sendLog("warn", "rfid", "Accès refusé")` | `POST /api/DeviceStatus/{id}/rfid-events` | `DeviceStatusService.RecordRfidEventAsync()` | `RfidEvent` |
| 2.3 | Historique scans | N/A | `GET /api/DeviceStatus/{id}/rfid-events` | `DeviceStatusService.GetRfidEventsAsync()` | `RfidEvent` |
| 2.4 | Scans par carte | N/A | `GET /api/DeviceStatus/rfid-events/by-card/{uid}` | `DeviceStatusService.GetRfidEventsByCardAsync()` | `RfidEvent` |
| 2.5 | Accès refusés | N/A | `GET /api/DeviceStatus/rfid-events/denied-access` | `DeviceStatusService.GetDeniedAccessAsync()` | `RfidEvent` |
| **3** | **Alarme Fumée** | | | | |
| 3.1 | Déclenchement | `declencherAlarmeFumee()` | `POST /api/DeviceStatus/{id}/smoke-alarms` | `DeviceStatusService.RecordSmokeAlarmAsync()` | `SmokeAlarmEvent` |
| 3.2 | Désactivation | `arreterAlarmeFumee()` | `POST /api/DeviceStatus/{id}/smoke-alarms/deactivate` | `DeviceStatusService.DeactivateSmokeAlarmAsync()` | `SmokeAlarmEvent` |
| 3.3 | État alarme | `alarmeFumee` | `GET /api/DeviceStatus/{id}/smoke-alarms/active` | `DeviceStatusService.GetActiveSmokeAlarmAsync()` | `SmokeAlarmEvent` |
| 3.4 | Historique | N/A | `GET /api/DeviceStatus/{id}/smoke-alarms` | `DeviceStatusService.GetSmokeAlarmsAsync()` | `SmokeAlarmEvent` |
| 3.5 | Toutes alarmes actives | N/A | `GET /api/DeviceStatus/smoke-alarms/all-active` | `DeviceStatusService.GetAllActiveAlarmsAsync()` | `SmokeAlarmEvent` |
| **4** | **Périphériques GPIO** | | | | |
| 4.1 | Configuration | `reportPeripheralsState()` | `GET /api/Devices/{id}/peripherals` | `DevicePeripheralService.GetPeripheralsByDeviceAsync()` | `DevicePeripheral` |
| 4.2 | Ajout périphérique | `applyDesiredPeripherals()` | `POST /api/Devices/{id}/peripherals` | `DevicePeripheralService.SyncPeripheralAsync()` | `DevicePeripheral` |
| 4.3 | État périphérique | `setPeripheralState()` | `POST /api/Devices/{id}/peripherals/{name}/commands/{cmd}` | `IotHubService.InvokeDeviceMethodAsync()` | N/A (Twin) |
| 4.4 | Statistiques énergie | `peripheralsEnergyTick()` | `GET /api/Devices/{id}/peripherals/energy` | `DevicePeripheralService.GetEnergyStatsAsync()` | `DevicePeripheral` |
| 4.5 | Reset énergie | `resetAllEnergy()` | `POST /api/Devices/{id}/peripherals/energy/reset` | `IotHubService.InvokeDeviceMethodAsync()` | N/A |
| **5** | **Télémétrie Capteurs** | | | | |
| 5.1 | Envoi télémétrie | `sendTelemetry()` | Auto via Background Worker | `TelemetrieIngestionWorker` | `Telemetrie` |
| 5.2 | Historique | N/A | `GET /api/telemetrie/history` | `TelemetrieService.GetHistoricalDataAsync()` | `Telemetrie` |
| 5.3 | Par périphérique | N/A | `GET /api/telemetrie/peripherique` | `TelemetrieService.GetTelemetriesByPeripheriqueIdAsync()` | `Telemetrie` |
| **6** | **Logs Firmware** | | | | |
| 6.1 | Envoi logs | `sendLog()` | Auto via Background Worker | `LogIngestionWorker` | `DeviceLog` |
| 6.2 | Recherche logs | N/A | `GET /api/DeviceLogs/{id}` | `DeviceLogService.GetLogsByDeviceIdAsync()` | `DeviceLog` |
| 6.3 | Logs par niveau | N/A | `GET /api/DeviceLogs/{id}/level/{level}` | `DeviceLogService.GetLogsByLevelAsync()` | `DeviceLog` |
| **7** | **Device Twin** | | | | |
| 7.1 | Rapport firmware info | `reportFirmwareInfo()` | `GET /api/IoTHub/device/{id}/twin` | `IotHubService.GetDeviceTwinAsync()` | N/A (Azure) |
| 7.2 | Rapport périphériques | `reportPeripheralsState()` | `GET /api/IoTHub/device/{id}/twin` | `IotHubService.GetDeviceTwinAsync()` | N/A (Azure) |
| 7.3 | Application desired | `applyDesiredPeripherals()` | `PUT /api/Devices/{id}/twin/desired` | `IotHubService.UpdateDesiredPropertiesAsync()` | N/A (Azure) |
| **8** | **Méthodes Directes** | | | | |
| 8.1 | Commande périphérique | `handleDirectMethod("peripheralCommand")` | `POST /api/IoTHub/device/{id}/method/peripheralCommand` | `IotHubService.InvokeDeviceMethodAsync()` | N/A |
| 8.2 | Get status | `handleDirectMethod("getStatus")` | `POST /api/IoTHub/device/{id}/method/getStatus` | `IotHubService.InvokeDeviceMethodAsync()` | N/A |
| 8.3 | Reboot | `handleDirectMethod("reboot")` | `POST /api/IoTHub/device/{id}/method/reboot` | `IotHubService.InvokeDeviceMethodAsync()` | N/A |
| 8.4 | Set telemetry interval | `handleDirectMethod("setTelemetryInterval")` | `POST /api/IoTHub/device/{id}/method/setTelemetryInterval` | `IotHubService.InvokeDeviceMethodAsync()` | N/A |
| 8.5 | Open/close door | `handleDirectMethod("openDoor"/"closeDoor")` | `POST /api/IoTHub/device/{id}/smart-commands/...` | `IotHubService.InvokeDeviceMethodAsync()` | N/A |
| 8.6 | Alarm on/off | `handleDirectMethod("alarmOn"/"alarmOff")` | `POST /api/IoTHub/device/{id}/smart-commands/...` | `IotHubService.InvokeDeviceMethodAsync()` | N/A |
| **9** | **Statut Complet** | | | | |
| 9.1 | Statut global | Multiple fonctions | `GET /api/DeviceStatus/{id}/full-status` | `DeviceStatusService.GetFullDeviceStatusAsync()` | Multiple |

---

## ?? Flux de Données Détaillés

### 1. Firmware ? Azure IoT Hub ? Backend (Télémétrie)

```
???????????????????????????????????????????????????????????????
? ESP32 Firmware                                              ?
???????????????????????????????????????????????????????????????
? sendTelemetry("peripheral", "metric", value)                ?
?   ? MQTT TLS (port 8883)                                    ?
???????????????????????????????????????????????????????????????
? Azure IoT Hub                                               ?
?   - Event Hub compatible endpoint                           ?
?   - Message retention: 1 day                                ?
?   ?                                                          ?
???????????????????????????????????????????????????????????????
? Backend - TelemetrieIngestionWorker                         ?
?   - Poll interval: 10s                                      ?
?   - Batch processing: 100 messages                          ?
?   ?                                                          ?
???????????????????????????????????????????????????????????????
? PostgreSQL - Telemetrie Table                               ?
?   - Indexed by (PeripheriqueLogiqueId, Horodatage)         ?
?   - Retention: configurable                                 ?
???????????????????????????????????????????????????????????????
```

### 2. Backend ? Azure IoT Hub ? Firmware (Commandes)

```
???????????????????????????????????????????????????????????????
? Backend API - IotHubService                                 ?
?   InvokeDeviceMethodAsync(deviceId, method, payload)        ?
?   ?                                                          ?
???????????????????????????????????????????????????????????????
? Azure IoT Hub                                               ?
?   - Direct Method invocation                                ?
?   - Timeout: 30s                                            ?
?   ? MQTT                                                    ?
???????????????????????????????????????????????????????????????
? ESP32 Firmware                                              ?
?   mqttCallback() ? handleDirectMethod()                     ?
?   - Execute command                                         ?
?   - Return response (status + payload)                      ?
?   ?                                                          ?
???????????????????????????????????????????????????????????????
? Backend API                                                 ?
?   - Receive response                                        ?
?   - Send SignalR notification (if error/success)            ?
???????????????????????????????????????????????????????????????
```

### 3. Device Twin Synchronisation

```
???????????????????????????????????????????????????????????????
? Backend API                                                 ?
?   UpdateDesiredPropertiesAsync(deviceId, json)              ?
?   ?                                                          ?
???????????????????????????????????????????????????????????????
? Azure IoT Hub - Device Twin                                 ?
?   - Desired Properties updated                              ?
?   - Version incremented                                     ?
?   ? MQTT                                                    ?
???????????????????????????????????????????????????????????????
? ESP32 Firmware                                              ?
?   handleTwinDesiredBody()                                   ?
?   - applyDesiredPeripherals()                               ?
?   - applyDesiredTelemetry()                                 ?
?   - Save to NVS                                             ?
?   ?                                                          ?
?   reportPeripheralsState()                                  ?
?   ? MQTT                                                    ?
???????????????????????????????????????????????????????????????
? Azure IoT Hub - Device Twin                                 ?
?   - Reported Properties updated                             ?
?   ?                                                          ?
???????????????????????????????????????????????????????????????
? Backend API                                                 ?
?   GetDeviceTwinAsync() ? Read reported state                ?
???????????????????????????????????????????????????????????????
```

---

## ?? Exemples Pratiques

### Exemple 1 : Scan RFID Autorisé

**Firmware** :
```cpp
// Carte autorisée détectée
String uidCarte = "a2de95ab";
sendTelemetryString("rfid", "last_scan_uid", uidCarte, "sensor");
sendLog("info", "rfid", "Accès autorisé : " + uidCarte, "rfid");
ouvrirPorte();
```

**Backend (auto-capture via Worker)** :
```csharp
// LogIngestionWorker détecte le log RFID
if (log.Source == "rfid" && log.Level == "info")
{
    await _deviceStatusService.RecordRfidEventAsync(new RfidEvent
    {
        IotDeviceId = log.DeviceId,
        CardUid = ExtractUidFromMessage(log.Message),
        AccessGranted = true,
        Action = "door_opened",
        Timestamp = log.Timestamp
    });
}
```

**Frontend (reçoit notification)** :
```javascript
connection.on("ReceiveNotification", (data) => {
  // Notification automatique pour accès refusés uniquement
  if (!data.accessGranted) {
    showAlert(data.title, data.message);
  }
});
```

---

### Exemple 2 : Commande Périphérique (Allumer LED)

**Frontend ? Backend** :
```http
POST /api/Devices/mon-esp32/peripherals/led_rdc/commands/on
Authorization: Bearer {token}
```

**Backend ? Azure IoT Hub** :
```csharp
var payload = JsonSerializer.Serialize(new {
    name = "led_rdc",
    command = "on",
    parameters = new {}
});
await _iotHubService.InvokeDeviceMethodAsync("mon-esp32", "peripheralCommand", payload);
```

**Firmware (exécute commande)** :
```cpp
void handleDirectMethod(const String& topic, const uint8_t* payload, unsigned int length) {
    if (method == "peripheralCommand") {
        String nameOrId = docIn["name"];
        String command = docIn["command"];
        Peripheral* per = findPeripheralByNameOrId(nameOrId);
        if (command == "on") {
            setPeripheralState(*per, true);
        }
        // Réponse
        mqtt.publish(respTopic.c_str(), "{\"ok\":true,\"state\":\"on\"}");
    }
}
```

---

### Exemple 3 : Récupération Statut Complet

**Frontend ? Backend** :
```http
GET /api/DeviceStatus/mon-esp32/full-status
Authorization: Bearer {token}
```

**Backend (agrège données)** :
```csharp
public async Task<DeviceFullStatusDto> GetFullDeviceStatusAsync(string iotDeviceId)
{
    // 1. Twin IoT Hub
    var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
    
    // 2. Dernières métriques
    var metrics = await _metricsRepo.GetLatestByDeviceIdAsync(iotDeviceId);
    
    // 3. Dernier scan RFID
    var rfid = await _rfidRepo.GetLatestByDeviceIdAsync(iotDeviceId);
    
    // 4. Alarme active
    var alarm = await _alarmRepo.GetActiveAlarmByDeviceIdAsync(iotDeviceId);
    
    // 5. Périphériques
    var peripherals = await _peripheralRepo.GetByIotDeviceIdAsync(iotDeviceId);
    
    return new DeviceFullStatusDto { ... };
}
```

**Réponse** :
```json
{
  "iotDeviceId": "mon-esp32",
  "isOnline": true,
  "lastActivity": "2025-01-23T14:30:00Z",
  "latestMetrics": {
    "rssi": -42,
    "heapFree": 245000,
    "uptimeSeconds": 7200,
    "firmwareVersion": "1.3.1"
  },
  "latestRfidEvent": {
    "cardUid": "a2de95ab",
    "accessGranted": true,
    "timestamp": "2025-01-23T14:25:00Z"
  },
  "activeAlarm": null,
  "peripherals": [
    {
      "name": "led_rdc",
      "type": "led",
      "gpio": 14,
      "state": "on",
      "energyWh": 0.25
    }
  ]
}
```

---

## ?? Points de Synchronisation Critiques

### 1. Timestamps UTC
- ? Firmware : `getISO8601()` utilise `gmtime_r()` (UTC)
- ? Backend : Tous les `DateTime` convertis en UTC avant BDD

### 2. Identifiants
- ? Firmware : `IotDeviceId` constant (`"mon-esp32"`)
- ? Backend : Utilise `IotDeviceId` comme clé primaire fonctionnelle

### 3. Format JSON
- ? Firmware : Utilise ArduinoJson (structure connue)
- ? Backend : DTOs fortement typés + AutoMapper

### 4. Gestion Erreurs
- ? Firmware : NVS Queue pour persistance offline
- ? Backend : Retry logic dans Workers + deadletter queue

---

## ?? Métriques de Performance

### Latences Typiques
```
Firmware ? Azure IoT Hub     : ~50-100ms (MQTT over TLS)
Azure IoT Hub ? Backend      : ~1-5s (polling interval)
Backend ? Firmware (command) : ~100-300ms (direct method)
Backend ? Frontend (SignalR) : ~10-50ms (WebSocket)
```

### Throughput
```
Télémétrie    : 1 message / 10s / device (configurable)
Métriques     : 1 message / 60s / device
Logs          : Variable (événements)
Commands      : À la demande
```

---

## ? Checklist Uniformité

- [x] Tous les événements firmware ont un endpoint backend
- [x] Tous les modèles de données firmware persistent en BDD
- [x] Toutes les commandes backend sont exécutables sur firmware
- [x] Formats JSON compatibles (firmware ? backend)
- [x] Gestion timestamps UTC cohérente
- [x] SignalR notifications pour événements critiques
- [x] Documentation mapping complète
- [x] Tests end-to-end disponibles

---

**Status** : ? **Backend 100% Uniforme avec Firmware ESP32 v1.3.1**

**Auteur** : GitHub Copilot  
**Date** : 2025-01-23
