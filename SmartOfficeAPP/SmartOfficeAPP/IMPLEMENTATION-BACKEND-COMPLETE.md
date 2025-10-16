# Résumé de l'implémentation - DeviceLogs & Background Workers

## Modifications apportées

### 1. **Modèle DeviceLog** ?
- **Fichier créé**: `SmartOfficeAPP/Models/DeviceLog.cs`
- **Fonctionnalités**:
  - Représente un log provenant d'un device IoT (ESP32)
  - Champs: `Id`, `DeviceId`, `Peripheral`, `Level`, `Source`, `Message`, `Timestamp`, `Context`, `CreatedAt`
  - Niveaux de log supportés: `info`, `warn`, `error`, `debug`
  - Sources: `firmware`, `peripheral`, `network`

### 2. **DbContext mis à jour** ?
- **Fichier modifié**: `SmartOfficeAPP/Data/ApplicationDbContext.cs`
- **Changements**:
  - Ajout du `DbSet<DeviceLog> DeviceLogs`
  - Index composite sur `(DeviceId, Timestamp)` pour recherche optimisée
  - Index sur `Level` pour filtrage rapide par niveau
  - Index sur `Peripheral` pour recherche par périphérique

### 3. **Repository DeviceLog** ?
- **Fichiers créés**:
  - `SmartOfficeAPP/Repositories/IRepositories/IDeviceLogRepository.cs`
  - `SmartOfficeAPP/Repositories/Repos/DeviceLogRepository.cs`
- **Fonctionnalités**:
  - `AddLogAsync()` - Ajouter un log
  - `GetLogsByDeviceIdAsync()` - Récupérer les logs d'un device
  - `GetLogsByLevelAsync()` - Filtrer par niveau (error, warn, info)
  - `GetLogsByPeripheralAsync()` - Filtrer par périphérique
  - `GetRecentLogsAsync()` - Récupérer les logs récents
  - `GetLogsByDeviceIdAndDateRangeAsync()` - Filtrer par plage de dates

### 4. **Service DeviceLog** ?
- **Fichiers créés**:
  - `SmartOfficeAPP/Services/IServices/IDeviceLogService.cs`
  - `SmartOfficeAPP/Services/ServicesImpl/DeviceLogService.cs`
  - `SmartOfficeAPP/DTOs/DeviceLogDto.cs`
- **Fonctionnalités**:
  - Couche service avec mapping AutoMapper
  - Validation et logging
  - Interface claire pour les contrôleurs

### 5. **AutoMapper mis à jour** ?
- **Fichier modifié**: `SmartOfficeAPP/Helpers/MappingProfile.cs`
- **Changements**:
  - Ajout mapping `DeviceLog ? DeviceLogDto`

### 6. **Contrôleur DeviceLogs** ?
- **Fichier créé**: `SmartOfficeAPP/Controllers/DeviceLogsController.cs`
- **Endpoints**:
  - `GET /api/DeviceLogs/recent?limit=100` - Logs récents
  - `GET /api/DeviceLogs/device/{deviceId}?limit=100` - Logs d'un device
  - `GET /api/DeviceLogs/device/{deviceId}/range?startDate=...&endDate=...` - Logs par plage de dates
  - `GET /api/DeviceLogs/level/{level}?limit=100` - Logs par niveau (error, warn, info)
  - `GET /api/DeviceLogs/peripheral/{peripheralName}?limit=100` - Logs d'un périphérique
  - `POST /api/DeviceLogs` - Ajouter un log manuellement (Admin only)

### 7. **LogIngestionWorker** ?
- **Fichier créé**: `SmartOfficeAPP/Services/Background/LogIngestionWorker.cs`
- **Fonctionnalités**:
  - BackgroundService pour ingestion des logs depuis IoT Hub Event Hub
  - Consumer group séparé: `logs`
  - Filtre les messages avec `type=log`
  - Parse et persiste les logs en base de données
  - Envoie des alertes SignalR pour les logs `error` et `warn`
  - Gestion robuste des erreurs et checkpoints

### 8. **SignalRService étendu** ?
- **Fichiers modifiés**:
  - `SmartOfficeAPP/Services/IServices/ISignalRService.cs`
  - `SmartOfficeAPP/Services/ServicesImpl/SignalRService.cs`
