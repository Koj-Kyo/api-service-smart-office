namespace SmartOfficeAPP.DTOs.Response
{
    public class CommandeResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int? CommandeId { get; set; }
    }
}
