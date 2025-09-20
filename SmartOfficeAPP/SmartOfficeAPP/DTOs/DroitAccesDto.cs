using SmartOfficeAPP.Enums;

namespace SmartOfficeAPP.DTOs;

public class DroitAccesDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int? ZoneId { get; set; }
    public string ZoneName { get; set; }
    public string ZoneDescription { get; set; } = string.Empty;
    public int? PeripheriqueLogiqueId { get; set; }
    public string PeripheriqueLogiqueName { get; set; }
    public string Type { get; set; }
}
