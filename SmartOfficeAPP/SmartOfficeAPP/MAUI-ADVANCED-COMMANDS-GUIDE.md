# ?? MAUI Mobile - Commandes Avancées : Luminosité, Couleur, Température

## ?? Réponses aux Questions Mobile

### ? Questions Posées

1. **Luminosité** : Format de l'endpoint et du payload ?
2. **Couleur** : Format pour RGB/HEX ?
3. **Température** : Format pour les thermostats ?

---

## ? Réponses Officielles du Backend

### 1?? Luminosité (Brightness)

#### Format Actuel
Le backend utilise un **endpoint générique** pour toutes les commandes :

```http
POST /api/Commandes
Content-Type: application/json
Authorization: Bearer {token}

{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setBrightness",
  "valeur": "75"
}
```

#### Détails
- **Endpoint** : `/api/Commandes` (générique)
- **Méthode** : `POST`
- **Payload** :
  ```json
  {
    "peripheriqueLogiqueId": 12,       // ID du périphérique (LED, ampoule, etc.)
    "nomCommande": "setBrightness",     // Nom de la commande
    "valeur": "75"                      // Valeur en % (0-100), envoyée en STRING
  }
  ```

#### Types de Valeurs Supportées
- **Pourcentage** : `"75"` (0-100%)
- **Absolue** : `"255"` (0-255 pour PWM)

#### Commandes Disponibles
| Commande | Description | Valeur |
|----------|-------------|--------|
| `setBrightness` | Définir luminosité | `"0"` à `"100"` (%) |
| `increaseBrightness` | Augmenter +10% | `"10"` |
| `decreaseBrightness` | Diminuer -10% | `"10"` |
| `setBrightnessPWM` | Valeur PWM directe | `"0"` à `"255"` |

---

### 2?? Couleur (Color)

#### Format Actuel

##### Option A : Couleur HEX (Recommandé)
```json
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setColor",
  "valeur": "#FF0000"  // Rouge en HEX
}
```

##### Option B : RGB Séparé (Alternative)
```json
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setColorRGB",
  "valeur": "255,0,0"  // Rouge : R=255, G=0, B=0
}
```

##### Option C : JSON Structuré (Si firmware supporte)
```json
{
  "peripheriqueLogiqueId": 12,
  "nomCommande": "setColorJSON",
  "valeur": "{\"r\":255,\"g\":0,\"b\":0}"
}
```

#### Couleurs Prédéfinies
| Commande | Couleur | Valeur HEX |
|----------|---------|------------|
| `setColorRed` | Rouge | `"#FF0000"` |
| `setColorGreen` | Vert | `"#00FF00"` |
| `setColorBlue` | Bleu | `"#0000FF"` |
| `setColorWhite` | Blanc | `"#FFFFFF"` |
| `setColorWarm` | Blanc chaud | `"#FFD700"` |
| `setColorCool` | Blanc froid | `"#E0FFFF"` |

#### Gestion des Espaces Colorimétriques
```json
// HSV (Teinte, Saturation, Valeur)
{
  "nomCommande": "setColorHSV",
  "valeur": "120,100,100"  // Vert pur (H=120°, S=100%, V=100%)
}

// HSL (Teinte, Saturation, Luminosité)
{
  "nomCommande": "setColorHSL",
  "valeur": "120,100,50"   // Vert pur (H=120°, S=100%, L=50%)
}
```

---

### 3?? Température (Thermostat)

#### Format Actuel

##### Définir Température Cible
```json
{
  "peripheriqueLogiqueId": 15,
  "nomCommande": "setTargetTemperature",
  "valeur": "22.5"  // Température en °C, STRING avec décimale
}
```

##### Lire Température Actuelle
```json
{
  "peripheriqueLogiqueId": 15,
  "nomCommande": "getTemperature",
  "valeur": ""  // Valeur vide pour lecture
}
```

#### Modes de Fonctionnement
```json
// Mode chauffage
{
  "nomCommande": "setThermostatMode",
  "valeur": "heat"
}

// Mode climatisation
{
  "nomCommande": "setThermostatMode",
  "valeur": "cool"
}

// Mode auto
{
  "nomCommande": "setThermostatMode",
  "valeur": "auto"
}

// Arrêt
{
  "nomCommande": "setThermostatMode",
  "valeur": "off"
}
```

#### Plages de Température
```json
// Définir plage min/max
{
  "nomCommande": "setTemperatureRange",
  "valeur": "18,25"  // Min 18°C, Max 25°C
}
```

---

## ?? Résumé des Formats Backend

