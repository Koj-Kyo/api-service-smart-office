using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Services.IServices;

public interface IDeviceLogService
{
    Task AddLogAsync(DeviceLogDto logDto);
    Task<IEnumerable<DeviceLogDto>> GetLogsByDeviceIdAsync(string deviceId, int limit = 100);
    Task<IEnumerable<DeviceLogDto>> GetLogsByLevelAsync(string level, int limit = 100);
    Task<IEnumerable<DeviceLogDto>> GetLogsByPeripheralAsync(string peripheral, int limit = 100);
    Task<IEnumerable<DeviceLogDto>> GetRecentLogsAsync(int limit = 100);
    Task<IEnumerable<DeviceLogDto>> GetLogsByDeviceIdAndDateRangeAsync(string deviceId, DateTime startDate, DateTime endDate);
}
