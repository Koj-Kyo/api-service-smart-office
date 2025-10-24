using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartOfficeAPP.Models;

/// <summary>
/// Événement d'alarme fumée
/// </summary>
public class SmokeAlarmEvent
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
    /// Timestamp du début de l'alarme
    /// </summary>
    [Required]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Timestamp de la fin de l'alarme (null si active)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Durée de l'alarme (secondes)
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Alarme active
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// Raison de la désactivation
    /// </summary>
    [MaxLength(255)]
    public string? DeactivationReason { get; set; }

    /// <summary>
    /// ID utilisateur qui a désactivé (si applicable)
    /// </summary>
    public string? DeactivatedBy { get; set; }

    /// <summary>
    /// Zone concernée
    /// </summary>
    public int? ZoneId { get; set; }

    [ForeignKey("ZoneId")]
    public Zone? Zone { get; set; }
}
