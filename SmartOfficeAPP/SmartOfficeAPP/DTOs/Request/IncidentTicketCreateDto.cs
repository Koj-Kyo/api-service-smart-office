using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs.Request
{
    public class IncidentTicketCreateDto
    {
        [Required]
        public string Sujet { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Statut { get; set; } // Par exemple: Ouvert, En cours, Résolu

        public string Priorite { get; set; } = "Moyenne";  // ✅ Priorité (Basse, Moyenne, Haute)

        public int? ZoneId { get; set; }
        public int? PeripheriqueLogiqueId { get; set; }
    }
}
