namespace SmartOfficeAPP.Services.IServices
{
    public interface IIotHubService
    {
        // Envoie une méthode directe à un appareil IoT.
        Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload);

        // Met à jour les desired properties (Device Twin) d'un appareil.
        // 'desiredJson' est un fragment JSON qui sera placé sous properties.desired.
        Task<bool> UpdateDesiredPropertiesAsync(string deviceId, string desiredJson);
    }
}
