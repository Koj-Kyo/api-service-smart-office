using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartOfficeAPP.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    /// <summary>
    /// Connexion d'un utilisateur au hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Ajouter l'utilisateur à un groupe personnel
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // Log de connexion (optionnel)
            Console.WriteLine($"Utilisateur {userId} connecté avec ConnectionId: {Context.ConnectionId}");
        }
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Déconnexion d'un utilisateur du hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Retirer l'utilisateur du groupe personnel
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            // Log de déconnexion (optionnel)
            Console.WriteLine($"Utilisateur {userId} déconnecté. ConnectionId: {Context.ConnectionId}");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Envoie une notification à un utilisateur spécifique
    /// </summary>
    /// <param name="userId">ID de l'utilisateur cible</param>
    /// <param name="title">Titre de la notification</param>
    /// <param name="message">Message de la notification</param>
    public async Task SendNotificationToUser(string userId, string title, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", new { title, message, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Envoie une notification à tous les utilisateurs d'une zone
    /// </summary>
    /// <param name="zoneId">ID de la zone</param>
    /// <param name="title">Titre de la notification</param>
    /// <param name="message">Message de la notification</param>
    public async Task SendNotificationToZone(int zoneId, string title, string message)
    {
        await Clients.Group($"Zone_{zoneId}").SendAsync("ReceiveZoneNotification", new { zoneId, title, message, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Rejoindre un groupe de zone pour recevoir les notifications de cette zone
    /// </summary>
    /// <param name="zoneId">ID de la zone à rejoindre</param>
    public async Task JoinZoneGroup(int zoneId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Zone_{zoneId}");
        await Clients.Caller.SendAsync("JoinedZoneGroup", zoneId);
    }

    /// <summary>
    /// Quitter un groupe de zone
    /// </summary>
    /// <param name="zoneId">ID de la zone à quitter</param>
    public async Task LeaveZoneGroup(int zoneId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Zone_{zoneId}");
        await Clients.Caller.SendAsync("LeftZoneGroup", zoneId);
    }

    /// <summary>
    /// Envoie une mise à jour de statut de périphérique
    /// </summary>
    /// <param name="peripheriqueId">ID du périphérique</param>
    /// <param name="status">Nouveau statut</param>
    /// <param name="data">Données additionnelles</param>
    public async Task SendDeviceStatusUpdate(int peripheriqueId, string status, object? data = null)
    {
        await Clients.All.SendAsync("DeviceStatusUpdate", new { peripheriqueId, status, data, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Envoie une mise à jour de télémétrie en temps réel
    /// </summary>
    /// <param name="peripheriqueId">ID du périphérique</param>
    /// <param name="telemetryData">Données de télémétrie</param>
    public async Task SendTelemetryUpdate(int peripheriqueId, object telemetryData)
    {
        await Clients.All.SendAsync("TelemetryUpdate", new { peripheriqueId, data = telemetryData, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Envoie une notification d'incident
    /// </summary>
    /// <param name="incidentId">ID de l'incident</param>
    /// <param name="title">Titre de l'incident</param>
    /// <param name="description">Description de l'incident</param>
    /// <param name="priority">Priorité de l'incident</param>
    public async Task SendIncidentAlert(int incidentId, string title, string description, string priority)
    {
        await Clients.All.SendAsync("IncidentAlert", new { incidentId, title, description, priority, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Envoie une notification de commande exécutée
    /// </summary>
    /// <param name="commandId">ID de la commande</param>
    /// <param name="peripheriqueId">ID du périphérique</param>
    /// <param name="status">Statut d'exécution</param>
    /// <param name="result">Résultat de la commande</param>
    public async Task SendCommandExecutionResult(int commandId, int peripheriqueId, string status, string result)
    {
        await Clients.All.SendAsync("CommandExecutionResult", new { commandId, peripheriqueId, status, result, timestamp = DateTime.UtcNow });
    }
}
