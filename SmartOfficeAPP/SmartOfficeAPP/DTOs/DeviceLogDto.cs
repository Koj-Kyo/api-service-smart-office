namespace SmartOfficeAPP.DTOs;

public class DeviceLogDto
{
    public int Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string? Peripheral { get; set; }
    public string Level { get; set; } = "info";
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Context { get; set; }
    public DateTime CreatedAt { get; set; }
}
