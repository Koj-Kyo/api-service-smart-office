using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;
    private readonly ISignalRService _signalRService;

    public NotificationService(
        INotificationRepository notificationRepository, 
        IMapper mapper, 
        ISignalRService signalRService)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
        _signalRService = signalRService;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<NotificationDto>> GetNotificationsForUserAsync(string userId)
    {
        var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

    /// <inheritdoc/>
    public async Task<NotificationDto> GetNotificationByIdAsync(int notificationId)
    {
        var notification = await _notificationRepository.GetNotificationsByIdAsync(notificationId);
        return _mapper.Map<NotificationDto>(notification);
    }

    /// <inheritdoc/>
    public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
    {
        var notification = await _notificationRepository.GetNotificationsByIdAsync(notificationId);
        if (notification == null)
        {
            return false;
        }

        notification.EstLue = true;
        return await _notificationRepository.MarkAsReadAsync(notification);
    }

    /// <inheritdoc/>
    public async Task<bool> SendNotificationAsync(string userId, string title, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Titre = title,
            Message = message,
            Horodatage = DateTime.UtcNow,
            EstLue = false
        };

        var addedNotification = await _notificationRepository.AddNotificationAsync(notification);
        if (addedNotification == null)
        {
            return false;
        }

        // Envoyer la notification en temps réel via SignalR
        await _signalRService.SendNotificationToUserAsync(userId, title, message);

        return true;
    }
}