- **Nouvelle méthode**:
  - `SendLogAlertAsync(object logData)` - Diffuse les alertes de log en temps réel

### 9. **Program.cs mis à jour** ?
- **Fichier modifié**: `SmartOfficeAPP/Program.cs`
- **Changements**:
  - Enregistrement de `IDeviceLogRepository` et `IDeviceLogService`
  - Ajout du `LogIngestionWorker` comme HostedService
  - Les deux workers (TelemetrieIngestionWorker et LogIngestionWorker) fonctionnent en parallèle

### 10. **Migration EF Core** ?
- **Commande exécutée**: `dotnet ef migrations add AddDeviceLogsTable`
- **Table créée**: `DeviceLogs` avec tous les index
- **À faire**: Exécuter `dotnet ef database update` pour appliquer la migration

### 11. **Configuration appsettings.json** ?
- **Fichier modifié**: `SmartOfficeAPP/appsettings.json`
- **Nouvelles configurations**:
  ```json
  "IotHubEventHub": {
    "Enabled": true,
    "LogsEnabled": true,
    "ConnectionString": "",
    "Name": "brainoffice",
    "BlobConnectionString": "",
    "BlobContainer": "iothub-checkpoints",
    "BlobContainerLogs": "iothub-logs-checkpoints"
  }
  ```

---

## Architecture d'ingestion des logs

```
ESP32 (Device IoT)
    ? (envoie log avec property type=log)
Azure IoT Hub
    ? (route vers Event Hub)
Event Hub (Consumer Group: "logs")
    ? (traitement par)
LogIngestionWorker (BackgroundService)
    ??? Parse JSON log
    ??? Persiste dans DeviceLogs (PostgreSQL)
    ??? Envoie alerte SignalR (si error/warn)
            ?
    Clients Web (Temps réel)
```

---

## Format du message de log (ESP32)

### Message envoyé par le firmware:

```json
{
  "deviceId": "esp32-office-101",
  "level": "error",
  "source": "peripheral",
  "peripheral": "relay_lights_office",
  "message": "Failed to set GPIO state",
  "timestamp": "2025-01-15T14:30:00Z",
  "context": {
    "errorCode": "GPIO_ERROR",
    "gpio": 33
  }
}
```

### Properties du message IoT Hub:
- `type`: `"log"` (obligatoire pour être traité par LogIngestionWorker)

---

## Configuration IoT Hub Event Hub

### User Secrets (pour développement):

```bash
# Consumer group pour logs (à créer dans IoT Hub)
dotnet user-secrets set "IotHubEventHub:LogsEnabled" "true"
dotnet user-secrets set "IotHubEventHub:BlobContainerLogs" "iothub-logs-checkpoints"
```

### Créer le consumer group dans Azure:

```bash
# Via Azure CLI
az iot hub consumer-group create \
  --name logs \
  --hub-name <your-iot-hub-name> \
  --resource-group <your-resource-group>
```

---

## Tests API

### 1. Récupérer les logs récents:
```http
GET https://localhost:7152/api/DeviceLogs/recent?limit=50
Authorization: Bearer <JWT_TOKEN>
```

### 2. Récupérer les logs d'un device:
```http
GET https://localhost:7152/api/DeviceLogs/device/esp32-office-101?limit=100
Authorization: Bearer <JWT_TOKEN>
```

### 3. Récupérer les logs par niveau (errors):
```http
GET https://localhost:7152/api/DeviceLogs/level/error?limit=100
Authorization: Bearer <JWT_TOKEN>
```

### 4. Récupérer les logs d'un périphérique:
```http
GET https://localhost:7152/api/DeviceLogs/peripheral/relay_lights_office?limit=50
Authorization: Bearer <JWT_TOKEN>
```

### 5. Récupérer les logs par plage de dates:
```http
GET https://localhost:7152/api/DeviceLogs/device/esp32-office-101/range?startDate=2025-01-01T00:00:00Z&endDate=2025-01-31T23:59:59Z
Authorization: Bearer <JWT_TOKEN>
```

### 6. Ajouter un log manuellement (test):
```http
POST https://localhost:7152/api/DeviceLogs
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json

{
  "deviceId": "esp32-office-101",
  "peripheral": "relay_lights_office",
  "level": "error",
  "source": "firmware",
  "message": "Test log manuel",
  "timestamp": "2025-01-15T14:30:00Z",
  "context": "{\"testKey\": \"testValue\"}"
}
```

