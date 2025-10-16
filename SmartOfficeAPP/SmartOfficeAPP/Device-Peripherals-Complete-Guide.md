# Guide Complet - Gestion des Périphériques IoT SmartOffice

## Vue d'ensemble

Ce guide décrit l'architecture complète pour gérer les capteurs et actionneurs physiques branchés sur les devices IoT (ESP32, etc.).

## Architecture

```
???????????????????????????????????????????????????????????????????????
?                          CLOUD (Azure)                               ?
???????????????????????????????????????????????????????????????????????
?                                                                      ?
?  ???????????????      ????????????????      ????????????????      ?
?  ?   IoT Hub   ????????Azure Function???????? PostgreSQL   ?      ?
?  ?             ?      ? (Ingestion)  ?      ?   Database   ?      ?
?  ???????????????      ????????????????      ????????????????      ?
?         ?                    ?                                      ?
?         ?                    ????????????                          ?
?         ?                               ?                          ?
?  ???????????????                 ????????????????                 ?
?  ?  ASP.NET    ???????????????????   SignalR    ?                 ?
?  ?     API     ?                 ?   (Temps     ?                 ?
?  ?             ?                 ?    réel)     ?                 ?
?  ???????????????                 ????????????????                 ?
?         ?                                                           ?
???????????????????????????????????????????????????????????????????????
          ?
          ? Commands / Config (C2D, Direct Methods, Desired Props)
          ? Telemetry / Logs (D2C Messages)
          ?
???????????????????????????????????????????????????????????????????????
?                        EDGE (ESP32)                                  ?
???????????????????????????????????????????????????????????????????????
?                                                                      ?
?  ??????????????????????????????????????????????????????????        ?
?  ?              ESP32 Firmware (C++)                       ?        ?
?  ?  - Azure IoT SDK                                        ?        ?
?  ?  - Device Twin Listener (desired.peripherals)          ?        ?
?  ?  - Direct Method Handler (peripheralCommand)           ?        ?
?  ?  - Telemetry Publisher (D2C)                            ?        ?
?  ?  - Log Publisher (D2C)                                  ?        ?
?  ???????????????????????????????????????????????????????????        ?
?         ?                                                            ?
?  ???????????????  ???????????  ???????????  ???????????           ?
?  ?   Relay     ?  ?  DHT22  ?  ?  MQ-2   ?  ?   PIR   ?           ?
?  ?  (GPIO 33)  ?  ?(GPIO 32)?  ?(GPIO 35)?  ?(GPIO 34)?           ?
?  ?  Lights     ?  ?  Temp   ?  ?  Smoke  ?  ? Motion  ?           ?
?  ???????????????  ???????????  ???????????  ???????????           ?
?                                                                      ?
????????????????????????????????????????????????????????????????????????
```

## Workflow Complet

### 1. Installation d'un Nouveau Périphérique

#### Côté Admin (Interface Web)

1. **Brancher physiquement le périphérique sur l'ESP32**
   - Exemple: DHT22 sur GPIO 32

2. **Enregistrer dans la base de données**
   ```http
   POST /api/DevicePeripherals
   {
     "name": "temp_sensor_room",
     "type": "temperature",
     "gpio": 32,
     "iotDeviceId": "esp32-office-101",
     "notes": "DHT22 - Angle nord-est du bureau",
     "unit": "°C",
     "wiringDiagram": "https://docs.example.com/wiring/dht22.png",
     "installedAt": "2025-01-15T10:45:00Z"
   }
   ```

3. **Synchroniser automatiquement avec IoT Hub**
   - Le service `DevicePeripheralService` met à jour les desired properties
   - IoT Hub notifie l'ESP32 de la nouvelle configuration

4. **L'ESP32 reçoit la mise à jour**
   ```json
   {
     "peripherals": [
       {
         "name": "temp_sensor_room",
         "type": "temperature",
         "gpio": 32,
         "unit": "°C"
       }
     ]
   }
   ```

5. **L'ESP32 configure le hardware et publie l'état**
   ```json
   {
     "peripherals": [
       {
         "name": "temp_sensor_room",
         "type": "temperature",
         "gpio": 32,
         "unit": "°C",
         "state": "active",
         "lastRead": 23.5,
         "lastUpdate": "2025-01-15T10:46:00Z"
       }
     ]
   }
   ```

### 2. Contrôle d'un Périphérique

#### Exemple: Allumer une lumière

