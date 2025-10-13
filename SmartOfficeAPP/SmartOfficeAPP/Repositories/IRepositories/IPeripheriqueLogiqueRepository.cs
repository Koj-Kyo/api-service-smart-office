using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface IPeripheriqueLogiqueRepository
{
    Task<PeripheriqueLogique?> GetByIdAsync(int id);
    Task<IEnumerable<PeripheriqueLogique>> GetAllAsync();
    Task AddAsync(PeripheriqueLogique peripherique);
    Task<bool> UpdateAsync(PeripheriqueLogique peripherique);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PeripheriqueLogique>> GetByZoneIdAsync(int zoneId);
    Task<PeripheriqueLogique?> GetByIotDeviceIdAndNameAsync(string iotDeviceId, string name);
    
    /// <summary>
    /// Récupère un périphérique par son ID IoT Hub
    /// </summary>
    Task<PeripheriqueLogique?> GetByIotDeviceIdAsync(string iotDeviceId);
    
    /// <summary>
    /// Récupère tous les périphériques avec leurs données de zone
    /// </summary>
    Task<IEnumerable<PeripheriqueLogique>> GetAllWithZoneAsync();
}
