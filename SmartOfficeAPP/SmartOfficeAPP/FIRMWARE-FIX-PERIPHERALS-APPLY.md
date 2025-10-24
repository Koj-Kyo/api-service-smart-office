# ?? FIX COMPLET - Backend + Firmware

## ?? Problème Identifié

### 1. Incompatibilité de Casse (Case Mismatch)

**Backend .NET envoyait** (PascalCase) :
```json
{
  "Name": "led_rdc",
  "Gpio": 27,
  "IsOutput": true,
  "ActiveHigh": true
}
```

**Firmware ESP32 attend** (camelCase) :
```json
{
  "name": "led_rdc",
  "gpio": 27,
  "isOutput": true,
  "activeHigh": true
}
```

### 2. Firmware ne configure pas les GPIO

Le firmware :
- ? Reçoit les desired properties
- ? Parse le JSON
- ? **Ne configure PAS** les GPIO (`pinMode()` / `digitalWrite()` manquants)
- ? **Publie `peripherals: []`** au lieu de l'état réel

---

## ? SOLUTION 1 : FIX BACKEND (APPLIQUÉ ?)

### Modification : `Program.cs`

**Ajout de la configuration JSON en camelCase** :

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

// ... existing code ...

// ? Configuration JSON pour camelCase (compatible firmware ESP32)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Utiliser camelCase pour les noms de propriétés JSON
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        
        // Ignorer les valeurs null dans la sérialisation
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        // Permettre la lecture de nombres depuis des chaînes
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        
        // Configuration pour gérer les références circulaires
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
        // Autres options utiles
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
```

### Résultat

**Avant** :
```json
{
  "Name": "led_rdc",
  "Gpio": 27,
  "IsOutput": true
}
```

**Après** :
```json
{
  "name": "led_rdc",
  "gpio": 27,
  "isOutput": true
}
```

---

## ? SOLUTION 2 : FIX FIRMWARE (À APPLIQUER)

### Preuve du Bug Firmware

**Device Twin Reported Properties** :
```json
{
  "applyStatus": "success",     // ? Dit "succès"
  "configVersion": 2,           // ? Version OK
  "peripherals": []             // ? MAIS VIDE !!!
}
```

### Code Firmware CORRIGÉ

#### Fichier: `ESP32_Firmware/src/twin.cpp`

```cpp
// Structure pour stocker les périphériques configurés
struct Peripheral {
  char name[32];
  int gpio;
  bool isOutput;
  bool activeHigh;
  bool configured;
  bool state;  // true = on/high, false = off/low
  unsigned long lastChange;
};

#define MAX_PERIPHERALS 16
Peripheral configuredPeripherals[MAX_PERIPHERALS];
int peripheralCount = 0;

// ? CORRIGÉ: Applique RÉELLEMENT la configuration
void applyDesiredPeripherals(JsonVariantConst peripheralsArray) {
  Serial.println("[Twin] ?? Application des périphériques...");
  
  // Réinitialiser la liste
  peripheralCount = 0;
  
  for (JsonVariantConst p : peripheralsArray.as<JsonArrayConst>()) {
    if (peripheralCount >= MAX_PERIPHERALS) {
      Serial.println("[Twin] ?? Limite atteinte");
      break;
    }
    
    // ? CAMELCASE: Compatible avec le backend JSON
    const char* name = p["name"];        // ? Minuscule
    int gpio = p["gpio"];                // ? Minuscule
    bool isOutput = p["isOutput"] | true;   // ? camelCase
    bool activeHigh = p["activeHigh"] | true; // ? camelCase
    
    // ? VALIDATION: Vérifier que le GPIO est valide
    if (!isValidGpio(gpio)) {
      Serial.printf("[Twin] ? GPIO %d invalide\n", gpio);
      continue;
    }
    
    // ? CONFIGURATION: Configurer le GPIO
    if (isOutput) {
      pinMode(gpio, OUTPUT);
      digitalWrite(gpio, activeHigh ? LOW : HIGH);  // État initial OFF
      Serial.printf("[Twin] ? GPIO %d configuré en OUTPUT (initial: OFF)\n", gpio);
    } else {
      pinMode(gpio, INPUT);
      Serial.printf("[Twin] ? GPIO %d configuré en INPUT\n", gpio);
    }
    
    // ? STOCKAGE: Enregistrer le périphérique
    Peripheral& periph = configuredPeripherals[peripheralCount];
    strncpy(periph.name, name, sizeof(periph.name) - 1);
    periph.gpio = gpio;
    periph.isOutput = isOutput;
    periph.activeHigh = activeHigh;
    periph.configured = true;
    periph.state = false;  // OFF par défaut
    periph.lastChange = millis();
    
    peripheralCount++;
  }
  
  Serial.printf("[Twin] ? %d périphériques configurés\n", peripheralCount);
  
  // ? RAPPORT: Publier le statut ET les périphériques
  reportApplyStatus("success", peripheralCount);
  reportPeripheralsState();
}

