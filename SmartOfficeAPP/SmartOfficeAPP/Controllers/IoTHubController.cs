using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Services.ServicesImpl;

namespace SmartOfficeAPP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IoTHubController : ControllerBase
{
    private readonly IIotHubService _iotHubService;
    private readonly IPeripheriqueLogiqueService _peripheriqueService;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<IoTHubController> _logger;

    public IoTHubController(
        IIotHubService iotHubService,
        IPeripheriqueLogiqueService peripheriqueService,
        ISignalRService signalRService,
        ILogger<IoTHubController> logger)
    {
        _iotHubService = iotHubService;
        _peripheriqueService = peripheriqueService;
        _signalRService = signalRService;
        _logger = logger;
    }

    /// <summary>
    /// Liste tous les devices disponibles dans Azure IoT Hub
    /// </summary>
    [HttpGet("devices")]
    public async Task<ActionResult<IEnumerable<IoTHubDeviceSummary>>> ListDevices([FromQuery] int max = 100)
    {
        try
        {
            var devices = await _iotHubService.ListDevicesAsync(Math.Clamp(max, 1, 1000));
            return Ok(devices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la liste des devices");
            return StatusCode(500, new { message = "Erreur lors de la récupération des devices" });
        }
    }

    /// <summary>
    /// Synchronise les devices depuis Azure IoT Hub vers la base de données
    /// </summary>
    [HttpPost("devices/sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncDevicesFromIoTHub()
    {
        try
        {
            var syncedCount = await _peripheriqueService.SyncDevicesFromIoTHubAsync();
            
            await _signalRService.SendBroadcastNotificationAsync(
                "IoT Hub Sync",
                $"{syncedCount} nouveaux devices synchronisés depuis Azure IoT Hub"
            );

            return Ok(new 
            { 
                success = true, 
                syncedCount, 
                message = $"{syncedCount} devices synchronisés avec succès depuis Azure IoT Hub" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation des devices depuis IoT Hub");
            return StatusCode(500, new { success = false, message = "Erreur lors de la synchronisation des devices" });
        }
    }

    /// <summary>
    /// Invoque une méthode directe sur un device IoT
    /// </summary>
    [HttpPost("device/{deviceId}/method/{methodName}")]
    public async Task<IActionResult> InvokeDeviceMethod(
        string deviceId,
        string methodName,
        [FromBody] DeviceMethodPayload payload)
    {
        try
        {
            var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, methodName, payload.Payload ?? "{}");
            
            if (success)
            {
                await _signalRService.SendDeviceStatusUpdateAsync(
                    0, // PeripheriqueId sera résolu côté client
                    "method_invoked",
                    new { deviceId, methodName, timestamp = DateTime.UtcNow }
                );

                return Ok(new { success = true, message = $"Méthode {methodName} invoquée avec succès sur {deviceId}" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Échec de l'invocation de la méthode {methodName} sur {deviceId}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'invocation de la méthode {MethodName} sur {DeviceId}", methodName, deviceId);
            return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Met à jour les desired properties d'un device twin
    /// </summary>
    [HttpPut("device/{deviceId}/twin/desired")]
    public async Task<IActionResult> UpdateDesiredProperties(
        string deviceId,
        [FromBody] DesiredPropertiesPayload payload)
    {
        try
        {
            var success = await _iotHubService.UpdateDesiredPropertiesAsync(deviceId, payload.DesiredProperties);
            
            if (success)
            {
                await _signalRService.SendDeviceStatusUpdateAsync(
                    0, // PeripheriqueId sera résolu côté client
                    "desired_properties_updated",
                    new { deviceId, properties = payload.DesiredProperties, timestamp = DateTime.UtcNow }
                );

                return Ok(new { success = true, message = $"Desired properties mises à jour pour {deviceId}" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Échec de la mise à jour des desired properties pour {deviceId}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour des desired properties pour {DeviceId}", deviceId);
            return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère les informations du device twin
    /// </summary>
    [HttpGet("device/{deviceId}/twin")]
    public async Task<ActionResult<DeviceTwinInfo>> GetDeviceTwin(string deviceId)
    {
        try
        {
            var twinInfo = await _iotHubService.GetDeviceTwinAsync(deviceId);
            
            if (twinInfo == null)
            {
                return NotFound(new { message = $"Device {deviceId} non trouvé" });
            }

            return Ok(twinInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du twin pour {DeviceId}", deviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Envoie un message Cloud-to-Device
    /// </summary>
    [HttpPost("device/{deviceId}/message")]
    public async Task<IActionResult> SendCloudToDeviceMessage(
        string deviceId,
        [FromBody] CloudToDeviceMessagePayload payload)
    {
        try
        {
            var success = await _iotHubService.SendCloudToDeviceMessageAsync(
                deviceId, 
                payload.Message, 
                payload.Properties);
            
            if (success)
            {
                await _signalRService.SendDeviceStatusUpdateAsync(
                    0, // PeripheriqueId sera résolu côté client
                    "cloud_message_sent",
                    new { deviceId, message = payload.Message, timestamp = DateTime.UtcNow }
                );

                return Ok(new { success = true, message = $"Message envoyé à {deviceId}" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Échec de l'envoi du message à {deviceId}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi du message à {DeviceId}", deviceId);
            return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Crée ou met à jour un device dans IoT Hub
    /// </summary>
    [HttpPost("device/{deviceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateOrUpdateDevice(
        string deviceId,
        [FromBody] CreateDevicePayload payload)
    {
        try
        {
            var success = await _iotHubService.CreateOrUpdateDeviceAsync(deviceId, payload.Enabled);
            
            if (success)
            {
                await _signalRService.SendBroadcastNotificationAsync(
                    "Device Management",
                    $"Device {deviceId} créé/mis à jour avec succès"
                );

                return Ok(new { success = true, message = $"Device {deviceId} créé/mis à jour avec succès" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Échec de la création/mise à jour du device {deviceId}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création/mise à jour du device {DeviceId}", deviceId);
            return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Supprime un device d'IoT Hub
    /// </summary>
    [HttpDelete("device/{deviceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDevice(string deviceId)
    {
        try
        {
            var success = await _iotHubService.DeleteDeviceAsync(deviceId);
            
            if (success)
            {
                await _signalRService.SendBroadcastNotificationAsync(
                    "Device Management",
                    $"Device {deviceId} supprimé avec succès"
                );

                return Ok(new { success = true, message = $"Device {deviceId} supprimé avec succès" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Échec de la suppression du device {deviceId}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du device {DeviceId}", deviceId);
            return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Obtient le statut de connexion de plusieurs devices
    /// </summary>
    [HttpPost("devices/connection-status")]
    public async Task<ActionResult<IEnumerable<DeviceConnectionInfo>>> GetDevicesConnectionStatus(
        [FromBody] DeviceStatusRequest request)
    {
        try
        {
            var connectionInfos = await _iotHubService.GetDevicesConnectionStatusAsync(request.DeviceIds);
            return Ok(connectionInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du statut de connexion des devices");
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Synchronise les périphériques physiques d'un device spécifique depuis IoT Hub
    /// </summary>
    [HttpPost("device/{iotDeviceId}/peripherals/sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncPeripheralsFromIoTHub(string iotDeviceId)
    {
        try
        {
            var peripheralService = HttpContext.RequestServices.GetRequiredService<IDevicePeripheralService>();
            var syncedCount = await peripheralService.SyncPeripheralsFromIoTHubAsync(iotDeviceId);
            
            await _signalRService.SendBroadcastNotificationAsync(
                "Peripherals Sync",
                $"{syncedCount} périphériques synchronisés depuis IoT Hub pour {iotDeviceId}"
            );

            return Ok(new 
            { 
                success = true, 
                syncedCount, 
                deviceId = iotDeviceId,
                message = $"{syncedCount} périphériques physiques synchronisés depuis IoT Hub" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation des périphériques pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { success = false, message = "Erreur lors de la synchronisation des périphériques" });
        }
    }

    /// <summary>
    /// Synchronise tous les périphériques physiques de tous les devices depuis IoT Hub
    /// </summary>
    [HttpPost("peripherals/sync-all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncAllPeripheralsFromIoTHub()
    {
        try
        {
            var peripheralService = HttpContext.RequestServices.GetRequiredService<IDevicePeripheralService>();
            var totalSynced = await peripheralService.SyncAllPeripheralsFromIoTHubAsync();
            
            await _signalRService.SendBroadcastNotificationAsync(
                "Peripherals Global Sync",
                $"{totalSynced} périphériques physiques synchronisés depuis tous les devices IoT Hub"
            );

            return Ok(new 
            { 
                success = true, 
                totalSynced, 
                message = $"{totalSynced} périphériques physiques synchronisés depuis IoT Hub" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation globale des périphériques");
            return StatusCode(500, new { success = false, message = "Erreur lors de la synchronisation globale" });
        }
    }

    /// <summary>
    /// Commandes prédéfinies pour les périphériques smart office
    /// </summary>
    [HttpPost("device/{deviceId}/smart-commands/{command}")]
    public async Task<IActionResult> ExecuteSmartCommand(
        string deviceId,
        string command,
        [FromBody] SmartCommandPayload? payload = null)
    {
        try
        {
            string methodName;
            string methodPayload;

            switch (command.ToLowerInvariant())
            {
                case "turn-on":
                    methodName = "setState";
                    methodPayload = "{\"state\": 1}";
                    break;
                case "turn-off":
                    methodName = "setState";
                    methodPayload = "{\"state\": 0}";
                    break;
                case "get-status":
                    methodName = "getStatus";
                    methodPayload = "{}";
                    break;
                case "reset":
                    methodName = "reset";
                    methodPayload = "{}";
                    break;
                case "set-brightness":
                    if (payload?.Value == null)
                        return BadRequest("La valeur de luminosité est requise");
                    methodName = "setBrightness";
                    methodPayload = $"{{\"brightness\": {payload.Value}}}";
                    break;
                case "set-temperature":
                    if (payload?.Value == null)
                        return BadRequest("La valeur de température est requise");
                    methodName = "setTemperature";
                    methodPayload = $"{{\"temperature\": {payload.Value}}}";
                    break;
                default:
                    return BadRequest($"Commande {command} non reconnue");
            }

            var success = await _iotHubService.InvokeDeviceMethodAsync(deviceId, methodName, methodPayload);
            
            if (success)
            {
                await _signalRService.SendDeviceStatusUpdateAsync(
                    0,
                    "smart_command_executed",
                    new { deviceId, command, payload = methodPayload, timestamp = DateTime.UtcNow }
                );

                return Ok(new { success = true, message = $"Commande {command} exécutée avec succès sur {deviceId}" });
            }
            else
            {
                return BadRequest(new { success = false, message = $"Échec de l'exécution de la commande {command} sur {deviceId}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'exécution de la commande {Command} sur {DeviceId}", command, deviceId);
            return StatusCode(500, new { success = false, message = "Erreur interne du serveur" });
        }
    }
}

#region DTOs pour les payloads

public class DeviceMethodPayload
{
    public string? Payload { get; set; }
}

public class DesiredPropertiesPayload
{
    public string DesiredProperties { get; set; } = "{}";
}

public class CloudToDeviceMessagePayload
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string>? Properties { get; set; }
}

public class CreateDevicePayload
{
    public bool Enabled { get; set; } = true;
}

public class DeviceStatusRequest
{
    public List<string> DeviceIds { get; set; } = new();
}

public class SmartCommandPayload
{
    public double? Value { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

#endregion