1. **Requête admin**
   ```http
   POST /api/Devices/esp32-office-101/peripherals/relay_lights_office/commands/on
   Authorization: Bearer <token>
   ```

2. **L'API invoque une méthode directe IoT Hub**
   ```json
   Method: "peripheralCommand"
   Payload: {
     "name": "relay_lights_office",
     "command": "on",
     "parameters": {}
   }
   ```

3. **L'ESP32 exécute la commande**
   ```cpp
   void handlePeripheralCommand(const char* payload) {
     // Parse JSON
     // Trouver le périphérique par name
     // Exécuter la commande (digitalWrite, etc.)
     // Mettre à jour reported properties
   }
   ```

4. **L'ESP32 publie le nouvel état**
   ```json
   {
     "peripherals": [
       {
         "name": "relay_lights_office",
         "state": "on",
         "lastChange": "2025-01-15T14:30:00Z"
       }
     ]
   }
   ```

5. **SignalR notifie les clients en temps réel**

### 3. Télémétrie Continue

#### Côté ESP32

```cpp
void publishTelemetry() {
  float temperature = dht.readTemperature();
  
  // Créer le message
  String message = "{"
    "\"deviceId\": \"esp32-office-101\","
    "\"peripheral\": \"temp_sensor_room\","
    "\"metric\": \"temperature\","
    "\"value\": " + String(temperature) + ","
    "\"unit\": \"°C\","
    "\"timestamp\": \"" + getISOTimestamp() + "\""
  "}";
  
  // Ajouter la propriété type=telemetry
  client.sendEvent(message, "type", "telemetry");
}
```

#### Côté Cloud

1. **IoT Hub reçoit le message D2C**
2. **Azure Function ingère et persiste**
3. **PostgreSQL stocke la donnée**
4. **SignalR notifie les clients**
5. **Dashboard affiche en temps réel**

### 4. Logs & Diagnostics

#### Logs depuis ESP32

```cpp
void logError(const char* peripheral, const char* message) {
  String log = "{"
    "\"deviceId\": \"esp32-office-101\","
    "\"level\": \"error\","
    "\"source\": \"peripheral\","
    "\"peripheral\": \"" + String(peripheral) + "\","
    "\"message\": \"" + String(message) + "\","
    "\"timestamp\": \"" + getISOTimestamp() + "\""
  "}";
  
  client.sendEvent(log, "type", "log");
}
```

#### Ingestion

- Azure Function filtre les logs par niveau
- Persiste dans table `DeviceLogs`
- Alerte en temps réel pour error/warn via SignalR

## Endpoints API Disponibles

### Device Peripherals (CRUD en DB)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/DevicePeripherals` | Liste tous les périphériques |
| GET | `/api/DevicePeripherals/{id}` | Récupère un périphérique par ID |
| GET | `/api/DevicePeripherals/device/{deviceId}` | Périphériques d'un device |
| POST | `/api/DevicePeripherals` | Crée un périphérique + sync IoT Hub |
| PUT | `/api/DevicePeripherals/{id}` | Met à jour + sync IoT Hub |
| DELETE | `/api/DevicePeripherals/{id}` | Supprime + sync IoT Hub |
| POST | `/api/DevicePeripherals/device/{deviceId}/sync` | Force la synchronisation |

### Devices (Gestion via Twin)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/api/Devices/{deviceId}/peripherals` | Liste depuis Twin (reported prioritaire) |
| GET | `/api/Devices/{deviceId}/peripherals/{name}` | Un périphérique depuis Twin |
| POST | `/api/Devices/{deviceId}/peripherals` | Ajoute dans desired (merge) |
| PUT | `/api/Devices/{deviceId}/peripherals` | Remplace desired.peripherals |
| DELETE | `/api/Devices/{deviceId}/peripherals/{name}` | Supprime de desired |
| GET | `/api/Devices/{deviceId}/peripherals/{name}/merged` | Vue fusionnée desired+reported |
| GET | `/api/Devices/{deviceId}/twin/full` | Twin complet avec métadonnées |

