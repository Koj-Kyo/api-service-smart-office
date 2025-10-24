using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Controllers;

/// <summary>
/// Contrôleur unifié pour toutes les fonctionnalités firmware ESP32
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class DeviceStatusController : ControllerBase
{
    private readonly IDeviceStatusService _deviceStatusService;
    private readonly ILogger<DeviceStatusController> _logger;

    public DeviceStatusController(
        IDeviceStatusService deviceStatusService,
        ILogger<DeviceStatusController> logger)
    {
        _deviceStatusService = deviceStatusService;
        _logger = logger;
    }

    // ===== STATUT COMPLET =====
    
    /// <summary>
    /// Récupère le statut complet d'un device (métriques + RFID + alarme + périphériques)
    /// </summary>
    /// <param name="iotDeviceId">ID du device IoT Hub</param>
    [HttpGet("{iotDeviceId}/full-status")]
    public async Task<ActionResult<DeviceFullStatusDto>> GetFullStatus(string iotDeviceId)
    {
        try
        {
            var status = await _deviceStatusService.GetFullDeviceStatusAsync(iotDeviceId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du statut complet pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    // ===== MÉTRIQUES SYSTÈME =====
    
    /// <summary>
    /// Enregistre des métriques système d'un device
    /// </summary>
    [HttpPost("{iotDeviceId}/metrics")]
    public async Task<ActionResult<DeviceMetricsDto>> RecordMetrics(
        string iotDeviceId,
        [FromBody] RecordMetricsRequest request)
    {
        try
        {
            var metrics = new DeviceMetrics
            {
                IotDeviceId = iotDeviceId,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                Rssi = request.Rssi,
                HeapFree = request.HeapFree,
                NvsQueueSize = request.NvsQueueSize,
                UptimeSeconds = request.UptimeSeconds,
                FirmwareVersion = request.FirmwareVersion,
                FirmwareCommit = request.FirmwareCommit,
                AdditionalData = request.AdditionalData
            };

            var result = await _deviceStatusService.RecordMetricsAsync(metrics);
            return CreatedAtAction(nameof(GetMetrics), new { iotDeviceId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement des métriques pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère les métriques d'un device
    /// </summary>
    [HttpGet("{iotDeviceId}/metrics")]
    public async Task<ActionResult<IEnumerable<DeviceMetricsDto>>> GetMetrics(
        string iotDeviceId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var metrics = await _deviceStatusService.GetMetricsAsync(iotDeviceId, startDate, endDate);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des métriques pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère la dernière métrique d'un device
    /// </summary>
    [HttpGet("{iotDeviceId}/metrics/latest")]
    public async Task<ActionResult<DeviceMetricsDto>> GetLatestMetrics(string iotDeviceId)
    {
        try
        {
            var metrics = await _deviceStatusService.GetLatestMetricsAsync(iotDeviceId);
            if (metrics == null)
                return NotFound(new { message = "Aucune métrique trouvée" });

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la dernière métrique pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    // ===== ÉVÉNEMENTS RFID =====
    
    /// <summary>
    /// Enregistre un événement RFID
    /// </summary>
    [HttpPost("{iotDeviceId}/rfid-events")]
    public async Task<ActionResult<RfidEventDto>> RecordRfidEvent(
        string iotDeviceId,
        [FromBody] RecordRfidEventRequest request)
    {
        try
        {
            var rfidEvent = new RfidEvent
            {
                IotDeviceId = iotDeviceId,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                CardUid = request.CardUid,
                AccessGranted = request.AccessGranted,
                DenialReason = request.DenialReason,
                UserId = request.UserId,
                Action = request.Action
            };

            var result = await _deviceStatusService.RecordRfidEventAsync(rfidEvent);
            return CreatedAtAction(nameof(GetRfidEvents), new { iotDeviceId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement de l'événement RFID pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère les événements RFID d'un device
    /// </summary>
    [HttpGet("{iotDeviceId}/rfid-events")]
    public async Task<ActionResult<IEnumerable<RfidEventDto>>> GetRfidEvents(
        string iotDeviceId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var events = await _deviceStatusService.GetRfidEventsAsync(iotDeviceId, startDate, endDate);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des événements RFID pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère les événements RFID par carte
    /// </summary>
    [HttpGet("rfid-events/by-card/{cardUid}")]
    public async Task<ActionResult<IEnumerable<RfidEventDto>>> GetRfidEventsByCard(
        string cardUid,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var events = await _deviceStatusService.GetRfidEventsByCardAsync(cardUid, startDate, endDate);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des événements RFID pour la carte {CardUid}", cardUid);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère tous les accès refusés
    /// </summary>
    [HttpGet("rfid-events/denied-access")]
    public async Task<ActionResult<IEnumerable<RfidEventDto>>> GetDeniedAccess(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var events = await _deviceStatusService.GetDeniedAccessAsync(startDate, endDate);
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des accès refusés");
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    // ===== ALARMES FUMÉE =====
    
    /// <summary>
    /// Enregistre un déclenchement d'alarme fumée
    /// </summary>
    [HttpPost("{iotDeviceId}/smoke-alarms")]
    public async Task<ActionResult<SmokeAlarmEventDto>> RecordSmokeAlarm(
        string iotDeviceId,
        [FromBody] RecordSmokeAlarmRequest request)
    {
        try
        {
            var alarmEvent = new SmokeAlarmEvent
            {
                IotDeviceId = iotDeviceId,
                StartTime = request.StartTime ?? DateTime.UtcNow,
                IsActive = true,
                ZoneId = request.ZoneId
            };

            var result = await _deviceStatusService.RecordSmokeAlarmAsync(alarmEvent);
            return CreatedAtAction(nameof(GetSmokeAlarms), new { iotDeviceId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement de l'alarme fumée pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Désactive une alarme fumée active
    /// </summary>
    [HttpPost("{iotDeviceId}/smoke-alarms/deactivate")]
    public async Task<IActionResult> DeactivateSmokeAlarm(
        string iotDeviceId,
        [FromBody] DeactivateSmokeAlarmRequest request)
    {
        try
        {
            var success = await _deviceStatusService.DeactivateSmokeAlarmAsync(
                iotDeviceId, 
                request.DeactivatedBy, 
                request.Reason);

            if (!success)
                return NotFound(new { message = "Aucune alarme active trouvée" });

            return Ok(new { success = true, message = "Alarme désactivée" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la désactivation de l'alarme pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère les alarmes fumée d'un device
    /// </summary>
    [HttpGet("{iotDeviceId}/smoke-alarms")]
    public async Task<ActionResult<IEnumerable<SmokeAlarmEventDto>>> GetSmokeAlarms(
        string iotDeviceId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var alarms = await _deviceStatusService.GetSmokeAlarmsAsync(iotDeviceId, startDate, endDate);
            return Ok(alarms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des alarmes pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère l'alarme active d'un device
    /// </summary>
    [HttpGet("{iotDeviceId}/smoke-alarms/active")]
    public async Task<ActionResult<SmokeAlarmEventDto>> GetActiveSmokeAlarm(string iotDeviceId)
    {
        try
        {
            var alarm = await _deviceStatusService.GetActiveSmokeAlarmAsync(iotDeviceId);
            if (alarm == null)
                return NotFound(new { message = "Aucune alarme active" });

            return Ok(alarm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'alarme active pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    /// <summary>
    /// Récupère toutes les alarmes actives de tous les devices
    /// </summary>
    [HttpGet("smoke-alarms/all-active")]
    public async Task<ActionResult<IEnumerable<SmokeAlarmEventDto>>> GetAllActiveAlarms()
    {
        try
        {
            var alarms = await _deviceStatusService.GetAllActiveAlarmsAsync();
            return Ok(alarms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de toutes les alarmes actives");
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }

    // ===== MAINTENANCE =====
    
    /// <summary>
    /// Nettoie les données anciennes (métriques, RFID, alarmes)
    /// </summary>
    /// <param name="retentionDays">Nombre de jours à conserver (défaut: 90)</param>
    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CleanupOldData([FromQuery] int retentionDays = 90)
    {
        try
        {
            var deletedCount = await _deviceStatusService.CleanupOldDataAsync(retentionDays);
            return Ok(new 
            { 
                success = true, 
                deletedCount, 
                message = $"{deletedCount} enregistrements supprimés" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du nettoyage des données");
            return StatusCode(500, new { message = "Erreur interne du serveur" });
        }
    }
}

#region Request DTOs

public class RecordMetricsRequest
{
    public DateTime? Timestamp { get; set; }
    public int? Rssi { get; set; }
    public int? HeapFree { get; set; }
    public int? NvsQueueSize { get; set; }
    public long? UptimeSeconds { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? FirmwareCommit { get; set; }
    public string? AdditionalData { get; set; }
}

public class RecordRfidEventRequest
{
    public DateTime? Timestamp { get; set; }
    public required string CardUid { get; set; }
    public bool AccessGranted { get; set; }
    public string? DenialReason { get; set; }
    public string? UserId { get; set; }
    public string? Action { get; set; }
}

public class RecordSmokeAlarmRequest
{
    public DateTime? StartTime { get; set; }
    public int? ZoneId { get; set; }
}

public class DeactivateSmokeAlarmRequest
{
    public string? DeactivatedBy { get; set; }
    public string? Reason { get; set; }
}

#endregion
