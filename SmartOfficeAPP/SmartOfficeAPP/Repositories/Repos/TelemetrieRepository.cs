using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos
{
    public class TelemetrieRepository : ITelemetrieRepository
    {
        private readonly ApplicationDbContext _context;

        public TelemetrieRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ajoute une nouvelle donnée de télémétrie à la base de données.
        public async Task<Telemetrie> AddTelemetrieAsync(Telemetrie telemetrie)
        {
            await _context.Telemetries.AddAsync(telemetrie);
            await _context.SaveChangesAsync();
            return telemetrie;
        }

        // Récupère les données de télémétrie pour un périphérique donné.
        public async Task<IEnumerable<Telemetrie>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId)
        {
            return await _context.Telemetries
                                 .Where(t => t.PeripheriqueLogiqueId == peripheriqueId)
                                 .OrderByDescending(t => t.Horodatage) // Ordonne par la date la plus récente.
                                 .ToListAsync();
        }
    }
}
