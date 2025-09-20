using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.Models;

public class Zone
{
    // Clé primaire de l'entité Zone
    public int Id { get; set; }

    // Nom de la zone (ex: "Salle de réunion", "Bureau 1")
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    // Collection des périphériques de cette zone (relation 1-à-plusieurs)
    public ICollection<PeripheriqueLogique> Peripheriques { get; set; }

    // Collection des droits d'accès à cette zone (relation plusieurs-à-plusieurs)
    public ICollection<DroitAcces> DroitsAcces { get; set; }
}
