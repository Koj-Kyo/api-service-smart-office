# Azure Function - Ingestion Télémétrie & Logs IoT Hub

Ce document décrit l'Azure Function pour ingérer les messages D2C (Device-to-Cloud) depuis IoT Hub.

## Architecture

```
ESP32 ? IoT Hub ? Azure Function (IoT Hub Trigger) ? {
    - PostgreSQL (télémétrie, logs)
    - SignalR (temps réel vers clients web)
}
```

## Création du projet Azure Function

```bash
# Créer un nouveau projet Azure Functions
dotnet new func --name SmartOfficeTelemetryFunction --worker-runtime dotnet-isolated

cd SmartOfficeTelemetryFunction

# Ajouter les packages nécessaires
dotnet add package Microsoft.Azure.Functions.Worker
dotnet add package Microsoft.Azure.Functions.Worker.Sdk
dotnet add package Microsoft.Azure.Functions.Worker.Extensions.EventHubs
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.SignalR.Client
dotnet add package System.Text.Json
```

## Structure du projet

```
SmartOfficeTelemetryFunction/
??? Functions/
?   ??? TelemetryIngestionFunction.cs    # Ingestion télémétrie
?   ??? LogIngestionFunction.cs          # Ingestion logs
?   ??? EventIngestionFunction.cs        # Événements système
??? Models/
?   ??? TelemetryMessage.cs
?   ??? LogMessage.cs
?   ??? EventMessage.cs
??? Services/
?   ??? DatabaseService.cs
?   ??? SignalRNotificationService.cs
??? host.json
??? local.settings.json
??? SmartOfficeTelemetryFunction.csproj
```

## 1. TelemetryIngestionFunction.cs

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;

namespace SmartOfficeTelemetryFunction.Functions;

public class TelemetryIngestionFunction
{
    private readonly ILogger<TelemetryIngestionFunction> _logger;
    private readonly DatabaseService _databaseService;
    private readonly SignalRNotificationService _signalRService;

    public TelemetryIngestionFunction(
        ILogger<TelemetryIngestionFunction> logger,
        DatabaseService databaseService,
        SignalRNotificationService signalRService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _signalRService = signalRService;
    }

    [Function("TelemetryIngestion")]
    public async Task Run(
        [EventHubTrigger(
            "iothub-ehub-smartoffi-12345-67890",  // Remplacer par le nom Event Hub de votre IoT Hub
            Connection = "IoTHubEventHubConnectionString",
            ConsumerGroup = "$Default")] EventData[] events)
    {
        foreach (EventData eventData in events)
        {
            try
            {
                // Vérifier le type de message via properties
                if (!eventData.Properties.TryGetValue("type", out var messageType) 
                    || messageType?.ToString() != "telemetry")
                {
                    continue; // Ignorer les messages non-télémétrie
                }

                var messageBody = Encoding.UTF8.GetString(eventData.EventBody.ToArray());
                var telemetryMessage = JsonSerializer.Deserialize<TelemetryMessage>(messageBody);

                if (telemetryMessage == null)
                {
                    _logger.LogWarning("Message de télémétrie invalide: {Body}", messageBody);
                    continue;
                }

                // Enrichir avec metadata IoT Hub
                telemetryMessage.EnqueuedTime = eventData.EnqueuedTime.UtcDateTime;
                telemetryMessage.IoTHubDeviceId = eventData.SystemProperties.TryGetValue("iothub-connection-device-id", out var deviceId) 
                    ? deviceId.ToString() 
                    : telemetryMessage.DeviceId;

                // Persister en base de données
                await _databaseService.SaveTelemetryAsync(telemetryMessage);

                // Notifier les clients en temps réel via SignalR
                await _signalRService.SendTelemetryUpdateAsync(telemetryMessage);

                _logger.LogInformation(
                    "Télémétrie persistée: Device={Device}, Peripheral={Peripheral}, Metric={Metric}, Value={Value}",
                    telemetryMessage.DeviceId,
                    telemetryMessage.Peripheral,
                    telemetryMessage.Metric,
                    telemetryMessage.Value);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erreur de désérialisation du message de télémétrie");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du message de télémétrie");
            }
        }
    }
}
```

## 2. LogIngestionFunction.cs

```csharp
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;

namespace SmartOfficeTelemetryFunction.Functions;

public class LogIngestionFunction
{
    private readonly ILogger<LogIngestionFunction> _logger;
    private readonly DatabaseService _databaseService;
    private readonly SignalRNotificationService _signalRService;

    public LogIngestionFunction(
        ILogger<LogIngestionFunction> logger,
        DatabaseService databaseService,
        SignalRNotificationService signalRService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _signalRService = signalRService;
    }

