using SmartOfficeAPP.DTOs;

namespace SmartOfficeAPP.Services.IServices
{
    public interface IZoneService
    {
        // Récupère toutes les zones.
        Task<IEnumerable<ZoneDto>> GetAllZonesAsync();

        // Récupère une zone par son ID.
        Task<ZoneDto> GetZoneByIdAsync(int id);

        // Crée une nouvelle zone.
        Task<ZoneDto> CreateZoneAsync(ZoneCreationDto zoneDto);

        // Met à jour une zone existante.
        Task<bool> UpdateZoneAsync(int id, ZoneUpdateDto zoneDto);

        // Supprime une zone.
        Task<bool> DeleteZoneAsync(int id);
    }
}
