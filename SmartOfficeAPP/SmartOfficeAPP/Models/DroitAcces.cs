using Microsoft.AspNetCore.Identity;
using SmartOfficeAPP.Enums;
using System.ComponentModel.DataAnnotations;
using SmartOfficeAPP.Models.Identity;

namespace SmartOfficeAPP.Models;

public class DroitAcces
{
    // Clé primaire composite pour cette entité de jonction
    public int Id { get; set; }

    // Clé étrangère vers l'utilisateur (IdentityUser)(Many-to-Many)(ApplicationUser)
    [Required]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    // Clé étrangère vers la zone
    public int? ZoneId { get; set; }
    public Zone Zone { get; set; }

    // Clé étrangère vers le périphérique
    public int? PeripheriqueLogiqueId { get; set; }
    public PeripheriqueLogique PeripheriqueLogique { get; set; }

    // Type de droit d'accès
    public DroitAccesType Type { get; set; }
    
    // ✅ Métadonnées de tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