    [Function("LogIngestion")]
    public async Task Run(
        [EventHubTrigger(
            "iothub-ehub-smartoffi-12345-67890",
            Connection = "IoTHubEventHubConnectionString",
            ConsumerGroup = "$Default")] EventData[] events)
    {
        foreach (EventData eventData in events)
        {
            try
            {
                if (!eventData.Properties.TryGetValue("type", out var messageType) 
                    || messageType?.ToString() != "log")
                {
                    continue;
                }

                var messageBody = Encoding.UTF8.GetString(eventData.EventBody.ToArray());
                var logMessage = JsonSerializer.Deserialize<LogMessage>(messageBody);

                if (logMessage == null)
                {
                    _logger.LogWarning("Message de log invalide: {Body}", messageBody);
                    continue;
                }

                logMessage.EnqueuedTime = eventData.EnqueuedTime.UtcDateTime;
                logMessage.IoTHubDeviceId = eventData.SystemProperties.TryGetValue("iothub-connection-device-id", out var deviceId) 
                    ? deviceId.ToString() 
                    : logMessage.DeviceId;

                await _databaseService.SaveLogAsync(logMessage);

                // Notifier uniquement les logs error/warn
                if (logMessage.Level == "error" || logMessage.Level == "warn")
                {
                    await _signalRService.SendLogAlertAsync(logMessage);
                }

                _logger.LogInformation(
                    "Log persisté: Device={Device}, Level={Level}, Source={Source}, Message={Message}",
                    logMessage.DeviceId,
                    logMessage.Level,
                    logMessage.Source,
                    logMessage.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du message de log");
            }
        }
    }
}
```

## 3. Models/TelemetryMessage.cs

```csharp
namespace SmartOfficeTelemetryFunction.Models;

public class TelemetryMessage
{
    public string DeviceId { get; set; } = string.Empty;
    public string? IoTHubDeviceId { get; set; }
    public string Peripheral { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime? EnqueuedTime { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}
```

## 4. Models/LogMessage.cs

```csharp
namespace SmartOfficeTelemetryFunction.Models;

public class LogMessage
{
    public string DeviceId { get; set; } = string.Empty;
    public string? IoTHubDeviceId { get; set; }
    public string Level { get; set; } = "info"; // info, warn, error
    public string Source { get; set; } = string.Empty; // firmware, peripheral, network
    public string Message { get; set; } = string.Empty;
    public string? Peripheral { get; set; }
    public DateTime Timestamp { get; set; }
    public DateTime? EnqueuedTime { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
```

## 5. Services/DatabaseService.cs

```csharp
using Npgsql;
using SmartOfficeTelemetryFunction.Models;
using System.Text.Json;

namespace SmartOfficeTelemetryFunction.Services;

public class DatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _connectionString = configuration["PostgreSQLConnectionString"] 
            ?? throw new InvalidOperationException("PostgreSQLConnectionString not configured");
        _logger = logger;
    }

    public async Task SaveTelemetryAsync(TelemetryMessage telemetry)
    {
        const string sql = @"
            INSERT INTO public.""Telemetries"" 
                (""PeripheriqueLogiqueId"", ""TypeDonnee"", ""Valeur"", ""Unite"", ""Horodatage"", ""DeviceId"", ""Peripheral"", ""Metadata"")
            VALUES 
                (@PeripheriqueLogiqueId, @TypeDonnee, @Valeur, @Unite, @Horodatage, @DeviceId, @Peripheral, @Metadata::jsonb)";

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("PeripheriqueLogiqueId", DBNull.Value); // Résolu côté app si nécessaire
            command.Parameters.AddWithValue("TypeDonnee", telemetry.Metric);
            command.Parameters.AddWithValue("Valeur", telemetry.Value);
            command.Parameters.AddWithValue("Unite", telemetry.Unit);
            command.Parameters.AddWithValue("Horodatage", telemetry.Timestamp);
            command.Parameters.AddWithValue("DeviceId", telemetry.IoTHubDeviceId ?? telemetry.DeviceId);
            command.Parameters.AddWithValue("Peripheral", telemetry.Peripheral);
            command.Parameters.AddWithValue("Metadata", JsonSerializer.Serialize(telemetry.Metadata ?? new Dictionary<string, object>()));

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde de la télémétrie");
            throw;
        }
    }

    public async Task SaveLogAsync(LogMessage log)
    {
        const string sql = @"
            INSERT INTO public.""DeviceLogs"" 
                (""DeviceId"", ""Peripheral"", ""Level"", ""Source"", ""Message"", ""Timestamp"", ""Context"")
            VALUES 
                (@DeviceId, @Peripheral, @Level, @Source, @Message, @Timestamp, @Context::jsonb)";

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("DeviceId", log.IoTHubDeviceId ?? log.DeviceId);
            command.Parameters.AddWithValue("Peripheral", (object?)log.Peripheral ?? DBNull.Value);
            command.Parameters.AddWithValue("Level", log.Level);
            command.Parameters.AddWithValue("Source", log.Source);
            command.Parameters.AddWithValue("Message", log.Message);
            command.Parameters.AddWithValue("Timestamp", log.Timestamp);
            command.Parameters.AddWithValue("Context", JsonSerializer.Serialize(log.Context ?? new Dictionary<string, object>()));

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde du log");
            throw;
        }
    }
}
```

## 6. Services/SignalRNotificationService.cs

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using SmartOfficeTelemetryFunction.Models;

namespace SmartOfficeTelemetryFunction.Services;

public class SignalRNotificationService
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(IConfiguration configuration, ILogger<SignalRNotificationService> logger)
    {
        _logger = logger;
        var signalRUrl = configuration["SignalRHubUrl"] ?? "https://localhost:7152/notificationhub";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(signalRUrl)
            .WithAutomaticReconnect()
            .Build();

        _ = ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        try
        {
            await _hubConnection.StartAsync();
            _logger.LogInformation("Connecté au hub SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible de se connecter au hub SignalR");
        }
    }

    public async Task SendTelemetryUpdateAsync(TelemetryMessage telemetry)
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("BroadcastTelemetry", telemetry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible d'envoyer la télémétrie via SignalR");
        }
    }

    public async Task SendLogAlertAsync(LogMessage log)
    {
        try
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("BroadcastLogAlert", log);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible d'envoyer l'alerte log via SignalR");
        }
    }
}
```

## 7. Program.cs

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmartOfficeTelemetryFunction.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<DatabaseService>();
        services.AddSingleton<SignalRNotificationService>();
    })
    .Build();

