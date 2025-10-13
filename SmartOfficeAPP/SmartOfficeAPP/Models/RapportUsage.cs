using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class RapportUsage
{
    // Clé primaire
    public int Id { get; set; }

    // Nom du rapport
    [Required]
    public required string NomRapport { get; set; }

    // Période de temps du rapport
    [Required]
    public required string Periode { get; set; }

    // Données du rapport, stockées en JSON
    [Required]
    public required string DonneesJson { get; set; }

    // Date de génération du rapport
    public DateTime DateGeneration { get; set; }

    // Clé étrangère vers la zone concernée (peut être null)
    public int? ZoneId { get; set; }
    public Zone? Zone { get; set; }

    // Clé étrangère vers le périphérique concerné (peut être null)
    public int? PeripheriqueLogiqueId { get; set; }
    public PeripheriqueLogique? PeripheriqueLogique { get; set; }
}
