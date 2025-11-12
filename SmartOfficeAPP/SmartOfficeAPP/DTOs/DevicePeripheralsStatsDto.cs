namespace SmartOfficeAPP.DTOs;

/// <summary>
/// DTO pour les statistiques agrégées des périphériques
/// Utilisé par l'app mobile pour afficher les compteurs globaux
/// </summary>
public class DevicePeripheralsStatsDto
{
    /// <summary>
    /// Nombre total de périphériques
    /// </summary>
    public int TotalDevices { get; set; }
    
    /// <summary>
    /// Nombre de périphériques actifs (en ligne)
    /// </summary>
    public int ActiveDevicesCount { get; set; }
    
    /// <summary>
    /// Nombre de zones distinctes
    /// </summary>
    public int ZonesCount { get; set; }
    
    /// <summary>
    /// Comptage par type de périphérique
    /// Ex: { "Lighting": 6, "Climate": 2, "Security": 3, "Sensor": 8, "Actuator": 5 }
    /// </summary>
    public Dictionary<string, int> CountsByType { get; set; } = new();
    
    /// <summary>
    /// Nombre de périphériques pouvant recevoir des commandes (actionneurs)
    /// </summary>
    public int ActionableDevicesCount { get; set; }
    
    /// <summary>
    /// Nombre de périphériques hors ligne
    /// </summary>
    public int OfflineDevicesCount { get; set; }
    
    /// <summary>
    /// Nombre de périphériques en erreur
    /// </summary>
    public int ErrorDevicesCount { get; set; }
    
    /// <summary>
    /// Consommation énergétique totale instantanée (W)
    /// </summary>
    public double TotalCurrentPowerW { get; set; }
    
    /// <summary>
    /// Énergie totale consommée aujourd'hui (Wh)
    /// </summary>
    public double TotalEnergyTodayWh { get; set; }
    
    /// <summary>
    /// Date de génération des statistiques (UTC)
    /// </summary>
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}
