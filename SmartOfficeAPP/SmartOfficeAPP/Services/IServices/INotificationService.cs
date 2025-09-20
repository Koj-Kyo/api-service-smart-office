using SmartOfficeAPP.DTOs;

namespace SmartOfficeAPP.Services.IServices
{
    public interface INotificationService
    {
        /// <summary>
        /// Asynchronously retrieves all notifications for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A collection of notification DTOs.</returns>
        Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(string userId);

        //get notification by id
        Task<NotificationDto> GetNotificationByIdAsync(int id);

        /// <summary>
        /// Asynchronously marks a notification as read by its ID.
        /// </summary>
        /// <param name="notificationId">The ID of the notification.</param>
        /// <returns>A boolean indicating success.</returns>
        Task<bool> MarkNotificationAsReadAsync(int notificationId);

        /// <summary>
        /// Asynchronously sends a new notification to a user.
        /// </summary>
        /// <param name="userId">The ID of the recipient user.</param>
        /// <param name="title">The title of the notification.</param>
        /// <param name="message">The content of the notification.</param>
        /// <returns>A boolean indicating success.</returns>
        Task<bool> SendNotificationAsync(string userId, string title, string message);
    }

}
