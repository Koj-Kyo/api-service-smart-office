using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Services.IServices;

public interface IDroitAccesService
{
    // Récupère tous les droits d'accès.
    Task<IEnumerable<DroitAccesDto>> GetDroitsAccesAsync();

    Task<DroitAccesDto> GetDroitAccesByIdAsync(int id);

    // Récupère les droits d'accès d'un utilisateur sur les zones.
    Task<IEnumerable<DroitAccesDto>> GetUserZoneAccessRightsAsync(string userId);

    // Récupère les droits d'accès d'un utilisateur sur les périphériques.
    Task<IEnumerable<DroitAccesDto>> GetUserPeripheriqueAccessRightsAsync(string userId);

    // Ajoute un nouveau droit d'accès.
    Task<DroitAccesDto> AddDroitAccesAsync(DroitAccesCreationDto droitAccesDto);

    // Met à jour un droit d'accès existant.
    Task<bool> UpdateDroitAccesAsync(int id, DroitAccesCreationDto droitAccesDto);

    // Supprime un droit d'accès.
    Task<bool> DeleteDroitAccesAsync(int id);
}
