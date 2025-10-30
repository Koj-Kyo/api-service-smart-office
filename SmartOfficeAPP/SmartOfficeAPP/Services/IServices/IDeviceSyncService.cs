namespace SmartOfficeAPP.Services.IServices;

/// <summary>
/// Service de synchronisation cloud ? base de données
/// </summary>
public interface IDeviceSyncService
{
 /// <summary>
    /// Synchronise le device twin (reported properties) vers la base de données
    /// </summary>
    Task SyncDeviceTwinToDbAsync(string deviceId);

    /// <summary>
    /// Synchronise les périphériques du twin vers PeripheriqueLogique
    /// </summary>
    Task SyncPeripheralsFromTwinAsync(string deviceId);

 /// <summary>
    /// Persiste une télémétrie reçue du device
    /// </summary>
    Task PersistTelemetryAsync(string deviceId, string telemetryJson);

    /// <summary>
    /// Persiste un log reçu du device
    /// </summary>
  Task PersistDeviceLogAsync(string deviceId, string logJson);

    /// <summary>
    /// Synchronise l'état de connexion d'un device
    /// </summary>
    Task SyncDeviceConnectionStatusAsync(string deviceId);

    /// <summary>
    /// Synchronisation complète d'un device (twin + peripherals + status)
    /// </summary>
    Task FullSyncDeviceAsync(string deviceId);
}
