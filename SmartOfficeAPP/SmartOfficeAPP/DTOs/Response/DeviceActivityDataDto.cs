namespace SmartOfficeAPP.DTOs.Response;

/// <summary>
/// DTO pour l'activité des appareils par type
/// </summary>
public class DeviceActivityDataDto
{
    /// <summary>
    /// Type d'appareil (ex: "Climatisation", "Éclairage", "Capteur")
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// Nombre d'appareils de ce type
    /// </summary>
    public int Count { get; set; }
}
