# ? Réponses Backend - Commandes Avancées Mobile

## ?? Questions Résolues

Vous avez demandé de clarifier le format des endpoints backend pour :
1. **Luminosité** (Brightness)
2. **Couleur** (Color RGB/HEX)
3. **Température** (Thermostat)

---

## ? Réponses Officielles

### ?? Résumé Technique

| Fonctionnalité | Endpoint | Format Valeur | Exemple |
|----------------|----------|---------------|---------|
| **Luminosité** | `POST /api/Commandes` | `"0"-"100"` (string %) | `{ "nomCommande": "setBrightness", "valeur": "75" }` |
| **Couleur HEX** | `POST /api/Commandes` | `"#RRGGBB"` (string) | `{ "nomCommande": "setColor", "valeur": "#FF0000" }` |
| **Couleur RGB** | `POST /api/Commandes` | `"R,G,B"` (string) | `{ "nomCommande": "setColorRGB", "valeur": "255,0,0" }` |
| **Température** | `POST /api/Commandes` | `"X.X"` (string °C) | `{ "nomCommande": "setTargetTemperature", "valeur": "22.5" }` |
| **Mode Thermostat** | `POST /api/Commandes` | `"mode"` (string) | `{ "nomCommande": "setThermostatMode", "valeur": "heat" }` |

---

## ?? Détails Confirmés

### 1?? Luminosité

**? Format Confirmé :**
```http
POST /api/Commandes
Content-Type: application/json
Authorization: Bearer {token}

{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setBrightness",
  "valeur": "75"  // STRING, 0-100%
}
```

**Commandes Disponibles :**
- `setBrightness` : Valeur absolue (0-100%)
- `increaseBrightness` : Augmenter de X%
- `decreaseBrightness` : Diminuer de X%
- `setBrightnessPWM` : Valeur PWM (0-255)

---

### 2?? Couleur

**? Format HEX (Recommandé) :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setColor",
  "valeur": "#FF0000"  // STRING HEX
}
```

**? Format RGB (Alternative) :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setColorRGB",
  "valeur": "255,0,0"  // STRING R,G,B
}
```

**Couleurs Prédéfinies :**
- `setColorRed` ? `"#FF0000"`
- `setColorGreen` ? `"#00FF00"`
- `setColorBlue` ? `"#0000FF"`
- `setColorWhite` ? `"#FFFFFF"`
- `setColorWarm` ? `"#FFD700"`
- `setColorCool` ? `"#E0FFFF"`

---

### 3?? Température

**? Format Confirmé :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 15,
  "nomCommande": "setTargetTemperature",
  "valeur": "22.5"  // STRING, °C avec décimale
}
```

**Modes Thermostat :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 15,
  "nomCommande": "setThermostatMode",
  "valeur": "heat"  // "heat", "cool", "auto", "off"
}
```

---

## ?? Documentation Créée

### Documents Principaux

1. **[MAUI-ADVANCED-COMMANDS-GUIDE.md](MAUI-ADVANCED-COMMANDS-GUIDE.md)**
   - Guide complet avec exemples
   - Code C# MAUI prêt à l'emploi
   - Exemples XAML
   - 60+ lignes de code réutilisable

2. **[MAUI-ADVANCED-COMMANDS-CHEAT-SHEET.md](MAUI-ADVANCED-COMMANDS-CHEAT-SHEET.md)**
   - Résumé 1 page
   - Réponses ultra-rapides
   - Table de référence

### Exemple de Code Fourni

**Classe `DeviceCommandService.cs` complète** avec :
- ? `SetBrightnessAsync(peripheriqueId, brightness)`
- ? `SetColorAsync(peripheriqueId, hexColor)`
- ? `SetColorRGBAsync(peripheriqueId, r, g, b)`
- ? `SetColorHSVAsync(peripheriqueId, h, s, v)`
- ? `SetTargetTemperatureAsync(peripheriqueId, temp)`
- ? `SetThermostatModeAsync(peripheriqueId, mode)`
- ? Gestion d'erreurs
- ? Validation des paramètres

**Page XAML complète** avec :
- Slider luminosité
- Boutons couleurs
- Stepper température
- Picker mode thermostat

---

## ?? Points Clés à Retenir

### Architecture Backend

1. **Endpoint Unique** : `/api/Commandes` (POST)
2. **Valeurs en String** : Toutes les valeurs sont des strings
3. **Validation Firmware** : Pas de validation backend
4. **Headers Requis** : `Authorization: Bearer {token}`

### Flux de Commande

```
MOBILE APP  ?  Backend API  ?  IoT Hub  ?  Firmware ESP32
(MAUI)         (.NET 9)        (Azure)     (Device)
```

### Gestion des Erreurs

```csharp
var response = await apiService.SendCommandAsync(...);

if (response?.IsSuccess == true) {
    // ? Commande réussie
} else {
    // ? Erreur : afficher response.Message
}
```

---

## ?? Exemple d'Utilisation MAUI

```csharp
// Service d'injection
services.AddSingleton<DeviceCommandService>();

// Dans votre Page
private readonly DeviceCommandService _commandService;

// Luminosité
await _commandService.SetBrightnessAsync(peripheriqueId, 75);

// Couleur
await _commandService.SetColorAsync(peripheriqueId, "#FF0000");
await _commandService.SetPresetColorAsync(peripheriqueId, "red");

// Température
await _commandService.SetTargetTemperatureAsync(peripheriqueId, 22.5);
await _commandService.SetThermostatModeAsync(peripheriqueId, ThermostatMode.Heat);
```

---

## ?? Checklist Validation

### Backend
- [x] Endpoint `/api/Commandes` documenté
- [x] Format valeur confirmé (string)
- [x] Exemples fournis
- [x] Tests HTTP disponibles

### Mobile
- [x] Service C# fourni
- [x] Méthodes typées créées
- [x] Validation paramètres implémentée
- [x] Gestion erreurs incluse
- [x] Exemples XAML fournis

### Documentation
- [x] Guide complet créé
- [x] Cheat sheet créé
- [x] Code réutilisable fourni
- [x] Index mis à jour

---

## ?? Liens Rapides

| Document | Usage |
|----------|-------|
| [MAUI-ADVANCED-COMMANDS-GUIDE.md](MAUI-ADVANCED-COMMANDS-GUIDE.md) | Référence complète + code |
| [MAUI-ADVANCED-COMMANDS-CHEAT-SHEET.md](MAUI-ADVANCED-COMMANDS-CHEAT-SHEET.md) | Aide-mémoire 1 page |
| [MAUI-MOBILE-INDEX.md](MAUI-MOBILE-INDEX.md) | Index général |
| [Tests/37-Incidents-Complete.http](Tests/37-Incidents-Complete.http) | Tests API backend |

---

## ? Prochaines Étapes

1. **Consulter** : [MAUI-ADVANCED-COMMANDS-GUIDE.md](MAUI-ADVANCED-COMMANDS-GUIDE.md)
2. **Copier** : La classe `DeviceCommandService.cs`
3. **Adapter** : Selon vos besoins
4. **Tester** : Avec le backend démarré
5. **Intégrer** : Dans votre app MAUI

---

**Tout est documenté et prêt à l'emploi ! ??**

**Questions résolues :**
- ? 1. Luminosité : `POST /api/Commandes` avec `{ "valeur": "75" }`
- ? 2. Couleur : `POST /api/Commandes` avec `{ "valeur": "#FF0000" }` ou `"255,0,0"`
- ? 3. Température : `POST /api/Commandes` avec `{ "valeur": "22.5" }`

Bon développement ! ??
