using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs.Request
{
    public class IncidentTicketCreateDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } // Par exemple: Ouvert, En cours, Résolu

        [Required]
        public string Priority { get; set; } // Par exemple: Basse, Moyenne, Haute

        public string AssignedToId { get; set; }
    }
}
