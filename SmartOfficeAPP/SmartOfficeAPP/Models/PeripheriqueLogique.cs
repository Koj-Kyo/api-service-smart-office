using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class PeripheriqueLogique
{
    // Clé primaire de l'entité PeripheriqueLogique
    public int Id { get; set; }

    // Nom du périphérique (ex: "Lampe de bureau")
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    // Type du périphérique (ex: "Capteur", "Lumière", "Prise connectée")
    [Required]
    [MaxLength(50)]
    public string Type { get; set; }

    // ID unique du périphérique IoT (lien avec Azure IoT Hub)
    [Required]
    [MaxLength(255)]
    public string IotDeviceId { get; set; }

    // Clé étrangère vers la zone à laquelle appartient ce périphérique
    public int ZoneId { get; set; }

    // Propriété de navigation vers la zone
    public Zone Zone { get; set; }

    // Collection des commandes envoyées à ce périphérique
    public ICollection<Commande> Commandes { get; set; }

    // Collection des données de télémétrie envoyées par ce périphérique
    public ICollection<Telemetrie> Telemetries { get; set; }
}