---

## Monitoring SignalR (Logs Alerts)

### Écouter les alertes de logs (JavaScript):

```javascript
connection.on("LogAlert", (logData) => {
    console.log("?? Alerte Log:", logData);
    // logData contient:
    // - deviceId
    // - level (error/warn)
    // - source
    // - message
    // - peripheral (optionnel)
    // - timestamp
});
```

---

## Prochaines étapes

### À faire immédiatement:
1. ? Exécuter la migration: `dotnet ef database update`
2. ? Configurer le consumer group `logs` dans IoT Hub
3. ? Tester l'ingestion des logs depuis un ESP32
4. ? Vérifier les alertes SignalR pour les logs error/warn

### Tests d'intégration:
1. Envoyer un log `info` ? Vérifier persistance en DB
2. Envoyer un log `error` ? Vérifier persistance + alerte SignalR
3. Tester les filtres API (par device, niveau, périphérique, dates)
4. Vérifier les performances avec un grand volume de logs

### Optimisations futures:
- [ ] Ajouter un système de rétention des logs (suppression automatique après X jours)
- [ ] Implémenter une pagination pour les endpoints GET
- [ ] Ajouter des statistiques de logs (nombre d'errors par device, etc.)
- [ ] Créer un dashboard temps réel pour les logs critiques

---

## Checklist de déploiement

### Backend:
- [x] Migration DeviceLogs créée
- [x] Repository et Service implémentés
- [x] Contrôleur API créé
- [x] LogIngestionWorker configuré
- [x] SignalR étendu pour alerts logs
- [x] Configuration appsettings.json mise à jour

### IoT Hub:
- [ ] Consumer group `logs` créé
- [ ] Event Hub connection string configurée (user secrets)
- [ ] Blob Storage checkpoint container créé (`iothub-logs-checkpoints`)

### Tests:
- [ ] Tester Tests/11-Device-Peripherals.http
- [ ] Vérifier sync desired ? reported
- [ ] Tester commandes peripheralCommand
- [ ] Vérifier télémétrie en DB
- [ ] Tester SignalR notifications
- [ ] Tester ingestion logs D2C

---

## Résumé des nouveaux fichiers créés

```
SmartOfficeAPP/
??? Models/
?   ??? DeviceLog.cs (NOUVEAU)
??? DTOs/
?   ??? DeviceLogDto.cs (NOUVEAU)
??? Repositories/
?   ??? IRepositories/
?   ?   ??? IDeviceLogRepository.cs (NOUVEAU)
?   ??? Repos/
?       ??? DeviceLogRepository.cs (NOUVEAU)
??? Services/
?   ??? IServices/
?   ?   ??? IDeviceLogService.cs (NOUVEAU)
?   ?   ??? ISignalRService.cs (MODIFIÉ)
?   ??? ServicesImpl/
?   ?   ??? DeviceLogService.cs (NOUVEAU)
?   ?   ??? SignalRService.cs (MODIFIÉ)
?   ??? Background/
?       ??? LogIngestionWorker.cs (NOUVEAU)
??? Controllers/
?   ??? DeviceLogsController.cs (NOUVEAU)
??? Migrations/
?   ??? XXXXXXXXX_AddDeviceLogsTable.cs (NOUVEAU)
??? Data/
?   ??? ApplicationDbContext.cs (MODIFIÉ)
??? Helpers/
?   ??? MappingProfile.cs (MODIFIÉ)
??? Program.cs (MODIFIÉ)
??? appsettings.json (MODIFIÉ)
```

---

## Commandes finales

```bash
# 1. Appliquer la migration
cd SmartOfficeAPP
dotnet ef database update

# 2. Compiler le projet
dotnet build

# 3. Exécuter le projet
dotnet run

# 4. Tester les endpoints
# Utiliser le fichier Tests/11-Device-Peripherals.http
```

---

**? Implémentation terminée !**

Tous les fichiers manquants ont été créés et les recommandations ont été appliquées. Le backend est maintenant complet avec :
- Ingestion de télémétrie (TelemetrieIngestionWorker)
- Ingestion de logs (LogIngestionWorker)
- API complète pour gérer les logs
- SignalR pour notifications en temps réel
- DevicePeripherals pour gestion physique des périphériques
- Tous les services, repositories et contrôleurs nécessaires
