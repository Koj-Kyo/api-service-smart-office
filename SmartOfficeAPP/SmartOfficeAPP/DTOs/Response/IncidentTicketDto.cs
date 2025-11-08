namespace SmartOfficeAPP.DTOs.Response
{
    public class IncidentTicketDto
    {
        public int Id { get; set; }
        
        // ✅ Informations du ticket
        public string Sujet { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string? Priorite { get; set; }  // ✅ Priorité (Basse, Moyenne, Haute)
        
        // ✅ Informations utilisateur (créateur)
        public string CreatedById { get; set; } = string.Empty;
        public string? CreatedByName { get; set; }
        public string? CreatedByEmail { get; set; }
        
        // ✅ Informations assignation (si applicable)
        public string? AssignedToId { get; set; }
        public string? AssignedToName { get; set; }
        
        // ✅ Informations zone
        public int? ZoneId { get; set; }
        public string? ZoneName { get; set; }
        
        // ✅ Informations périphérique
        public int? PeripheriqueLogiqueId { get; set; }
        public string? PeripheriqueLogiqueName { get; set; }
        public string? PeripheriqueLogiqueType { get; set; }
        
        // ✅ Dates
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}

