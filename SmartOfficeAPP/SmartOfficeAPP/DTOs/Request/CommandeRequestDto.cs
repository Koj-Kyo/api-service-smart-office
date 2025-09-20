using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs.Request
{
    public class CommandeRequestDto
    {
        [Required]
        public int PeripheriqueLogiqueId { get; set; }

        [Required]
        public string NomCommande { get; set; }

        public string Valeur { get; set; }
    }
}
