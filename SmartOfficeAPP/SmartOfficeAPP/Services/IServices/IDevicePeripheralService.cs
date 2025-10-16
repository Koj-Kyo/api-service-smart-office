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
}
