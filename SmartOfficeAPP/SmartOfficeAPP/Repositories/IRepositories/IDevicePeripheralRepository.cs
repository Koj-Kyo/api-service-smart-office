using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface IDevicePeripheralRepository
{
    Task<IEnumerable<DevicePeripheral>> GetAllAsync();
    Task<DevicePeripheral?> GetByIdAsync(int id);
    Task<IEnumerable<DevicePeripheral>> GetByIotDeviceIdAsync(string iotDeviceId);
    Task<DevicePeripheral?> GetByNameAsync(string iotDeviceId, string name);
    Task<DevicePeripheral> AddAsync(DevicePeripheral peripheral);
    Task<bool> UpdateAsync(DevicePeripheral peripheral);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(string iotDeviceId, string name);
}
