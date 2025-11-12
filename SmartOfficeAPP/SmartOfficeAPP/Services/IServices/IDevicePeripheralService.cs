using SmartOfficeAPP.DTOs;

namespace SmartOfficeAPP.Services.IServices;

public interface IDevicePeripheralService
{
    Task<IEnumerable<DevicePeripheralDto>> GetAllAsync();
    Task<DevicePeripheralDto?> GetByIdAsync(int id);
    Task<IEnumerable<DevicePeripheralDto>> GetByIotDeviceIdAsync(string iotDeviceId);
    Task<DevicePeripheralDto?> GetByNameAsync(string iotDeviceId, string name);
    Task<DevicePeripheralDto> CreateAsync(DevicePeripheralCreationDto dto);
    Task<bool> UpdateAsync(int id, DevicePeripheralUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> SyncWithDesiredPropertiesAsync(string iotDeviceId);
    
    /// <summary>
    /// Synchronise les périphériques physiques depuis IoT Hub (reported properties) vers la base de données
    /// </summary>
    Task<int> SyncPeripheralsFromIoTHubAsync(string iotDeviceId);
    
    /// <summary>
    /// Synchronise tous les périphériques de tous les devices IoT Hub
    /// </summary>
    Task<int> SyncAllPeripheralsFromIoTHubAsync();

    // =============================
    // ✅ NOUVEAUX: Pour app mobile
    // =============================

    /// <summary>
    /// Récupère tous les périphériques enrichis (DB + IoT Hub + Zone)
    /// </summary>
    Task<IEnumerable<DevicePeripheralEnrichedDto>> GetAllEnrichedAsync();

    /// <summary>
    /// Récupère un périphérique enrichi par ID
    /// </summary>
    Task<DevicePeripheralEnrichedDto?> GetEnrichedByIdAsync(int id);

    /// <summary>
    /// Récupère tous les périphériques enrichis d'un device IoT Hub
    /// </summary>
    Task<IEnumerable<DevicePeripheralEnrichedDto>> GetEnrichedByIotDeviceIdAsync(string iotDeviceId);

    /// <summary>
    /// Récupère les statistiques agrégées des périphériques
    /// </summary>
    Task<DevicePeripheralsStatsDto> GetStatsAsync();

    /// <summary>
    /// Récupère les périphériques enrichis par zone
    /// </summary>
    Task<IEnumerable<DevicePeripheralEnrichedDto>> GetEnrichedByZoneIdAsync(int zoneId);
}

