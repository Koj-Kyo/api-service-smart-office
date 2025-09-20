using SmartOfficeAPP.Services.IServices;
using Microsoft.Azure.Devices;
namespace SmartOfficeAPP.Services.ServicesImpl;

public class IotHubService : IIotHubService
{
    private readonly ServiceClient _serviceClient;

    // La chaîne de connexion doit être ajoutée dans appsettings.json.
    public IotHubService(IConfiguration configuration)
    {
        var iotHubConnectionString = configuration["IotHubConnectionString"];
        _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
    }

    // Envoie une méthode directe à un appareil IoT et retourne le statut.
    public async Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload)
    {
        var methodInvocation = new CloudToDeviceMethod(methodName) { ResponseTimeout = TimeSpan.FromSeconds(30) };
        methodInvocation.SetPayloadJson(payload);

        try
        {
            var result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation);
            return result.Status == 200; // Vérifie le statut de la réponse de l'appareil.
        }
        catch (Exception ex)
        {
            // Enregistrer l'erreur (via un logger)
            Console.WriteLine($"Erreur lors de l'envoi de la méthode directe : {ex.Message}");
            return false;
        }
    }
}
