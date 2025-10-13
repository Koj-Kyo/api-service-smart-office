# Configuration Azure IoT Hub pour SmartOffice

## Vue d'ensemble

Cette documentation explique comment configurer et utiliser Azure IoT Hub avec l'API SmartOffice pour :
- Contrôler les périphériques IoT en temps réel
- Recevoir et traiter la télémétrie
- Calculer des statistiques de consommation énergétique
- Détecter des anomalies
- Générer des rapports

## Prérequis Azure

### 1. Création d'IoT Hub
```bash
# Créer un groupe de ressources
az group create --name rg-smartoffice --location "West Europe"

# Créer IoT Hub
az iot hub create --name smartoffice-iothub --resource-group rg-smartoffice --sku S1
```

### 2. Récupération des chaînes de connexion
```bash
# Chaîne de connexion du service
az iot hub connection-string show --hub-name smartoffice-iothub --policy-name service

# Chaîne de connexion Event Hub compatible
az iot hub connection-string show --hub-name smartoffice-iothub --default-eventhub
```

### 3. Création du stockage pour checkpoints
```bash
# Créer un compte de stockage
az storage account create --name smartofficestg --resource-group rg-smartoffice --location "West Europe" --sku Standard_LRS

# Récupérer la chaîne de connexion
az storage account show-connection-string --name smartofficestg --resource-group rg-smartoffice
```

## Configuration appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=SmartOfficeDB;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "Secret": "your-jwt-secret-key",
    "Issuer": "SmartOfficeAPI",
    "Audience": "SmartOfficeClient"
  },
  "IotHubConnectionString": "HostName=smartoffice-iothub.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=your-service-key",
  "IotHubEventHub": {
    "ConnectionString": "Endpoint=sb://ihsuproddmres001dednamespace.servicebus.windows.net/;SharedAccessKeyName=service;SharedAccessKey=your-eventhub-key;EntityPath=smartoffice-iothub",
    "Name": "smartoffice-iothub",
    "BlobConnectionString": "DefaultEndpointsProtocol=https;AccountName=smartofficestg;AccountKey=your-storage-key;EndpointSuffix=core.windows.net",
    "BlobContainer": "iothub-checkpoints"
  }
}
```

## Architecture des périphériques IoT

### Format des messages télémétrie (v2)
```json
{
  "deviceId": "smartoffice-device-001",
  "timestamp": "2024-01-15T14:30:00Z",
  "peripherals": [
    {
      "name": "led_strip_1",
      "type": "lighting",
      "gpio": 2,
      "state": 1,
      "p_mw": 850,
      "e_mwh": 12450,
      "brightness": 75,
      "temperature": 23.5
    },
    {
      "name": "smart_plug_1",
      "type": "outlet",
      "gpio": 4,
      "state": 0,
      "p_mw": 0,
      "e_mwh": 8230
    }
  ]
}
```

### Méthodes directes supportées
1. **setState** - Contrôler l'état on/off
   ```json
   {"state": 1}
   ```

2. **setBrightness** - Régler la luminosité
   ```json
   {"brightness": 75}
   ```

3. **setTemperature** - Régler la température
   ```json
   {"temperature": 22.5}
   ```

4. **getStatus** - Obtenir le statut actuel
   ```json
   {}
   ```

5. **reset** - Redémarrer le périphérique
   ```json
   {}
   ```

### Device Twin - Desired Properties
```json
{
  "properties": {
    "desired": {
      "schedules": [
        {
          "name": "work_hours",
          "days": ["mon", "tue", "wed", "thu", "fri"],
          "start": "08:00",
          "end": "18:00",
          "action": "turn_on"
        }
      ],
      "energy_saving": {
        "enabled": true,
        "max_power_mw": 1000,
        "auto_off_minutes": 30
      },
      "reporting_interval_seconds": 30
    }
  }
}
```

## Utilisation des APIs

### 1. Contrôle des périphériques

#### Commandes directes
```http
POST /api/IoTHub/device/{deviceId}/method/{methodName}
Authorization: Bearer {token}

{
  "payload": "{\"state\": 1}"
}
```

#### Commandes smart prédéfinies
```http
POST /api/IoTHub/device/{deviceId}/smart-commands/turn-on
Authorization: Bearer {token}
```

#### Mise à jour des propriétés désirées
```http
PUT /api/IoTHub/device/{deviceId}/twin/desired
Authorization: Bearer {token}

{
  "desiredProperties": "{\"targetTemperature\": 22.5}"
}
```

### 2. Statistiques de consommation

#### Statistiques par périphérique
```http
GET /api/ConsommationStats/peripherique/1?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer {token}
```

#### Rapport de consommation
```http
GET /api/ConsommationStats/rapport?zoneId=1&startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer {token}
```

#### Top des consommateurs
```http
GET /api/ConsommationStats/top-consommateurs?startDate=2024-01-01&endDate=2024-01-31&top=10
Authorization: Bearer {token}
```

#### Détection d'anomalies
```http
GET /api/ConsommationStats/peripherique/1/anomalies?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer {token}
```

### 3. Données temps réel via SignalR

#### Connexion côté client
```javascript
const connection = new HubConnectionBuilder()
    .withUrl('/notificationhub', {
        accessTokenFactory: () => localStorage.getItem('accessToken')
    })
    .withAutomaticReconnect()
    .build();

