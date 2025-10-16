# Guide Déploiement Azure Functions - Ingestion Télémétrie & Logs

## Vue d'ensemble

Ce guide explique comment déployer deux Azure Functions pour ingérer les données IoT Hub (télémétrie et logs) et les persister dans PostgreSQL avec notifications SignalR en temps réel.

### Architecture

```
ESP32 Device
    ? (D2C messages)
Azure IoT Hub
    ? (Event Hub compatible endpoint)
    ??? TelemetryIngestionFunction (consumer group: $Default)
    ?       ??? PostgreSQL (Telemetries table)
    ?       ??? SignalR Hub (temps réel)
    ?
    ??? LogIngestionFunction (consumer group: logs)
            ??? PostgreSQL (DeviceLogs table)
            ??? SignalR Hub (alertes error/warn)
```

---

## Option 1: Utiliser les Background Workers dans l'API (Recommandé)

### ? Avantages:
- Déjà implémenté dans le backend
- Pas besoin de déployer des Azure Functions séparées
- Moins coûteux (pas de frais supplémentaires)
- Plus simple à maintenir

### Configuration Backend (déjà en place):
```csharp
// Program.cs
builder.Services.AddHostedService<TelemetrieIngestionWorker>();
builder.Services.AddHostedService<LogIngestionWorker>();
```

### User Secrets à configurer:
```bash
dotnet user-secrets set "IotHubEventHub:Enabled" "true"
dotnet user-secrets set "IotHubEventHub:LogsEnabled" "true"
dotnet user-secrets set "IotHubEventHub:ConnectionString" "Endpoint=sb://iothub-ns-...servicebus.windows.net/;SharedAccessKeyName=service;SharedAccessKey=..."
dotnet user-secrets set "IotHubEventHub:Name" "your-iothub-event-hub-name"
dotnet user-secrets set "IotHubEventHub:BlobConnectionString" "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
dotnet user-secrets set "IotHubEventHub:BlobContainer" "iothub-checkpoints"
dotnet user-secrets set "IotHubEventHub:BlobContainerLogs" "iothub-logs-checkpoints"
```

### Créer les consumer groups dans IoT Hub:
```bash
# Consumer group pour logs (le $Default est déjà présent)
az iot hub consumer-group create \
  --name logs \
  --hub-name <your-iot-hub-name> \
  --resource-group <your-resource-group>
```

---

## Option 2: Déployer des Azure Functions séparées

### Quand utiliser cette option:
- Vous voulez séparer la logique d'ingestion de l'API principale
- Vous avez besoin d'une scalabilité indépendante
- Vous préférez une architecture microservices

### 1. Créer un nouveau projet Azure Functions

```bash
# Installer les templates Azure Functions
dotnet new install Microsoft.Azure.Functions.Worker.ProjectTemplates

# Créer le projet
dotnet new func --name SmartOfficeTelemetryFunctions --worker-runtime dotnet-isolated

cd SmartOfficeTelemetryFunctions

# Ajouter les packages nécessaires
dotnet add package Microsoft.Azure.Functions.Worker
dotnet add package Microsoft.Azure.Functions.Worker.Sdk
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.EventHubs
dotnet add package Npgsql
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package System.Text.Json
```

### 2. Structure du projet

```
SmartOfficeTelemetryFunctions/
??? Functions/
?   ??? TelemetryIngestionFunction.cs
?   ??? LogIngestionFunction.cs
??? Services/
?   ??? DatabaseService.cs
?   ??? SignalRNotificationService.cs
??? Models/
?   ??? TelemetryMessage.cs
?   ??? LogMessage.cs
??? host.json
??? local.settings.json
??? SmartOfficeTelemetryFunctions.csproj
```

### 3. Configuration local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "IoTHubEventHubConnectionString": "Endpoint=sb://...;SharedAccessKeyName=service;SharedAccessKey=...;EntityPath=your-event-hub",
    "PostgreSQLConnectionString": "Host=your-postgres-server.postgres.database.azure.com;Database=smartoffice;Username=adminuser;Password=YourPassword;SSL Mode=Require",
    "SignalRHubUrl": "https://your-api.azurewebsites.net/notificationhub",
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=yourstorageaccount;AccountKey=...;EndpointSuffix=core.windows.net"
  }
}
```

### 4. Déployer vers Azure

```bash
# Publier vers Azure
func azure functionapp publish SmartOfficeTelemetryApp

# Ou avec Azure CLI
az functionapp deployment source config-zip \
  --resource-group <your-resource-group> \
  --name SmartOfficeTelemetryApp \
  --src ./publish.zip
