using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

/// <summary>
/// Représente un log provenant d'un device IoT (ESP32, etc.)
/// </summary>
public class DeviceLog
{
    public int Id { get; set; }

    /// <summary>
    /// ID du device IoT dans IoT Hub
    /// </summary>
    [Required]
    [MaxLength(255)]
    public required string DeviceId { get; set; }

    /// <summary>
    /// Nom du périphérique concerné (optionnel)
    /// </summary>
    [MaxLength(100)]
    public string? Peripheral { get; set; }

    /// <summary>
    /// Niveau de log: info, warn, error
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Level { get; set; }

    /// <summary>
    /// Source du log: firmware, peripheral, network
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Source { get; set; }

    /// <summary>
    /// Message du log
    /// </summary>
    [Required]
    public required string Message { get; set; }

    /// <summary>
    /// Horodatage du log (timestamp du device)
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Contexte additionnel (JSON)
    /// </summary>
    public string? Context { get; set; }

    /// <summary>
    /// Date de création dans la base
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
