using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartOfficeAPP.Models;

/// <summary>
/// Événement RFID (scan de carte)
/// </summary>
public class RfidEvent
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
    /// Timestamp du scan
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// UID de la carte RFID (format hexadécimal)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public required string CardUid { get; set; }

    /// <summary>
    /// Accès autorisé ou refusé
    /// </summary>
    [Required]
    public bool AccessGranted { get; set; }

    /// <summary>
    /// Raison du refus (si applicable)
    /// </summary>
    [MaxLength(255)]
    public string? DenialReason { get; set; }

    /// <summary>
    /// ID utilisateur associé (si identifié)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Action effectuée (ouverture porte, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? Action { get; set; }
}
