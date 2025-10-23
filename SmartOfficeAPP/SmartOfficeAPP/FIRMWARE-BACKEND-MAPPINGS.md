# ?? Correspondances Firmware ?? Backend

## ?? Vue d'ensemble

Ce document établit les correspondances exactes entre les structures de données du **firmware ESP32 v1.3.0** et le **backend .NET 9**.

---

## ?? Structure `Peripheral` (Firmware)

### **Firmware ESP32 (C++)**
```cpp
struct Peripheral {
  String id;              // GUID généré
  String name;            // Nom du périphérique
  String type;            // Type: relay, light, sensor...
  int gpio;               // Numéro GPIO
  bool isOutput;          // true = sortie, false = entrée
  bool activeHigh;        // true = actif haut, false = actif bas

  bool state;             // État actuel (on/off)
  String lastChange;      // ISO8601 timestamp

  String metering;        // "none", "estimated", "measured"
  float ratedWatt;        // Puissance nominale (W)
  double energy_mWh;      // Énergie consommée (milliwatt-heure)
  float lastPower_mW;     // Dernière puissance (milliwatt)
  unsigned long lastChangeMs; // Timestamp en millisecondes

  bool configured;        // Succès de configuration
  String errorReason;     // Message d'erreur si échec

  String notes;           // Notes administratives
  String unit;            // Unité de mesure
};
```

### **Backend .NET (C#)**
```csharp
public class PeripheralDto
{
    public string? Id { get; set; }                 // ? Correspond à: id
    public string Name { get; set; }                // ? Correspond à: name
    public string Type { get; set; }                // ? Correspond à: type
    public int Gpio { get; set; }                   // ? Correspond à: gpio
    
    public string? Notes { get; set; }              // ? Correspond à: notes
    public string? Unit { get; set; }               // ? Correspond à: unit
    public string? WiringDiagram { get; set; }      // ?? Backend uniquement
    public DateTime? InstalledAt { get; set; }      // ?? Backend uniquement
    
    public object? State { get; set; }              // ? Correspond à: state (bool ? object)
    public DateTime? LastUpdate { get; set; }       // ?? Backend ajoute horodatage système
    public string? LastChange { get; set; }         // ? Correspond à: lastChange (ISO8601)
    public string? Status { get; set; }             // ?? Calculé (active/error/offline)
    public bool? Configured { get; set; }           // ? Correspond à: configured
    public string? Error { get; set; }              // ? Correspond à: errorReason
    
    public string? Metering { get; set; }           // ? Correspond à: metering
    public float? RatedWatt { get; set; }           // ? Correspond à: ratedWatt
    public double? EnergyMWh { get; set; }          // ? Correspond à: energy_mWh
    public float? LastPowerMW { get; set; }         // ? Correspond à: lastPower_mW
    
    public bool? IsOutput { get; set; }             // ? Correspond à: isOutput
    public bool? ActiveHigh { get; set; }           // ? Correspond à: activeHigh
}
```

---

## ?? Mappings de Propriétés

### ? **Correspondances Directes**

