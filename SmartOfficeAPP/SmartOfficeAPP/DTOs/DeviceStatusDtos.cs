namespace SmartOfficeAPP.DTOs;

/// <summary>
/// DTO pour les métriques système d'un device
/// </summary>
public class DeviceMetricsDto
{
    public int Id { get; set; }
    public string IotDeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int? Rssi { get; set; }
    public int? HeapFree { get; set; }
    public int? NvsQueueSize { get; set; }
    public long? UptimeSeconds { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? FirmwareCommit { get; set; }
    public string? AdditionalData { get; set; }
}

/// <summary>
/// DTO pour les événements RFID
/// </summary>
public class RfidEventDto
{
    public int Id { get; set; }
    public string IotDeviceId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string CardUid { get; set; } = string.Empty;
    public bool AccessGranted { get; set; }
    public string? DenialReason { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
}

/// <summary>
/// DTO pour les événements d'alarme fumée
/// </summary>
public class SmokeAlarmEventDto
{
    public int Id { get; set; }
    public string IotDeviceId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsActive { get; set; }
    public string? DeactivationReason { get; set; }
    public string? DeactivatedBy { get; set; }
    public int? ZoneId { get; set; }
}

/// <summary>
/// DTO agrégé pour le statut complet d'un device
/// </summary>
public class DeviceFullStatusDto
{
    public string IotDeviceId { get; set; } = string.Empty;
    public DeviceMetricsDto? LatestMetrics { get; set; }
    public RfidEventDto? LatestRfidEvent { get; set; }
    public SmokeAlarmEventDto? ActiveAlarm { get; set; }
    public List<DevicePeripheralStatusDto> Peripherals { get; set; } = new();
    public bool IsOnline { get; set; }
    public DateTime? LastActivity { get; set; }
}

/// <summary>
/// DTO pour le statut d'un périphérique avec données énergétiques
/// </summary>
public class DevicePeripheralStatusDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Gpio { get; set; }
    public bool IsConfigured { get; set; }
    public string State { get; set; } = "unknown";
    public DateTime? LastUpdate { get; set; }
    
    // Métriques énergétiques
    public string Metering { get; set; } = "none";
    public float RatedWatt { get; set; }
    public double EnergyWh { get; set; }
    public float CurrentPowerW { get; set; }
}
