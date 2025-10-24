using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Services.IServices;

/// <summary>
/// Service unifié pour gérer toutes les fonctionnalités firmware ESP32
/// </summary>
public interface IDeviceStatusService
{
    // ===== MÉTRIQUES =====
    /// <summary>
    /// Enregistre des métriques système d'un device
    /// </summary>
    Task<DeviceMetricsDto> RecordMetricsAsync(DeviceMetrics metrics);

    /// <summary>
    /// Récupère les métriques d'un device
    /// </summary>
    Task<IEnumerable<DeviceMetricsDto>> GetMetricsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère la dernière métrique d'un device
    /// </summary>
    Task<DeviceMetricsDto?> GetLatestMetricsAsync(string iotDeviceId);

    // ===== RFID =====
    /// <summary>
    /// Enregistre un événement RFID
    /// </summary>
    Task<RfidEventDto> RecordRfidEventAsync(RfidEvent rfidEvent);

    /// <summary>
    /// Récupère les événements RFID d'un device
    /// </summary>
    Task<IEnumerable<RfidEventDto>> GetRfidEventsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère les événements RFID par carte
    /// </summary>
    Task<IEnumerable<RfidEventDto>> GetRfidEventsByCardAsync(string cardUid, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère les accès refusés
    /// </summary>
    Task<IEnumerable<RfidEventDto>> GetDeniedAccessAsync(DateTime? startDate = null, DateTime? endDate = null);

    // ===== ALARMES FUMÉE =====
    /// <summary>
    /// Enregistre un déclenchement d'alarme fumée
    /// </summary>
    Task<SmokeAlarmEventDto> RecordSmokeAlarmAsync(SmokeAlarmEvent alarmEvent);

    /// <summary>
    /// Désactive une alarme fumée
    /// </summary>
    Task<bool> DeactivateSmokeAlarmAsync(string iotDeviceId, string? deactivatedBy = null, string? reason = null);

    /// <summary>
    /// Récupère les alarmes fumée d'un device
    /// </summary>
    Task<IEnumerable<SmokeAlarmEventDto>> GetSmokeAlarmsAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère l'alarme active d'un device
    /// </summary>
    Task<SmokeAlarmEventDto?> GetActiveSmokeAlarmAsync(string iotDeviceId);

    /// <summary>
    /// Récupère toutes les alarmes actives
    /// </summary>
    Task<IEnumerable<SmokeAlarmEventDto>> GetAllActiveAlarmsAsync();

    // ===== STATUT COMPLET =====
    /// <summary>
    /// Récupère le statut complet d'un device (métriques + RFID + alarme + périphériques)
    /// </summary>
    Task<DeviceFullStatusDto> GetFullDeviceStatusAsync(string iotDeviceId);

    // ===== NETTOYAGE =====
    /// <summary>
    /// Nettoie les données anciennes (maintenance)
    /// </summary>
    Task<int> CleanupOldDataAsync(int retentionDays = 90);
}
