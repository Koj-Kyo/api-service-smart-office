using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class Notification
{
    // Clé primaire
    public int Id { get; set; }

    // Clé étrangère vers l'utilisateur destinataire
    [Required]
    public string UserId { get; set; }

    // Titre de la notification
    [Required]
    public string Titre { get; set; }

    // Contenu du message
    [Required]
    public string Message { get; set; }

    // Date et heure de la notification
    public DateTime Horodatage { get; set; }

    // Statut de lecture (par défaut, non lu)
    public bool EstLue { get; set; } = false;
}
