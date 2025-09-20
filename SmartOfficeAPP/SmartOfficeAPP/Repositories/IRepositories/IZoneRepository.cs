using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories
{
    public interface IZoneRepository
    {
        Task<IEnumerable<Zone>> GetAllZonesAsync();
        Task<Zone> GetZoneByIdAsync(int id);
        Task<Zone> AddZoneAsync(Zone zone);
        Task<bool> UpdateZoneAsync(Zone zone);
        Task<bool> DeleteZoneAsync(Zone zone);
    }
}
