using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class Telemetrie
{
    // Clé primaire de l'entité Telemetrie
    public int Id { get; set; }

    // Date et heure de la donnée
    public DateTime Horodatage { get; set; }

    // Type de la donnée (ex: "température", "consommation_electrique")
    [Required]
    public string TypeDonnee { get; set; }

    // Valeur de la donnée
    public decimal Valeur { get; set; }

    // Clé étrangère vers le périphérique source
    public int PeripheriqueLogiqueId { get; set; }
    public PeripheriqueLogique PeripheriqueLogique { get; set; }
}
