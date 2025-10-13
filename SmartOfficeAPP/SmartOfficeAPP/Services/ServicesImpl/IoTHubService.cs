using SmartOfficeAPP.Services.IServices;
using Microsoft.Azure.Devices;
<<<<<<< HEAD
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Text;
using System.Text.Json;
=======
using Microsoft.Azure.Devices.Shared; // pour Twin si besoin
using System.Text;
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0

namespace SmartOfficeAPP.Services.ServicesImpl;

public class IotHubService : IIotHubService
{
    private readonly ServiceClient _serviceClient;
    private readonly RegistryManager _registryManager;
<<<<<<< HEAD
    private readonly ILogger<IotHubService> _logger;
    private readonly ISignalRService _signalRService;

    public IotHubService(
        IConfiguration configuration, 
        ILogger<IotHubService> logger,
        ISignalRService signalRService)
=======

    // La chaîne de connexion doit être ajoutée dans appsettings.json (IotHubConnectionString).
    public IotHubService(IConfiguration configuration)
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
    {
        var iotHubConnectionString = configuration["IotHubConnectionString"];
        
        if (string.IsNullOrWhiteSpace(iotHubConnectionString))
        {
            _logger?.LogWarning("IotHubConnectionString n'est pas configuré");
            throw new InvalidOperationException("IotHubConnectionString est requis");
        }

        _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        _registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
<<<<<<< HEAD
        _logger = logger;
        _signalRService = signalRService;
=======
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
    }

