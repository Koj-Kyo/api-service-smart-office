using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Enums;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;


namespace SmartOfficeAPP.Services.ServicesImpl;

public class DroitAccesService : IDroitAccesService
{
    private readonly IDroitAccesRepository _droitAccesRepository;

    public DroitAccesService(IDroitAccesRepository droitAccesRepository)
    {
        _droitAccesRepository = droitAccesRepository;
    }

    // Récupère tous les droits d'accès et les mappe vers un DTO.
    public async Task<IEnumerable<DroitAccesDto>> GetDroitsAccesAsync()
    {
        var droitsAcces = await _droitAccesRepository.GetDroitsAccesAsync();
        return droitsAcces.Select(da => MapToDto(da));
    }
    // Récupère un droit d'accès par son ID et le mappe vers un DTO.
    public async Task<DroitAccesDto> GetDroitAccesByIdAsync(int id)
    {
        var droitAcces = await _droitAccesRepository.GetDroitAccesByIdAsync(id);
        return droitAcces == null ? null : MapToDto(droitAcces);
    }

    // Récupère les droits d'accès d'un utilisateur sur les zones.
    public async Task<IEnumerable<DroitAccesDto>> GetUserZoneAccessRightsAsync(string userId)
    {
        var droitsAcces = await _droitAccesRepository.GetDroitsAccesByUserZonesAsync(userId);
        return droitsAcces.Select(da => MapToDto(da));
    }

    // Récupère les droits d'accès d'un utilisateur sur les périphériques.
    public async Task<IEnumerable<DroitAccesDto>> GetUserPeripheriqueAccessRightsAsync(string userId)
    {
        var droitsAcces = await _droitAccesRepository.GetDroitsAccesByUserPeripheriquesAsync(userId);
        return droitsAcces.Select(da => MapToDto(da));
    }

    // Ajoute un nouveau droit d'accès et le retourne sous forme de DTO.
    public async Task<DroitAccesDto> AddDroitAccesAsync(DroitAccesCreationDto droitAccesDto)
    {
        var droitAcces = new DroitAcces
        {
            UserId = droitAccesDto.UserId,
            ZoneId = droitAccesDto.ZoneId,
            PeripheriqueLogiqueId = droitAccesDto.PeripheriqueLogiqueId,
            Type = (DroitAccesType)Enum.Parse(typeof(DroitAccesType), droitAccesDto.Type, true)
        };
        var newDroitAcces = await _droitAccesRepository.AddDroitAccesAsync(droitAcces);
        return MapToDto(newDroitAcces);
    }

    // Met à jour un droit d'accès existant.
    public async Task<bool> UpdateDroitAccesAsync(int id, DroitAccesCreationDto droitAccesDto)
    {
        var existingDroitAcces = await _droitAccesRepository.GetDroitAccesByIdAsync(id);
        if (existingDroitAcces == null)
        {
            return false;
        }

        existingDroitAcces.UserId = droitAccesDto.UserId;
        existingDroitAcces.ZoneId = droitAccesDto.ZoneId;
        existingDroitAcces.PeripheriqueLogiqueId = droitAccesDto.PeripheriqueLogiqueId;
        existingDroitAcces.Type = (DroitAccesType)Enum.Parse(typeof(DroitAccesType),droitAccesDto.Type, true);

        return await _droitAccesRepository.UpdateDroitAccesAsync(existingDroitAcces);
    }

    // Supprime un droit d'accès.
    public async Task<bool> DeleteDroitAccesAsync(int id)
    {
        return await _droitAccesRepository.DeleteDroitAccesAsync(id);
    }

    // Méthode utilitaire pour mapper une entité DroitAcces vers un DTO.
    private DroitAccesDto MapToDto(DroitAcces droitAcces)
    {
        return new DroitAccesDto
        {
            Id = droitAcces.Id,
            UserId = droitAcces.UserId,
            ZoneId = droitAcces.ZoneId,
            ZoneName = droitAcces.Zone?.Name,
            PeripheriqueLogiqueId = droitAcces.PeripheriqueLogiqueId,
            PeripheriqueLogiqueName = droitAcces.PeripheriqueLogique?.Name,
            Type = droitAcces.Type.ToString()
        };
    }
}
