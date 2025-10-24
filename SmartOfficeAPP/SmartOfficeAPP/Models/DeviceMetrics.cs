using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartOfficeAPP.Models;

/// <summary>
/// Métriques système du device (RSSI, Heap, Uptime, etc.)
/// </summary>
public class DeviceMetrics
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID du device IoT Hub
    /// </summary>
    [Required]
    [MaxLength(255)]
    public required string IotDeviceId { get; set; }

    /// <summary>
    /// Timestamp de la métrique
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Signal WiFi (dBm)
    /// </summary>
    public int? Rssi { get; set; }

    /// <summary>
    /// Mémoire libre (bytes)
    /// </summary>
    public int? HeapFree { get; set; }

    /// <summary>
    /// Taille de la queue NVS
    /// </summary>
    public int? NvsQueueSize { get; set; }

    /// <summary>
    /// Uptime du device (secondes)
    /// </summary>
    public long? UptimeSeconds { get; set; }

    /// <summary>
    /// Version du firmware
    /// </summary>
    [MaxLength(50)]
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// Commit du firmware
    /// </summary>
    [MaxLength(100)]
    public string? FirmwareCommit { get; set; }

    /// <summary>
    /// Données JSON additionnelles
    /// </summary>
    public string? AdditionalData { get; set; }
}
