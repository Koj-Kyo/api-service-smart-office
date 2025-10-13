using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.Background;

public class TelemetrieIngestionWorker : BackgroundService
{
    private readonly ILogger<TelemetrieIngestionWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventProcessorClient? _processor;
    private readonly string _consumerGroup;

    public TelemetrieIngestionWorker(
        ILogger<TelemetrieIngestionWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _consumerGroup = "$Default";

        var ehConn = configuration["IotHubEventHub:ConnectionString"];
        var ehName = configuration["IotHubEventHub:Name"];
        var blobConn = configuration["IotHubEventHub:BlobConnectionString"];
        var blobContainer = configuration["IotHubEventHub:BlobContainer"];

        if (string.IsNullOrWhiteSpace(ehConn) || string.IsNullOrWhiteSpace(ehName) ||
            string.IsNullOrWhiteSpace(blobConn) || string.IsNullOrWhiteSpace(blobContainer))
        {
            _logger.LogWarning("Ingestion IoT Hub désactivée: configuration IotHubEventHub incomplète.");
            return;
        }

        try
        {
            var storageClient = new BlobContainerClient(blobConn, blobContainer);
            storageClient.CreateIfNotExists();
            
            _processor = new EventProcessorClient(storageClient, _consumerGroup, ehConn, ehName);
            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;
            
            _logger.LogInformation("EventProcessorClient initialisé pour IoT Hub: {EventHubName}", ehName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation de l'EventProcessorClient");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_processor is null)
        {
            _logger.LogWarning("EventProcessorClient non initialisé (config manquante).");
            return;
        }

        _logger.LogInformation("Démarrage de l'ingestion IoT Hub...");
        
        try
        {
            await _processor.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken); // Check every 5 seconds
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Arrêt de l'ingestion IoT Hub demandé.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur durant l'exécution de l'ingestion IoT Hub");
        }
        finally
        {
            _logger.LogInformation("Arrêt de l'ingestion IoT Hub...");
            try
            {
                await _processor.StopProcessingAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'arrêt de l'EventProcessorClient");
            }
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, 
            "Erreur EventHub - Partition: {PartitionId}, Operation: {Operation}", 
            args.PartitionId, args.Operation);
        return Task.CompletedTask;
    }

    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var telemetrieRepository = scope.ServiceProvider.GetRequiredService<ITelemetrieRepository>();
            var periphRepo = scope.ServiceProvider.GetRequiredService<IPeripheriqueLogiqueRepository>();
            var signalRService = scope.ServiceProvider.GetService<ISignalRService>();

            var body = Encoding.UTF8.GetString(args.Data.EventBody.ToArray());
            var enqueued = args.Data.EnqueuedTime.UtcDateTime;
            var deviceId = TryGetDeviceId(args.Data);

            _logger.LogDebug("Traitement événement IoT - Device: {DeviceId}, Size: {Size}bytes", 
                deviceId ?? "Unknown", body.Length);

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                _logger.LogDebug("Événement sans deviceId, ignoré.");
                await args.UpdateCheckpointAsync();
                return;
            }

