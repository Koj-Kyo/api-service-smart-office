using SmartOfficeAPP.Services.ServicesImpl;

namespace SmartOfficeAPP.Services.IServices
{
    public interface IIotHubService : IDisposable
    {
        /// <summary>
        /// Envoie une méthode directe à un appareil IoT
        /// </summary>
        Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload);

        /// <summary>
        /// Met à jour les desired properties (Device Twin) d'un appareil
        /// </summary>
        Task<bool> UpdateDesiredPropertiesAsync(string deviceId, string desiredJson);

        /// <summary>
        /// Récupère les informations du Device Twin
        /// </summary>
        Task<DeviceTwinInfo?> GetDeviceTwinAsync(string deviceId);

        /// <summary>
        /// Envoie un message Cloud-to-Device
        /// </summary>
        Task<bool> SendCloudToDeviceMessageAsync(string deviceId, string message, Dictionary<string, string>? properties = null);

        /// <summary>
        /// Crée ou met à jour un device dans IoT Hub
        /// </summary>
        Task<bool> CreateOrUpdateDeviceAsync(string deviceId, bool enabled = true);

        /// <summary>
        /// Supprime un device d'IoT Hub
        /// </summary>
        Task<bool> DeleteDeviceAsync(string deviceId);

        /// <summary>
        /// Obtient le statut de connexion de plusieurs devices
        /// </summary>
        Task<IEnumerable<DeviceConnectionInfo>> GetDevicesConnectionStatusAsync(IEnumerable<string> deviceIds);

        /// <summary>
        /// Liste les appareils IoT Hub
        /// </summary>
        Task<IEnumerable<IoTHubDeviceSummary>> ListDevicesAsync(int maxCount = 100);
    }
}
