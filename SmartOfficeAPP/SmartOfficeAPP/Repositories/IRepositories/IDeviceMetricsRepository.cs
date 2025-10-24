using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

/// <summary>
/// Repository pour les métriques système des devices
/// </summary>
public interface IDeviceMetricsRepository
{
    /// <summary>
    /// Ajoute une nouvelle métrique
    /// </summary>
    Task<DeviceMetrics> AddAsync(DeviceMetrics metrics);

    /// <summary>
    /// Récupère les métriques d'un device dans une plage de temps
    /// </summary>
    Task<IEnumerable<DeviceMetrics>> GetByDeviceIdAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère la dernière métrique d'un device
    /// </summary>
    Task<DeviceMetrics?> GetLatestByDeviceIdAsync(string iotDeviceId);

    /// <summary>
    /// Supprime les métriques anciennes (nettoyage)
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
}
