using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartOfficeAPP.Models;

public class Telemetrie
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID du périphérique logique concerné.
    /// </summary>
    [Required]
    public int PeripheriqueLogiqueId { get; set; }

    /// <summary>
    /// Réfèrence vers l'objet PeripheriqueLogique.
    /// </summary>
    [ForeignKey("PeripheriqueLogiqueId")]
<<<<<<< HEAD
    public PeripheriqueLogique? PeripheriqueLogique { get; set; }
=======
    public PeripheriqueLogique PeripheriqueLogique { get; set; }
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0

    /// <summary>
    /// Date et heure de l'événement. Un index sera créé sur cette colonne.
    /// </summary>
    [Required]
    public DateTime Horodatage { get; set; }

    /// <summary>
    /// Type d'événement (par exemple, "Impression", "Scan", "Activation", "Consommation_Energie").
    /// </summary>
    [Required]
<<<<<<< HEAD
    public required string TypeEvenement { get; set; }
=======
    public string TypeEvenement { get; set; }
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0

    /// <summary>
    /// Valeur associée à l'événement (par exemple, nombre de pages imprimées, kWh consommés).
    /// </summary>
    public double Valeur { get; set; }

    /// <summary>
<<<<<<< HEAD
    /// Informations supplémentaires en JSON (nullable pour compatibilité avec migration initiale).
    /// </summary>
    public string? DonneesJson { get; set; }
=======
    /// Informations supplémentaires en JSON.
    /// </summary>
    public string DonneesJson { get; set; }
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
}