| Fonctionnalité | Endpoint | Méthode | Format Valeur | Exemple |
|----------------|----------|---------|---------------|---------|
| **Luminosité** | `/api/Commandes` | POST | `"0"-"100"` (%) | `"75"` |
| **Couleur HEX** | `/api/Commandes` | POST | `"#RRGGBB"` | `"#FF0000"` |
| **Couleur RGB** | `/api/Commandes` | POST | `"R,G,B"` | `"255,0,0"` |
| **Température** | `/api/Commandes` | POST | `"X.X"` (°C) | `"22.5"` |
| **Mode Thermostat** | `/api/Commandes` | POST | `"heat|cool|auto|off"` | `"heat"` |

---

## ?? Exemples de Code MAUI

### Classe `DeviceCommandService.cs`

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartOfficeMobile.Services
{
    public class DeviceCommandService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DeviceCommandService(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        /// <summary>
        /// Envoie une commande générique à un périphérique
        /// </summary>
        private async Task<CommandeResponseDto?> SendCommandAsync(int peripheriqueId, string command, string value)
        {
            try
            {
                var payload = new CommandeRequestDto
                {
                    PeripheriqueLogiqueId = peripheriqueId,
                    NomCommande = command,
                    Valeur = value
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/Commandes", payload);
                
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CommandeResponseDto>>();
                    return apiResponse?.Data;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur commande {command}: {ex.Message}");
                return null;
            }
        }

        // ==============================================
        // ? LUMINOSITÉ
        // ==============================================

        /// <summary>
        /// Définit la luminosité d'un périphérique (0-100%)
        /// </summary>
        public async Task<bool> SetBrightnessAsync(int peripheriqueId, int brightness)
        {
            if (brightness < 0 || brightness > 100)
                throw new ArgumentOutOfRangeException(nameof(brightness), "La luminosité doit être entre 0 et 100");

            var result = await SendCommandAsync(peripheriqueId, "setBrightness", brightness.ToString());
            return result?.IsSuccess ?? false;
        }

        /// <summary>
        /// Augmente la luminosité de X%
        /// </summary>
        public async Task<bool> IncreaseBrightnessAsync(int peripheriqueId, int step = 10)
        {
            var result = await SendCommandAsync(peripheriqueId, "increaseBrightness", step.ToString());
            return result?.IsSuccess ?? false;
        }

        /// <summary>
        /// Diminue la luminosité de X%
        /// </summary>
        public async Task<bool> DecreaseBrightnessAsync(int peripheriqueId, int step = 10)
        {
            var result = await SendCommandAsync(peripheriqueId, "decreaseBrightness", step.ToString());
            return result?.IsSuccess ?? false;
        }

        // ==============================================
        // ? COULEUR
        // ==============================================

        /// <summary>
        /// Définit la couleur en HEX (#RRGGBB)
        /// </summary>
        public async Task<bool> SetColorAsync(int peripheriqueId, string hexColor)
        {
            if (!hexColor.StartsWith("#") || hexColor.Length != 7)
                throw new ArgumentException("Format HEX invalide. Utilisez #RRGGBB", nameof(hexColor));

            var result = await SendCommandAsync(peripheriqueId, "setColor", hexColor);
            return result?.IsSuccess ?? false;
        }

        /// <summary>
        /// Définit la couleur en RGB (0-255 chacun)
        /// </summary>
        public async Task<bool> SetColorRGBAsync(int peripheriqueId, int red, int green, int blue)
        {
            if (red < 0 || red > 255 || green < 0 || green > 255 || blue < 0 || blue > 255)
                throw new ArgumentOutOfRangeException("Les valeurs RGB doivent être entre 0 et 255");

            var rgbValue = $"{red},{green},{blue}";
            var result = await SendCommandAsync(peripheriqueId, "setColorRGB", rgbValue);
            return result?.IsSuccess ?? false;
        }

        /// <summary>
        /// Définit une couleur prédéfinie
        /// </summary>
        public async Task<bool> SetPresetColorAsync(int peripheriqueId, string preset)
        {
            var colors = new Dictionary<string, string>
            {
                { "red", "#FF0000" },
                { "green", "#00FF00" },
                { "blue", "#0000FF" },
                { "white", "#FFFFFF" },
                { "warm", "#FFD700" },
                { "cool", "#E0FFFF" }
            };

            if (!colors.ContainsKey(preset.ToLower()))
                throw new ArgumentException($"Preset invalide. Options: {string.Join(", ", colors.Keys)}");

            return await SetColorAsync(peripheriqueId, colors[preset.ToLower()]);
        }

        /// <summary>
        /// Définit la couleur en HSV
        /// </summary>
        public async Task<bool> SetColorHSVAsync(int peripheriqueId, int hue, int saturation, int value)
        {
            if (hue < 0 || hue > 360 || saturation < 0 || saturation > 100 || value < 0 || value > 100)
                throw new ArgumentOutOfRangeException("H: 0-360°, S: 0-100%, V: 0-100%");

            var hsvValue = $"{hue},{saturation},{value}";
            var result = await SendCommandAsync(peripheriqueId, "setColorHSV", hsvValue);
            return result?.IsSuccess ?? false;
        }

        // ==============================================
        // ? TEMPÉRATURE
        // ==============================================

        /// <summary>
        /// Définit la température cible en °C
        /// </summary>
        public async Task<bool> SetTargetTemperatureAsync(int peripheriqueId, double temperature)
        {
            if (temperature < -50 || temperature > 50)
                throw new ArgumentOutOfRangeException(nameof(temperature), "Température hors limites");

            var result = await SendCommandAsync(peripheriqueId, "setTargetTemperature", temperature.ToString("F1"));
            return result?.IsSuccess ?? false;
        }

        /// <summary>
        /// Définit le mode du thermostat
        /// </summary>
        public async Task<bool> SetThermostatModeAsync(int peripheriqueId, ThermostatMode mode)
        {
            var modeValue = mode switch
            {
                ThermostatMode.Heat => "heat",
                ThermostatMode.Cool => "cool",
                ThermostatMode.Auto => "auto",
                ThermostatMode.Off => "off",
                _ => throw new ArgumentException("Mode invalide")
            };

            var result = await SendCommandAsync(peripheriqueId, "setThermostatMode", modeValue);
            return result?.IsSuccess ?? false;
        }

        /// <summary>
        /// Lit la température actuelle
        /// </summary>
        public async Task<double?> GetCurrentTemperatureAsync(int peripheriqueId)
        {
            var result = await SendCommandAsync(peripheriqueId, "getTemperature", "");
            
            if (result?.IsSuccess == true && double.TryParse(result.Message, out var temp))
            {
                return temp;
            }
            
            return null;
        }

        /// <summary>
        /// Définit une plage de température (min, max)
        /// </summary>
        public async Task<bool> SetTemperatureRangeAsync(int peripheriqueId, double minTemp, double maxTemp)
        {
            if (minTemp >= maxTemp)
                throw new ArgumentException("Température min doit être < température max");

            var rangeValue = $"{minTemp:F1},{maxTemp:F1}";
            var result = await SendCommandAsync(peripheriqueId, "setTemperatureRange", rangeValue);
            return result?.IsSuccess ?? false;
        }
    }

    // ==============================================
    // DTOs
    // ==============================================

    public class CommandeRequestDto
    {
        public int PeripheriqueLogiqueId { get; set; }
        public string NomCommande { get; set; } = string.Empty;
        public string Valeur { get; set; } = string.Empty;
    }

    public class CommandeResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? CommandeId { get; set; }
    }

    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime? CachedAt { get; set; }
    }

    public enum ThermostatMode
    {
        Off,
        Heat,
        Cool,
        Auto
    }
}
```

---

## ?? Exemples d'Utilisation dans MAUI

### Page XAML : Contrôle Luminosité

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SmartOfficeMobile.Pages.DeviceControlPage"
             Title="Contrôle Périphérique">
    
    <StackLayout Padding="20">
        
        <!-- Luminosité -->
        <Label Text="Luminosité" FontSize="18" FontAttributes="Bold"/>
        <Slider x:Name="BrightnessSlider" 
                Minimum="0" 
                Maximum="100" 
                Value="50"
                ValueChanged="OnBrightnessChanged"/>
        <Label x:Name="BrightnessLabel" Text="50%" HorizontalTextAlignment="Center"/>
        
        <!-- Couleur -->
        <Label Text="Couleur" FontSize="18" FontAttributes="Bold" Margin="0,20,0,0"/>
        <HorizontalStackLayout Spacing="10" HorizontalOptions="Center">
            <Button Text="?? Rouge" Clicked="OnSetColorRed"/>
            <Button Text="?? Vert" Clicked="OnSetColorGreen"/>
            <Button Text="?? Bleu" Clicked="OnSetColorBlue"/>
            <Button Text="? Blanc" Clicked="OnSetColorWhite"/>
        </HorizontalStackLayout>
        
        <!-- Température -->
        <Label Text="Température" FontSize="18" FontAttributes="Bold" Margin="0,20,0,0"/>
        <Stepper x:Name="TempStepper" 
                 Minimum="16" 
                 Maximum="30" 
                 Increment="0.5" 
                 Value="22"
                 ValueChanged="OnTempChanged"/>
        <Label x:Name="TempLabel" Text="22.0°C" HorizontalTextAlignment="Center"/>
        
        <!-- Mode Thermostat -->
        <Picker x:Name="ThermostatModePicker" 
                Title="Mode Thermostat"
                SelectedIndexChanged="OnThermostatModeChanged">
            <Picker.ItemsSource>
                <x:Array Type="{x:Type x:String}">
                    <x:String>Off</x:String>
                    <x:String>Heat</x:String>
                    <x:String>Cool</x:String>
                    <x:String>Auto</x:String>
                </x:Array>
            </Picker.ItemsSource>
        </Picker>
        
    </StackLayout>
</ContentPage>
```

