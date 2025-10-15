namespace SmartOfficeAPP.DTOs;

public class TelemetrieDto
{
    public int Id { get; set; }
    public DateTime Horodatage { get; set; }
    public string TypeEvenement { get; set; } // Correction: TypeEvenement au lieu de TypeDonnee
    public double Valeur { get; set; } // Changé de decimal à double pour correspondre au modèle
    public int PeripheriqueLogiqueId { get; set; }
}