### Commandes

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/api/Devices/{deviceId}/peripherals/{name}/commands/{command}` | Exécute une commande |

Exemples de commandes:
- `on` / `off` (relay, led)
- `read` (capteurs)
- `reset` (réinitialisation)
- `setBrightness` (LED avec paramètres)
- `setColor` (LED RGB)

## Schéma de Données

### DevicePeripheral (PostgreSQL)

```sql
CREATE TABLE "DevicePeripherals" (
  "Id" SERIAL PRIMARY KEY,
  "Name" VARCHAR(100) NOT NULL,
  "Type" VARCHAR(50) NOT NULL,
  "Gpio" INT NOT NULL,
  "IotDeviceId" VARCHAR(255) NOT NULL,
  "Notes" VARCHAR(1000),
  "Unit" VARCHAR(20),
  "WiringDiagram" VARCHAR(500),
  "InstalledAt" TIMESTAMP,
  "LastKnownState" TEXT,
  "LastUpdateReceived" TIMESTAMP,
  "Status" VARCHAR(50) DEFAULT 'active',
  "PeripheriqueLogiqueId" INT,
  "CreatedAt" TIMESTAMP DEFAULT NOW(),
  "UpdatedAt" TIMESTAMP,
  CONSTRAINT "UK_DevicePeripherals_IotDeviceId_Name" UNIQUE ("IotDeviceId", "Name")
);
```

### DeviceLogs (PostgreSQL)

```sql
CREATE TABLE "DeviceLogs" (
  "Id" SERIAL PRIMARY KEY,
  "DeviceId" VARCHAR(255) NOT NULL,
  "Peripheral" VARCHAR(100),
  "Level" VARCHAR(50) NOT NULL,
  "Source" VARCHAR(100) NOT NULL,
  "Message" TEXT NOT NULL,
  "Timestamp" TIMESTAMP NOT NULL,
  "Context" JSONB,
  "CreatedAt" TIMESTAMP DEFAULT NOW()
);
```

### Device Twin (Azure IoT Hub)

```json
{
  "deviceId": "esp32-office-101",
  "properties": {
    "desired": {
      "peripherals": [
        {
          "name": "relay_lights_office",
          "type": "relay",
          "gpio": 33,
          "notes": "Relais éclairage principal"
        },
        {
          "name": "temp_sensor_room",
          "type": "temperature",
          "gpio": 32,
          "unit": "°C"
        }
      ],
      "telemetry": {
        "intervalSec": 10
      },
      "$version": 3
    },
    "reported": {
      "peripherals": [
        {
          "name": "relay_lights_office",
          "type": "relay",
          "gpio": 33,
          "state": "off",
          "lastChange": "2025-01-15T14:30:00Z"
        },
        {
          "name": "temp_sensor_room",
          "type": "temperature",
          "gpio": 32,
          "state": "active",
          "lastRead": 23.5,
          "lastUpdate": "2025-01-15T14:35:00Z"
        }
      ],
      "firmware": {
        "version": "1.2.3",
        "commit": "abcd123"
      },
      "metrics": {
        "rssi": -58,
        "heapFree": 123456,
        "uptime": 86400
      },
      "$version": 5
    }
  }
}
```

## Firmware ESP32 - Implémentation Requise

### 1. Initialisation

```cpp
#include <WiFi.h>
#include <AzureIoTHub.h>
#include <DHT.h>

// Configuration périphériques
struct Peripheral {
  String name;
  String type;
  int gpio;
  String unit;
  void* handler; // Pointeur fonction
};

std::vector<Peripheral> peripherals;

