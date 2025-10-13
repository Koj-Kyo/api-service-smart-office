namespace SmartOfficeAPP.Services.IServices;

/// <summary>
/// Service pour gérer les communications SignalR
/// </summary>
public interface ISignalRService
{
    /// <summary>
    /// Envoie une notification à un utilisateur spécifique
    /// </summary>
    Task SendNotificationToUserAsync(string userId, string title, string message);

    /// <summary>
    /// Envoie une notification à tous les utilisateurs d'une zone
    /// </summary>
    Task SendNotificationToZoneAsync(int zoneId, string title, string message);

    /// <summary>
    /// Envoie une mise à jour de statut de périphérique
    /// </summary>
    Task SendDeviceStatusUpdateAsync(int peripheriqueId, string status, object? data = null);

    /// <summary>
    /// Envoie une mise à jour de télémétrie en temps réel
    /// </summary>
    Task SendTelemetryUpdateAsync(int peripheriqueId, object telemetryData);

    /// <summary>
    /// Envoie une alerte d'incident
    /// </summary>
    Task SendIncidentAlertAsync(int incidentId, string title, string description, string priority);

    /// <summary>
    /// Envoie le résultat d'exécution d'une commande
    /// </summary>
    Task SendCommandExecutionResultAsync(int commandId, int peripheriqueId, string status, string result);

    /// <summary>
    /// Envoie une notification à tous les utilisateurs connectés
    /// </summary>
    Task SendBroadcastNotificationAsync(string title, string message);
}