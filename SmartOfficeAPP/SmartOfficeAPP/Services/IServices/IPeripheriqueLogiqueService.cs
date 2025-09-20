using SmartOfficeAPP.DTOs;

namespace SmartOfficeAPP.Services.IServices
{
    public interface IPeripheriqueLogiqueService
    {
        Task<IEnumerable<PeripheriqueLogiqueDto>> GetPeripheriquesByZoneIdAsync(int zoneId);
        Task<PeripheriqueLogiqueDto> GetByIdAsync(int id);
        Task<PeripheriqueLogiqueDto> CreateAsync(PeripheriqueLogiqueCreationDto peripheriqueDto);
        Task<bool> UpdateAsync(int id, PeripheriqueLogiqueUpdateDto peripheriqueDto);
        Task<bool> DeleteAsync(int id);
    }
}