// ? VALIDATION: Vérifier que le GPIO est autorisé
bool isValidGpio(int gpio) {
  // GPIO réservés (SPI RFID)
  if (gpio == 18 || gpio == 19 || gpio == 23 || gpio == 5) {
    return false;
  }
  
  // GPIO invalides ESP32
  if (gpio < 0 || gpio > 39 || gpio == 20 || gpio == 24 || gpio >= 28 && gpio <= 31) {
    return false;
  }
  
  return true;
}

// ? CORRIGÉ: Publie l'état RÉEL des périphériques
void reportPeripheralsState() {
  DynamicJsonDocument doc(4096);
  JsonObject reported = doc.to<JsonObject>();
  
  JsonArray peripherals = reported.createNestedArray("peripherals");
  
  // ? Boucler sur les périphériques configurés
  for (int i = 0; i < peripheralCount; i++) {
    Peripheral& p = configuredPeripherals[i];
    
    JsonObject pObj = peripherals.createNestedObject();
    pObj["name"] = p.name;
    pObj["gpio"] = p.gpio;
    pObj["state"] = p.state ? "on" : "off";
    pObj["configured"] = p.configured;
    pObj["lastUpdate"] = getISOTimestamp();
    
    // ? Si c'est un INPUT, lire l'état réel
    if (!p.isOutput) {
      int digitalState = digitalRead(p.gpio);
      pObj["value"] = digitalState;
    }
  }
  
  Serial.printf("[Twin] ?? Rapport: %d périphériques\n", peripheralCount);
  publishReportedProperties(doc);
}

// ? Fonction pour contrôler un périphérique
bool setPeripheralState(const char* name, bool state) {
  for (int i = 0; i < peripheralCount; i++) {
    Peripheral& p = configuredPeripherals[i];
    
    if (strcmp(p.name, name) == 0 && p.isOutput) {
      // Appliquer l'état en tenant compte de activeHigh
      int digitalState = (state == p.activeHigh) ? HIGH : LOW;
      digitalWrite(p.gpio, digitalState);
      
      p.state = state;
      p.lastChange = millis();
      
      Serial.printf("[Periph] %s -> %s (GPIO %d = %s)\n", 
        name, state ? "ON" : "OFF", p.gpio, digitalState ? "HIGH" : "LOW");
      
      // ? Reporter immédiatement l'état
      reportPeripheralsState();
      
      return true;
    }
  }
  
  Serial.printf("[Periph] ? Périphérique '%s' introuvable\n", name);
  return false;
}

// ? Callback pour méthode directe "peripheralCommand"
int onPeripheralCommand(const char* payload, unsigned char** response, size_t* response_size) {
  DynamicJsonDocument doc(256);
  deserializeJson(doc, payload);
  
  const char* name = doc["name"];
  const char* command = doc["command"];
  
  if (!name || !command) {
    return 400;
  }
  
  bool newState;
  if (strcmp(command, "on") == 0) {
    newState = true;
  } else if (strcmp(command, "off") == 0) {
    newState = false;
  } else if (strcmp(command, "toggle") == 0) {
    // Trouver l'état actuel et inverser
    for (int i = 0; i < peripheralCount; i++) {
      if (strcmp(configuredPeripherals[i].name, name) == 0) {
        newState = !configuredPeripherals[i].state;
        break;
      }
    }
  } else {
    return 400;  // Commande invalide
  }
  
  if (setPeripheralState(name, newState)) {
    *response = (unsigned char*)strdup("{\"ok\":true}");
    *response_size = strlen((char*)*response);
    return 200;
  } else {
    *response = (unsigned char*)strdup("{\"ok\":false,\"error\":\"Peripheral not found\"}");
    *response_size = strlen((char*)*response);
    return 404;
  }
}
```

---

## ?? Modifications Firmware Requises

### 1. Ajouter la structure de données globale

```cpp
struct Peripheral {
  char name[32];
  int gpio;
  bool isOutput;
  bool activeHigh;
  bool configured;
  bool state;
  unsigned long lastChange;
};

Peripheral configuredPeripherals[16];
int peripheralCount = 0;
```

### 2. Corriger le parsing JSON (camelCase)

```cpp
// ? AVANT (PascalCase)
const char* name = p["Name"];

// ? APRÈS (camelCase)
const char* name = p["name"];
```

### 3. Implémenter la configuration GPIO

```cpp
pinMode(gpio, isOutput ? OUTPUT : INPUT);
digitalWrite(gpio, activeHigh ? LOW : HIGH);
```

### 4. Reporter l'état réel

```cpp
void reportPeripheralsState() {
  for (int i = 0; i < peripheralCount; i++) {
    // Publier l'état de chaque périphérique
  }
}
```

---

## ?? Déploiement du Fix

### Étape 1 : Backend (? DÉJÀ FAIT)

Le backend envoie maintenant en **camelCase** automatiquement.

**Vérification** :
```
PUT https://localhost:7144/api/Devices/mon-esp32/peripherals
Authorization: Bearer {token}