            // Valider le JSON
            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("JSON invalide reçu du device {DeviceId}: {Error}", deviceId, ex.Message);
                await args.UpdateCheckpointAsync();
                return;
            }

            using (doc)
            {
                var root = doc.RootElement;

                // Schéma v2 du firmware: contient "peripherals":[{ name,type,gpio,state,p_mw,e_mwh }, ...]
                if (root.TryGetProperty("peripherals", out var peripherals) && peripherals.ValueKind == JsonValueKind.Array)
                {
                    await ProcessPeripheralsDataAsync(peripherals, deviceId, enqueued, telemetrieRepository, periphRepo, signalRService);
                }
                // Schéma v1 ou données directes
                else
                {
                    await ProcessDirectDataAsync(root, deviceId, enqueued, telemetrieRepository, periphRepo, signalRService);
                }
            }

            await args.UpdateCheckpointAsync();
            
            stopwatch.Stop();
            _logger.LogDebug("Événement traité en {ElapsedMs}ms pour {DeviceId}", 
                stopwatch.ElapsedMilliseconds, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement d'un événement IoT Hub - Partition: {PartitionId}", 
                args.Partition.PartitionId);
            
            // En cas d'erreur, on fait quand même le checkpoint pour éviter de bloquer
            try
            {
                await args.UpdateCheckpointAsync();
            }
            catch (Exception checkpointEx)
            {
                _logger.LogError(checkpointEx, "Erreur lors du checkpoint après échec de traitement");
            }
        }
    }

    private async Task ProcessPeripheralsDataAsync(
        JsonElement peripherals, 
        string deviceId, 
        DateTime timestamp,
        ITelemetrieRepository telemetrieRepository,
        IPeripheriqueLogiqueRepository periphRepo,
        ISignalRService? signalRService)
    {
        var telemetriesAdded = 0;

        foreach (var p in peripherals.EnumerateArray())
        {
            var name = p.TryGetProperty("name", out var n) ? n.GetString() : null;
            if (string.IsNullOrWhiteSpace(name)) continue;

            var periph = await periphRepo.GetByIotDeviceIdAndNameAsync(deviceId, name);
            if (periph is null)
            {
                _logger.LogDebug("Périphérique non trouvé: DeviceId={DeviceId}, Name={Name}", deviceId, name);
                continue;
            }

            // Traiter les différents types de données
            var telemetriesForDevice = new List<Telemetrie>();

            // Puissance instantanée (power_mw)
            if (p.TryGetProperty("p_mw", out var pMwEl) && TryGetNumericValue(pMwEl, out var pMw))
            {
                telemetriesForDevice.Add(new Telemetrie
                {
                    Horodatage = timestamp,
                    TypeEvenement = "power_mw",
                    Valeur = pMw,
                    PeripheriqueLogiqueId = periph.Id
                });
            }

            // Énergie cumulée (energy_mwh)
            if (p.TryGetProperty("e_mwh", out var eMwhEl) && TryGetNumericValue(eMwhEl, out var eMwh))
            {
                telemetriesForDevice.Add(new Telemetrie
                {
                    Horodatage = timestamp,
                    TypeEvenement = "energy_mwh",
                    Valeur = eMwh,
                    PeripheriqueLogiqueId = periph.Id
                });
            }

            // État (state)
            if (p.TryGetProperty("state", out var stEl))
            {
                var state = stEl.ValueKind switch
                {
                    JsonValueKind.String => stEl.GetString() == "1" ? 1 : 0,
                    JsonValueKind.Number => stEl.GetInt32(),
                    JsonValueKind.True => 1,
                    JsonValueKind.False => 0,
                    _ => 0
                };
                
                telemetriesForDevice.Add(new Telemetrie
                {
                    Horodatage = timestamp,
                    TypeEvenement = "state",
                    Valeur = state,
                    PeripheriqueLogiqueId = periph.Id
                });
            }

            // Autres propriétés (température, humidité, etc.)
            await ProcessAdditionalPropertiesAsync(p, periph.Id, timestamp, telemetriesForDevice);

            // Sauvegarder toutes les télémétries pour ce périphérique
            foreach (var telemetrie in telemetriesForDevice)
            {
                await telemetrieRepository.AddTelemetrieAsync(telemetrie);
                telemetriesAdded++;

                // Envoyer via SignalR si disponible
                if (signalRService != null)
                {
                await signalRService.SendTelemetryUpdateAsync(periph.Id, new
                {
                    deviceId,
                    peripheriqueName = name,
                    type = telemetrie.TypeEvenement,
                    value = telemetrie.Valeur,
                    timestamp = telemetrie.Horodatage
                });
                }
            }
        }

        if (telemetriesAdded > 0)
        {
            _logger.LogDebug("Ajouté {Count} télémétries pour le device {DeviceId}", telemetriesAdded, deviceId);
        }
    }

    private async Task ProcessDirectDataAsync(
        JsonElement root, 
        string deviceId, 
        DateTime timestamp,
        ITelemetrieRepository telemetrieRepository,
        IPeripheriqueLogiqueRepository periphRepo,
        ISignalRService? signalRService)
    {
        // Traitement pour les données directes (ancien format ou données simples)
        var periph = await periphRepo.GetByIotDeviceIdAsync(deviceId);
        if (periph == null)
        {
            _logger.LogDebug("Aucun périphérique trouvé pour le deviceId {DeviceId}", deviceId);
            return;
        }

        var telemetries = new List<Telemetrie>();

        // Traiter les propriétés connues
        foreach (var property in root.EnumerateObject())
        {
            if (TryGetNumericValue(property.Value, out var value))
            {
                var eventType = property.Name.ToLowerInvariant() switch
                {
                    "temperature" => "temperature",
                    "humidity" => "humidity",
                    "power" => "power_mw",
                    "energy" => "energy_mwh",
                    "state" => "state",
                    _ => property.Name
                };

                telemetries.Add(new Telemetrie
                {
                    Horodatage = timestamp,
                    TypeEvenement = eventType,
                    Valeur = value,
                    PeripheriqueLogiqueId = periph.Id,
                    DonneesJson = JsonSerializer.Serialize(new Dictionary<string, object> { [property.Name] = property.Value })
                });
            }
        }

        // Sauvegarder et diffuser
        foreach (var telemetrie in telemetries)
        {
            await telemetrieRepository.AddTelemetrieAsync(telemetrie);

            if (signalRService != null)
            {
                await signalRService.SendTelemetryUpdateAsync(periph.Id, new
                {
                    deviceId,
                    type = telemetrie.TypeEvenement,
                    value = telemetrie.Valeur,
                    timestamp = telemetrie.Horodatage
                });
            }
        }

        if (telemetries.Any())
        {
            _logger.LogDebug("Ajouté {Count} télémétries directes pour le device {DeviceId}", telemetries.Count, deviceId);
        }
    }

    private async Task ProcessAdditionalPropertiesAsync(
        JsonElement peripheral, 
        int peripheriqueId, 
        DateTime timestamp, 
        List<Telemetrie> telemetries)
    {
        // Traiter d'autres propriétés comme température, humidité, etc.
        var additionalProperties = new[] { "temperature", "humidity", "voltage", "current", "brightness" };

        foreach (var propName in additionalProperties)
        {
            if (peripheral.TryGetProperty(propName, out var propEl) && TryGetNumericValue(propEl, out var value))
            {
                telemetries.Add(new Telemetrie
                {
                    Horodatage = timestamp,
                    TypeEvenement = propName,
                    Valeur = value,
                    PeripheriqueLogiqueId = peripheriqueId
                });
            }
        }

        await Task.CompletedTask; // Pour éviter le warning async
    }

    private static bool TryGetNumericValue(JsonElement element, out double value)
    {
        value = 0;
        
        return element.ValueKind switch
        {
            JsonValueKind.Number => element.TryGetDouble(out value),
            JsonValueKind.String when double.TryParse(element.GetString(), out value) => true,
            JsonValueKind.True => (value = 1) == 1,
            JsonValueKind.False => (value = 0) == 0,
            _ => false
        };
    }

    private static string? TryGetDeviceId(EventData data)
    {
        // IoT Hub renseigne généralement "iothub-connection-device-id" dans les propriétés
        if (data.Properties.TryGetValue("iothub-connection-device-id", out var deviceIdObj) && 
            deviceIdObj is string deviceId && !string.IsNullOrWhiteSpace(deviceId))
        {
            return deviceId;
        }

        // Fallback: essayer d'autres propriétés
        var fallbackKeys = new[] { "deviceId", "device-id", "device_id" };
        foreach (var key in fallbackKeys)
        {
            if (data.Properties.TryGetValue(key, out var fallbackObj) && 
                fallbackObj is string fallbackId && !string.IsNullOrWhiteSpace(fallbackId))
            {
                return fallbackId;
            }
        }

        return null;
    }

    public override void Dispose()
    {
        try
        {
            _processor?.StopProcessingAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'arrêt de l'EventProcessorClient");
        }
        base.Dispose();
    }
}