### Code-Behind

```csharp
using SmartOfficeMobile.Services;

namespace SmartOfficeMobile.Pages
{
    public partial class DeviceControlPage : ContentPage
    {
        private readonly DeviceCommandService _commandService;
        private int _currentPeripheriqueId = 12; // ID du périphérique

        public DeviceControlPage(DeviceCommandService commandService)
        {
            InitializeComponent();
            _commandService = commandService;
        }

        // ==============================================
        // Luminosité
        // ==============================================

        private async void OnBrightnessChanged(object sender, ValueChangedEventArgs e)
        {
            int brightness = (int)e.NewValue;
            BrightnessLabel.Text = $"{brightness}%";
            
            // Envoyer la commande
            var success = await _commandService.SetBrightnessAsync(_currentPeripheriqueId, brightness);
            
            if (!success)
            {
                await DisplayAlert("Erreur", "Échec de modification de la luminosité", "OK");
            }
        }

        // ==============================================
        // Couleur
        // ==============================================

        private async void OnSetColorRed(object sender, EventArgs e)
        {
            await _commandService.SetPresetColorAsync(_currentPeripheriqueId, "red");
        }

        private async void OnSetColorGreen(object sender, EventArgs e)
        {
            await _commandService.SetPresetColorAsync(_currentPeripheriqueId, "green");
        }

        private async void OnSetColorBlue(object sender, EventArgs e)
        {
            await _commandService.SetPresetColorAsync(_currentPeripheriqueId, "blue");
        }

        private async void OnSetColorWhite(object sender, EventArgs e)
        {
            await _commandService.SetPresetColorAsync(_currentPeripheriqueId, "white");
        }

        // ==============================================
        // Température
        // ==============================================

        private async void OnTempChanged(object sender, ValueChangedEventArgs e)
        {
            double temp = e.NewValue;
            TempLabel.Text = $"{temp:F1}°C";
            
            var success = await _commandService.SetTargetTemperatureAsync(_currentPeripheriqueId, temp);
            
            if (!success)
            {
                await DisplayAlert("Erreur", "Échec de modification de la température", "OK");
            }
        }

        private async void OnThermostatModeChanged(object sender, EventArgs e)
        {
            if (ThermostatModePicker.SelectedIndex == -1) return;
            
            var modeString = ThermostatModePicker.SelectedItem as string;
            var mode = Enum.Parse<ThermostatMode>(modeString ?? "Off");
            
            var success = await _commandService.SetThermostatModeAsync(_currentPeripheriqueId, mode);
            
            if (!success)
            {
                await DisplayAlert("Erreur", "Échec de changement de mode", "OK");
            }
        }
    }
}
```

