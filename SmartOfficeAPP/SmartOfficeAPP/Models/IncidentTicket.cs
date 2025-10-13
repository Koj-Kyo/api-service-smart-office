using SmartOfficeAPP.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class IncidentTicket
{
    // Clé primaire
    public int Id { get; set; }

    // Sujet du ticket
    [Required]
    public required string Sujet { get; set; }

    // Description du problème
    [Required]
    public required string Description { get; set; }

    // Statut du ticket (ex: "Ouvert", "En cours", "Fermé")
    [Required]
    public required string Statut { get; set; }

    // Date de création
    public DateTime DateCreation { get; set; }

    // Date de résolution (peut être null)
    public DateTime? DateResolution { get; set; }

    // Clé étrangère vers l'utilisateur qui a créé le ticket
    [Required]
    public required string UserId { get; set; }
    public ApplicationUser? User { get; set; }

    // Clé étrangère vers la zone concernée (peut être null)
    public int? ZoneId { get; set; }
    public Zone? Zone { get; set; }

    // Clé étrangère vers le périphérique concerné (peut être null)
    public int? PeripheriqueLogiqueId { get; set; }
    public PeripheriqueLogique? PeripheriqueLogique { get; set; }
}