    /// <summary>
    /// Envoie une méthode directe à un appareil IoT
    /// </summary>
    public async Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload)
    {
        try
        {
            var methodInvocation = new CloudToDeviceMethod(methodName) 
            { 
                ResponseTimeout = TimeSpan.FromSeconds(30),
                ConnectionTimeout = TimeSpan.FromSeconds(10)
            };

            if (!string.IsNullOrEmpty(payload))
            {
                methodInvocation.SetPayloadJson(payload);
            }

            _logger.LogInformation("Invocation de la méthode {MethodName} sur le device {DeviceId}", methodName, deviceId);

            var result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
<<<<<<< HEAD
            
            var success = result.Status == 200;
            var responsePayload = result.GetPayloadAsJson();

            _logger.LogInformation("Méthode {MethodName} sur {DeviceId}: Status={Status}, Response={Response}", 
                methodName, deviceId, result.Status, responsePayload);

            // Notifier via SignalR du résultat de la commande
            await _signalRService.SendCommandExecutionResultAsync(
                0, // CommandId sera défini par le service appelant
                0, // PeripheriqueId sera défini par le service appelant  
                success ? "Success" : "Failed",
                responsePayload ?? $"Status: {result.Status}"
            );

            return success;
        }
        catch (DeviceNotFoundException)
        {
            _logger.LogError("Device {DeviceId} non trouvé", deviceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'invocation de la méthode {MethodName} sur {DeviceId}", methodName, deviceId);
=======
            return result.Status == 200;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur InvokeDeviceMethodAsync : {ex.Message}");
            return false;
        }
    }

    // Met à jour les desired properties du Device Twin avec le fragment JSON fourni.
    public async Task<bool> UpdateDesiredPropertiesAsync(string deviceId, string desiredJson)
    {
        try
        {
            var twin = await _registryManager.GetTwinAsync(deviceId);
            // Construit un patch JSON valide pour UpdateTwinAsync
            var patch = $"{{ \"properties\": {{ \"desired\": {desiredJson} }} }}";
            await _registryManager.UpdateTwinAsync(deviceId, patch, twin.ETag);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur UpdateDesiredPropertiesAsync : {ex.Message}");
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
            return false;
        }
    }

    /// <summary>
    /// Met à jour les desired properties du Device Twin
    /// </summary>
    public async Task<bool> UpdateDesiredPropertiesAsync(string deviceId, string desiredJson)
    {
        try
        {
            var twin = await _registryManager.GetTwinAsync(deviceId);
            var patch = $"{{ \"properties\": {{ \"desired\": {desiredJson} }} }}";
            
            await _registryManager.UpdateTwinAsync(deviceId, patch, twin.ETag);
            
            _logger.LogInformation("Desired properties mises à jour pour {DeviceId}: {Properties}", deviceId, desiredJson);
            
            return true;
        }
        catch (DeviceNotFoundException)
        {
            _logger.LogError("Device {DeviceId} non trouvé pour mise à jour des desired properties", deviceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour des desired properties pour {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Récupère les propriétés (reported et desired) d'un device twin
    /// </summary>
    public async Task<DeviceTwinInfo?> GetDeviceTwinAsync(string deviceId)
    {
        try
        {
            var twin = await _registryManager.GetTwinAsync(deviceId);
            
            return new DeviceTwinInfo
            {
                DeviceId = deviceId,
                Status = twin.Status?.ToString(),
                ConnectionState = twin.ConnectionState?.ToString(),
                LastActivityTime = twin.LastActivityTime,
                ReportedProperties = twin.Properties.Reported.ToJson(),
                DesiredProperties = twin.Properties.Desired.ToJson(),
                Version = twin.Version ?? 0
            };
        }
        catch (DeviceNotFoundException)
        {
            _logger.LogError("Device {DeviceId} non trouvé", deviceId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du twin pour {DeviceId}", deviceId);
            return null;
        }
    }

    /// <summary>
    /// Envoie un message Cloud-to-Device
    /// </summary>
    public async Task<bool> SendCloudToDeviceMessageAsync(string deviceId, string message, Dictionary<string, string>? properties = null)
    {
        try
        {
            var commandMessage = new Message(Encoding.UTF8.GetBytes(message));
            
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    commandMessage.Properties.Add(prop.Key, prop.Value);
                }
            }

            await _serviceClient.SendAsync(deviceId, commandMessage);
            
            _logger.LogInformation("Message Cloud-to-Device envoyé à {DeviceId}: {Message}", deviceId, message);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'envoi du message Cloud-to-Device à {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Crée ou met à jour un device dans IoT Hub
    /// </summary>
    public async Task<bool> CreateOrUpdateDeviceAsync(string deviceId, bool enabled = true)
    {
        try
        {
            var device = new Device(deviceId)
            {
                Status = enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled
            };

            await _registryManager.AddDeviceAsync(device);
            _logger.LogInformation("Device {DeviceId} créé avec succès", deviceId);
            
            return true;
        }
        catch (DeviceAlreadyExistsException)
        {
            // Device existe déjà, on le met à jour si nécessaire
            try
            {
                var existingDevice = await _registryManager.GetDeviceAsync(deviceId);
                if (existingDevice.Status != (enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled))
                {
                    existingDevice.Status = enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;
                    await _registryManager.UpdateDeviceAsync(existingDevice);
                    _logger.LogInformation("Device {DeviceId} mis à jour", deviceId);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du device {DeviceId}", deviceId);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Supprime un device d'IoT Hub
    /// </summary>
    public async Task<bool> DeleteDeviceAsync(string deviceId)
    {
        try
        {
            await _registryManager.RemoveDeviceAsync(deviceId);
            _logger.LogInformation("Device {DeviceId} supprimé avec succès", deviceId);
            return true;
        }
        catch (DeviceNotFoundException)
        {
            _logger.LogWarning("Device {DeviceId} non trouvé pour suppression", deviceId);
            return true; // Considéré comme succès si déjà supprimé
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Obtient la liste des devices avec leur statut de connexion
    /// </summary>
    public async Task<IEnumerable<DeviceConnectionInfo>> GetDevicesConnectionStatusAsync(IEnumerable<string> deviceIds)
    {
        var result = new List<DeviceConnectionInfo>();

        foreach (var deviceId in deviceIds)
        {
            try
            {
                var device = await _registryManager.GetDeviceAsync(deviceId);
                var twin = await _registryManager.GetTwinAsync(deviceId);

                result.Add(new DeviceConnectionInfo
                {
                    DeviceId = deviceId,
                    Status = device.Status.ToString(),
                    ConnectionState = device.ConnectionState.ToString(),
                    LastActivityTime = device.LastActivityTime,
                    IsConnected = device.ConnectionState == DeviceConnectionState.Connected,
                    TwinVersion = twin.Version ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible de récupérer le statut pour {DeviceId}", deviceId);
                result.Add(new DeviceConnectionInfo
                {
                    DeviceId = deviceId,
                    Status = "Unknown",
                    ConnectionState = "Unknown",
                    IsConnected = false
                });
            }
        }

        return result;
    }

    public void Dispose()
    {
        _serviceClient?.Dispose();
        _registryManager?.Dispose();
    }
}

/// <summary>
/// Informations sur le Device Twin
/// </summary>
public class DeviceTwinInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? ConnectionState { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public string ReportedProperties { get; set; } = "{}";
    public string DesiredProperties { get; set; } = "{}";
    public long Version { get; set; }
}

/// <summary>
/// Informations de connexion d'un device
/// </summary>
public class DeviceConnectionInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ConnectionState { get; set; } = string.Empty;
    public DateTime? LastActivityTime { get; set; }
    public bool IsConnected { get; set; }
    public long TwinVersion { get; set; }
}
