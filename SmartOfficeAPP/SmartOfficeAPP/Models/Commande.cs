using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class Commande
{
    // Clé primaire de l'entité Commande
    public int Id { get; set; }

    // Date et heure de la commande
    public DateTime Horodatage { get; set; }

    // Type de la commande (ex: "allumer", "éteindre", "régler_température")
    [Required]
    public required string NomCommande { get; set; }

    // Valeur de la commande (ex: "true", "false", "22.5")
    [Required]
    public required string Valeur { get; set; }

    // Clé étrangère vers le périphérique ciblé
    public int PeripheriqueLogiqueId { get; set; }
    public PeripheriqueLogique? PeripheriqueLogique { get; set; }

    // Statut de la commande (ex: "En attente", "Succès", "Échec")
    [Required]
    public required string Statut { get; set; }

    // Clé étrangère vers l'utilisateur qui a envoyé la commande
    [Required]
    public required string UserId { get; set; }
}
