using SmartOfficeAPP.Services.IServices;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared; // pour Twin si besoin
using System.Text;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class IotHubService : IIotHubService
{
    private readonly ServiceClient _serviceClient;
    private readonly RegistryManager _registryManager;

    // La chaîne de connexion doit être ajoutée dans appsettings.json (IotHubConnectionString).
    public IotHubService(IConfiguration configuration)
    {
        var iotHubConnectionString = configuration["IotHubConnectionString"];
        _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        _registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
    }

    // Envoie une méthode directe à un appareil IoT et retourne le statut.
    public async Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload)
    {
        var methodInvocation = new CloudToDeviceMethod(methodName) { ResponseTimeout = TimeSpan.FromSeconds(30) };
        methodInvocation.SetPayloadJson(payload);

        try
        {
            var result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
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
            return false;
        }
    }
}
