using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs
{
    public class PeripheriqueLogiqueUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        [Required]
        [MaxLength(50)]
        public string Type { get; set; }
    }
}
