using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class SmokeAlarmEventRepository : ISmokeAlarmEventRepository
{
    private readonly ApplicationDbContext _context;

    public SmokeAlarmEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SmokeAlarmEvent> AddAsync(SmokeAlarmEvent alarmEvent)
    {
        // Assurer UTC
        if (alarmEvent.StartTime.Kind == DateTimeKind.Unspecified)
        {
            alarmEvent.StartTime = DateTime.SpecifyKind(alarmEvent.StartTime, DateTimeKind.Utc);
        }
        else if (alarmEvent.StartTime.Kind == DateTimeKind.Local)
        {
            alarmEvent.StartTime = alarmEvent.StartTime.ToUniversalTime();
        }

        if (alarmEvent.EndTime.HasValue)
        {
            if (alarmEvent.EndTime.Value.Kind == DateTimeKind.Unspecified)
            {
                alarmEvent.EndTime = DateTime.SpecifyKind(alarmEvent.EndTime.Value, DateTimeKind.Utc);
            }
            else if (alarmEvent.EndTime.Value.Kind == DateTimeKind.Local)
            {
                alarmEvent.EndTime = alarmEvent.EndTime.Value.ToUniversalTime();
            }
        }

        _context.SmokeAlarmEvents.Add(alarmEvent);
        await _context.SaveChangesAsync();
        return alarmEvent;
    }

    public async Task<SmokeAlarmEvent?> UpdateAsync(SmokeAlarmEvent alarmEvent)
    {
        var existing = await _context.SmokeAlarmEvents.FindAsync(alarmEvent.Id);
        if (existing == null) return null;

        existing.EndTime = alarmEvent.EndTime;
        existing.DurationSeconds = alarmEvent.DurationSeconds;
        existing.IsActive = alarmEvent.IsActive;
        existing.DeactivationReason = alarmEvent.DeactivationReason;
        existing.DeactivatedBy = alarmEvent.DeactivatedBy;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<IEnumerable<SmokeAlarmEvent>> GetByDeviceIdAsync(
        string iotDeviceId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.SmokeAlarmEvents
            .Where(s => s.IotDeviceId == iotDeviceId);

        if (startDate.HasValue)
        {
            var start = startDate.Value.ToUniversalTime();
            query = query.Where(s => s.StartTime >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.ToUniversalTime();
            query = query.Where(s => s.StartTime <= end);
        }

        return await query
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<SmokeAlarmEvent?> GetActiveAlarmByDeviceIdAsync(string iotDeviceId)
    {
        return await _context.SmokeAlarmEvents
            .Where(s => s.IotDeviceId == iotDeviceId && s.IsActive)
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SmokeAlarmEvent>> GetActiveAlarmsAsync()
    {
        return await _context.SmokeAlarmEvents
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<bool> DeactivateAlarmAsync(int alarmId, string? deactivatedBy = null, string? reason = null)
    {
        var alarm = await _context.SmokeAlarmEvents.FindAsync(alarmId);
        if (alarm == null || !alarm.IsActive) return false;

        alarm.IsActive = false;
        alarm.EndTime = DateTime.UtcNow;
        alarm.DurationSeconds = (int)(alarm.EndTime.Value - alarm.StartTime).TotalSeconds;
        alarm.DeactivatedBy = deactivatedBy;
        alarm.DeactivationReason = reason;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
    {
        var cutoff = cutoffDate.ToUniversalTime();
        var toDelete = await _context.SmokeAlarmEvents
            .Where(s => s.StartTime < cutoff && !s.IsActive)
            .ToListAsync();

        _context.SmokeAlarmEvents.RemoveRange(toDelete);
        return await _context.SaveChangesAsync();
    }
}
