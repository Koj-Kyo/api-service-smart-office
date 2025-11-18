# ? MAUI - Commandes Avancées : Cheat Sheet

## ?? Réponses Rapides

### 1?? Luminosité (Brightness)

**Format Backend :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setBrightness",
  "valeur": "75"  // String, 0-100%
}
```

**Code MAUI :**
```csharp
await commandService.SetBrightnessAsync(peripheriqueId, 75);
```

---

### 2?? Couleur (Color)

**Format Backend (HEX) :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setColor",
  "valeur": "#FF0000"  // String HEX
}
```

**Format Backend (RGB) :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setColorRGB",
  "valeur": "255,0,0"  // String R,G,B
}
```

**Code MAUI :**
```csharp
// HEX
await commandService.SetColorAsync(peripheriqueId, "#FF0000");

// RGB
await commandService.SetColorRGBAsync(peripheriqueId, 255, 0, 0);
```

---

### 3?? Température (Thermostat)

**Format Backend :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 15,
  "nomCommande": "setTargetTemperature",
  "valeur": "22.5"  // String, °C
}
```

**Mode Thermostat :**
```http
POST /api/Commandes
{
  "peripheriqueLogiqueId": 15,
  "nomCommande": "setThermostatMode",
  "valeur": "heat"  // "heat", "cool", "auto", "off"
}
```

**Code MAUI :**
```csharp
// Température
await commandService.SetTargetTemperatureAsync(peripheriqueId, 22.5);

// Mode
await commandService.SetThermostatModeAsync(peripheriqueId, ThermostatMode.Heat);
```

---

## ?? Table de Référence

| Commande | Format Valeur | Exemple | Description |
|----------|---------------|---------|-------------|
| `setBrightness` | `"0"-"100"` | `"75"` | Luminosité en % |
| `setColor` | `"#RRGGBB"` | `"#FF0000"` | Couleur HEX |
| `setColorRGB` | `"R,G,B"` | `"255,0,0"` | Couleur RGB |
| `setColorHSV` | `"H,S,V"` | `"120,100,100"` | Couleur HSV |
| `setTargetTemperature` | `"X.X"` | `"22.5"` | Température en °C |
| `setThermostatMode` | `"mode"` | `"heat"` | Mode thermostat |

---

## ?? Points Clés

? **Endpoint unique** : `/api/Commandes` (POST)  
? **Valeur en String** : Toutes les valeurs en `string`  
? **Headers requis** : `Authorization: Bearer {token}`  
? **Validation** : Côté firmware, pas backend  

---

**Documentation complète** : `MAUI-ADVANCED-COMMANDS-GUIDE.md`
