using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs
{
    public class ZoneUpdateDto
    {
        [Required(ErrorMessage = "Le nom de la zone est requis.")]
        [MaxLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Nom { get; set; }
    }
}
