using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class DeviceMetricsRepository : IDeviceMetricsRepository
{
    private readonly ApplicationDbContext _context;

    public DeviceMetricsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceMetrics> AddAsync(DeviceMetrics metrics)
    {
        // Assurer UTC
        if (metrics.Timestamp.Kind == DateTimeKind.Unspecified)
        {
            metrics.Timestamp = DateTime.SpecifyKind(metrics.Timestamp, DateTimeKind.Utc);
        }
        else if (metrics.Timestamp.Kind == DateTimeKind.Local)
        {
            metrics.Timestamp = metrics.Timestamp.ToUniversalTime();
        }

        _context.DeviceMetrics.Add(metrics);
        await _context.SaveChangesAsync();
        return metrics;
    }

    public async Task<IEnumerable<DeviceMetrics>> GetByDeviceIdAsync(
        string iotDeviceId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.DeviceMetrics
            .Where(m => m.IotDeviceId == iotDeviceId);

        if (startDate.HasValue)
        {
            var start = startDate.Value.ToUniversalTime();
            query = query.Where(m => m.Timestamp >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.ToUniversalTime();
            query = query.Where(m => m.Timestamp <= end);
        }

        return await query
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<DeviceMetrics?> GetLatestByDeviceIdAsync(string iotDeviceId)
    {
        return await _context.DeviceMetrics
            .Where(m => m.IotDeviceId == iotDeviceId)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
    {
        var cutoff = cutoffDate.ToUniversalTime();
        var toDelete = await _context.DeviceMetrics
            .Where(m => m.Timestamp < cutoff)
            .ToListAsync();

        _context.DeviceMetrics.RemoveRange(toDelete);
        return await _context.SaveChangesAsync();
    }
}