void setup() {
  // Connexion WiFi + IoT Hub
  connectToIoTHub();
  
  // Abonnement aux mises à jour desired properties
  deviceTwinCallback(onDesiredPropertyUpdate);
  
  // Enregistrement méthode directe
  deviceMethodCallback(onDirectMethodInvoked);
  
  // Récupération de la config initiale
  getTwinDocument();
}
```

### 2. Gestion Desired Properties

```cpp
void onDesiredPropertyUpdate(const char* payload) {
  // Parser le JSON
  JsonDocument doc;
  deserializeJson(doc, payload);
  
  if (doc.containsKey("peripherals")) {
    // Reconfigurer les périphériques
    peripherals.clear();
    
    for (JsonObject p : doc["peripherals"].as<JsonArray>()) {
      Peripheral peripheral;
      peripheral.name = p["name"].as<String>();
      peripheral.type = p["type"].as<String>();
      peripheral.gpio = p["gpio"].as<int>();
      peripheral.unit = p["unit"].as<String>();
      
      // Initialiser le GPIO selon le type
      if (peripheral.type == "relay") {
        pinMode(peripheral.gpio, OUTPUT);
        peripheral.handler = &handleRelay;
      } else if (peripheral.type == "temperature") {
        peripheral.handler = &handleTemperature;
      }
      
      peripherals.push_back(peripheral);
    }
    
    // Publier reported properties
    publishReportedProperties();
  }
}
```

### 3. Méthodes Directes

```cpp
int onDirectMethodInvoked(const char* method_name, const unsigned char* payload, size_t size) {
  if (strcmp(method_name, "peripheralCommand") == 0) {
    JsonDocument doc;
    deserializeJson(doc, payload, size);
    
    String name = doc["name"];
    String command = doc["command"];
    
    // Trouver le périphérique
    for (auto& p : peripherals) {
      if (p.name == name) {
        // Exécuter la commande
        if (p.type == "relay") {
          if (command == "on") {
            digitalWrite(p.gpio, HIGH);
          } else if (command == "off") {
            digitalWrite(p.gpio, LOW);
          }
        }
        
        // Mettre à jour reported
        publishReportedProperties();
        return 200; // Success
      }
    }
    
    return 404; // Peripheral not found
  }
  
  return 501; // Method not implemented
}
```

### 4. Télémétrie

```cpp
void publishTelemetry() {
  for (auto& p : peripherals) {
    if (p.type == "temperature") {
      DHT* dht = (DHT*)p.handler;
      float temp = dht->readTemperature();
      
      String message = "{";
      message += "\"deviceId\":\"" + deviceId + "\",";
      message += "\"peripheral\":\"" + p.name + "\",";
      message += "\"metric\":\"temperature\",";
      message += "\"value\":" + String(temp) + ",";
      message += "\"unit\":\"" + p.unit + "\",";
      message += "\"timestamp\":\"" + getISOTimestamp() + "\"";
      message += "}";
      
      sendEvent(message.c_str(), "type", "telemetry");
    }
  }
}
```

### 5. Logs

```cpp
void logMessage(const char* level, const char* source, const char* peripheral, const char* message) {
  String log = "{";
  log += "\"deviceId\":\"" + deviceId + "\",";
  log += "\"level\":\"" + String(level) + "\",";
  log += "\"source\":\"" + String(source) + "\",";
  if (peripheral) {
    log += "\"peripheral\":\"" + String(peripheral) + "\",";
  }
  log += "\"message\":\"" + String(message) + "\",";
  log += "\"timestamp\":\"" + getISOTimestamp() + "\"";
  log += "}";
  
  sendEvent(log.c_str(), "type", "log");
}
```

## Bonnes Pratiques

### Sécurité

1. **Validation des commandes côté firmware**
   - Liste blanche des commandes par type de périphérique
   - Vérification des plages de valeurs

2. **Rate limiting**
   - Limiter la fréquence des commandes
   - Queue de commandes avec déduplication

3. **Authentification**
   - Certificats X.509 pour production
   - Rotation régulière des clés

### Performance

1. **Batch des messages de télémétrie**
   - Regrouper plusieurs métriques dans un seul message
   - Réduire la charge sur IoT Hub

2. **Compression**
   - Utiliser GZIP pour les payloads volumineux

3. **Offline handling**
   - Cache local sur ESP32 (SPIFFS, SD Card)
   - Rejeu automatique à la reconnexion

### Fiabilité

1. **Retry avec backoff exponentiel**
2. **Watchdog timer** pour redémarrage auto
3. **OTA updates** pour déploiement firmware
4. **Health checks** périodiques

## Monitoring & Troubleshooting

### Dashboards

1. **Télémétrie en temps réel**
   - Grafana + PostgreSQL
   - SignalR pour updates live

2. **Logs centralisés**
   - Filtres par device, niveau, périphérique
   - Alertes sur erreurs critiques

3. **Statuts devices**
   - Connexion (connected/disconnected)
   - Dernière activité
   - Version firmware

### Métriques Clés

- Latence commande ? exécution
- Taux de succès des commandes
- Fréquence des messages D2C
- Taux d'erreurs par périphérique
- Uptime des devices

## Ressources

- [Azure IoT Hub Documentation](https://docs.microsoft.com/azure/iot-hub/)
- [ESP32 Arduino Core](https://github.com/espressif/arduino-esp32)
- [Azure IoT SDK for C](https://github.com/Azure/azure-iot-sdk-c)
- [Tests API](./Tests/11-Device-Peripherals.http)
- [Azure Function Ingestion](./Azure-Function-Telemetry-Ingestion.md)
