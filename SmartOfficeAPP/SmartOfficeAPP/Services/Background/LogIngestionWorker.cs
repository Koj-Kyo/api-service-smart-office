using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.Background;

public class LogIngestionWorker : BackgroundService
{
    private readonly ILogger<LogIngestionWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly EventProcessorClient? _processor;
    private readonly string _consumerGroup;
    private readonly bool _isEventHubEnabled;

    public LogIngestionWorker(
        ILogger<LogIngestionWorker> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _consumerGroup = "logs"; // Consumer group séparé pour les logs
        _isEventHubEnabled = false;

        var ehConn = configuration["IotHubEventHub:ConnectionString"];
        var ehName = configuration["IotHubEventHub:Name"];
        var blobConn = configuration["IotHubEventHub:BlobConnectionString"];
        var blobContainer = configuration["IotHubEventHub:BlobContainerLogs"] ?? "iothub-logs-checkpoints";
        
        var eventHubEnabled = configuration.GetValue<bool>("IotHubEventHub:LogsEnabled", true);
        
        if (!eventHubEnabled)
        {
            _logger.LogInformation("Service d'ingestion logs Event Hub désactivé par configuration.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ehConn) || string.IsNullOrWhiteSpace(ehName) ||
            string.IsNullOrWhiteSpace(blobConn) || string.IsNullOrWhiteSpace(blobContainer))
        {
            _logger.LogInformation("Service d'ingestion logs Event Hub désactivé : configuration manquante.");
            return;
        }

        try
        {
            var storageClient = new BlobContainerClient(blobConn, blobContainer);
            
            if (!storageClient.Exists())
            {
                _logger.LogInformation("Création du conteneur de checkpoints logs : {ContainerName}", blobContainer);
                storageClient.CreateIfNotExists();
            }
            
            _processor = new EventProcessorClient(storageClient, _consumerGroup, ehConn, ehName);
            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorHandler;
            
            _isEventHubEnabled = true;
            _logger.LogInformation("Service d'ingestion logs Event Hub initialisé avec succès.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation du service d'ingestion logs Event Hub");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_isEventHubEnabled || _processor is null)
        {
            _logger.LogInformation("Service d'ingestion logs Event Hub désactivé.");
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Arrêt du service d'ingestion logs.");
            }
            return;
        }

        _logger.LogInformation("Démarrage de l'ingestion logs Event Hub...");
        
        try
        {
            await _processor.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Arrêt de l'ingestion logs Event Hub demandé.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur durant l'exécution de l'ingestion logs Event Hub");
        }
        finally
        {
            if (_processor != null)
            {
                _logger.LogInformation("Arrêt de l'ingestion logs Event Hub...");
                try
                {
                    await _processor.StopProcessingAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'arrêt de l'EventProcessorClient pour logs");
                }
            }
        }
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, 
            "Erreur Event Hub Logs - Partition: {PartitionId}, Operation: {Operation}", 
            args.PartitionId, args.Operation);
        return Task.CompletedTask;
    }

    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var deviceLogService = scope.ServiceProvider.GetRequiredService<IDeviceLogService>();
            var signalRService = scope.ServiceProvider.GetService<ISignalRService>();

            var body = Encoding.UTF8.GetString(args.Data.EventBody.ToArray());
            var deviceId = TryGetDeviceId(args.Data);

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                _logger.LogDebug("Événement log sans deviceId, ignoré.");
                await args.UpdateCheckpointAsync();
                return;
            }

            // Vérifier que c'est bien un message de type "log"
            if (!args.Data.Properties.TryGetValue("type", out var messageType) || 
                messageType?.ToString() != "log")
            {
                await args.UpdateCheckpointAsync();
                return;
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("JSON de log invalide reçu du device {DeviceId}: {Error}", deviceId, ex.Message);
                await args.UpdateCheckpointAsync();
                return;
            }

            using (doc)
            {
                var root = doc.RootElement;
                
                var logDto = new DeviceLogDto
                {
                    DeviceId = deviceId,
                    Level = root.TryGetProperty("level", out var lvl) ? lvl.GetString() ?? "info" : "info",
                    Source = root.TryGetProperty("source", out var src) ? src.GetString() ?? "unknown" : "unknown",
                    Message = root.TryGetProperty("message", out var msg) ? msg.GetString() ?? "" : "",
                    Peripheral = root.TryGetProperty("peripheral", out var per) ? per.GetString() : null,
                    Timestamp = root.TryGetProperty("timestamp", out var ts) && ts.TryGetDateTime(out var timestamp) 
                        ? timestamp.ToUniversalTime() 
                        : args.Data.EnqueuedTime.UtcDateTime,
                    Context = root.TryGetProperty("context", out var ctx) ? ctx.GetRawText() : null
                };

                await deviceLogService.AddLogAsync(logDto);

                // Notifier via SignalR si log error ou warn
                if (signalRService != null && (logDto.Level == "error" || logDto.Level == "warn"))
                {
                    await signalRService.SendLogAlertAsync(new
                    {
                        deviceId = logDto.DeviceId,
                        level = logDto.Level,
                        source = logDto.Source,
                        message = logDto.Message,
                        peripheral = logDto.Peripheral,
                        timestamp = logDto.Timestamp
                    });
                }

                _logger.LogDebug("Log persisté: Device={Device}, Level={Level}, Source={Source}, Message={Message}",
                    logDto.DeviceId, logDto.Level, logDto.Source, logDto.Message);
            }

            await args.UpdateCheckpointAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement d'un événement log IoT Hub");
            
            try
            {
                await args.UpdateCheckpointAsync();
            }
            catch (Exception checkpointEx)
            {
                _logger.LogError(checkpointEx, "Erreur lors du checkpoint après échec de traitement log");
            }
        }
    }

    private static string? TryGetDeviceId(EventData data)
    {
        if (data.Properties.TryGetValue("iothub-connection-device-id", out var deviceIdObj) && 
            deviceIdObj is string deviceId && !string.IsNullOrWhiteSpace(deviceId))
        {
            return deviceId;
        }

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
        if (_processor != null)
        {
            try
            {
                _processor.StopProcessingAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'arrêt de l'EventProcessorClient pour logs");
            }
        }
        base.Dispose();
    }
}
