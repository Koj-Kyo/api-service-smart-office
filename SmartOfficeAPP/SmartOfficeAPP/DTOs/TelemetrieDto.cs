namespace SmartOfficeAPP.DTOs;

public class TelemetrieDto
{
    public int Id { get; set; }
    public DateTime Horodatage { get; set; }
    public string TypeDonnee { get; set; }
    public decimal Valeur { get; set; }
    public int PeripheriqueLogiqueId { get; set; }
}
