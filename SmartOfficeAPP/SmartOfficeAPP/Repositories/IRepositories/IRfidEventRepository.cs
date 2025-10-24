using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

/// <summary>
/// Repository pour les événements RFID
/// </summary>
public interface IRfidEventRepository
{
    /// <summary>
    /// Ajoute un nouvel événement RFID
    /// </summary>
    Task<RfidEvent> AddAsync(RfidEvent rfidEvent);

    /// <summary>
    /// Récupère les événements RFID d'un device dans une plage de temps
    /// </summary>
    Task<IEnumerable<RfidEvent>> GetByDeviceIdAsync(string iotDeviceId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère les événements RFID par UID de carte
    /// </summary>
    Task<IEnumerable<RfidEvent>> GetByCardUidAsync(string cardUid, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Récupère le dernier scan RFID d'un device
    /// </summary>
    Task<RfidEvent?> GetLatestByDeviceIdAsync(string iotDeviceId);

    /// <summary>
    /// Récupère les accès refusés
    /// </summary>
    Task<IEnumerable<RfidEvent>> GetDeniedAccessAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Supprime les événements anciens (nettoyage)
    /// </summary>
    Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
}
