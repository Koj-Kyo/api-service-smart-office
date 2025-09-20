using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Repositories.IRepositories;
using AutoMapper;


namespace SmartOfficeAPP.Services.ServicesImpl;

public class ZoneService : IZoneService
{
    private readonly IZoneRepository _zoneRepository;
    private readonly IMapper _mapper;

    public ZoneService(IZoneRepository zoneRepository, IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ZoneDto>> GetAllZonesAsync()
    {
        var zones = await _zoneRepository.GetAllZonesAsync();
        return _mapper.Map<IEnumerable<ZoneDto>>(zones);
    }

    public async Task<ZoneDto> GetZoneByIdAsync(int id)
    {
        var zone = await _zoneRepository.GetZoneByIdAsync(id);
        if (zone == null)
        {
            return null;
        }
        return _mapper.Map<ZoneDto>(zone);
    }

    public async Task<ZoneDto> CreateZoneAsync(ZoneCreationDto zoneDto)
    {
        var zone = _mapper.Map<Models.Zone>(zoneDto);
        var createdZone = await _zoneRepository.AddZoneAsync(zone);
        return _mapper.Map<ZoneDto>(createdZone);
    }

    public async Task<bool> UpdateZoneAsync(int id, ZoneUpdateDto zoneDto)
    {
        var zone = await _zoneRepository.GetZoneByIdAsync(id);
        if (zone == null)
        {
            return false;
        }
        _mapper.Map(zoneDto, zone);
        return await _zoneRepository.UpdateZoneAsync(zone);
    }

    public async Task<bool> DeleteZoneAsync(int id)
    {
        var zone = await _zoneRepository.GetZoneByIdAsync(id);
        if (zone == null)
        {
            return false;
        }
        return await _zoneRepository.DeleteZoneAsync(zone);
    }
}