```

### 5. Configurer les variables d'environnement dans Azure

```bash
az functionapp config appsettings set \
  --name SmartOfficeTelemetryApp \
  --resource-group <your-resource-group> \
  --settings \
    IoTHubEventHubConnectionString="<connection-string>" \
    PostgreSQLConnectionString="<connection-string>" \
    SignalRHubUrl="https://your-api.azurewebsites.net/notificationhub" \
    StorageConnectionString="<blob-storage-connection-string>"
```

---

## Configuration Azure IoT Hub

### 1. Obtenir la connection string Event Hub compatible

```bash
# Via Azure CLI
az iot hub connection-string show \
  --hub-name <your-iot-hub-name> \
  --policy-name service \
  --key primary
```

### 2. Obtenir le nom de l'Event Hub

```bash
# Via Azure CLI (généralement au format: iothub-ehub-{hub-name}-{random})
az iot hub show \
  --name <your-iot-hub-name> \
  --query "properties.eventHubEndpoints.events.path" \
  --output tsv
```

### 3. Créer les consumer groups

```bash
# Consumer group par défaut ($Default) existe déjà
# Créer un consumer group pour les logs
az iot hub consumer-group create \
  --name logs \
  --hub-name <your-iot-hub-name> \
  --resource-group <your-resource-group>
```

---

## Configuration Azure Blob Storage (Checkpoints)

### 1. Créer un Storage Account

```bash
az storage account create \
  --name smartofficecheckpoints \
  --resource-group <your-resource-group> \
  --location <your-region> \
  --sku Standard_LRS
```

### 2. Créer les containers

```bash
# Container pour checkpoints télémétrie
az storage container create \
  --name iothub-checkpoints \
  --account-name smartofficecheckpoints

# Container pour checkpoints logs
az storage container create \
  --name iothub-logs-checkpoints \
  --account-name smartofficecheckpoints
```

### 3. Obtenir la connection string

```bash
az storage account show-connection-string \
  --name smartofficecheckpoints \
  --resource-group <your-resource-group> \
  --output tsv
```

---

## Configuration PostgreSQL Azure

### 1. Créer un serveur PostgreSQL

```bash
az postgres flexible-server create \
  --name smartoffice-db \
  --resource-group <your-resource-group> \
  --location <your-region> \
  --admin-user adminuser \
  --admin-password YourSecurePassword123! \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 15 \
  --storage-size 32 \
  --public-access 0.0.0.0
```

### 2. Configurer le firewall

```bash
# Autoriser les services Azure
az postgres flexible-server firewall-rule create \
  --resource-group <your-resource-group> \
  --name smartoffice-db \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Autoriser votre IP locale (pour migrations)
az postgres flexible-server firewall-rule create \
  --resource-group <your-resource-group> \
  --name smartoffice-db \
  --rule-name AllowMyIP \
  --start-ip-address <your-ip> \
  --end-ip-address <your-ip>
```

### 3. Appliquer les migrations

```bash
# Dans votre projet backend
dotnet ef database update

# Ou via script SQL direct
psql "host=smartoffice-db.postgres.database.azure.com port=5432 dbname=smartoffice user=adminuser password=YourSecurePassword123! sslmode=require" < migration.sql
```

---

## Format des messages ESP32

### Message de Télémétrie

```cpp
// Property à ajouter dans le message IoT Hub
message.properties.add("type", "telemetry");

// Body JSON
{
  "deviceId": "esp32-office-101",
  "peripherals": [
    {
      "name": "relay_lights_office",
      "type": "relay",
      "gpio": 33,
      "state": 1,
      "p_mw": 5000,
      "e_mwh": 120000
    },
    {
      "name": "temp_sensor_room",
      "type": "temp",
      "gpio": 32,
      "temperature": 23.5,
      "humidity": 45.2
    }
  ],
  "timestamp": "2025-01-15T14:30:00Z",
  "metadata": {
    "rssi": -58,
    "heapFree": 123456,
    "uptime": 3600000
  }
}
```

### Message de Log

```cpp
// Property à ajouter dans le message IoT Hub
message.properties.add("type", "log");

// Body JSON
{
  "deviceId": "esp32-office-101",
  "level": "error",
  "source": "peripheral",
  "peripheral": "relay_lights_office",
  "message": "Failed to set GPIO state",
  "timestamp": "2025-01-15T14:30:00Z",
  "context": {
    "errorCode": "GPIO_ERROR",
    "gpio": 33,
    "attemptCount": 3
  }
}
```

---

## Monitoring & Troubleshooting

### 1. Vérifier les logs Azure Functions

```bash
# Via Azure CLI
az functionapp log tail \
  --name SmartOfficeTelemetryApp \
  --resource-group <your-resource-group>
