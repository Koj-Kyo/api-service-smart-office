using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface INotificationRepository
{
    /// <summary>
    /// Asynchronously adds a new notification to the database.
    /// </summary>
    /// <param name="notification">The notification to add.</param>
    /// <returns>The added notification.</returns>
    Task<Notification> AddNotificationAsync(Notification notification);

    /// <summary>
    /// Asynchronously retrieves all notifications for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of notifications for the specified user.</returns>
    /// 
    //Get notifications by id
    Task<Notification> GetNotificationsByIdAsync(int id);
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId);

    /// <summary>
    /// Asynchronously marks a notification as read.
    /// </summary>
    /// <param name="notification">The notification to mark as read.</param>
    /// <returns>A boolean indicating success.</returns>
    Task<bool> MarkAsReadAsync(Notification notification);
}