// Écouter les mises à jour de télémétrie
connection.on("TelemetryUpdate", (telemetry) => {
    updateDashboard(telemetry.peripheriqueId, telemetry.data);
});

// Écouter les mises à jour de statut
connection.on("DeviceStatusUpdate", (update) => {
    updateDeviceStatus(update.peripheriqueId, update.status);
});
```

## Firmware des périphériques

### Exemple de code Arduino/ESP32
```cpp
#include <WiFi.h>
#include <ArduinoJson.h>
#include <AzureIoTHub.h>

// Configuration périphériques
struct Peripheral {
  String name;
  String type;
  int gpio;
  int state;
  float power_mw;
  float energy_mwh;
};

Peripheral peripherals[] = {
  {"led_strip_1", "lighting", 2, 0, 0, 0},
  {"smart_plug_1", "outlet", 4, 0, 0, 0}
};

void sendTelemetry() {
  DynamicJsonDocument doc(1024);
  JsonArray periphArray = doc.createNestedArray("peripherals");
  
  for (auto& periph : peripherals) {
    JsonObject p = periphArray.createNestedObject();
    p["name"] = periph.name;
    p["type"] = periph.type;
    p["gpio"] = periph.gpio;
    p["state"] = periph.state;
    p["p_mw"] = periph.power_mw;
    p["e_mwh"] = periph.energy_mwh;
  }
  
  String telemetryJson;
  serializeJson(doc, telemetryJson);
  
  IoTHubClient_SendEventAsync(telemetryJson.c_str());
}

// Gestionnaire de méthodes directes
int setState(const char* methodName, const unsigned char* payload, size_t size) {
  DynamicJsonDocument doc(256);
  deserializeJson(doc, payload, size);
  
  int state = doc["state"];
  
  // Appliquer l'état à tous les périphériques ou spécifique
  for (auto& periph : peripherals) {
    periph.state = state;
    digitalWrite(periph.gpio, state);
    
    // Simuler la consommation
    if (state) {
      periph.power_mw = random(50, 100);
    } else {
      periph.power_mw = 0;
    }
  }
  
  return 200; // Succès
}
```

## Monitoring et observabilité

### Métriques Azure Monitor
- Nombre de messages Device-to-Cloud
- Nombre de méthodes directes
- Statut de connexion des périphériques
- Erreurs de communication

### Logs Application Insights
```csharp
// Dans le TelemetrieIngestionWorker
_logger.LogInformation("Télémétrie reçue: Device={DeviceId}, Périphériques={Count}", 
    deviceId, peripheralCount);

// Dans IoTHubService
_logger.LogWarning("Échec de la méthode {MethodName} sur {DeviceId}: {Error}", 
    methodName, deviceId, error);
```

### Alertes recommandées
1. **Périphérique déconnecté** > 5 minutes
2. **Consommation anormale** > 150% de la moyenne
3. **Échec de commande** > 3 tentatives
4. **Anomalie énergétique** détectée

## Sécurité

### Authentification des périphériques
- Certificats X.509 (recommandé pour production)
- Clés symétriques (développement/test)
- Token SAS avec rotation automatique

### Chiffrement
- TLS 1.2 pour toutes les communications
- Chiffrement des données au repos
- Rotation des clés périodique

### Autorisation
- Stratégies d'accès granulaires
- Limitation par périphérique/utilisateur
- Audit complet des actions

## Déploiement en production

### Configuration recommandée
- IoT Hub Standard S2 (10M messages/jour)
- Stockage Premium pour checkpoints
- Application Insights pour monitoring
- Key Vault pour secrets

### Scalabilité
- Auto-scaling des instances API
- Partitioning des Event Hubs
- Load balancing avec Azure Front Door
- CDN pour assets statiques

### Backup et récupération
- Backup automatique PostgreSQL
- Réplication géographique IoT Hub
- Stratégie de disaster recovery
- Tests de récupération réguliers

## Troubleshooting

### Problèmes courants

1. **Périphérique non connecté**
   - Vérifier les credentials
   - Contrôler la connectivité réseau
   - Vérifier les certificats

2. **Télémétrie manquante**
   - Contrôler EventHub configuration
   - Vérifier les checkpoints
   - Valider le format JSON

3. **Commandes échouées**
   - Timeout des méthodes directes
   - Format payload incorrect
   - Périphérique occupé

### Commandes de diagnostic
```bash
# Test de connectivité IoT Hub
az iot hub monitor-events --hub-name smartoffice-iothub

# Test d'une méthode directe
az iot hub invoke-device-method --hub-name smartoffice-iothub --device-id device001 --method-name getStatus

# Vérifier les métriques
az monitor metrics list --resource /subscriptions/{sub}/resourceGroups/rg-smartoffice/providers/Microsoft.Devices/IotHubs/smartoffice-iothub
```