namespace SmartOfficeAPP.Services.IServices
{
    public interface IIotHubService
    {
        // Envoie une méthode directe à un appareil IoT.
        Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload);
    }
}
