namespace SmartOfficeAPP.DTOs.Response
{
    public class IncidentTicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string CreatedById { get; set; }
        public string AssignedToId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
