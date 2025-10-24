using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class DeviceStatusService : IDeviceStatusService
{
    private readonly IDeviceMetricsRepository _metricsRepo;
    private readonly IRfidEventRepository _rfidRepo;
    private readonly ISmokeAlarmEventRepository _alarmRepo;
    private readonly IDevicePeripheralRepository _peripheralRepo;
    private readonly IIotHubService _iotHubService;
    private readonly ISignalRService _signalRService;
    private readonly IMapper _mapper;
    private readonly ILogger<DeviceStatusService> _logger;

    public DeviceStatusService(
        IDeviceMetricsRepository metricsRepo,
        IRfidEventRepository rfidRepo,
        ISmokeAlarmEventRepository alarmRepo,
        IDevicePeripheralRepository peripheralRepo,
        IIotHubService iotHubService,
        ISignalRService signalRService,
        IMapper mapper,
        ILogger<DeviceStatusService> logger)
    {
        _metricsRepo = metricsRepo;
        _rfidRepo = rfidRepo;
        _alarmRepo = alarmRepo;
        _peripheralRepo = peripheralRepo;
        _iotHubService = iotHubService;
        _signalRService = signalRService;
        _mapper = mapper;
        _logger = logger;
    }

    // ===== MÉTRIQUES =====
    public async Task<DeviceMetricsDto> RecordMetricsAsync(DeviceMetrics metrics)
    {
        try
        {
            var saved = await _metricsRepo.AddAsync(metrics);
            _logger.LogInformation("Métriques enregistrées pour device {DeviceId}", metrics.IotDeviceId);
            return _mapper.Map<DeviceMetricsDto>(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement des métriques pour {DeviceId}", metrics.IotDeviceId);
            throw;
        }
    }

    public async Task<IEnumerable<DeviceMetricsDto>> GetMetricsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var metrics = await _metricsRepo.GetByDeviceIdAsync(iotDeviceId, startDate, endDate);
        return _mapper.Map<IEnumerable<DeviceMetricsDto>>(metrics);
    }

    public async Task<DeviceMetricsDto?> GetLatestMetricsAsync(string iotDeviceId)
    {
        var metrics = await _metricsRepo.GetLatestByDeviceIdAsync(iotDeviceId);
        return metrics == null ? null : _mapper.Map<DeviceMetricsDto>(metrics);
    }

    // ===== RFID =====
    public async Task<RfidEventDto> RecordRfidEventAsync(RfidEvent rfidEvent)
    {
        try
        {
            var saved = await _rfidRepo.AddAsync(rfidEvent);
            _logger.LogInformation(
                "Événement RFID enregistré : Device={DeviceId}, CardUID={CardUid}, Access={Access}",
                rfidEvent.IotDeviceId, rfidEvent.CardUid, rfidEvent.AccessGranted ? "GRANTED" : "DENIED");

            // ?? Notification SignalR pour accès refusés
            if (!rfidEvent.AccessGranted)
            {
                await _signalRService.SendBroadcastNotificationAsync(
                    "Accès RFID Refusé",
                    $"Tentative d'accès refusée avec carte {rfidEvent.CardUid} sur {rfidEvent.IotDeviceId}");
            }

            return _mapper.Map<RfidEventDto>(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement de l'événement RFID pour {DeviceId}", rfidEvent.IotDeviceId);
            throw;
        }
    }

    public async Task<IEnumerable<RfidEventDto>> GetRfidEventsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var events = await _rfidRepo.GetByDeviceIdAsync(iotDeviceId, startDate, endDate);
        return _mapper.Map<IEnumerable<RfidEventDto>>(events);
    }

    public async Task<IEnumerable<RfidEventDto>> GetRfidEventsByCardAsync(string cardUid, DateTime? startDate = null, DateTime? endDate = null)
    {
        var events = await _rfidRepo.GetByCardUidAsync(cardUid, startDate, endDate);
        return _mapper.Map<IEnumerable<RfidEventDto>>(events);
    }

    public async Task<IEnumerable<RfidEventDto>> GetDeniedAccessAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var events = await _rfidRepo.GetDeniedAccessAsync(startDate, endDate);
        return _mapper.Map<IEnumerable<RfidEventDto>>(events);
    }

    // ===== ALARMES FUMÉE =====
    public async Task<SmokeAlarmEventDto> RecordSmokeAlarmAsync(SmokeAlarmEvent alarmEvent)
    {
        try
        {
            var saved = await _alarmRepo.AddAsync(alarmEvent);
            _logger.LogWarning("?? Alarme fumée déclenchée sur device {DeviceId}", alarmEvent.IotDeviceId);

            // ?? Notification SignalR critique
            await _signalRService.SendBroadcastNotificationAsync(
                "?? ALARME FUMÉE",
                $"Alarme fumée détectée sur {alarmEvent.IotDeviceId} à {alarmEvent.StartTime:HH:mm:ss}");

            return _mapper.Map<SmokeAlarmEventDto>(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement de l'alarme fumée pour {DeviceId}", alarmEvent.IotDeviceId);
            throw;
        }
    }

    public async Task<bool> DeactivateSmokeAlarmAsync(string iotDeviceId, string? deactivatedBy = null, string? reason = null)
    {
        try
        {
            var activeAlarm = await _alarmRepo.GetActiveAlarmByDeviceIdAsync(iotDeviceId);
            if (activeAlarm == null) return false;

            var success = await _alarmRepo.DeactivateAlarmAsync(activeAlarm.Id, deactivatedBy, reason);
            
            if (success)
            {
                _logger.LogInformation("Alarme fumée désactivée sur device {DeviceId}", iotDeviceId);
                await _signalRService.SendBroadcastNotificationAsync(
                    "Alarme Désactivée",
                    $"Alarme fumée désactivée sur {iotDeviceId}");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la désactivation de l'alarme pour {DeviceId}", iotDeviceId);
            throw;
        }
    }

    public async Task<IEnumerable<SmokeAlarmEventDto>> GetSmokeAlarmsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var alarms = await _alarmRepo.GetByDeviceIdAsync(iotDeviceId, startDate, endDate);
        return _mapper.Map<IEnumerable<SmokeAlarmEventDto>>(alarms);
    }

    public async Task<SmokeAlarmEventDto?> GetActiveSmokeAlarmAsync(string iotDeviceId)
    {
        var alarm = await _alarmRepo.GetActiveAlarmByDeviceIdAsync(iotDeviceId);
        return alarm == null ? null : _mapper.Map<SmokeAlarmEventDto>(alarm);
    }

    public async Task<IEnumerable<SmokeAlarmEventDto>> GetAllActiveAlarmsAsync()
    {
        var alarms = await _alarmRepo.GetActiveAlarmsAsync();
        return _mapper.Map<IEnumerable<SmokeAlarmEventDto>>(alarms);
    }

    // ===== STATUT COMPLET =====
    public async Task<DeviceFullStatusDto> GetFullDeviceStatusAsync(string iotDeviceId)
    {
        try
        {
            // Récupérer le twin IoT Hub pour le statut de connexion
            var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);

            // Récupérer les données locales
            var latestMetrics = await _metricsRepo.GetLatestByDeviceIdAsync(iotDeviceId);
            var latestRfid = await _rfidRepo.GetLatestByDeviceIdAsync(iotDeviceId);
            var activeAlarm = await _alarmRepo.GetActiveAlarmByDeviceIdAsync(iotDeviceId);
            var peripherals = await _peripheralRepo.GetByIotDeviceIdAsync(iotDeviceId);

            var status = new DeviceFullStatusDto
            {
                IotDeviceId = iotDeviceId,
                IsOnline = twin?.ConnectionState == "Connected",
                LastActivity = twin?.LastActivityTime,
                LatestMetrics = latestMetrics == null ? null : _mapper.Map<DeviceMetricsDto>(latestMetrics),
                LatestRfidEvent = latestRfid == null ? null : _mapper.Map<RfidEventDto>(latestRfid),
                ActiveAlarm = activeAlarm == null ? null : _mapper.Map<SmokeAlarmEventDto>(activeAlarm),
                Peripherals = peripherals.Select(p => new DevicePeripheralStatusDto
                {
                    Id = p.Id.ToString(),
                    Name = p.Name,
                    Type = p.Type,
                    Gpio = p.Gpio,
                    IsConfigured = p.Status == "active",
                    State = p.LastKnownState ?? "unknown",
                    LastUpdate = p.LastUpdateReceived,
                    Metering = "none",  // TODO: enrichir quand Twin reporté
                    RatedWatt = 0,
                    EnergyWh = 0,
                    CurrentPowerW = 0
                }).ToList()
            };

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du statut complet pour {DeviceId}", iotDeviceId);
            throw;
        }
    }

    // ===== NETTOYAGE =====
    public async Task<int> CleanupOldDataAsync(int retentionDays = 90)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            
            var metricsDeleted = await _metricsRepo.DeleteOlderThanAsync(cutoffDate);
            var rfidDeleted = await _rfidRepo.DeleteOlderThanAsync(cutoffDate);
            var alarmsDeleted = await _alarmRepo.DeleteOlderThanAsync(cutoffDate);

            var totalDeleted = metricsDeleted + rfidDeleted + alarmsDeleted;
            
            _logger.LogInformation(
                "Nettoyage terminé : {Total} enregistrements supprimés (Métriques: {Metrics}, RFID: {Rfid}, Alarmes: {Alarms})",
                totalDeleted, metricsDeleted, rfidDeleted, alarmsDeleted);

            return totalDeleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du nettoyage des données anciennes");
            throw;
        }
    }
}
