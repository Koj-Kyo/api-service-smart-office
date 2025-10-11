using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface ITelemetrieRepository
{
    // Enregistre une nouvelle donnée de télémétrie dans la base de données.
    Task<Telemetrie> AddTelemetrieAsync(Telemetrie telemetrie);

    // Récupère les données de télémétrie pour un périphérique donné.
    Task<IEnumerable<Telemetrie>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId);

    // Récupère les données historiques de télémétrie dans une plage de dates pour un périphérique donné. GetHistoricalDataAsync
    Task<IEnumerable<Telemetrie>> GetHistoricalDataAsync(DateTime startDate, DateTime endDate, int? peripheriqueId = null);
}
