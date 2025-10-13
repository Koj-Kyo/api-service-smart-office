namespace SmartOfficeAPP.DTOs
{
    public class PeripheriqueLogiqueDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string IotDeviceId { get; set; } = string.Empty;
        public int ZoneId { get; set; }
    }
}
