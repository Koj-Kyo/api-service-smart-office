using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.Background;

/// <summary>
/// Worker background pour synchronisation périodique des devices IoT Hub vers DB
/// </summary>
public class DeviceSyncWorker : BackgroundService
{
private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DeviceSyncWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _syncInterval;

    public DeviceSyncWorker(
        IServiceProvider serviceProvider,
        ILogger<DeviceSyncWorker> logger,
        IConfiguration configuration)
    {
   _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
      
    // Configurable: par défaut 5 minutes
        _syncInterval = TimeSpan.FromMinutes(
            _configuration.GetValue<int>("DeviceSync:IntervalMinutes", 5)
        );
  }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeviceSyncWorker démarré avec un intervalle de {Interval} minutes", _syncInterval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
 {
                await PerformSyncAsync();
             await Task.Delay(_syncInterval, stoppingToken);
    }
        catch (OperationCanceledException)
       {
         _logger.LogInformation("DeviceSyncWorker arrêté");
              break;
            }
       catch (Exception ex)
            {
      _logger.LogError(ex, "Erreur durant la synchronisation périodique des devices");
    
      // Attendre un délai plus court en cas d'erreur
          await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
      }
}
}

    private async Task PerformSyncAsync()
 {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
     
   try
   {
       using var scope = _serviceProvider.CreateScope();
       var deviceSyncService = scope.ServiceProvider.GetRequiredService<IDeviceSyncService>();
 var iotHubService = scope.ServiceProvider.GetRequiredService<IIotHubService>();

            _logger.LogDebug("Début synchronisation périodique des devices...");

       // 1. Lister tous les devices IoT Hub
  var iotDevices = await iotHubService.ListDevicesAsync(1000);
            var deviceIds = iotDevices.Select(d => d.DeviceId).ToList();

    if (!deviceIds.Any())
      {
        _logger.LogDebug("Aucun device trouvé dans IoT Hub");
           return;
      }

         _logger.LogInformation("Synchronisation de {DeviceCount} devices IoT Hub", deviceIds.Count);

    // 2. Synchroniser chaque device (en parallèle mais limité)
            var semaphore = new SemaphoreSlim(5, 5); // Max 5 devices en parallèle
   var syncTasks = deviceIds.Select(async deviceId =>
      {
           await semaphore.WaitAsync();
      try
                {
 await SyncSingleDeviceAsync(deviceSyncService, deviceId);
    }
           finally
           {
          semaphore.Release();
  }
            });

            await Task.WhenAll(syncTasks);

  stopwatch.Stop();
       _logger.LogInformation("Synchronisation terminée en {ElapsedMs}ms pour {DeviceCount} devices", 
    stopwatch.ElapsedMilliseconds, deviceIds.Count);
        }
        catch (Exception ex)
        {
    _logger.LogError(ex, "Erreur durant la synchronisation périodique");
 stopwatch.Stop();
 }
    }

    private async Task SyncSingleDeviceAsync(IDeviceSyncService deviceSyncService, string deviceId)
    {
        try
        {
  // Synchronisation complète du device
        await deviceSyncService.FullSyncDeviceAsync(deviceId);
          
            _logger.LogDebug("Device {DeviceId} synchronisé avec succès", deviceId);
      }
        catch (Exception ex)
  {
  _logger.LogWarning(ex, "Erreur lors de la synchronisation du device {DeviceId}", deviceId);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeviceSyncWorker en cours d'arrêt...");
        await base.StopAsync(stoppingToken);
        _logger.LogInformation("DeviceSyncWorker arrêté");
    }
}