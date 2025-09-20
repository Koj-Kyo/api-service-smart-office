using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class JournalAudit
{
    // Clé primaire
    public int Id { get; set; }

    // Date et heure de l'événement
    public DateTime Horodatage { get; set; }

    // Clé étrangère vers l'utilisateur impliqué (peut être null si l'action est système)
    public string UserId { get; set; }

    // Description de l'action
    [Required]
    public string Action { get; set; }

    // Contexte de l'action (ex: "Modification de rôle", "Envoi de commande")
    [Required]
    public string Contexte { get; set; }
}