# La réponse et les données envoyées à IoT Hub seront en camelCase
```

### Étape 2 : Firmware ESP32 (À FAIRE)

#### 2.1 - Modifier le firmware
```bash
cd ESP32_Firmware
code src/twin.cpp  # Appliquer les corrections ci-dessus
```

#### 2.2 - Recompiler
```bash
pio run -e esp32dev
```

#### 2.3 - Flasher l'ESP32
```bash
pio run -e esp32dev -t upload -t monitor
```

#### 2.4 - Vérifier les logs série
```
[Twin] ?? Application des périphériques...
[Twin] ? GPIO 27 configuré en OUTPUT (initial: OFF)
[Twin] ? GPIO 26 configuré en OUTPUT (initial: OFF)
[Twin] ? GPIO 14 configuré en OUTPUT (initial: OFF)
[Twin] ? GPIO 32 configuré en INPUT
[Twin] ? GPIO 13 configuré en OUTPUT (initial: OFF)
[Twin] ? GPIO 25 configuré en OUTPUT (initial: OFF)
[Twin] ? 6 périphériques configurés
[Twin] ?? Rapport: 6 périphériques
```

---

## ?? Tests de Validation

### Test 1 : Vérifier le camelCase Backend

```
GET https://localhost:7144/api/Devices/mon-esp32/peripherals
Authorization: Bearer {token}
```

**Résultat attendu** :
```json
[
  {
    "id": "led-rdc-001",
    "name": "led_rdc",          // ? camelCase
    "gpio": 27,                 // ? camelCase
    "isOutput": true,           // ? camelCase
    "activeHigh": true          // ? camelCase
  }
]
```

### Test 2 : Configuration Firmware

```
PUT https://localhost:7144/api/Devices/mon-esp32/peripherals
Authorization: Bearer {token}
Content-Type: application/json

{
  "peripherals": [
    {
      "name": "led_rdc",
      "gpio": 27,
      "type": "light",
      "isOutput": true,
      "activeHigh": true
    }
  ]
}
```

**Attendre 10 secondes**, puis vérifier :

```
GET https://localhost:7144/api/Devices/mon-esp32/twin/full
Authorization: Bearer {token}
```

**Résultat attendu** :
```json
{
  "reportedPeripherals": [
    {
      "name": "led_rdc",
      "gpio": 27,
      "state": "off",
      "configured": true,
      "lastUpdate": "2025-01-23T18:00:00Z"
    }
  ]
}
```

### Test 3 : Commande LED

```
POST https://localhost:7144/api/Devices/mon-esp32/peripherals/led_rdc/commands/on
Authorization: Bearer {token}
```

**Vérifications** :
- ? **Physique** : LED GPIO 27 s'allume
- ? **Logs série** : `[Periph] led_rdc -> ON (GPIO 27 = HIGH)`
- ? **Reported** : `"state": "on"`

---

## ? Validation Finale

### Checklist Backend
- [x] Configuration JSON en camelCase ajoutée
- [x] API envoie en camelCase vers IoT Hub
- [ ] Tests API validés avec camelCase

### Checklist Firmware
- [ ] Structure `Peripheral` ajoutée
- [ ] Parsing JSON en camelCase
- [ ] `pinMode()` et `digitalWrite()` implémentés
- [ ] `reportPeripheralsState()` publie l'état réel
- [ ] Tests physiques LEDs validés

---

## ?? Dépannage

### Problème : Backend envoie encore en PascalCase

**Solution** : Redémarrer l'application backend après modification de `Program.cs` :
```bash
cd SmartOfficeAPP
dotnet build
dotnet run
```

### Problème : Firmware ne reçoit toujours pas les données

**Solution** : Vérifier le Device Twin brut dans Azure Portal :
1. Ouvrir **Azure Portal** ? **IoT Hub** ? **Devices** ? **mon-esp32**
2. Vérifier `desired.peripherals`
3. Vérifier que les clés sont en **camelCase**

### Problème : Firmware parse mais ne configure pas

**Solution** : Ajouter des logs dans `applyDesiredPeripherals()` :
```cpp
Serial.printf("[Twin] name=%s, gpio=%d\n", name, gpio);
Serial.printf("[Twin] Calling pinMode(%d, %s)\n", gpio, isOutput ? "OUTPUT" : "INPUT");
```

---

## ?? Support

**Problèmes Backend** :
- Vérifier `Program.cs` contient la configuration JSON
- Vérifier les logs : `dotnet run`
- Tester avec Postman/REST Client

**Problèmes Firmware** :
- Consulter les logs série (`pio device monitor`)
- Vérifier le Device Twin dans Azure Portal
- Forcer une synchronisation : `POST /api/Devices/mon-esp32/peripherals/sync`

---

## ?? Résultat Final

Une fois les deux fixes appliqués :

? **Backend envoie en camelCase** ? Firmware comprend
? **Firmware configure les GPIO** ? LEDs/capteurs fonctionnent
? **Firmware reporte l'état** ? Backend affiche l'état réel
? **Synchronisation complète** ? desired = reported

?? **Le système est opérationnel !**
