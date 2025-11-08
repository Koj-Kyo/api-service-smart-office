using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs;

public class DroitAccesDto
{
  public int Id { get; set; }
    
    // ✅ Utilisateur
    public string UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    
    // ✅ Zone
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
public string? ZoneDescription { get; set; }
    
    // ✅ Périphérique
    public int? PeripheriqueLogiqueId { get; set; }
  public string? PeripheriqueLogiqueName { get; set; }
    public string? PeripheriqueLogiqueType { get; set; }

    // ✅ Type d'accès
    public string Type { get; set; }
    
    // ✅ Métadonnées
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
