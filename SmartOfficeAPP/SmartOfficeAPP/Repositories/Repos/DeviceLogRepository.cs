using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class DeviceLogRepository : IDeviceLogRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddLogAsync(DeviceLog log)
    {
        await _context.DeviceLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<DeviceLog>> GetLogsByDeviceIdAsync(string deviceId, int limit = 100)
    {
        return await _context.DeviceLogs
            .Where(l => l.DeviceId == deviceId)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> GetLogsByLevelAsync(string level, int limit = 100)
    {
        return await _context.DeviceLogs
            .Where(l => l.Level == level)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> GetLogsByPeripheralAsync(string peripheral, int limit = 100)
    {
        return await _context.DeviceLogs
            .Where(l => l.Peripheral == peripheral)
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> GetRecentLogsAsync(int limit = 100)
    {
        return await _context.DeviceLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<DeviceLog>> GetLogsByDeviceIdAndDateRangeAsync(string deviceId, DateTime startDate, DateTime endDate)
    {
        return await _context.DeviceLogs
            .Where(l => l.DeviceId == deviceId && l.Timestamp >= startDate && l.Timestamp <= endDate)
            .OrderByDescending(l => l.Timestamp)
            .ToListAsync();
    }
}
