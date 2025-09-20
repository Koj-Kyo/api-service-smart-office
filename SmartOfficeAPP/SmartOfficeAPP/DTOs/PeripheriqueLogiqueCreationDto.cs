using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs
{
    public class PeripheriqueLogiqueCreationDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [MaxLength(50)]
        public string Type { get; set; }
        [Required]
        [MaxLength(255)]
        public string IotDeviceId { get; set; }
        [Required]
        public int ZoneId { get; set; }
    }
}
