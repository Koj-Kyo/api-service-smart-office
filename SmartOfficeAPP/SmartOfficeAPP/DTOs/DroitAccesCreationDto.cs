using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs
{
    public class DroitAccesCreationDto
    {
        [Required]
        public string UserId { get; set; }
        public int? ZoneId { get; set; }
        public int? PeripheriqueLogiqueId { get; set; }
        [Required]
        public string Type { get; set; }
    }
}
