namespace SmartOfficeAPP.DTOs;

public class TelemetrieDto
{
    public int Id { get; set; }
    public DateTime Horodatage { get; set; }
    public string TypeEvenement { get; set; } // Correction: TypeEvenement au lieu de TypeDonnee
    public decimal Valeur { get; set; }
    public int PeripheriqueLogiqueId { get; set; }
}