host.Run();
```

## 8. local.settings.json

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "IoTHubEventHubConnectionString": "Endpoint=sb://iothub-ns-smartoffi-12345-67890.servicebus.windows.net/;SharedAccessKeyName=service;SharedAccessKey=xxxxx;EntityPath=iothub-ehub-smartoffi-12345-67890",
    "PostgreSQLConnectionString": "Host=localhost;Database=SmartOfficeDB;Username=postgres;Password=yourpassword",
    "SignalRHubUrl": "https://localhost:7152/notificationhub"
  }
}
```

## 9. Création de la table DeviceLogs

```sql
CREATE TABLE public."DeviceLogs" (
    "Id" SERIAL PRIMARY KEY,
    "DeviceId" VARCHAR(255) NOT NULL,
    "Peripheral" VARCHAR(100),
    "Level" VARCHAR(50) NOT NULL,
    "Source" VARCHAR(100) NOT NULL,
    "Message" TEXT NOT NULL,
    "Timestamp" TIMESTAMP NOT NULL,
    "Context" JSONB,
    "CreatedAt" TIMESTAMP DEFAULT NOW()
);

CREATE INDEX "IX_DeviceLogs_DeviceId_Timestamp" ON public."DeviceLogs" ("DeviceId", "Timestamp" DESC);
CREATE INDEX "IX_DeviceLogs_Level" ON public."DeviceLogs" ("Level");
CREATE INDEX "IX_DeviceLogs_Peripheral" ON public."DeviceLogs" ("Peripheral");
```

## Déploiement

```bash
# Publier vers Azure
func azure functionapp publish SmartOfficeTelemetryFunctionApp

# Configurer les variables d'environnement dans Azure Portal
az functionapp config appsettings set \
  --name SmartOfficeTelemetryFunctionApp \
  --resource-group SmartOfficeResourceGroup \
  --settings \
    IoTHubEventHubConnectionString="<connection-string>" \
    PostgreSQLConnectionString="<connection-string>" \
    SignalRHubUrl="https://your-api.azurewebsites.net/notificationhub"
```

## Monitoring & Logs

- Azure Application Insights pour tracer les exécutions
- Logs persistés dans PostgreSQL pour audit
- Dashboard temps réel via SignalR

## Exemple de message firmware ESP32

```cpp
// Télémétrie
{
  "deviceId": "esp32-office-101",
  "peripheral": "temp_sensor_room",
  "metric": "temperature",
  "value": 23.5,
  "unit": "°C",
  "timestamp": "2025-01-15T14:30:00Z",
  "metadata": {
    "rssi": -58,
    "heapFree": 123456
  }
}
// Propriété: type=telemetry

// Log
{
  "deviceId": "esp32-office-101",
  "level": "error",
  "source": "peripheral",
  "peripheral": "relay_lights_office",
  "message": "Failed to set GPIO state",
  "timestamp": "2025-01-15T14:30:00Z",
  "context": {
    "errorCode": "GPIO_ERROR",
    "gpio": 33
  }
}
// Propriété: type=log
```
