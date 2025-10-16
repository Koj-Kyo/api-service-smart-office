using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

/// <summary>
/// Représente un capteur ou actionneur physique branché sur un device IoT (ESP32, etc.)
/// </summary>
public class DevicePeripheral
{
    public int Id { get; set; }

    /// <summary>
    /// Nom unique du périphérique (doit correspondre au nom dans le firmware)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Type de périphérique: relay, temp, smoke, motion, etc.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string Type { get; set; }

    /// <summary>
    /// Numéro GPIO sur lequel le périphérique est branché
    /// </summary>
    [Required]
    public int Gpio { get; set; }

    /// <summary>
    /// ID du device IoT parent (ESP32) dans IoT Hub
    /// </summary>
    [Required]
    [MaxLength(255)]
    public required string IotDeviceId { get; set; }

    /// <summary>
    /// Notes de câblage et installation (optionnel)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Unité de mesure si capteur (°C, %, W, etc.)
    /// </summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    /// <summary>
    /// Schéma de câblage (URL ou texte)
    /// </summary>
    [MaxLength(500)]
    public string? WiringDiagram { get; set; }

    /// <summary>
    /// Date d'installation physique
    /// </summary>
    public DateTime? InstalledAt { get; set; }

    /// <summary>
    /// Dernier état connu (JSON sérialisé)
    /// </summary>
    public string? LastKnownState { get; set; }

    /// <summary>
    /// Dernière mise à jour reçue du firmware
    /// </summary>
    public DateTime? LastUpdateReceived { get; set; }

    /// <summary>
    /// Statut: active, error, offline, maintenance
    /// </summary>
    [MaxLength(50)]
    public string Status { get; set; } = "active";

    /// <summary>
    /// Lien optionnel vers PeripheriqueLogique (device virtuel/logique dans la zone)
    /// </summary>
    public int? PeripheriqueLogiqueId { get; set; }
    public PeripheriqueLogique? PeripheriqueLogique { get; set; }

    /// <summary>
    /// Dates de tracking
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
