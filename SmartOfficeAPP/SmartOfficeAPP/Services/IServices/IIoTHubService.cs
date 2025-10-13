using SmartOfficeAPP.Services.ServicesImpl;

namespace SmartOfficeAPP.Services.IServices
{
    public interface IIotHubService : IDisposable
    {
        /// <summary>
        /// Envoie une méthode directe à un appareil IoT
        /// </summary>
        Task<bool> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload);

<<<<<<< HEAD
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
=======
        // Met à jour les desired properties (Device Twin) d'un appareil.
        // 'desiredJson' est un fragment JSON qui sera placé sous properties.desired.
        Task<bool> UpdateDesiredPropertiesAsync(string deviceId, string desiredJson);
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
    }
}
