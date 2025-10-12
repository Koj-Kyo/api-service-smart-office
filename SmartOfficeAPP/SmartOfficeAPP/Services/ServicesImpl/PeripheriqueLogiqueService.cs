using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl
{
    public class PeripheriqueLogiqueService : IPeripheriqueLogiqueService
    {
        private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
        private readonly IMapper _mapper;

        public PeripheriqueLogiqueService(IPeripheriqueLogiqueRepository peripheriqueRepository, IMapper mapper)
        {
            _peripheriqueRepository = peripheriqueRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PeripheriqueLogiqueDto>> GetPeripheriquesByZoneIdAsync(int zoneId)
        {
            var peripheriques = await _peripheriqueRepository.GetByZoneIdAsync(zoneId);
            return _mapper.Map<IEnumerable<PeripheriqueLogiqueDto>>(peripheriques);
        }

        public async Task<IEnumerable<PeripheriqueLogiqueDto>> GetAllAsync()
        {
            var peripheriques = await _peripheriqueRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PeripheriqueLogiqueDto>>(peripheriques);
        }

        public async Task<PeripheriqueLogiqueDto?> GetByIdAsync(int id)
        {
            var peripherique = await _peripheriqueRepository.GetByIdAsync(id);
            return _mapper.Map<PeripheriqueLogiqueDto>(peripherique);
        }

        public async Task<PeripheriqueLogiqueDto> CreateAsync(PeripheriqueLogiqueCreationDto peripheriqueDto)
        {
            var peripherique = _mapper.Map<PeripheriqueLogique>(peripheriqueDto);
            await _peripheriqueRepository.AddAsync(peripherique);
            return _mapper.Map<PeripheriqueLogiqueDto>(peripherique);
        }

        public async Task<bool> UpdateAsync(int id, PeripheriqueLogiqueUpdateDto peripheriqueDto)
        {
            var peripherique = await _peripheriqueRepository.GetByIdAsync(id);
            if (peripherique == null) return false;

            _mapper.Map(peripheriqueDto, peripherique);
            return await _peripheriqueRepository.UpdateAsync(peripherique);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _peripheriqueRepository.DeleteAsync(id);
        }
    }
}