---

## ?? Résumé Final

### ? Réponses Confirmées

| Question | Réponse | Format |
|----------|---------|--------|
| **1. Luminosité** | `POST /api/Commandes` avec `nomCommande: "setBrightness"` | `{ "valeur": "75" }` (string, 0-100%) |
| **2. Couleur HEX** | `POST /api/Commandes` avec `nomCommande: "setColor"` | `{ "valeur": "#FF0000" }` (string HEX) |
| **2. Couleur RGB** | `POST /api/Commandes` avec `nomCommande: "setColorRGB"` | `{ "valeur": "255,0,0" }` (string R,G,B) |
| **3. Température** | `POST /api/Commandes` avec `nomCommande: "setTargetTemperature"` | `{ "valeur": "22.5" }` (string, °C) |
| **3. Mode Thermostat** | `POST /api/Commandes` avec `nomCommande: "setThermostatMode"` | `{ "valeur": "heat" }` (heat/cool/auto/off) |

### ?? Points Clés

1. **Endpoint Unique** : `/api/Commandes` (POST) pour toutes les commandes
2. **Valeur en String** : Toutes les valeurs sont envoyées en `string`
3. **Format Flexible** : Le firmware détermine comment interpréter la valeur
4. **Pas de Validation Backend** : La validation est faite côté firmware

### ?? Documentation Complète

- **API Tests** : `SmartOfficeAPP/Tests/37-Incidents-Complete.http`
- **Backend Controller** : `SmartOfficeAPP/Controllers/CommandeController.cs`
- **Service** : `SmartOfficeAPP/Services/ServicesImpl/CommandeService.cs`

---

**Bon développement ! ??**