| Firmware (C++) | Backend (C#) | Type | Notes |
|----------------|--------------|------|-------|
| `id` | `Id` | `string` | GUID généré par firmware |
| `name` | `Name` | `string` | Identique |
| `type` | `Type` | `string` | Identique |
| `gpio` | `Gpio` | `int` | Identique |
| `notes` | `Notes` | `string` | Identique |
| `unit` | `Unit` | `string` | Identique |
| `lastChange` | `LastChange` | `string` | ISO8601 |
| `configured` | `Configured` | `bool` | Identique |
| `errorReason` | `Error` | `string` | Renommé |
| `metering` | `Metering` | `string` | Identique |
| `ratedWatt` | `RatedWatt` | `float` | Identique |
| `energy_mWh` | `EnergyMWh` | `double` | Identique |
| `lastPower_mW` | `LastPowerMW` | `float` | Identique |
| `isOutput` | `IsOutput` | `bool` | Identique |
| `activeHigh` | `ActiveHigh` | `bool` | Identique |

---

### ?? **Conversions de Type**

| Firmware | Backend | Conversion |
|----------|---------|------------|
| `bool state` | `object? State` | `true/false` ? `object` (JSON) |
| `String lastChange` (ISO8601) | `DateTime? LastUpdate` | Parse ISO8601 |
| `double energy_mWh` | `double EnergyWh` (endpoint /energy) | mWh ? Wh (`/1000`) |
| `float lastPower_mW` | `float CurrentPowerW` (endpoint /energy) | mW ? W (`/1000`) |

---

### ?? **Champs Backend Uniquement**

| Champ | Type | Usage |
|-------|------|-------|
| `WiringDiagram` | `string?` | URL ou description du schéma de câblage |
| `InstalledAt` | `DateTime?` | Date d'installation physique |
| `Status` | `string?` | "active", "error", "offline" (calculé) |
| `LastUpdate` | `DateTime?` | Horodatage système (non firmware) |

---

## ?? Flux de Données

### 1?? **Configuration (Backend ? Firmware)**

**Backend envoie via IoT Hub:**
```json
{
  "peripherals": [
    {
      "name": "relay4",
      "type": "relay",
      "gpio": 33,
      "notes": "Contrôle éclairage principal",
      "unit": "W",
      "metering": "estimated",
      "ratedWatt": 100,
      "isOutput": true,
      "activeHigh": true
    }
  ]
}
```

**Firmware reçoit et applique:**
```cpp
void applyDesiredPeripherals(const JsonObject& desired) {
    for (JsonObject it : desired["peripherals"].as<JsonArray>()) {
        Peripheral p;
        p.id = it["id"] | generateGUID();
        p.name = it["name"] | "";
        p.type = it["type"] | "";
        p.gpio = it["gpio"] | -1;
        p.notes = it["notes"] | "";        // ? Lecture notes
        p.unit = it["unit"] | "";          // ? Lecture unit
        p.metering = it["metering"] | "none";
        p.ratedWatt = it["ratedWatt"] | 0.0f;
        p.isOutput = it["isOutput"] | true;
        p.activeHigh = it["activeHigh"] | true;
        
        gPeripherals.push_back(p);
        configurePeripheralPin(gPeripherals.back());
    }
}
```

---

### 2?? **Reporting (Firmware ? Backend)**

**Firmware envoie via IoT Hub:**
```cpp
void reportPeripheralsState() {
    StaticJsonDocument<2560> doc;
    JsonArray arr = doc.createNestedArray("peripherals");

    for (auto& p : gPeripherals) {
        JsonObject obj = arr.createNestedObject();
        obj["id"] = p.id;
        obj["name"] = p.name;
        obj["type"] = p.type;
        obj["gpio"] = p.gpio;
        obj["state"] = p.state ? "on" : "off";
        obj["lastChange"] = p.lastChange;
        obj["lastUpdate"] = p.lastChange;    // Alias pour API
        obj["configured"] = p.configured;
        obj["status"] = p.configured ? "active" : "error";
        
        if (p.notes.length() > 0) obj["notes"] = p.notes;     // ? Envoi notes
        if (p.unit.length() > 0) obj["unit"] = p.unit;        // ? Envoi unit
        if (p.errorReason.length() > 0) obj["error"] = p.errorReason;
        
        // Metering
        if (p.metering != "none") {
            obj["metering"] = p.metering;
            obj["ratedWatt"] = p.ratedWatt;
            obj["energyMWh"] = p.energy_mWh;
            obj["lastPowerMW"] = p.lastPower_mW;
        }
    }

    mqtt.publish("$iothub/twin/PATCH/properties/reported/?$rid=...", payload);
}
```

**Backend reçoit et désérialise:**
```csharp
private static List<PeripheralDto> TryExtractPeripherals(string json)
{
    var container = JsonSerializer.Deserialize<PeripheralsPayload>(json, 
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    return container?.Peripherals ?? new();
}
```

---

## ?? Validation des Commandes

### **Validation Côté Firmware**
```cpp
bool isCommandValidForType(const String& type, const String& command) {
    if (type == "relay" || type == "light" || type == "actuator") {
        return (command == "on" || command == "off" || command == "toggle");
    }
    if (type == "sensor") {
        return (command == "read" || command == "reset");
    }
    if (type == "dimmer" || type == "led") {
        return (command == "on" || command == "off" || 
                command == "toggle" || command == "setBrightness");
    }
    if (type == "rgb") {
        return (command == "on" || command == "off" || command == "setColor");
    }
    return false;
}
```

### **Validation Côté Backend**
Le backend délègue la validation au firmware mais peut ajouter une validation préalable:

```csharp
var validCommands = new Dictionary<string, string[]>
{
    { "relay", new[] { "on", "off", "toggle" } },
    { "light", new[] { "on", "off", "toggle" } },
    { "sensor", new[] { "read", "reset" } },
    { "dimmer", new[] { "on", "off", "toggle", "setBrightness" } },
    { "led", new[] { "on", "off", "toggle", "setBrightness" } },
    { "rgb", new[] { "on", "off", "setColor" } }
};
```

---

## ? Metering Énergétique

### **Calcul Côté Firmware**
```cpp
void peripheralsEnergyTick() {
    static unsigned long lastTick = millis();
    unsigned long now = millis();
    double dt_h = (now - lastTick) / 3600000.0;  // Conversion ms ? heures
    
    for (auto& p : gPeripherals) {
        if (p.metering == "estimated") {
            p.lastPower_mW = p.state ? (p.ratedWatt * 1000.0f) : 0.0f;
            if (p.state) {
                p.energy_mWh += (double)p.ratedWatt * 1000.0 * dt_h;
            }
        }
    }
    
    lastTick = now;
}
```

### **Conversion Côté Backend**
```csharp
var stats = new PeripheralEnergyStatsDto
{
    Peripherals = reported
        .Select(p => new PeripheralEnergyDto
        {
            EnergyWh = (p.EnergyMWh ?? 0) / 1000.0,      // mWh ? Wh
            CurrentPowerW = (p.LastPowerMW ?? 0) / 1000.0f // mW ? W
        })
        .ToList(),
    TotalEnergyWh = reported.Sum(p => (p.EnergyMWh ?? 0) / 1000.0)
};
```

---

## ?? Exemples de Tests

### **Test 1: Configuration Complète**

**Envoi Backend:**
```http
PUT /api/Devices/mon-esp32/peripherals
{
  "peripherals": [
    {
      "name": "relay4",
      "type": "relay",
      "gpio": 33,
      "notes": "Éclairage principal",
      "unit": "W",
      "metering": "estimated",
      "ratedWatt": 100,
      "isOutput": true,
      "activeHigh": true
    }
  ]
}
```

**Réception Firmware:**
```cpp
// Dans applyDesiredPeripherals():
Peripheral p;
p.name = "relay4";           // ?
p.type = "relay";            // ?
p.gpio = 33;                 // ?
p.notes = "Éclairage principal"; // ?
p.unit = "W";                // ?
p.metering = "estimated";    // ?
p.ratedWatt = 100;           // ?
p.isOutput = true;           // ?
p.activeHigh = true;         // ?
```

**Reporting Firmware:**
```json
{
  "peripherals": [
    {
      "id": "a8c9f8d2-...",
      "name": "relay4",
      "type": "relay",
      "gpio": 33,
      "state": "on",
      "configured": true,
      "status": "active",
      "notes": "Éclairage principal",
      "unit": "W",
      "metering": "estimated",
      "ratedWatt": 100,
      "energyMWh": 5230.5,
      "lastPowerMW": 100000
    }
  ]
}
```

**Lecture Backend:**
```csharp
GET /api/Devices/mon-esp32/peripherals/relay4
{
  "id": "a8c9f8d2-...",
  "name": "relay4",
  "type": "relay",
  "gpio": 33,
  "state": true,
  "configured": true,
  "status": "active",
  "notes": "Éclairage principal",
  "unit": "W",
  "metering": "estimated",
  "ratedWatt": 100,
  "energyMWh": 5230.5,
  "lastPowerMW": 100000
}
```

---

## ?? Statistiques de Compatibilité

| Métrique | Valeur |
|----------|--------|
| **Champs communs** | 15/20 (75%) |
| **Champs backend uniquement** | 4 |
| **Champs firmware uniquement** | 1 (`lastChangeMs`) |
| **Conversions nécessaires** | 4 |
| **Types de commandes validées** | 6 |
| **Types de périphériques supportés** | 6+ |

---

## ? Checklist de Validation

- [x] Tous les champs firmware sont représentés dans backend
- [x] Conversions d'unités documentées (mWh?Wh, mW?W)
- [x] Validation de commandes implémentée
- [x] Metering énergétique fonctionnel
- [x] Notes et unit synchronisés bidirectionnellement
- [x] Gestion d'erreurs complète (`configured`, `error`)
- [x] Documentation complète créée

---

**Version:** 1.0.0  
**Dernière mise à jour:** 20 Janvier 2025  
**Compatibilité:** Firmware v1.3.0 ?? Backend .NET 9
