using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos
{
    public class ZoneRepository : IZoneRepository
    {
        private readonly ApplicationDbContext _context;

        public ZoneRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Zone>> GetAllZonesAsync()
        {
            return await _context.Zones.ToListAsync();
        }

        public async Task<Zone> GetZoneByIdAsync(int id)
        {
            return await _context.Zones.FindAsync(id);
        }

        public async Task<Zone> AddZoneAsync(Zone zone)
        {
            await _context.Zones.AddAsync(zone);
            await _context.SaveChangesAsync();
            return zone;
        }

        public async Task<bool> UpdateZoneAsync(Zone zone)
        {
            _context.Zones.Update(zone);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteZoneAsync(Zone zone)
        {
            _context.Zones.Remove(zone);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
