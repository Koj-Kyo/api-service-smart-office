namespace SmartOfficeAPP.DTOs.Response;

/// <summary>
/// DTO pour un point de données de consommation énergétique
/// </summary>
public class EnergyDataPointDto
{
    /// <summary>
    /// Label de la période (ex: "Lun", "Mar", "Jan", "01/12")
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Valeur de consommation en kWh
    /// </summary>
    public double Value { get; set; }
}
