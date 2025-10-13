using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SignalRTestController : ControllerBase
{
    private readonly ISignalRService _signalRService;

    public SignalRTestController(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    /// <summary>
    /// Teste l'envoi d'une notification à un utilisateur spécifique
    /// </summary>
    [HttpPost("test-user-notification")]
    public async Task<IActionResult> TestUserNotification([FromBody] TestNotificationRequest request)
    {
        await _signalRService.SendNotificationToUserAsync(request.UserId, request.Title, request.Message);
        return Ok(new { message = "Notification envoyée à l'utilisateur", userId = request.UserId });
    }

    /// <summary>
    /// Teste l'envoi d'une notification broadcast
    /// </summary>
    [HttpPost("test-broadcast")]
    public async Task<IActionResult> TestBroadcast([FromBody] TestBroadcastRequest request)
    {
        await _signalRService.SendBroadcastNotificationAsync(request.Title, request.Message);
        return Ok(new { message = "Notification broadcast envoyée" });
    }

    /// <summary>
    /// Teste l'envoi d'une mise à jour de périphérique
    /// </summary>
    [HttpPost("test-device-update")]
    public async Task<IActionResult> TestDeviceUpdate([FromBody] TestDeviceUpdateRequest request)
    {
        await _signalRService.SendDeviceStatusUpdateAsync(request.PeripheriqueId, request.Status, request.Data);
        return Ok(new { message = "Mise à jour de périphérique envoyée", peripheriqueId = request.PeripheriqueId });
    }

    /// <summary>
    /// Teste l'envoi d'une alerte d'incident
    /// </summary>
    [HttpPost("test-incident-alert")]
    public async Task<IActionResult> TestIncidentAlert([FromBody] TestIncidentAlertRequest request)
    {
        await _signalRService.SendIncidentAlertAsync(request.IncidentId, request.Title, request.Description, request.Priority);
        return Ok(new { message = "Alerte d'incident envoyée", incidentId = request.IncidentId });
    }

    /// <summary>
    /// Teste l'envoi d'une mise à jour de télémétrie
    /// </summary>
    [HttpPost("test-telemetry-update")]
    public async Task<IActionResult> TestTelemetryUpdate([FromBody] TestTelemetryUpdateRequest request)
    {
        await _signalRService.SendTelemetryUpdateAsync(request.PeripheriqueId, request.Data);
        return Ok(new { message = "Mise à jour de télémétrie envoyée", peripheriqueId = request.PeripheriqueId });
    }

    /// <summary>
    /// Teste l'envoi d'un résultat de commande
    /// </summary>
    [HttpPost("test-command-result")]
    public async Task<IActionResult> TestCommandResult([FromBody] TestCommandResultRequest request)
    {
        await _signalRService.SendCommandExecutionResultAsync(request.CommandId, request.PeripheriqueId, request.Status, request.Result);
        return Ok(new { message = "Résultat de commande envoyé", commandId = request.CommandId });
    }

    /// <summary>
    /// Teste l'envoi d'une notification à une zone
    /// </summary>
    [HttpPost("test-zone-notification")]
    public async Task<IActionResult> TestZoneNotification([FromBody] TestZoneNotificationRequest request)
    {
        await _signalRService.SendNotificationToZoneAsync(request.ZoneId, request.Title, request.Message);
        return Ok(new { message = "Notification envoyée à la zone", zoneId = request.ZoneId });
    }
}

public class TestNotificationRequest
{
    public required string UserId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
}

public class TestBroadcastRequest
{
    public required string Title { get; set; }
    public required string Message { get; set; }
}

public class TestDeviceUpdateRequest
{
    public int PeripheriqueId { get; set; }
    public required string Status { get; set; }
    public object? Data { get; set; }
}

public class TestIncidentAlertRequest
{
    public int IncidentId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Priority { get; set; }
}

public class TestTelemetryUpdateRequest
{
    public int PeripheriqueId { get; set; }
    public required object Data { get; set; }
}

public class TestCommandResultRequest
{
    public int CommandId { get; set; }
    public int PeripheriqueId { get; set; }
    public required string Status { get; set; }
    public required string Result { get; set; }
}

public class TestZoneNotificationRequest
{
    public int ZoneId { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
}