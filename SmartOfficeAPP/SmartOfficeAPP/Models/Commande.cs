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
    public string NomCommande { get; set; }

    // Valeur de la commande (ex: "true", "false", "22.5")
    public string Valeur { get; set; }

    // Clé étrangère vers le périphérique ciblé
    public int PeripheriqueLogiqueId { get; set; }
    public PeripheriqueLogique PeripheriqueLogique { get; set; }

    // Statut de la commande (ex: "En attente", "Succès", "Échec")
    [Required]
    public string Statut { get; set; }

    // Clé étrangère vers l'utilisateur qui a envoyé la commande
    [Required]
    public string UserId { get; set; }
}
