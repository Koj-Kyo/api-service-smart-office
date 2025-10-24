# ?? DÉPANNAGE - Firmware "0 périphériques configurés"

## ?? Symptôme

```
[Twin] ? 0 périphériques configurés
[Twin] ? Propriétés souhaitées appliquées
```

**Problème** : Le firmware **reçoit** les desired properties mais **ne trouve AUCUN périphérique** dans le JSON.

---

## ?? Diagnostic Étape par Étape

### Étape 1 : Vérifier le Format JSON Envoyé

#### 1.1 - Ouvrir le fichier de test

```
SmartOfficeAPP/Tests/27-Debug-Firmware-Zero-Peripherals.http
```

#### 1.2 - Exécuter le Test #2.1 (Twin IoT Hub)

```
GET /api/IoTHub/device/mon-esp32/twin
```

#### 1.3 - Analyser la Réponse

**Cherchez la section `desired.peripherals`** :

##### ? Format CORRECT (camelCase)
```json
{
  "desired": {
    "peripherals": [
      {
        "name": "led_rdc",        // ? Minuscule
        "gpio": 27,               // ? Minuscule
        "type": "light",          // ? Minuscule
        "isOutput": true,         // ? camelCase
        "activeHigh": true,       // ? camelCase
        "ratedWatt": 5            // ? camelCase
      }
    ]
  }
}
```

**Si vous voyez ce format** ? Passez à l'Étape 2 (Firmware)

##### ? Format INCORRECT (PascalCase)
```json
{
  "desired": {
    "peripherals": [
      {
        "Name": "led_rdc",        // ? Majuscule
        "Gpio": 27,               // ? Majuscule
        "Type": "light",          // ? Majuscule
        "IsOutput": true,         // ? PascalCase
        "ActiveHigh": true,       // ? PascalCase
        "RatedWatt": 5            // ? PascalCase
      }
    ]
  }
}
```

**Si vous voyez ce format** ? **PROBLÈME BACKEND** : Le backend n'a pas été redémarré après le fix camelCase.

**SOLUTION** :
```bash
# 1. Arrêter le backend (Ctrl+C)
cd SmartOfficeAPP

# 2. Nettoyer et recompiler
dotnet clean
dotnet build

# 3. Redémarrer
dotnet run

# 4. Re-tester avec 27-Debug-Firmware-Zero-Peripherals.http
```

---

### Étape 2 : Vérifier le Code Firmware

#### 2.1 - Localiser la Fonction de Parsing

**Fichier** : `ESP32_Firmware/src/twin.cpp` (ou similaire)

**Cherchez la fonction** : `applyDesiredPeripherals()` ou `handleTwinDesiredBody()`

#### 2.2 - Vérifier le Code Actuel

**Code BUGUÉ (Hypothétique)** :
```cpp
void applyDesiredPeripherals(JsonVariantConst peripheralsArray) {
  Serial.println("[Twin] ?? Application des périphériques...");
  
  int count = 0;
  
  // ? PROBLÈME 1: Si peripheralsArray est null/invalide, boucle = 0 itérations
  for (JsonVariantConst p : peripheralsArray.as<JsonArrayConst>()) {
    // ? PROBLÈME 2: Clés en PascalCase au lieu de camelCase
    const char* name = p["Name"];  // ? Majuscule
    int gpio = p["Gpio"];          // ? Majuscule
    
    // ? PROBLÈME 3: Pas de validation
    if (!name) {
      continue;  // ? Pas de log
    }
    
    count++;
  }
  
  // ? PROBLÈME 4: Ne configure PAS les GPIO
  Serial.printf("[Twin] ? %d périphériques configurés\n", count);
}
```

**PROBLÈMES IDENTIFIÉS** :
1. ? Clés JSON en **PascalCase** au lieu de **camelCase**
2. ? Pas de logs de débogage
3. ? Pas de validation
4. ? Ne configure pas les GPIO

---

### Étape 3 : Ajouter des Logs de Débogage

**Modifier le code firmware pour ajouter des logs** :

```cpp
void applyDesiredPeripherals(JsonVariantConst peripheralsArray) {
  Serial.println("[Twin] ?? Application des périphériques...");
  
  // ? LOG 1: Vérifier que l'array existe
  if (peripheralsArray.isNull()) {
    Serial.println("[Twin] ? peripheralsArray est NULL !");
    return;
  }
  
  // ? LOG 2: Vérifier le type
  if (!peripheralsArray.is<JsonArrayConst>()) {
    Serial.println("[Twin] ? peripherals n'est pas un array !");
    return;
  }
  
  // ? LOG 3: Afficher la taille
  JsonArrayConst arr = peripheralsArray.as<JsonArrayConst>();
  Serial.printf("[Twin] ?? Nombre d'éléments dans l'array: %d\n", arr.size());
  
  int count = 0;
  
  for (JsonVariantConst p : arr) {
    // ? LOG 4: Afficher le JSON brut de chaque élément
    Serial.println("[Twin] ?? Élément JSON:");
    serializeJsonPretty(p, Serial);
    Serial.println();
    
    // ? CORRECTION: Utiliser camelCase
    const char* name = p["name"];        // ? Minuscule
    int gpio = p["gpio"];                // ? Minuscule
    bool isOutput = p["isOutput"] | true;   // ? camelCase
    bool activeHigh = p["activeHigh"] | true; // ? camelCase
    
    // ? LOG 5: Afficher les valeurs parsées
    Serial.printf("[Twin] - name=%s, gpio=%d, isOutput=%d, activeHigh=%d\n",
                  name ? name : "NULL", gpio, isOutput, activeHigh);
    
    // ? VALIDATION
    if (!name || gpio == 0) {
      Serial.println("[Twin] ?? Périphérique invalide, ignoré");
      continue;
    }
    
    // ? CONFIGURATION: Configurer le GPIO
    if (isOutput) {
      pinMode(gpio, OUTPUT);
      digitalWrite(gpio, activeHigh ? LOW : HIGH);
      Serial.printf("[Twin] ? GPIO %d configuré en OUTPUT\n", gpio);
    } else {
      pinMode(gpio, INPUT);
      Serial.printf("[Twin] ? GPIO %d configuré en INPUT\n", gpio);
    }
    
    count++;
  }
  
  Serial.printf("[Twin] ? %d périphériques configurés\n", count);
}
```

