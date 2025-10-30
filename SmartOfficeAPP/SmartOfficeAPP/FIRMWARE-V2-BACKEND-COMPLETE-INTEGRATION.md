# ?? FIRMWARE ESP32 v2.0 - INTÉGRATION BACKEND COMPLÈTE

**Date:** 25 janvier 2025  
**Firmware Version:** 2.0.0  
**Author:** Koj-Kyo  
**Backend Version:** SmartOfficeAPP v2.0  

---

## ?? TABLE DES MATIÈRES

1. [Vue d'ensemble](#vue-densemble)
2. [Nouveautés Firmware v2.0](#nouveautés-firmware-v20)
3. [Architecture Backend](#architecture-backend)
4. [Endpoints API](#endpoints-api)
5. [Méthodes Directes IoT Hub](#méthodes-directes-iot-hub)
6. [Device Twin & Configuration](#device-twin--configuration)
7. [Energy Metering](#energy-metering)
8. [Monitoring & Diagnostics](#monitoring--diagnostics)
9. [Tests & Validation](#tests--validation)

---

## ?? VUE D'ENSEMBLE

### Fonctionnalités Principales

#### ? FIRMWARE v2.0.0
- **WiFi Manager** : Portail de configuration WiFi (captive portal)
- **BLE Control** : Contrôle Bluetooth Low Energy (SmartOffice-ESP32)
- **NVS Queue** : File d'attente persistante (64 messages offline)
- **Energy Metering** : Mesure consommation énergétique (mWh/kWh/€)
- **Heartbeat** : Monitoring automatique (60s interval)
- **Logs Structurés** : Logs avec niveau/source/message
- **Diagnostics** : Rapports système complets
- **Metrics** : Statistiques (reboot count, MQTT stats, RFID scans, etc.)

#### ? BACKEND v2.0
- **IoTHubDeviceController** : Nouveau contrôleur pour commandes directes
- **Support Device Twin** : Synchronisation desired ? reported properties
- **Energy API** : Endpoints pour statistiques énergétiques
- **Diagnostics API** : Endpoints pour monitoring système
- **SignalR** : Notifications temps réel
- **CamelCase Enforcement** : Middleware pour compatibilité firmware

---

## ?? NOUVEAUTÉS FIRMWARE v2.0

### 1. WiFi Manager
```cpp
// Configuration WiFi via portail captif
connecterWiFi(); // Auto-connect ou portail config
resetWiFiConfig(); // Reset via bouton BOOT (5s)
```

**Fonctionnalités:**
- Auto-connexion avec credentials sauvegardés
- Portail de configuration (http://192.168.4.1)
- Timeout 180 secondes
- Reset config : maintenir bouton BOOT 5s

### 2. BLE Control
```cpp
// Service UUID: 0000A001-0000-1000-8000-00805F9B34FB
// Characteristics:
// - CMD (write): 0000A002-...
// - STATE (read/notify): 0000A003-...
// - RFID (read): 0000A004-...
```

**Commandes BLE:**
```json
// Format JSON
{
  "cmd": "setPeripheral",
  "params": {
    "name": "led_rdc",
    "state": 1
  }
}
```

### 3. NVS Queue (Offline Messages)
```cpp
// Capacité: 64 messages
// Persistance: Flash NVS
// Flush automatique à la reconnexion
```

**Structure Message:**
```cpp
struct NvsQueuedMsg {
  String topic;
  String payload;
  bool retained;
  String ts; // ISO8601
  String msgId; // GUID
};
```

### 4. Energy Metering
```cpp
// Par périphérique:
float powerWatts; // Puissance nominale (W)
unsigned long totalOnTimeMs; // Temps ON cumulé
float totalEnergyMwh; // Énergie totale (mWh)

// Calculs automatiques:
// energyMwh = powerW × timeHours × 1000
// costEur = energyKwh × pricePerKwh
```

**Types de périphériques:**
- Light: 10W
- Actuator (servo): 2W
- Actuator (buzzer): 0.5W
- Sensor: 0.1W

### 5. Heartbeat & Monitoring
```cpp
// Heartbeat automatique toutes les 60s
const unsigned long HEARTBEAT_INTERVAL = 60000;

// Métriques:
uint32_t gRebootCount;
uint32_t gTotalMqttPublished;
uint32_t gTotalMqttFailed;
uint32_t gTotalRfidScans;
uint32_t gTotalDoorOpenings;
```

### 6. Logs Structurés
```cpp
void sendLog(const String& level, const String& source, const String& message);

// Niveaux: info, warning, error, debug
// Sources: wifi, mqtt, ble, peripheral, alarm, energy, etc.
```

---

## ??? ARCHITECTURE BACKEND

### Contrôleurs
```
Controllers/
??? IoTHubDeviceController.cs ? NOUVEAU (v2.0)
??? DevicesController.cs (existing)
??? CommandeController.cs (existing)
??? ...
```

### Services
```
Services/
??? IIotHubService.cs
??? IoTHubService.cs
??? ISignalRService.cs
??? SignalRService.cs
```

### DTOs
```csharp
// Nouveaux DTOs v2.0
public class DirectMethodPayload { }
public class PeripheralControlParameters { }
public class TestLogPayload { }
public class DeviceListPayload { }
public class CloudMessagePayload { }
public class DeviceUpdatePayload { }
```

---

## ?? ENDPOINTS API

### 1. Statut & Informations

#### **GET** `/api/IoTHubDevice/{deviceId}/status`
Force une requête de statut complet du device.

**Réponse firmware (télémétrie):**
```json
{
  "door": "open|closed",
  "alarm": "active|inactive",
  "smoke": "detected|normal",
  "rfid": "a2de95ab",
  "rssi": -45,
  "heap": 245000,
  "uptime": 3600,
  "peripheralsCount": 6,
  "peripherals": [...]
}
```

#### **GET** `/api/IoTHubDevice/{deviceId}/twin`
Récupère le Device Twin complet (desired + reported).

#### **GET** `/api/IoTHubDevice/list`
Liste tous les devices IoT Hub.

**Paramètres:**
- `maxCount` (query) : Nombre max de devices (default: 100)

---

### 2. Contrôle Porte

#### **POST** `/api/IoTHubDevice/{deviceId}/door/open`
Ouvre la porte (servo ? 175°).

#### **POST** `/api/IoTHubDevice/{deviceId}/door/close`
Ferme la porte (servo ? 35°).

**Blocage:** Si alarme active, la porte ne peut PAS s'ouvrir.

---

### 3. Contrôle Périphériques

#### **POST** `/api/IoTHubDevice/{deviceId}/peripheral/{name}/{command}`
Contrôle un périphérique spécifique.

**Paramètres:**
- `name` : Nom du périphérique (ex: "led_rdc")
- `command` : Commande (on/off/toggle)

**Body (optionnel):**
```json
{
  "parameters": {
    "brightness": 80,
    "color": "#FF5733"
  }
}
```

**Exemple:**
```http
POST /api/IoTHubDevice/mon-esp32/peripheral/led_rdc/on
POST /api/IoTHubDevice/mon-esp32/peripheral/led_etage2/off
POST /api/IoTHubDevice/mon-esp32/peripheral/led_etage3/toggle
```

---

### 4. Alarme Fumée

#### **POST** `/api/IoTHubDevice/{deviceId}/alarm/on`
Active l'alarme fumée.

**Effets:**
- Buzzer sonne (800 Hz)
- LEDs clignotent (500ms)
- Porte bloquée

#### **POST** `/api/IoTHubDevice/{deviceId}/alarm/off`
Désactive l'alarme fumée.

---

### 5. Énergie (v2.0)

#### **POST** `/api/IoTHubDevice/{deviceId}/energy/report`
Force un rapport énergétique complet.

**Réponse firmware (télémétrie):**
```json
{
  "deviceId": "mon-esp32",
  "uptimeHours": 2.5,
  "totalEnergyMwh": 25000,
  "totalEnergyKwh": 0.025,
  "totalCostEur": 0.00375,
  "costPerKwh": 0.15,
  "peripherals": [
    {
      "name": "led_rdc",
      "powerWatts": 10.0,
      "totalOnTimeMs": 1800000,
      "totalOnTimeHours": 0.5,
      "totalEnergyMwh": 5000,
      "totalEnergyKwh": 0.005,
      "costEur": 0.00075
    }
  ]
}
```

#### **POST** `/api/IoTHubDevice/{deviceId}/energy/reset`
Réinitialise tous les compteurs énergétiques.

#### **GET** `/api/Devices/{deviceId}/peripherals/energy`
Obtient les statistiques énergétiques (backend).

**Réponse:**
```json
{
  "deviceId": "mon-esp32",
  "timestamp": "2025-01-25T14:30:00Z",
  "peripherals": [
    {
      "name": "led_rdc",
      "type": "light",
      "metering": "estimated",
      "ratedWatt": 10,
      "energyWh": 5.0,
      "currentPowerW": 10.0,
      "state": "on",
      "lastUpdate": "2025-01-25T14:25:00Z"
    }
  ],
  "totalEnergyWh": 25.0,
  "totalPowerW": 20.0
}
```

---

### 6. Monitoring & Diagnostics (v2.0)

#### **POST** `/api/IoTHubDevice/{deviceId}/diagnostics`
Obtient un rapport de diagnostics complet.

**Réponse firmware (logs):**
```
??????????????????????????????????????????????????
?          DIAGNOSTICS COMPLET                   ?
??????????????????????????????????????????????????
? FIRMWARE                                       ?
?   Version       : 2.0.0                        ?
?   Build Date    : Jan 25 2025 14:30:00        ?
?   Author        : Koj-Kyo                      ?
? DEVICE                                         ?
?   ID            : mon-esp32                    ?
?   Chip Model    : ESP32-D0WDQ6                 ?
?   CPU Freq      : 240 MHz                      ?
?   Flash Size    : 4 MB                         ?
?   Free Heap     : 245000 bytes                 ?
?   Uptime        : 2.50 heures                  ?
?   Reboot Count  : 15                           ?
?   Last Reboot   : PowerOn                      ?
? NETWORK                                        ?
?   WiFi Status   : ? Connected                 ?
?   SSID          : MyWiFi                       ?
?   IP            : 192.168.1.100                ?
?   RSSI          : -42 dBm                      ?
?   MAC           : 24:0A:C4:XX:XX:XX            ?
?   MQTT Status   : ? Connected                 ?
?   MQTT Pub OK   : 1250                         ?
?   MQTT Pub Fail : 3                            ?
?   Queue Size    : 0 / 64                       ?
? BLUETOOTH                                      ?
?   BLE Status    : ? Active                    ?
?   Subscribers   : 1                            ?
? PERIPHERALS                                    ?
?   Configured    : 6                            ?
?     led_rdc     : ON  GPIO 27  10.0W          ?
?     led_etage2  : OFF GPIO 26  10.0W          ?
? ENERGY                                         ?
?   Total Energy  : 0.0250 kWh                   ?
?   Total Cost    : 0.0038 €                     ?
? ACTIVITY                                       ?
?   RFID Scans    : 45                           ?
?   Door Openings : 23                           ?
?   Alarm Active  : NO                           ?
??????????????????????????????????????????????????
```

#### **POST** `/api/IoTHubDevice/{deviceId}/heartbeat`
Force un heartbeat immédiat.

**Réponse firmware (télémétrie):**
```json
{
  "deviceId": "mon-esp32",
  "type": "heartbeat",
  "timestamp": "2025-01-25T14:30:00Z",
  "uptime": 7200,
  "heap": 240000,
  "rssi": -42,
  "firmwareVersion": "2.0.0",
  "rebootCount": 15,
  "queueSize": 0,
  "mqttPublished": 1250,
  "mqttFailed": 3,
  "rfidScans": 45,
  "doorOpenings": 23
}
```

#### **POST** `/api/IoTHubDevice/{deviceId}/telemetry/send`
Force une télémétrie immédiate.

#### **POST** `/api/IoTHubDevice/{deviceId}/test/log`
Envoie un log de test depuis le device.

**Body:**
```json
{
  "level": "info",
  "source": "api_test",
  "message": "Test log from backend API"
}
```

---

### 7. Device Twin & Configuration

#### **GET** `/api/Devices/{deviceId}/peripherals`
Liste tous les périphériques (reported).

#### **PUT** `/api/Devices/{deviceId}/peripherals`
Remplace tous les périphériques (desired).

**Body:**
```json
{
  "peripherals": [
    {
      "id": "led-rdc-001",
      "name": "led_rdc",
      "type": "light",
      "gpio": 27,
      "isOutput": true,
      "activeHigh": true,
      "metering": "estimated",
      "ratedWatt": 10,
      "notes": "LED RDC - GPIO 27"
    }
  ]
}
```

#### **POST** `/api/Devices/{deviceId}/peripherals`
Ajoute un nouveau périphérique.

#### **DELETE** `/api/Devices/{deviceId}/peripherals/{name}`
Supprime un périphérique.

#### **GET** `/api/Devices/{deviceId}/peripherals/{name}/merged`
Vue fusionnée (desired vs reported).

**Réponse:**
```json
{
  "name": "led_rdc",
  "desiredConfig": {
    "id": "led-rdc-001",
    "name": "led_rdc",
    "type": "light",
    "gpio": 27,
    "isOutput": true,
    "activeHigh": true,
    "metering": "estimated",
    "ratedWatt": 10
  },
  "reportedState": {
    "id": "led-rdc-001",
    "name": "led_rdc",
    "type": "light",
    "gpio": 27,
    "state": "on",
    "configured": true,
    "status": "active",
    "lastUpdate": "2025-01-25T14:30:00Z"
  },
  "isSynchronized": true
}
```

---

### 8. Système

#### **POST** `/api/IoTHubDevice/{deviceId}/reboot`
Redémarre le device (délai 2s).

?? **Attention:** Le device sera indisponible pendant ~30 secondes.

#### **PUT** `/api/IoTHubDevice/{deviceId}` (Admin)
Crée ou met à jour un device dans IoT Hub.

#### **DELETE** `/api/IoTHubDevice/{deviceId}` (Admin)
Supprime un device d'IoT Hub.

?? **Attention:** Suppression définitive.

#### **POST** `/api/IoTHubDevice/{deviceId}/message`
Envoie un message Cloud-to-Device.

**Body:**
```json
{
  "message": "Test message",
  "properties": {
    "source": "api",
    "priority": "high"
  }
}
```

---

## ?? MÉTHODES DIRECTES IOT HUB

### Format Standard
```csharp
await _iotHubService.InvokeDeviceMethodAsync(
    deviceId,
    methodName,
    payloadJson
);
```

### Méthodes Disponibles

#### 1. **getStatus**
```json
// Payload: {}
// Response: { door, alarm, smoke, rfid, rssi, heap, uptime, peripheralsCount, peripherals[] }
```

#### 2. **peripheralCommand**
```json
// Payload:
{
  "name": "led_rdc",
  "command": "on|off|toggle",
  "parameters": {}
}

// Response:
{
  "status": "ok",
  "state": "on"
}
```

#### 3. **openDoor / closeDoor**
```json
// Payload: {}
// Response: { status: "ok", door: "opened|closed" }
// Error (alarme): { status: "error", message: "Alarm active" } (HTTP 423)
```

#### 4. **alarmOn / alarmOff**
```json
// Payload: {}
// Response: { status: "ok" }
```

#### 5. **reboot**
```json
// Payload: {}
// Response: { status: "ok", message: "Rebooting in 2s" }
```

#### 6. **resetEnergy** (v2.0)
```json
// Payload: {}
// Response: { status: "ok", message: "Energy counters reset" }
```

#### 7. **getEnergyReport** (v2.0)
```json
// Payload: {}
// Response: { deviceId, uptimeHours, totalEnergyKwh, totalCostEur, peripherals[] }
```

#### 8. **getDiagnostics** (v2.0)
```json
// Payload: {}
// Response: Full diagnostics report (see Monitoring section)
```

#### 9. **sendHeartbeat** (v2.0)
```json
// Payload: {}
// Response: Heartbeat telemetry (see Monitoring section)
```

#### 10. **sendLog** (v2.0)
```json
// Payload:
{
  "level": "info|warning|error|debug",
  "source": "api|firmware|peripheral|...",
  "message": "Log message text"
}

// Response: { status: "ok" }
```

---

## ?? DEVICE TWIN & CONFIGURATION

### Structure Device Twin

#### **Desired Properties**
Configuration envoyée par le backend ? ESP32.

```json
{
  "peripherals": [
    {
      "id": "led-rdc-001",
      "name": "led_rdc",
      "type": "light",
      "gpio": 27,
      "isOutput": true,
      "activeHigh": true,
      "metering": "estimated",
      "ratedWatt": 10,
      "notes": "LED RDC - GPIO 27",
      "unit": "W"
    }
  ]
}
```

#### **Reported Properties**
État actuel reporté par l'ESP32 ? backend.

```json
{
  "peripherals": [
    {
      "id": "led-rdc-001",
      "name": "led_rdc",
      "type": "light",
      "gpio": 27,
      "state": "on",
      "configured": true,
      "status": "active",
      "lastUpdate": "2025-01-25T14:30:00Z",
      "lastChange": "2025-01-25T14:25:00Z",
      "totalOnTimeMs": 300000,
      "totalEnergyMwh": 5000,
      "powerWatts": 10.0
    }
  ]
}
```

### Synchronisation

#### **Processus:**
1. Backend met à jour **desired.peripherals**
2. IoT Hub notifie l'ESP32 (MQTT topic `$iothub/twin/PATCH/properties/desired/`)
3. ESP32 reçoit et parse les **desired properties**
4. ESP32 applique la configuration (`applyDesiredPeripherals()`)
5. ESP32 configure les GPIOs (`configurePeripheralPin()`)
6. ESP32 publie les **reported properties** (MQTT topic `$iothub/twin/PATCH/properties/reported/`)
7. Backend lit les **reported properties** pour vérifier synchronisation

#### **Délai:**
?? Attendre 5-10 secondes après modification des desired properties pour laisser le firmware appliquer les changements.

---

## ? ENERGY METERING

### Configuration

#### **Types de Metering**
```csharp
"metering": "none" | "estimated" | "measured"
```

- **none**: Pas de mesure énergétique
- **estimated**: Calcul basé sur puissance nominale
- **measured**: Mesure réelle (hardware externe requis)

#### **Puissances Nominales**
```cpp
// Firmware v2.0
if (p.type == "light") {
  p.powerWatts = 10.0;  // LED 10W
} else if (p.type == "actuator") {
  if (p.name.indexOf("servo") >= 0) {
    p.powerWatts = 2.0;  // Servo 2W
  } else if (p.name.indexOf("buzzer") >= 0) {
    p.powerWatts = 0.5;  // Buzzer 0.5W
  } else {
    p.powerWatts = 5.0;  // Actuateur générique
  }
} else if (p.type == "sensor") {
  p.powerWatts = 0.1;  // Capteur 0.1W
}
```

### Calculs

#### **Énergie Consommée**
```cpp
// mWh = Watt × Heures × 1000
float deltaHours = deltaMs / 3600000.0;
float energyMwh = powerWatts * deltaHours * 1000.0;
```

#### **Coût Énergétique**
```cpp
// €  = kWh × Prix (€/kWh)
const float ENERGY_COST_PER_KWH = 0.15;
float costEur = (energyMwh / 1000000.0) * ENERGY_COST_PER_KWH;
```

### Exemple Calcul

#### **LED 10W allumée 30 minutes**
```
Puissance : 10W
Temps : 30 min = 0.5 heures
Énergie : 10W × 0.5h = 5 Wh = 5000 mWh = 0.005 kWh
Coût : 0.005 kWh × 0.15 €/kWh = 0.00075 €
```

### API Énergie

#### **Obtenir les statistiques**
```http
GET /api/Devices/mon-esp32/peripherals/energy
```

#### **Réinitialiser les compteurs**
```http
POST /api/Devices/mon-esp32/peripherals/energy/reset
```

#### **Rapport énergétique complet (firmware)**
```http
POST /api/IoTHubDevice/mon-esp32/energy/report
```

---

## ?? MONITORING & DIAGNOSTICS

### Heartbeat Automatique

#### **Intervalle:** 60 secondes
```cpp
const unsigned long HEARTBEAT_INTERVAL = 60000;
```

#### **Contenu:**
```json
{
  "deviceId": "mon-esp32",
  "type": "heartbeat",
  "timestamp": "2025-01-25T14:30:00Z",
  "uptime": 7200,
  "heap": 240000,
  "rssi": -42,
  "firmwareVersion": "2.0.0",
  "rebootCount": 15,
  "queueSize": 0,
  "mqttPublished": 1250,
  "mqttFailed": 3,
  "rfidScans": 45,
  "doorOpenings": 23
}
```

### Métriques Système

#### **Collectées par le firmware:**
```cpp
uint32_t gRebootCount;          // Nombre de redémarrages
String gLastRebootReason;       // Raison dernier reboot
uint32_t gTotalMqttPublished;   // Messages MQTT publiés
uint32_t gTotalMqttFailed;      // Messages MQTT échoués
uint32_t gTotalRfidScans;       // Scans RFID effectués
uint32_t gTotalDoorOpenings;    // Ouvertures de porte
float gTotalEnergyMwh;          // Énergie totale consommée
```

### Logs Structurés

#### **Format:**
```json
{
  "deviceId": "mon-esp32",
  "timestamp": "2025-01-25T14:30:00Z",
  "level": "info|warning|error|debug",
  "source": "wifi|mqtt|ble|peripheral|alarm|energy|system",
  "message": "Log message text",
  "uptime": 7200,
  "heap": 240000
}
```

#### **Exemples:**
```cpp
sendLog("info", "wifi", "Connected to MyWiFi");
sendLog("warning", "mqtt", "MQTT publish failed, queued");
sendLog("error", "peripheral", "GPIO 27 configuration failed");
sendLog("info", "energy", "Energy counters reset");
```

### Diagnostics Complets

#### **API:**
```http
POST /api/IoTHubDevice/mon-esp32/diagnostics
```

#### **Sections:**
- **FIRMWARE** : Version, build date, author
- **DEVICE** : Chip model, CPU, flash, heap, uptime, reboots
- **NETWORK** : WiFi (SSID, IP, RSSI, MAC), MQTT (status, pub/fail counts, queue)
- **BLUETOOTH** : BLE status, subscribers
- **PERIPHERALS** : Liste configurés avec état
- **ENERGY** : Énergie totale, coût
- **ACTIVITY** : RFID scans, door openings, alarm

---

## ? TESTS & VALIDATION

### Checklist Complète

#### **COMMANDES DE BASE**
- [ ] getStatus - Statut complet device
- [ ] openDoor - Ouverture porte (servo 175°)
- [ ] closeDoor - Fermeture porte (servo 35°)
- [ ] reboot - Redémarrage device

#### **CONTRÔLE PÉRIPHÉRIQUES**
- [ ] peripheralCommand (on) - Allumer LED
- [ ] peripheralCommand (off) - Éteindre LED
- [ ] peripheralCommand (toggle) - Toggle LED

#### **ALARME FUMÉE**
- [ ] alarmOn - Activation alarme (buzzer + LEDs)
- [ ] alarmOff - Désactivation alarme
- [ ] Blocage porte pendant alarme

#### **ÉNERGIE (v2.0)**
- [ ] Metering automatique (mWh)
- [ ] Calcul puissance instantanée (mW)
- [ ] Calcul coût énergétique (€)
- [ ] resetEnergy - Réinitialisation compteurs
- [ ] getEnergyReport - Rapport énergétique JSON

#### **MONITORING (v2.0)**
- [ ] Heartbeat automatique (60s)
- [ ] sendHeartbeat - Force heartbeat
- [ ] sendLog - Envoi log structuré
- [ ] getDiagnostics - Diagnostics complets

#### **DEVICE TWIN**
- [ ] GET Twin - Récupération twin
- [ ] PATCH Desired - Mise à jour config
- [ ] PATCH Reported - Report état périphériques
- [ ] Synchronisation desired ? reported

#### **BLE (v2.0)**
- [ ] Service SmartOffice-ESP32
- [ ] Characteristic CMD (write)
- [ ] Characteristic STATE (read/notify)
- [ ] Characteristic RFID (read)

#### **NVS QUEUE (v2.0)**
- [ ] Enqueue messages offline
- [ ] Flush queue à la reconnexion
- [ ] Persistance messages (64 max)

#### **WIFI MANAGER (v2.0)**
- [ ] Portail configuration WiFi
- [ ] Auto-connexion credentials sauvegardés
- [ ] Reset config (bouton BOOT 5s)

### Scénarios de Test

#### **Scénario 1: Ouverture Automatique**
1. Vérifier alarme OFF
2. Allumer LED RDC
3. Ouvrir porte
4. Attendre 5 secondes
5. Fermer porte
6. Éteindre LED RDC

#### **Scénario 2: Test Alarme Complète**
1. Allumer toutes les LEDs
2. Activer alarme
3. Tenter d'ouvrir porte (échec attendu)
4. Attendre 10 secondes
5. Désactiver alarme
6. Éteindre toutes les LEDs

#### **Scénario 3: Monitoring Énergétique**
1. Réinitialiser compteurs
2. Allumer LED RDC (10W)
3. Attendre 1 minute
4. Allumer LED Étage 2 (10W)
5. Attendre 1 minute
6. Obtenir rapport énergétique
7. Vérifier :
   - LED RDC : ~0.0166 kWh (10W × 2min)
   - LED Étage 2 : ~0.0083 kWh (10W × 1min)
   - Total : ~0.0249 kWh
   - Coût : ~0.00374 € (à 0.15 €/kWh)
8. Éteindre toutes les LEDs

### Fichiers de Test

#### **Tests HTTP:**
```
Tests/
??? 29-Firmware-v2-Complete-Commands.http ? NOUVEAU (v2.0)
??? 25-Configuration-Peripherals-Real.http (existing)
??? 17-Device-Status-Complete.http (existing)
??? ...
```

#### **Utilisation:**
```http
# Fichier: 29-Firmware-v2-Complete-Commands.http
# Contient tous les tests pour firmware v2.0

# 1. Authentification
POST https://localhost:7144/api/Auth/login
{ "email": "adminer@smartoffice.com", "password": "Admin1234!" }

# 2. Tests complets (voir fichier pour détails)
```

---

## ?? NOTES IMPORTANTES

### Compatibilité
- **Firmware:** v2.0.0+
- **Backend:** SmartOfficeAPP v2.0+
- **IoT Hub SDK:** Microsoft.Azure.Devices 1.39+
- **.NET:** 9.0

### Sécurité
- Authentification JWT requise pour tous les endpoints
- Rôles : `Admin`, `Manager`
- HTTPS obligatoire en production

### Performance
- Timeout méthodes directes : 30s
- Intervalle télémétrie : 10s
- Intervalle heartbeat : 60s
- Capacité queue NVS : 64 messages
- Intervalle flush queue : 5s (max 10 messages/batch)

### Limitations
- **GPIOs valides:** 12, 13, 14, 15, 16, 17, 21, 22, 25, 26, 27, 32, 33
- **GPIOs réservés:** 4 (RST), 5 (SS), 18/19/23 (SPI), 13 (SERVO), 25 (BUZZER), 32 (SMOKE)
- **Périphériques max:** Limité par mémoire (64 recommandé)
- **Queue NVS:** 64 messages max

### Dépannage

#### **Problème:** Device ne répond pas
**Solutions:**
1. Vérifier connexion IoT Hub (`GET /api/IoTHubDevice/{deviceId}/twin`)
2. Vérifier statut WiFi (série ou BLE)
3. Forcer heartbeat (`POST /api/IoTHubDevice/{deviceId}/heartbeat`)
4. Redémarrer device (`POST /api/IoTHubDevice/{deviceId}/reboot`)

#### **Problème:** Périphériques non configurés
**Solutions:**
1. Vérifier Device Twin (`GET /api/Devices/{deviceId}/twin/full`)
2. Vérifier GPIOs (valides et non réservés)
3. Attendre 10s après modification desired properties
4. Consulter logs device (`POST /api/IoTHubDevice/{deviceId}/test/log`)

#### **Problème:** Queue NVS pleine
**Solutions:**
1. Vérifier connexion MQTT
2. Vérifier taille queue (`POST /api/IoTHubDevice/{deviceId}/diagnostics`)
3. Forcer flush queue (automatique à la reconnexion)
4. Réduire fréquence télémétrie

---

## ?? RESSOURCES

### Documentation
- [Azure IoT Hub SDK](https://docs.microsoft.com/azure/iot-hub/)
- [ESP32 Arduino Core](https://docs.espressif.com/projects/arduino-esp32/)
- [NimBLE for ESP32](https://github.com/h2zero/NimBLE-Arduino)
- [WiFiManager ESP32](https://github.com/tzapu/WiFiManager)

### Code Source
- Firmware: `smartoffice_v2.0_etape3.ino`
- Backend Controller: `Controllers/IoTHubDeviceController.cs`
- Tests: `Tests/29-Firmware-v2-Complete-Commands.http`

### Support
- GitHub Issues: [https://github.com/Koj-Kyo/api-service-smart-office/issues](https://github.com/Koj-Kyo/api-service-smart-office/issues)
- Email: support@smartoffice.com

---

## ?? CHANGELOG

### v2.0.0 (25 janvier 2025)
- ? WiFi Manager (portail configuration)
- ? BLE Control (SmartOffice-ESP32)
- ? NVS Queue (64 messages offline)
- ? Energy Metering (mWh/kWh/€)
- ? Heartbeat automatique (60s)
- ? Logs structurés (level/source/message)
- ? Diagnostics complets
- ? Metrics système (reboot, MQTT, RFID, door)
- ? Backend IoTHubDeviceController
- ? Tests complets (29-Firmware-v2-Complete-Commands.http)

### v1.3.1 (janvier 2025)
- Gestion périphériques via Device Twin
- Configuration GPIO dynamique
- Support metering énergétique basique

### v1.0.0 (décembre 2024)
- Version initiale
- Contrôle LEDs, porte, alarme
- Communication MQTT

---

**© 2025 SmartOffice - Koj-Kyo - Tous droits réservés**
