using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

/// <summary>
/// Repository pour les événements d'alarme fumée
/// </summary>
public interface ISmokeAlarmEventRepository
{
    /// <summary>
    /// Ajoute un nouvel événement d'alarme
    /// </summary>
    Task<SmokeAlarmEvent> AddAsync(SmokeAlarmEvent alarmEvent);

    /// <summary>
    /// Met à jour un événement d'alarme existant
    /// </summary>
    Task<SmokeAlarmEvent?> UpdateAsync(SmokeAlarmEvent alarmEvent);

    /// <summary>
    /// Récupère les alarmes d'un device dans une plage de temps
    /// </summary>
    Task<IEnumerable<SmokeAlarmEvent>> GetByDeviceIdAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère l'alarme active d'un device (s'il y en a une)
    /// </summary>
    Task<SmokeAlarmEvent?> GetActiveAlarmByDeviceIdAsync(string iotDeviceId);

    /// <summary>
    /// Récupère toutes les alarmes actives
    /// </summary>
    Task<IEnumerable<SmokeAlarmEvent>> GetActiveAlarmsAsync();

    /// <summary>
    /// Désactive une alarme
    /// </summary>
    Task<bool> DeactivateAlarmAsync(int alarmId, string? deactivatedBy = null, string? reason = null);

    /// <summary>
    /// Supprime les événements anciens (nettoyage)
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
}