```

### 2. Vérifier les logs Background Workers (Backend)

```bash
# Dans les logs de l'application
# Rechercher:
# - "Service d'ingestion télémétrie Event Hub initialisé"
# - "Service d'ingestion logs Event Hub initialisé"
# - "Télémétrie persistée"
# - "Log persisté"
```

### 3. Vérifier les messages dans IoT Hub

```bash
# Surveiller les messages D2C
az iot hub monitor-events \
  --hub-name <your-iot-hub-name> \
  --device-id esp32-office-101
```

### 4. Vérifier les checkpoints Blob Storage

```bash
# Lister les blobs de checkpoint
az storage blob list \
  --container-name iothub-checkpoints \
  --account-name smartofficecheckpoints
```

### 5. Vérifier les données en base

```sql
-- Vérifier les télémétries
SELECT COUNT(*) FROM "Telemetries";
SELECT * FROM "Telemetries" ORDER BY "Horodatage" DESC LIMIT 10;

-- Vérifier les logs
SELECT COUNT(*) FROM "DeviceLogs";
SELECT * FROM "DeviceLogs" ORDER BY "Timestamp" DESC LIMIT 10;
```

---

## Performance & Scalabilité

### Background Workers (Backend):
- **Avantages**: Simple, pas de coût supplémentaire
- **Limites**: 
  - Scalabilité limitée à l'instance de l'API
  - Doit rester en exécution constante
- **Recommandé pour**: Faible à moyen volume (<1000 messages/min)

### Azure Functions:
- **Avantages**: Scalabilité automatique, isolation
- **Coûts**: 
  - Consumption Plan: ~0.20€/million d'exécutions
  - Premium Plan: ~140€/mois (scalabilité garantie)
- **Recommandé pour**: Haut volume (>1000 messages/min)

---

## Checklist de Déploiement

### Prérequis Azure:
- [ ] IoT Hub créé et configuré
- [ ] PostgreSQL Flexible Server créé
- [ ] Blob Storage Account créé
- [ ] Consumer groups créés ($Default, logs)
- [ ] Firewall rules configurées

### Configuration Backend:
- [ ] User Secrets configurés (connexions IoT Hub, DB, Blob)
- [ ] Migration DeviceLogs appliquée (`dotnet ef database update`)
- [ ] Background Workers activés (IotHubEventHub:Enabled=true)
- [ ] SignalR Hub accessible

### Tests:
- [ ] Envoyer un message D2C depuis ESP32
- [ ] Vérifier persistance en DB (Telemetries)
- [ ] Envoyer un log error depuis ESP32
- [ ] Vérifier persistance en DB (DeviceLogs)
- [ ] Vérifier alerte SignalR pour log error
- [ ] Tester API DeviceLogs endpoints

---

## Scripts de Déploiement Automatique

### deploy-infra.sh

```bash
#!/bin/bash

RESOURCE_GROUP="smartoffice-rg"
LOCATION="francecentral"
IOT_HUB_NAME="smartoffice-iothub"
STORAGE_NAME="smartofficecheckpoints"
POSTGRES_SERVER="smartoffice-db"
POSTGRES_ADMIN="adminuser"
POSTGRES_PASSWORD="YourSecurePassword123!"

# Créer le resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Créer IoT Hub
az iot hub create \
  --name $IOT_HUB_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku S1

# Créer consumer group logs
az iot hub consumer-group create \
  --name logs \
  --hub-name $IOT_HUB_NAME \
  --resource-group $RESOURCE_GROUP

# Créer Storage Account
az storage account create \
  --name $STORAGE_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS

# Créer les containers
az storage container create \
  --name iothub-checkpoints \
  --account-name $STORAGE_NAME

az storage container create \
  --name iothub-logs-checkpoints \
  --account-name $STORAGE_NAME

# Créer PostgreSQL
az postgres flexible-server create \
  --name $POSTGRES_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $POSTGRES_ADMIN \
  --admin-password $POSTGRES_PASSWORD \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --version 15 \
  --storage-size 32 \
  --public-access 0.0.0.0

echo "? Infrastructure déployée avec succès!"
```

---

## Conclusion

**Recommandation**: Pour la plupart des cas d'usage, utilisez les **Background Workers** déjà implémentés dans le backend. Ils sont simples, efficaces et suffisants pour des volumes moyens.

Passez aux **Azure Functions** uniquement si vous avez besoin de:
- Scalabilité automatique pour très haut volume
- Isolation complète de la logique d'ingestion
- Architecture distribuée complexe

**L'implémentation actuelle avec les Background Workers est production-ready et recommandée!** ?
