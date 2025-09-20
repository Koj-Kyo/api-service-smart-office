using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class TelemetrieService : ITelemetrieService
{
    private readonly ITelemetrieRepository _telemetrieRepository;
    private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
    private readonly IMapper _mapper;

    public TelemetrieService(ITelemetrieRepository telemetrieRepository, IPeripheriqueLogiqueRepository peripheriqueRepository, IMapper mapper)
    {
        _telemetrieRepository = telemetrieRepository;
        _peripheriqueRepository = peripheriqueRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TelemetrieDto>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId)
    {
        var peripherique = await _peripheriqueRepository.GetByIdAsync(peripheriqueId);
        if (peripherique == null)
        {
            return null;
        }

        var telemetries = await _telemetrieRepository.GetTelemetriesByPeripheriqueIdAsync(peripheriqueId);
        return _mapper.Map<IEnumerable<TelemetrieDto>>(telemetries);
    }
}
