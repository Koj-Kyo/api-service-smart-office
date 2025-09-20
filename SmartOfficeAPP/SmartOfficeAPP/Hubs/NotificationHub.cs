using Microsoft.AspNetCore.SignalR;

namespace SmartOfficeAPP.Hubs;

public class NotificationHub : Hub
{
    // Envoie une notification à un utilisateur spécifique.
    public async Task SendNotificationToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }
}
