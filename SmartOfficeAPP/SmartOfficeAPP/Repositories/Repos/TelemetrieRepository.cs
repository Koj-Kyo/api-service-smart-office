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
<<<<<<< HEAD
            var query = _context.Telemetries
                                .AsNoTracking()
                                .Where(t => t.Horodatage >= startDate && t.Horodatage <= endDate);

=======
            var query = _context.Telemetries.AsQueryable();
            query = query.Where(t => t.Horodatage >= startDate && t.Horodatage <= endDate);
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
            if (peripheriqueId.HasValue)
            {
                query = query.Where(t => t.PeripheriqueLogiqueId == peripheriqueId.Value);
            }
<<<<<<< HEAD

            return await query
                .OrderByDescending(t => t.Horodatage)
                .ToListAsync();
=======
            return await query.OrderByDescending(t => t.Horodatage).ToListAsync();
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
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
