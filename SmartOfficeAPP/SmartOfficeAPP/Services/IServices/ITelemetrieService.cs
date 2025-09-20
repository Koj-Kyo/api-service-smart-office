using SmartOfficeAPP.DTOs;

namespace SmartOfficeAPP.Services.IServices;

public interface ITelemetrieService
{
    // Récupère les données de télémétrie pour un périphérique spécifique.
    Task<IEnumerable<TelemetrieDto>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId);
}
