using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class PeripheriqueLogique
{
    // Clé primaire de l'entité PeripheriqueLogique
    public int Id { get; set; }

    // Nom du périphérique (ex: "Lampe de bureau")
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; } // Nom logique utilisé par le firmware

    // Type du périphérique (ex: "Capteur", "Lumière", "Prise connectée")
    [Required]
    [MaxLength(50)]
    public required string Type { get; set; }

    // ID unique du périphérique IoT (lien avec Azure IoT Hub)
    [Required]
    [MaxLength(255)]
    public required string IotDeviceId { get; set; }

    // Clé étrangère vers la zone à laquelle appartient ce périphérique
    public int ZoneId { get; set; }

    // Propriété de navigation vers la zone (nullable pour éviter les warnings à la construction)
    public Zone? Zone { get; set; }

    // Collection des commandes envoyées à ce périphérique
    public ICollection<Commande> Commandes { get; set; } = new List<Commande>();

    // Collection des données de télémétrie envoyées par ce périphérique
    public ICollection<Telemetrie> Telemetries { get; set; } = new List<Telemetrie>();
}
