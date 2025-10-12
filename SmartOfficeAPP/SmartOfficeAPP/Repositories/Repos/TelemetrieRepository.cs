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

        // Récupère les données historiques de télémétrie dans une plage de dates pour un périphérique donné. GetHistoricalDataAsync

        public async Task<IEnumerable<Telemetrie>> GetHistoricalDataAsync(DateTime startDate, DateTime endDate, int? peripheriqueId = null)
        {
            var query = _context.Telemetries
                                .AsNoTracking()
                                .Where(t => t.Horodatage >= startDate && t.Horodatage <= endDate);

            if (peripheriqueId.HasValue)
            {
                query = query.Where(t => t.PeripheriqueLogiqueId == peripheriqueId.Value);
            }

            return await query
                .OrderByDescending(t => t.Horodatage)
                .ToListAsync();
        }
        // Récupère les données de télémétrie pour un périphérique donné.
        public async Task<IEnumerable<Telemetrie>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId)
        {
            return await _context.Telemetries
                                 .AsNoTracking()
                                 .Where(t => t.PeripheriqueLogiqueId == peripheriqueId)
                                 .OrderByDescending(t => t.Horodatage) // Ordonne par la date la plus récente.
                                 .ToListAsync();
        }
    }
}
