using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.Services.IServices;
using System.Text.Json;

namespace SmartOfficeAPP.Controllers;

/// <summary>
/// Contrôleur pour les commandes directes vers les devices IoT
/// Compatible avec firmware ESP32 v2.0.0
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,Manager")]
public class IoTHubDeviceController : ControllerBase
{
    private readonly IIotHubService _iotHubService;
    private readonly ILogger<IoTHubDeviceController> _logger;

    public IoTHubDeviceController(
        IIotHubService iotHubService,
        ILogger<IoTHubDeviceController> logger)
    {
        _iotHubService = iotHubService;
        _logger = logger;
    }

    /// <summary>
    /// Invoque une méthode directe sur un device
    /// </summary>
    [HttpPost("{deviceId}/method/{methodName}")]
    public async Task<IActionResult> InvokeMethod(
        string deviceId, 
        string methodName, 
        [FromBody] DirectMethodPayload payload)
    {
        var payloadJson = payload?.Payload ?? "{}";
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, methodName, payloadJson);
        
        if (!success)
            return BadRequest(new { message = $"Échec de l'invocation de la méthode '{methodName}'" });
        
        return Ok(new { message = $"Méthode '{methodName}' exécutée avec succès" });
    }

    /// <summary>
    /// Obtient le statut complet d'un device (firmware v2.0)
    /// </summary>
    [HttpPost("{deviceId}/status")]
    public async Task<IActionResult> GetDeviceStatus(string deviceId)
    {
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "getStatus", "{}");
        
        if (!success)
            return BadRequest(new { message = "Impossible de récupérer le statut du device" });
        
        return Ok(new { message = "Requête de statut envoyée. Consultez les logs pour la réponse." });
    }

    /// <summary>
    /// Ouvre la porte d'un device
    /// </summary>
    [HttpPost("{deviceId}/door/open")]
    public async Task<IActionResult> OpenDoor(string deviceId)
    {
        _logger.LogInformation("Commande d'ouverture de porte pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "openDoor", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de l'ouverture de la porte" });
        
        return Ok(new { message = "Porte ouverte avec succès" });
    }

    /// <summary>
    /// Ferme la porte d'un device
    /// </summary>
    [HttpPost("{deviceId}/door/close")]
    public async Task<IActionResult> CloseDoor(string deviceId)
    {
        _logger.LogInformation("Commande de fermeture de porte pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "closeDoor", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de la fermeture de la porte" });
        
        return Ok(new { message = "Porte fermée avec succès" });
    }

    /// <summary>
    /// Redémarre un device
    /// </summary>
    [HttpPost("{deviceId}/reboot")]
    public async Task<IActionResult> RebootDevice(string deviceId)
    {
        _logger.LogWarning("Redémarrage du device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "reboot", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec du redémarrage du device" });
        
        return Ok(new { message = "Device en cours de redémarrage (2s)" });
    }

    /// <summary>
    /// Contrôle un périphérique spécifique (firmware v2.0)
    /// </summary>
    [HttpPost("{deviceId}/peripheral/{peripheralName}/{command}")]
    public async Task<IActionResult> ControlPeripheral(
        string deviceId, 
        string peripheralName, 
        string command,
        [FromBody] PeripheralControlParameters? parameters = null)
    {
        _logger.LogInformation("Commande périphérique: {Device}/{Peripheral}/{Command}", 
            deviceId, peripheralName, command);

        var payload = new
        {
            name = peripheralName,
            command = command,
            parameters = parameters?.Parameters ?? new Dictionary<string, object>()
        };

        var success = await _iotHubService.InvokeDeviceMethodAsync(
            deviceId, 
            "peripheralCommand", 
            JsonSerializer.Serialize(payload));
        
        if (!success)
            return BadRequest(new { message = $"Échec de la commande '{command}' sur '{peripheralName}'" });
        
        return Ok(new { 
            message = $"Commande '{command}' exécutée sur '{peripheralName}'",
            device = deviceId,
            peripheral = peripheralName,
            command = command
        });
    }

    /// <summary>
    /// Active l'alarme fumée
    /// </summary>
    [HttpPost("{deviceId}/alarm/on")]
    public async Task<IActionResult> AlarmOn(string deviceId)
    {
        _logger.LogWarning("Activation alarme fumée pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "alarmOn", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de l'activation de l'alarme" });
        
        return Ok(new { message = "Alarme fumée activée" });
    }

    /// <summary>
    /// Désactive l'alarme fumée
    /// </summary>
    [HttpPost("{deviceId}/alarm/off")]
    public async Task<IActionResult> AlarmOff(string deviceId)
    {
        _logger.LogInformation("Désactivation alarme fumée pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "alarmOff", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de la désactivation de l'alarme" });
        
        return Ok(new { message = "Alarme fumée désactivée" });
    }

    /// <summary>
    /// Réinitialise les compteurs énergétiques d'un device (firmware v2.0)
    /// </summary>
    [HttpPost("{deviceId}/energy/reset")]
    public async Task<IActionResult> ResetEnergy(string deviceId)
    {
        _logger.LogInformation("Réinitialisation compteurs énergie pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "resetEnergy", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de la réinitialisation des compteurs" });
        
        return Ok(new { message = "Compteurs énergétiques réinitialisés" });
    }

    /// <summary>
    /// Force un heartbeat immédiat
    /// </summary>
    [HttpPost("{deviceId}/heartbeat")]
    public async Task<IActionResult> ForceHeartbeat(string deviceId)
    {
        _logger.LogInformation("Force heartbeat pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "sendHeartbeat", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de l'envoi du heartbeat" });
        
        return Ok(new { message = "Heartbeat forcé envoyé" });
    }

    /// <summary>
    /// Envoie un log de test depuis le device
    /// </summary>
    [HttpPost("{deviceId}/test/log")]
    public async Task<IActionResult> SendTestLog(string deviceId, [FromBody] TestLogPayload payload)
    {
        _logger.LogInformation("Envoi log test pour device {DeviceId}", deviceId);

        var logPayload = new
        {
            level = payload.Level ?? "info",
            source = payload.Source ?? "api_test",
            message = payload.Message ?? "Test log from API"
        };
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(
            deviceId, 
            "sendLog", 
            JsonSerializer.Serialize(logPayload));
        
        if (!success)
            return BadRequest(new { message = "Échec de l'envoi du log test" });
        
        return Ok(new { message = "Log test envoyé avec succès" });
    }

    /// <summary>
    /// Force une télémétrie immédiate
    /// </summary>
    [HttpPost("{deviceId}/telemetry/send")]
    public async Task<IActionResult> ForceTelemetry(string deviceId)
    {
        _logger.LogInformation("Force télémétrie pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "sendTelemetry", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de l'envoi de la télémétrie" });
        
        return Ok(new { message = "Télémétrie forcée envoyée" });
    }

    /// <summary>
    /// Récupère les informations détaillées du Device Twin
    /// </summary>
    [HttpGet("{deviceId}/twin")]
    public async Task<IActionResult> GetDeviceTwin(string deviceId)
    {
        var twin = await _iotHubService.GetDeviceTwinAsync(deviceId);
        
        if (twin == null)
            return NotFound(new { message = $"Device Twin introuvable pour '{deviceId}'" });
        
        return Ok(twin);
    }

    /// <summary>
    /// Liste tous les devices IoT Hub
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> ListDevices([FromQuery] int maxCount = 100)
    {
        var devices = await _iotHubService.ListDevicesAsync(maxCount);
        return Ok(devices);
    }

    /// <summary>
    /// Obtient le statut de connexion de plusieurs devices
    /// </summary>
    [HttpPost("connection-status")]
    public async Task<IActionResult> GetConnectionStatus([FromBody] DeviceListPayload payload)
    {
        if (payload?.DeviceIds == null || !payload.DeviceIds.Any())
            return BadRequest(new { message = "Liste de devices requise" });

        var status = await _iotHubService.GetDevicesConnectionStatusAsync(payload.DeviceIds);
        return Ok(status);
    }

    /// <summary>
    /// Envoie un message Cloud-to-Device
    /// </summary>
    [HttpPost("{deviceId}/message")]
    public async Task<IActionResult> SendCloudToDeviceMessage(
        string deviceId, 
        [FromBody] CloudMessagePayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload?.Message))
            return BadRequest(new { message = "Message requis" });

        var success = await _iotHubService.SendCloudToDeviceMessageAsync(
            deviceId, 
            payload.Message, 
            payload.Properties);
        
        if (!success)
            return BadRequest(new { message = "Échec de l'envoi du message" });
        
        return Ok(new { message = "Message Cloud-to-Device envoyé avec succès" });
    }

    /// <summary>
    /// Crée ou met à jour un device dans IoT Hub
    /// </summary>
    [HttpPut("{deviceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateOrUpdateDevice(string deviceId, [FromBody] DeviceUpdatePayload payload)
    {
        var success = await _iotHubService.CreateOrUpdateDeviceAsync(deviceId, payload?.Enabled ?? true);
        
        if (!success)
            return StatusCode(500, new { message = $"Échec de création/mise à jour du device '{deviceId}'" });
        
        return Ok(new { message = $"Device '{deviceId}' créé/mis à jour avec succès" });
    }

    /// <summary>
    /// Supprime un device d'IoT Hub
    /// </summary>
    [HttpDelete("{deviceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDevice(string deviceId)
    {
        _logger.LogWarning("Suppression du device {DeviceId}", deviceId);
        
        var success = await _iotHubService.DeleteDeviceAsync(deviceId);
        
        if (!success)
            return StatusCode(500, new { message = $"Échec de suppression du device '{deviceId}'" });
        
        return Ok(new { message = $"Device '{deviceId}' supprimé avec succès" });
    }

    /// <summary>
    /// Obtient le rapport énergétique d'un device (firmware v2.0)
    /// </summary>
    [HttpPost("{deviceId}/energy/report")]
    public async Task<IActionResult> GetEnergyReport(string deviceId)
    {
        _logger.LogInformation("Demande rapport énergie pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "getEnergyReport", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de la récupération du rapport énergétique" });
        
        return Ok(new { message = "Rapport énergétique demandé. Consultez les logs pour les détails." });
    }

    /// <summary>
    /// Obtient les diagnostics complets d'un device (firmware v2.0)
    /// </summary>
    [HttpPost("{deviceId}/diagnostics")]
    public async Task<IActionResult> GetDiagnostics(string deviceId)
    {
        _logger.LogInformation("Demande diagnostics pour device {DeviceId}", deviceId);
        
        var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, "getDiagnostics", "{}");
        
        if (!success)
            return BadRequest(new { message = "Échec de la récupération des diagnostics" });
        
        return Ok(new { message = "Diagnostics demandés. Consultez les logs pour les détails." });
    }
}

#region DTOs pour IoTHubDevice

public class DirectMethodPayload
{
    public string? Payload { get; set; }
}

public class PeripheralControlParameters
{
    public Dictionary<string, object>? Parameters { get; set; }
}

public class TestLogPayload
{
    public string? Level { get; set; }
    public string? Source { get; set; }
    public string? Message { get; set; }
}

public class DeviceListPayload
{
    public List<string> DeviceIds { get; set; } = new();
}

public class CloudMessagePayload
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string>? Properties { get; set; }
}

public class DeviceUpdatePayload
{
    public bool Enabled { get; set; } = true;
}

#endregion
