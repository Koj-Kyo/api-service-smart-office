using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface IPeripheriqueLogiqueRepository
{
    Task<PeripheriqueLogique> GetByIdAsync(int id);
    Task<IEnumerable<PeripheriqueLogique>> GetAllAsync();
    Task AddAsync(PeripheriqueLogique peripherique);
    Task<bool> UpdateAsync(PeripheriqueLogique peripherique);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PeripheriqueLogique>> GetByZoneIdAsync(int zoneId);
}
