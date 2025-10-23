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
}
