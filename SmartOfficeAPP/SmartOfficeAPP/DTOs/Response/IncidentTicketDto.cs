namespace SmartOfficeAPP.DTOs.Response
{
    public class IncidentTicketDto
    {
        public int Id { get; set; }
        public string Sujet { get; set; }
        public string Description { get; set; }
        public string Statut { get; set; }
        public string UserId { get; set; }
        public DateTime DateCreation { get; set; }
        public DateTime? DateResolution { get; set; }
        public int? ZoneId { get; set; }
        public int? PeripheriqueLogiqueId { get; set; }
    }
}
