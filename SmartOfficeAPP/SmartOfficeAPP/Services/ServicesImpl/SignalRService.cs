using Microsoft.AspNetCore.SignalR;
using SmartOfficeAPP.Hubs;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

/// <summary>
/// Service pour gérer les communications SignalR
/// </summary>
public class SignalRService : ISignalRService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<SignalRService> _logger;

    public SignalRService(IHubContext<NotificationHub> hubContext, ILogger<SignalRService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SendNotificationToUserAsync(string userId, string title, string message)
    {
        try
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new 
            { 
                title, 
                message, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogInformation("Notification envoyée à l'utilisateur {UserId}: {Title}", userId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de notification à l'utilisateur {UserId}", userId);
        }
    }

    /// <inheritdoc/>
    public async Task SendNotificationToZoneAsync(int zoneId, string title, string message)
    {
        try
        {
            await _hubContext.Clients.Group($"Zone_{zoneId}").SendAsync("ReceiveZoneNotification", new 
            { 
                zoneId, 
                title, 
                message, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogInformation("Notification envoyée à la zone {ZoneId}: {Title}", zoneId, title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de notification à la zone {ZoneId}", zoneId);
        }
    }

    /// <inheritdoc/>
    public async Task SendDeviceStatusUpdateAsync(int peripheriqueId, string status, object? data = null)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("DeviceStatusUpdate", new 
            { 
                peripheriqueId, 
                status, 
                data, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogInformation("Mise à jour de statut envoyée pour le périphérique {PeripheriqueId}: {Status}", peripheriqueId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de mise à jour de statut pour le périphérique {PeripheriqueId}", peripheriqueId);
        }
    }

    /// <inheritdoc/>
    public async Task SendTelemetryUpdateAsync(int peripheriqueId, object telemetryData)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("TelemetryUpdate", new 
            { 
                peripheriqueId, 
                data = telemetryData, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogDebug("Mise à jour télémétrie envoyée pour le périphérique {PeripheriqueId}", peripheriqueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de mise à jour télémétrie pour le périphérique {PeripheriqueId}", peripheriqueId);
        }
    }

    /// <inheritdoc/>
    public async Task SendIncidentAlertAsync(int incidentId, string title, string description, string priority)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("IncidentAlert", new 
            { 
                incidentId, 
                title, 
                description, 
                priority, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogWarning("Alerte d'incident envoyée - ID: {IncidentId}, Titre: {Title}, Priorité: {Priority}", incidentId, title, priority);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi d'alerte d'incident {IncidentId}", incidentId);
        }
    }

    /// <inheritdoc/>
    public async Task SendCommandExecutionResultAsync(int commandId, int peripheriqueId, string status, string result)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("CommandExecutionResult", new 
            { 
                commandId, 
                peripheriqueId, 
                status, 
                result, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogInformation("Résultat de commande envoyé - CommandId: {CommandId}, Status: {Status}", commandId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi du résultat de commande {CommandId}", commandId);
        }
    }

    /// <inheritdoc/>
    public async Task SendBroadcastNotificationAsync(string title, string message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveBroadcast", new 
            { 
                title, 
                message, 
                timestamp = DateTime.UtcNow 
            });
            
            _logger.LogInformation("Notification broadcast envoyée: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi de notification broadcast");
        }
    }
}