namespace SmartOfficeAPP.DTOs;

public class DevicePeripheralDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Gpio { get; set; }
    public string IotDeviceId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Unit { get; set; }
    public string? WiringDiagram { get; set; }
    public DateTime? InstalledAt { get; set; }
    public string? LastKnownState { get; set; }
    public DateTime? LastUpdateReceived { get; set; }
    public string Status { get; set; } = "active";
    public int? PeripheriqueLogiqueId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DevicePeripheralCreationDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Gpio { get; set; }
    public string IotDeviceId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Unit { get; set; }
    public string? WiringDiagram { get; set; }
    public DateTime? InstalledAt { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}

public class DevicePeripheralUpdateDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public int? Gpio { get; set; }
    public string? Notes { get; set; }
    public string? Unit { get; set; }
    public string? WiringDiagram { get; set; }
    public DateTime? InstalledAt { get; set; }
    public string? Status { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}