---

### Étape 4 : Recompiler et Flasher

```bash
cd ESP32_Firmware

# Recompiler
pio run -e esp32dev

# Flasher et monitorer
pio run -e esp32dev -t upload -t monitor
```

---

### Étape 5 : Re-Tester avec Logs

#### 5.1 - Envoyer la Configuration

```
PUT /api/Devices/mon-esp32/peripherals
# (avec payload camelCase)
```

#### 5.2 - Observer les Logs Série

**Logs ATTENDUS (Si tout fonctionne)** :
```
[Twin] ?? Application des périphériques...
[Twin] ?? Nombre d'éléments dans l'array: 6
[Twin] ?? Élément JSON:
{
  "name": "led_rdc",
  "gpio": 27,
  "type": "light",
  "isOutput": true,
  "activeHigh": true
}
[Twin] - name=led_rdc, gpio=27, isOutput=1, activeHigh=1
[Twin] ? GPIO 27 configuré en OUTPUT
[Twin] - name=led_etage2, gpio=26, isOutput=1, activeHigh=1
[Twin] ? GPIO 26 configuré en OUTPUT
...
[Twin] ? 6 périphériques configurés
```

**Logs ACTUELS (Problème)** :
```
[Twin] ?? Application des périphériques...
[Twin] ?? Nombre d'éléments dans l'array: 0  // ? ARRAY VIDE
[Twin] ? 0 périphériques configurés
```

**OU** :
```
[Twin] ?? Application des périphériques...
[Twin] ?? Nombre d'éléments dans l'array: 6  // ? Array OK
[Twin] ?? Élément JSON:
{
  "Name": "led_rdc",  // ? PascalCase
  "Gpio": 27
}
[Twin] - name=NULL, gpio=0, isOutput=1, activeHigh=1  // ? name=NULL
[Twin] ?? Périphérique invalide, ignoré
...
[Twin] ? 0 périphériques configurés
```

---

## ?? Solutions par Scénario

### Scénario A : Array Vide (`size = 0`)

**Cause** : Le backend n'envoie pas le JSON correctement

**Solution** :
1. Vérifier que `Program.cs` a la configuration camelCase
2. Redémarrer le backend
3. Re-envoyer la configuration
4. Vérifier le Twin dans Azure Portal

---

### Scénario B : Clés PascalCase (`name=NULL`)

**Cause** : Le firmware utilise des clés PascalCase au lieu de camelCase

**Solution** :
1. Modifier le firmware pour utiliser `p["name"]` au lieu de `p["Name"]`
2. Recompiler et flasher
3. Re-tester

---

### Scénario C : Structure JSON Différente

**Cause** : Le firmware s'attend à une structure différente

**Exemple** :
```cpp
// Firmware s'attend à:
JsonObject peripherals = desired["peripherals"];
const char* name = peripherals["led_rdc"]["name"];

// Mais backend envoie:
JsonArray peripherals = desired["peripherals"];
```

**Solution** :
1. Consulter la documentation du firmware
2. Adapter le format backend si nécessaire
3. OU adapter le firmware pour accepter un array

---

## ?? Checklist de Dépannage

### Backend
- [ ] Configuration camelCase dans `Program.cs`
- [ ] Backend redémarré après modification
- [ ] GET `/peripherals` retourne camelCase
- [ ] GET `/twin` (IoT Hub) retourne camelCase

### Firmware
- [ ] Parsing JSON en camelCase (`name`, `gpio`, `isOutput`)
- [ ] Logs de débogage ajoutés
- [ ] Validation des données
- [ ] Configuration GPIO (`pinMode`, `digitalWrite`)
- [ ] Recompilé et flashé

### Tests
- [ ] Configuration envoyée avec camelCase
- [ ] Logs firmware affichent `size > 0`
- [ ] Logs firmware affichent `name != NULL`
- [ ] Logs firmware affichent `X périphériques configurés` (X > 0)
- [ ] LEDs s'allument physiquement

---

## ?? Test Final

Une fois tous les fixes appliqués :

```
PUT /api/Devices/mon-esp32/peripherals
# (payload camelCase)

# Attendre 10 secondes

# Logs ESP32 attendus:
# [Twin] ? 6 périphériques configurés

# Vérifier:
GET /api/Devices/mon-esp32/peripherals
# Résultat attendu:
# {
#   "name": "led_rdc",
#   "state": "off",       // ? PAS null
#   "configured": true    // ? PAS null
# }
```

---

## ?? Support

**Si le problème persiste après tous ces tests** :

1. **Partagez les logs complets** du firmware
2. **Partagez la réponse** de `GET /api/IoTHub/device/mon-esp32/twin`
3. **Partagez le code firmware** de `applyDesiredPeripherals()`

Cela permettra d'identifier précisément où se situe le blocage. ??
