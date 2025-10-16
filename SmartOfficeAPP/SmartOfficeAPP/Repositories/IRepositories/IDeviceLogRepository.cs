using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface IDeviceLogRepository
{
    Task AddLogAsync(DeviceLog log);
    Task<IEnumerable<DeviceLog>> GetLogsByDeviceIdAsync(string deviceId, int limit = 100);
    Task<IEnumerable<DeviceLog>> GetLogsByLevelAsync(string level, int limit = 100);
    Task<IEnumerable<DeviceLog>> GetLogsByPeripheralAsync(string peripheral, int limit = 100);
    Task<IEnumerable<DeviceLog>> GetRecentLogsAsync(int limit = 100);
    Task<IEnumerable<DeviceLog>> GetLogsByDeviceIdAndDateRangeAsync(string deviceId, DateTime startDate, DateTime endDate);
}
