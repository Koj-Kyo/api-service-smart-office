using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Services.Background;

public class TelemetrieIngestionWorker : BackgroundService
{
    private readonly ILogger<TelemetrieIngestionWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventProcessorClient? _processor;

    public TelemetrieIngestionWorker(
        ILogger<TelemetrieIngestionWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

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

        var storageClient = new BlobContainerClient(blobConn, blobContainer);
        storageClient.CreateIfNotExists();
        _processor = new EventProcessorClient(storageClient, "$Default", ehConn, ehName);
        _processor.ProcessEventAsync += ProcessEventHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_processor is null)
        {
            _logger.LogWarning("EventProcessorClient non initialisé (config manquante).");
            return;
        }

        _logger.LogInformation("Démarrage ingestion IoT Hub...");
        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        finally
        {
            _logger.LogInformation("Arrêt ingestion IoT Hub...");
            await _processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Erreur EventHub Partition {PartitionId}", args.PartitionId);
        return Task.CompletedTask;
    }

    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var telemetrieRepository = scope.ServiceProvider.GetRequiredService<ITelemetrieRepository>();
            var periphRepo = scope.ServiceProvider.GetRequiredService<IPeripheriqueLogiqueRepository>();

            var body = Encoding.UTF8.GetString(args.Data.EventBody.ToArray());
            var enqueued = args.Data.EnqueuedTime.UtcDateTime;
            var deviceId = TryGetDeviceId(args.Data);

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                _logger.LogDebug("Event sans deviceId, ignoré.");
                await args.UpdateCheckpointAsync();
                return;
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            // Schéma v2 du firmware: contient "peripherals":[{ name,type,gpio,state,p_mw,e_mwh }, ...]
            if (root.TryGetProperty("peripherals", out var peripherals) && peripherals.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in peripherals.EnumerateArray())
                {
                    var name = p.TryGetProperty("name", out var n) ? n.GetString() : null;
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var periph = await periphRepo.GetByIotDeviceIdAndNameAsync(deviceId, name);
                    if (periph is null) continue;

                    // power_mw
                    if (p.TryGetProperty("p_mw", out var pMwEl) && pMwEl.TryGetInt32(out var pMw))
                    {
                        await telemetrieRepository.AddTelemetrieAsync(new Telemetrie
                        {
                            Horodatage = enqueued,
                            TypeEvenement = "power_mw", // Correction: utiliser TypeEvenement
                            Valeur = pMw,
                            PeripheriqueLogiqueId = periph.Id
                        });
                    }

                    // energy_mwh
                    if (p.TryGetProperty("e_mwh", out var eMwhEl) && eMwhEl.TryGetInt32(out var eMwh))
                    {
                        await telemetrieRepository.AddTelemetrieAsync(new Telemetrie
                        {
                            Horodatage = enqueued,
                            TypeEvenement = "energy_mwh", // Correction: utiliser TypeEvenement
                            Valeur = eMwh,
                            PeripheriqueLogiqueId = periph.Id
                        });
                    }

                    // state (0/1)
                    if (p.TryGetProperty("state", out var stEl))
                    {
                        var state = stEl.ValueKind switch
                        {
                            JsonValueKind.String => stEl.GetString() == "1" ? 1 : 0,
                            JsonValueKind.Number => stEl.GetInt32(),
                            _ => 0
                        };
                        await telemetrieRepository.AddTelemetrieAsync(new Telemetrie
                        {
                            Horodatage = enqueued,
                            TypeEvenement = "state", // Correction: utiliser TypeEvenement
                            Valeur = state,
                            PeripheriqueLogiqueId = periph.Id
                        });
                    }
                }
            }

            await args.UpdateCheckpointAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement d'un événement IoT Hub.");
        }
    }

    private static string? TryGetDeviceId(EventData data)
    {
        // IoT Hub renseigne généralement "iothub-connection-device-id" dans les propriétés
        if (data.Properties.TryGetValue("iothub-connection-device-id", out var v) && v is string s && !string.IsNullOrWhiteSpace(s))
            return s;

        // fallback possible si présent dans SystemProperties (selon transport)
        return null;
    }
}