using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class RfidEventRepository : IRfidEventRepository
{
    private readonly ApplicationDbContext _context;

    public RfidEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RfidEvent> AddAsync(RfidEvent rfidEvent)
    {
        // Assurer UTC
        if (rfidEvent.Timestamp.Kind == DateTimeKind.Unspecified)
        {
            rfidEvent.Timestamp = DateTime.SpecifyKind(rfidEvent.Timestamp, DateTimeKind.Utc);
        }
        else if (rfidEvent.Timestamp.Kind == DateTimeKind.Local)
        {
            rfidEvent.Timestamp = rfidEvent.Timestamp.ToUniversalTime();
        }

        _context.RfidEvents.Add(rfidEvent);
        await _context.SaveChangesAsync();
        return rfidEvent;
    }

    public async Task<IEnumerable<RfidEvent>> GetByDeviceIdAsync(
        string iotDeviceId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.RfidEvents
            .Where(r => r.IotDeviceId == iotDeviceId);

        if (startDate.HasValue)
        {
            var start = startDate.Value.ToUniversalTime();
            query = query.Where(r => r.Timestamp >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.ToUniversalTime();
            query = query.Where(r => r.Timestamp <= end);
        }

        return await query
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<RfidEvent>> GetByCardUidAsync(
        string cardUid, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.RfidEvents
            .Where(r => r.CardUid == cardUid);

        if (startDate.HasValue)
        {
            var start = startDate.Value.ToUniversalTime();
            query = query.Where(r => r.Timestamp >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.ToUniversalTime();
            query = query.Where(r => r.Timestamp <= end);
        }

        return await query
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }

    public async Task<RfidEvent?> GetLatestByDeviceIdAsync(string iotDeviceId)
    {
        return await _context.RfidEvents
            .Where(r => r.IotDeviceId == iotDeviceId)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RfidEvent>> GetDeniedAccessAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.RfidEvents
            .Where(r => !r.AccessGranted);

        if (startDate.HasValue)
        {
            var start = startDate.Value.ToUniversalTime();
            query = query.Where(r => r.Timestamp >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.ToUniversalTime();
            query = query.Where(r => r.Timestamp <= end);
        }

        return await query
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
    {
        var cutoff = cutoffDate.ToUniversalTime();
        var toDelete = await _context.RfidEvents
            .Where(r => r.Timestamp < cutoff)
            .ToListAsync();

        _context.RfidEvents.RemoveRange(toDelete);
        return await _context.SaveChangesAsync();
    }
}
