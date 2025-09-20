using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Notification> AddNotificationAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    /// <inheritdoc/>
    /// 
    public async Task<Notification?> GetNotificationsByIdAsync(int id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Horodatage)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAsReadAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return true;
    }
}
