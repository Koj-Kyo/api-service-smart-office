using System.Text.Json;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class DeviceSyncService : IDeviceSyncService
{
    private readonly IIotHubService _iotHubService;
    private readonly IPeripheriqueLogiqueRepository _peripheriqueRepo;
    private readonly IDevicePeripheralRepository _devicePeripheralRepo;
    private readonly ITelemetrieRepository _telemetrieRepo;
    private readonly IDeviceLogRepository _deviceLogRepo;
    private readonly ILogger<DeviceSyncService> _logger;

    public DeviceSyncService(
        IIotHubService iotHubService,
   IPeripheriqueLogiqueRepository peripheriqueRepo,
     IDevicePeripheralRepository devicePeripheralRepo,
        ITelemetrieRepository telemetrieRepo,
        IDeviceLogRepository deviceLogRepo,
        ILogger<DeviceSyncService> logger)
    {
        _iotHubService = iotHubService;
     _peripheriqueRepo = peripheriqueRepo;
        _devicePeripheralRepo = devicePeripheralRepo;
      _telemetrieRepo = telemetrieRepo;
        _deviceLogRepo = deviceLogRepo;
        _logger = logger;
    }

    public async Task SyncDeviceTwinToDbAsync(string deviceId)
    {
        try
        {
    var twin = await _iotHubService.GetDeviceTwinAsync(deviceId);
            if (twin == null)
            {
_logger.LogWarning("Device twin introuvable pour {DeviceId}", deviceId);
 return;
            }

        // Synchroniser les périphériques depuis reported properties
            await SyncPeripheralsFromTwinAsync(deviceId);

            _logger.LogInformation("Device twin synchronisé pour {DeviceId}", deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation du twin pour {DeviceId}", deviceId);
        }
    }

    public async Task SyncPeripheralsFromTwinAsync(string deviceId)
 {
        try
      {
            var twin = await _iotHubService.GetDeviceTwinAsync(deviceId);
        if (twin == null) return;

            var reportedJson = JsonDocument.Parse(twin.ReportedProperties);
   if (!reportedJson.RootElement.TryGetProperty("peripherals", out var peripheralsElement))
         {
 return;
 }

 foreach (var peripheralElement in peripheralsElement.EnumerateArray())
            {
      var name = peripheralElement.GetProperty("name").GetString();
          if (string.IsNullOrEmpty(name)) continue;

            // Chercher ou créer le périphérique dans DevicePeripheral
   var existing = await _devicePeripheralRepo.GetByDeviceAndNameAsync(deviceId, name);
  if (existing == null)
    {
      var newPeripheral = new DevicePeripheral
            {
     DeviceId = deviceId,
   Name = name,
     Type = peripheralElement.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : "unknown",
      Gpio = peripheralElement.TryGetProperty("gpio", out var gpioEl) ? gpioEl.GetInt32() : 0,
   IsOutput = peripheralElement.TryGetProperty("isOutput", out var isOutputEl) && isOutputEl.GetBoolean(),
        ActiveHigh = peripheralElement.TryGetProperty("activeHigh", out var activeHighEl) && activeHighEl.GetBoolean(),
       RatedWatt = peripheralElement.TryGetProperty("ratedWatt", out var wattEl) ? wattEl.GetDouble() : 0,
     State = peripheralElement.TryGetProperty("state", out var stateEl) ? stateEl.GetString() : "unknown",
              LastUpdated = DateTime.UtcNow
                 };
            await _devicePeripheralRepo.AddAsync(newPeripheral);
    }
    else
           {
            // Mettre à jour l'état
  if (peripheralElement.TryGetProperty("state", out var stateEl))
        {
  existing.State = stateEl.GetString();
      }
   existing.LastUpdated = DateTime.UtcNow;
        await _devicePeripheralRepo.UpdateAsync(existing);
}
     }

    _logger.LogInformation("Périphériques synchronisés pour {DeviceId}", deviceId);
    }
        catch (Exception ex)
 {
     _logger.LogError(ex, "Erreur lors de la synchronisation des périphériques pour {DeviceId}", deviceId);
        }
    }

    public async Task PersistTelemetryAsync(string deviceId, string telemetryJson)
  {
        try
        {
            var telemetryDoc = JsonDocument.Parse(telemetryJson);
 var root = telemetryDoc.RootElement;

     // Trouver le périphérique principal (peut être amélioré selon votre modèle)
            var peripherique = await _peripheriqueRepo.GetByIotDeviceIdAsync(deviceId);
            if (peripherique == null)
    {
        _logger.LogWarning("Périphérique introuvable pour deviceId {DeviceId}, télémétrie non persistée", deviceId);
     return;
   }

       var telemetry = new Telemetrie
            {
              Horodatage = DateTime.UtcNow,
             Valeur = telemetryJson,
              PeripheriqueLogiqueId = peripherique.Id
            };

            await _telemetrieRepo.AddTelemetrieAsync(telemetry);
     _logger.LogInformation("Télémétrie persistée pour {DeviceId}", deviceId);
        }
        catch (Exception ex)
   {
          _logger.LogError(ex, "Erreur lors de la persistance de la télémétrie pour {DeviceId}", deviceId);
        }
    }

    public async Task PersistDeviceLogAsync(string deviceId, string logJson)
    {
        try
        {
            var logDoc = JsonDocument.Parse(logJson);
            var root = logDoc.RootElement;

    var deviceLog = new DeviceLog
    {
     DeviceId = deviceId,
      Timestamp = root.TryGetProperty("timestamp", out var tsEl)
      ? DateTime.Parse(tsEl.GetString()!)
       : DateTime.UtcNow,
          Level = root.TryGetProperty("level", out var levelEl) ? levelEl.GetString() : "info",
    Source = root.TryGetProperty("source", out var srcEl) ? srcEl.GetString() : "unknown",
             Message = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "",
     Data = root.TryGetProperty("data", out var dataEl) ? dataEl.GetRawText() : null
          };

    await _deviceLogRepo.AddAsync(deviceLog);
   _logger.LogInformation("Log persisté pour {DeviceId}: {Message}", deviceId, deviceLog.Message);
        }
        catch (Exception ex)
        {
     _logger.LogError(ex, "Erreur lors de la persistance du log pour {DeviceId}", deviceId);
        }
    }

 public async Task SyncDeviceConnectionStatusAsync(string deviceId)
 {
        try
     {
            var connectionInfo = await _iotHubService.GetDevicesConnectionStatusAsync(new[] { deviceId });
            var info = connectionInfo.FirstOrDefault();
       if (info == null) return;

       // Mettre à jour le statut dans DevicePeripheral ou une table dédiée
  var peripherals = await _devicePeripheralRepo.GetByDeviceIdAsync(deviceId);
          foreach (var peripheral in peripherals)
            {
 peripheral.ConnectionState = info.ConnectionState;
            peripheral.LastActivityTime = info.LastActivityTime;
    await _devicePeripheralRepo.UpdateAsync(peripheral);
         }

          _logger.LogInformation("Statut de connexion synchronisé pour {DeviceId}: {State}", deviceId, info.ConnectionState);
   }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation du statut de connexion pour {DeviceId}", deviceId);
        }
    }

    public async Task FullSyncDeviceAsync(string deviceId)
  {
        _logger.LogInformation("Synchronisation complète démarrée pour {DeviceId}", deviceId);

 await SyncDeviceTwinToDbAsync(deviceId);
        await SyncPeripheralsFromTwinAsync(deviceId);
        await SyncDeviceConnectionStatusAsync(deviceId);

 _logger.LogInformation("Synchronisation complète terminée pour {DeviceId}", deviceId);
    }
}
