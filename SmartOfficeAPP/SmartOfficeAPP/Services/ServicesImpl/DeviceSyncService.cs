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
                var existing = await _devicePeripheralRepo.GetByNameAsync(deviceId, name);
                if (existing == null)
                {
                    var newPeripheral = new DevicePeripheral
                    {
                        IotDeviceId = deviceId,
                        Name = name,
                        Type = peripheralElement.TryGetProperty("type", out var typeEl) ? typeEl.GetString() ?? "unknown" : "unknown",
                        Gpio = peripheralElement.TryGetProperty("gpio", out var gpioEl) ? gpioEl.GetInt32() : 0,
                        LastKnownState = peripheralElement.TryGetProperty("state", out var stateEl) ? stateEl.GetString() : "unknown",
                        LastUpdateReceived = DateTime.UtcNow,
                        Status = "active"
                    };
                    await _devicePeripheralRepo.AddAsync(newPeripheral);
                }
                else
                {
                    // Mettre à jour l'état
                    if (peripheralElement.TryGetProperty("state", out var stateEl))
                    {
                        existing.LastKnownState = stateEl.GetString();
                    }
                    existing.LastUpdateReceived = DateTime.UtcNow;
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

            // Extraire une valeur numérique ou utiliser une valeur par défaut
            double valeur = 0;
            if (root.TryGetProperty("value", out var valueEl) && valueEl.ValueKind == JsonValueKind.Number)
            {
                valeur = valueEl.GetDouble();
            }
            else if (root.TryGetProperty("temperature", out var tempEl) && tempEl.ValueKind == JsonValueKind.Number)
            {
                valeur = tempEl.GetDouble();
            }

            var telemetry = new Telemetrie
            {
                Horodatage = DateTime.UtcNow,
                Valeur = valeur,
                TypeEvenement = "telemetry",
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
                Level = root.TryGetProperty("level", out var levelEl) ? levelEl.GetString() ?? "info" : "info",
                Source = root.TryGetProperty("source", out var srcEl) ? srcEl.GetString() ?? "unknown" : "unknown",
                Message = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() ?? "" : "",
                Context = root.TryGetProperty("data", out var dataEl) ? dataEl.GetRawText() : null,
                CreatedAt = DateTime.UtcNow
            };

            await _deviceLogRepo.AddLogAsync(deviceLog);
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
            var peripherals = await _devicePeripheralRepo.GetByIotDeviceIdAsync(deviceId);
            foreach (var peripheral in peripherals)
            {
                // Note: Ces propriétés n'existent pas dans DevicePeripheral, on peut les ajouter au modèle si nécessaire
                // ou créer une table séparée pour le statut de connexion
                peripheral.Status = info.IsConnected ? "connected" : "disconnected";
                peripheral.LastUpdateReceived = info.LastActivityTime ?? DateTime.UtcNow;
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
