using SmartOfficeAPP.Services.IServices;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class IotHubService : IIotHubService, IDisposable
{
    private readonly ServiceClient _serviceClient;
    private readonly RegistryManager _registryManager;
    private readonly ILogger<IotHubService> _logger;
    private readonly ISignalRService _signalRService;

    public IotHubService(
        IConfiguration configuration, 
        ILogger<IotHubService> logger,
        ISignalRService signalRService)
    {
        _logger = logger;
        _signalRService = signalRService;
        
        var iotHubConnectionString = configuration["IotHubConnectionString"];
        
        if (string.IsNullOrWhiteSpace(iotHubConnectionString))
        {
            _logger.LogError("IotHubConnectionString n'est pas configuré. Veuillez configurer la chaîne de connexion dans les User Secrets.");
            throw new InvalidOperationException("IotHubConnectionString est requis. Configurez-le dans les User Secrets avec : dotnet user-secrets set \"IotHubConnectionString\" \"votre-chaine-de-connexion\"");
        }

        try
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
            _registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
            _logger.LogInformation("Service IoT Hub initialisé avec succès avec Azure Cloud");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'initialisation du service IoT Hub. Vérifiez votre chaîne de connexion.");
            throw;
        }
    }

    /// <summary>
    /// Envoie une méthode directe à un appareil IoT
    /// </summary>
    public async Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogError("DeviceId ne peut pas être null ou vide");
            return false;
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            _logger.LogError("MethodName ne peut pas être null ou vide");
            return false;
        }

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
            if (result == null)
            {
                _logger.LogError("Résultat null reçu d'Azure IoT Hub pour la méthode {MethodName} sur {DeviceId}", methodName, deviceId);
                return false;
            }

            var success = result.Status is >= 200 and < 300; // Accepte 2xx
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
            _logger.LogError("Device {DeviceId} non trouvé dans Azure IoT Hub", deviceId);
            return false;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Argument null lors de l'invocation de la méthode {MethodName} sur {DeviceId}", methodName, deviceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'invocation de la méthode {MethodName} sur {DeviceId}", methodName, deviceId);
            return false;
        }
    }

    /// <summary>
    /// Met à jour les desired properties du Device Twin
    /// </summary>
    public async Task<bool> UpdateDesiredPropertiesAsync(string deviceId, string desiredJson)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogError("DeviceId ne peut pas être null ou vide");
            return false;
        }

        if (string.IsNullOrWhiteSpace(desiredJson))
        {
            _logger.LogError("DesiredJson ne peut pas être null ou vide");
            return false;
        }

        try
        {
            var twin = await _registryManager.GetTwinAsync(deviceId);
            
            if (twin == null)
            {
                _logger.LogError("Twin null reçu pour le device {DeviceId}", deviceId);
                return false;
            }

            // ✅ CONVERSION RÉCURSIVE EN CAMELCASE
            var jsonNode = JsonNode.Parse(desiredJson);
            if (jsonNode != null)
            {
                ConvertToCamelCase(jsonNode);
            }
            
            var camelCaseJson = jsonNode?.ToJsonString(new JsonSerializerOptions 
            { 
                WriteIndented = false 
            }) ?? desiredJson;
            
            var patch = $"{{ \"properties\": {{ \"desired\": {camelCaseJson} }} }}";
            
            await _registryManager.UpdateTwinAsync(deviceId, patch, twin.ETag);
            
            _logger.LogInformation("Desired properties mises à jour pour {DeviceId} (camelCase): {Properties}", deviceId, camelCaseJson);
            
            return true;
        }
        catch (DeviceNotFoundException)
        {
            _logger.LogError("Device {DeviceId} non trouvé dans Azure IoT Hub pour mise à jour des desired properties", deviceId);
            return false;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Argument null lors de la mise à jour des desired properties pour {DeviceId}", deviceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour des desired properties pour {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Convertit récursivement toutes les clés JSON en camelCase
    /// </summary>
    private static void ConvertToCamelCase(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var keysToRename = obj.Select(kvp => kvp.Key).ToList();
            
            foreach (var key in keysToRename)
            {
                var camelCaseKey = ToCamelCase(key);
                
                if (key != camelCaseKey)
                {
                    var value = obj[key];
                    obj.Remove(key);
                    
                    if (value != null)
                    {
                        // Conversion récursive des enfants
                        ConvertToCamelCase(value);
                        obj[camelCaseKey] = value;
                    }
                }
                else if (obj[key] != null)
                {
                    // Même si la clé est déjà en camelCase, convertir les enfants
                    ConvertToCamelCase(obj[key]!);
                }
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item != null)
                {
                    ConvertToCamelCase(item);
                }
            }
        }
    }

    /// <summary>
    /// Convertit une chaîne en camelCase
    /// </summary>
    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str.Substring(1);
    }

    /// <summary>
    /// Récupère les propriétés (reported et desired) d'un device twin
    /// </summary>
    public async Task<DeviceTwinInfo?> GetDeviceTwinAsync(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogError("DeviceId ne peut pas être null ou vide");
            return null;
        }

        try
        {
            var twin = await _registryManager.GetTwinAsync(deviceId);
            
            if (twin == null)
            {
                _logger.LogError("Twin null reçu pour le device {DeviceId}", deviceId);
                return null;
            }

            return new DeviceTwinInfo
            {
                DeviceId = deviceId,
                Status = twin.Status?.ToString(),
                ConnectionState = twin.ConnectionState?.ToString(),
                LastActivityTime = twin.LastActivityTime,
                ReportedProperties = twin.Properties?.Reported?.ToJson() ?? "{}",
                DesiredProperties = twin.Properties?.Desired?.ToJson() ?? "{}",
                Version = twin.Version ?? 0
            };
        }
        catch (DeviceNotFoundException)
        {
            _logger.LogError("Device {DeviceId} non trouvé dans Azure IoT Hub", deviceId);
            return null;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Argument null lors de la récupération du twin pour {DeviceId}", deviceId);
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
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogError("DeviceId ne peut pas être null ou vide");
            return false;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            _logger.LogError("Message ne peut pas être null ou vide");
            return false;
        }

        try
        {
            var commandMessage = new Message(Encoding.UTF8.GetBytes(message));
            
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    if (prop.Key != null && prop.Value != null)
                    {
                        commandMessage.Properties.Add(prop.Key, prop.Value);
                    }
                }
            }

            await _serviceClient.SendAsync(deviceId, commandMessage);
            
            _logger.LogInformation("Message Cloud-to-Device envoyé à {DeviceId}: {Message}", deviceId, message);
            
            return true;
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Argument null lors de l'envoi du message Cloud-to-Device à {DeviceId}", deviceId);
            return false;
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
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogError("DeviceId ne peut pas être null ou vide");
            return false;
        }

        try
        {
            var device = new Device(deviceId)
            {
                Status = enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled
            };

            await _registryManager.AddDeviceAsync(device);
            _logger.LogInformation("Device {DeviceId} créé avec succès dans Azure IoT Hub", deviceId);
            
            return true;
        }
        catch (DeviceAlreadyExistsException)
        {
            // Device existe déjà, on le met à jour si nécessaire
            try
            {
                var existingDevice = await _registryManager.GetDeviceAsync(deviceId);
                
                if (existingDevice == null)
                {
                    _logger.LogError("Device existant null pour {DeviceId}", deviceId);
                    return false;
                }

                if (existingDevice.Status != (enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled))
                {
                    existingDevice.Status = enabled ? DeviceStatus.Enabled : DeviceStatus.Disabled;
                    await _registryManager.UpdateDeviceAsync(existingDevice);
                    _logger.LogInformation("Device {DeviceId} mis à jour dans Azure IoT Hub", deviceId);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour du device {DeviceId}", deviceId);
                return false;
            }
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Argument null lors de la création du device {DeviceId}", deviceId);
            return false;
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
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            _logger.LogError("DeviceId ne peut pas être null ou vide");
            return false;
        }

        try
        {
            await _registryManager.RemoveDeviceAsync(deviceId);
            _logger.LogInformation("Device {DeviceId} supprimé avec succès d'Azure IoT Hub", deviceId);
            return true;
        }
        catch (DeviceNotFoundException)
        {
            _logger.LogWarning("Device {DeviceId} non trouvé dans Azure IoT Hub pour suppression", deviceId);
            return true; // Considéré comme succès si déjà supprimé
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex, "Argument null lors de la suppression du device {DeviceId}", deviceId);
            return false;
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

        if (deviceIds == null)
        {
            _logger.LogError("DeviceIds ne peut pas être null");
            return result;
        }

        foreach (var deviceId in deviceIds)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                _logger.LogWarning("DeviceId null ou vide ignoré dans la liste");
                continue;
            }

            try
            {
                var device = await _registryManager.GetDeviceAsync(deviceId);
                var twin = await _registryManager.GetTwinAsync(deviceId);

                if (device != null)
                {
                    result.Add(new DeviceConnectionInfo
                    {
                        DeviceId = deviceId,
                        Status = device.Status.ToString(),
                        ConnectionState = device.ConnectionState.ToString(),
                        LastActivityTime = device.LastActivityTime,
                        IsConnected = device.ConnectionState == DeviceConnectionState.Connected,
                        TwinVersion = twin?.Version ?? 0
                    });
                }
                else
                {
                    _logger.LogWarning("Device null reçu pour {DeviceId}", deviceId);
                    result.Add(new DeviceConnectionInfo
                    {
                        DeviceId = deviceId,
                        Status = "Unknown",
                        ConnectionState = "Unknown",
                        IsConnected = false
                    });
                }
            }
            catch (DeviceNotFoundException)
            {
                _logger.LogWarning("Device {DeviceId} non trouvé dans Azure IoT Hub", deviceId);
                result.Add(new DeviceConnectionInfo
                {
                    DeviceId = deviceId,
                    Status = "NotFound",
                    ConnectionState = "Disconnected",
                    IsConnected = false
                });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Argument null pour le device {DeviceId}", deviceId);
                result.Add(new DeviceConnectionInfo
                {
                    DeviceId = deviceId,
                    Status = "Error",
                    ConnectionState = "Unknown",
                    IsConnected = false
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

    /// <summary>
    /// Liste tous les devices disponibles dans Azure IoT Hub
    /// </summary>
    public async Task<IEnumerable<IoTHubDeviceSummary>> ListDevicesAsync(int maxCount = 100)
    {
        var capped = Math.Clamp(maxCount, 1, 1000);
        try
        {
            var devices = await _registryManager.GetDevicesAsync(capped);
            return devices.Select(d => new IoTHubDeviceSummary
            {
                DeviceId = d.Id,
                Status = d.Status.ToString(),
                ConnectionState = d.ConnectionState.ToString(),
                LastActivityTime = d.LastActivityTime,
                StatusUpdatedTime = d.StatusUpdatedTime,
                ConnectionStateUpdatedTime = d.ConnectionStateUpdatedTime
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la liste des devices IoT Hub");
            return Enumerable.Empty<IoTHubDeviceSummary>();
        }
    }

    public void Dispose()
    {
        try
        {
            _serviceClient?.Dispose();
            _registryManager?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la libération des ressources IoT Hub");
        }
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

/// <summary>
/// Résumé d'un device IoT Hub
/// </summary>
public class IoTHubDeviceSummary
{
    public string DeviceId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string ConnectionState { get; set; } = string.Empty;
    public DateTime? LastActivityTime { get; set; }
    public DateTime? StatusUpdatedTime { get; set; }
    public DateTime? ConnectionStateUpdatedTime { get; set; }
}
