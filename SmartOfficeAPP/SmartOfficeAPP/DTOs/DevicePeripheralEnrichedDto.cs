namespace SmartOfficeAPP.DTOs;

/// <summary>
/// DTO enrichi fusionnant DevicePeripheral (DB) + État temps réel (IoT Hub) + Zone
/// Utilisé par l'app mobile pour afficher la page Devices
/// </summary>
public class DevicePeripheralEnrichedDto
{
    // =============================
    // 1. IDENTITÉ / BASE
    // =============================
    
    /// <summary>
    /// ID unique du périphérique (DB)
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// GUID du périphérique (depuis firmware si disponible, sinon généré)
    /// </summary>
    public string? Guid { get; set; }
    
    /// <summary>
    /// Nom affiché du périphérique
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type de périphérique: Lighting, Climate, Security, Media, Cleaner, Door, Sensor, Actuator
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Zone / Pièce où se trouve le périphérique
    /// </summary>
    public ZoneDto? Zone { get; set; }
    
    /// <summary>
    /// ID de la zone (si Zone est null)
    /// </summary>
    public int? ZoneId { get; set; }
    
    /// <summary>
    /// État global: true = actif/en ligne, false = hors ligne/inactif
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Liste des fonctionnalités disponibles pour éviter de tester des nulls
    /// Ex: ["Brightness", "Color", "Temperature", "Lock"]
    /// </summary>
    public List<string> Capabilities { get; set; } = new();

    // =============================
    // 2. ÉTAT TEMPS RÉEL GÉNÉRIQUE
    // =============================
    
    /// <summary>
    /// Statut de connectivité: Online, Offline, Degraded
    /// </summary>
    public string ConnectivityStatus { get; set; } = "Unknown";
    
    /// <summary>
    /// Horodatage de la dernière mise à jour (UTC)
    /// </summary>
    public DateTime? LastUpdatedUtc { get; set; }
    
    /// <summary>
    /// Statut de santé: OK, Warning, Error
    /// </summary>
    public string HealthStatus { get; set; } = "Unknown";
    
    /// <summary>
    /// Message de santé (si erreur ou avertissement)
    /// </summary>
    public string? HealthMessage { get; set; }

    // =============================
    // 3. MÉTADONNÉES HARDWARE
    // =============================
    
    /// <summary>
    /// ID du device IoT Hub parent (ESP32)
    /// </summary>
    public string IotDeviceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Numéro GPIO
    /// </summary>
    public int Gpio { get; set; }
    
    /// <summary>
    /// Unité de mesure (°C, %, W, etc.)
    /// </summary>
    public string? Unit { get; set; }
    
    /// <summary>
    /// Notes de câblage
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Date d'installation
    /// </summary>
    public DateTime? InstalledAt { get; set; }

    // =============================
    // 4. ÉTAT SPÉCIFIQUE PAR TYPE (objet dynamique)
    // =============================
    
    /// <summary>
    /// État spécifique au type de périphérique (JSON flexible)
    /// Exemples:
    /// - Lighting: { brightness: 75, lightColor: "#FF5733", colorTemperature: 3000, powerConsumptionW: 12.5, energyTodayWh: 150 }
    /// - Climate: { targetTemperature: 22, currentTemperature: 21.5, climateMode: "Cool", fanSpeed: 3, humidityPercent: 55 }
    /// - Security: { armedState: "Armed", lastAlarmUtc: "2025-01-15T10:30:00Z", batteryPercent: 85 }
    /// - Media: { volume: 60, isPlaying: true, currentMediaTitle: "Song Name", mute: false }
    /// - Cleaner: { cleanerMode: "Full", batteryPercent: 70, docked: false, progressPercent: 45 }
    /// - Door: { isLocked: true, doorOpen: false, lockBatteryPercent: 90 }
    /// </summary>
    public object? TypeSpecificState { get; set; }

    // =============================
    // 5. DONNÉES ÉNERGÉTIQUES (si metering activé)
    // =============================
    
    /// <summary>
    /// Type de mesure énergétique: none, estimated, measured
    /// </summary>
    public string? Metering { get; set; }
    
    /// <summary>
    /// Puissance nominale (W)
    /// </summary>
    public float? RatedWatt { get; set; }
    
    /// <summary>
    /// Énergie consommée aujourd'hui (Wh)
    /// </summary>
    public double? EnergyTodayWh { get; set; }
    
    /// <summary>
    /// Puissance instantanée (W)
    /// </summary>
    public float? CurrentPowerW { get; set; }

    // =============================
    // 6. MÉTADONNÉES ADMINISTRATIVES
    // =============================
    
    /// <summary>
    /// Statut administratif: active, error, offline, maintenance
    /// </summary>
    public string Status { get; set; } = "active";
    
    /// <summary>
    /// Peut recevoir des commandes
    /// </summary>
    public bool IsActionable { get; set; }
    
    /// <summary>
    /// Date de création (DB)
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Dernière mise à jour (DB)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
