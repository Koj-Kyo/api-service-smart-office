using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;
namespace SmartOfficeAPP.Services.ServicesImpl;

public class TelemetrieService : ITelemetrieService
{
    private readonly ITelemetrieRepository _telemetrieRepository;
    private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
    private readonly ISuggestionRepository _suggestionRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructeur avec injection de dépendances pour les dépôts et le mapper.
    /// </summary>
    public TelemetrieService(ITelemetrieRepository telemetrieRepository,
                             IPeripheriqueLogiqueRepository peripheriqueRepository,
                             ISuggestionRepository suggestionRepository,
                             IMapper mapper)
    {
        _telemetrieRepository = telemetrieRepository;
        _peripheriqueRepository = peripheriqueRepository;
        _suggestionRepository = suggestionRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Ajoute un nouvel enregistrement de télémétrie.
    /// </summary>
    /// <param name="telemetrie">L'objet Telemetrie à ajouter.</param>
    public async Task AddTelemetryAsync(Telemetrie telemetrie)
    {
        await _telemetrieRepository.AddTelemetrieAsync(telemetrie);
    }

    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique spécifique.
    /// </summary>
    /// <param name="peripheriqueId">L'ID du périphérique logique.</param>
    /// <returns>Une collection de DTO de télémétrie.</returns>
    public async Task<IEnumerable<TelemetrieDto>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId)
    {
        var telemetries = await _telemetrieRepository.GetTelemetriesByPeripheriqueIdAsync(peripheriqueId);
        return _mapper.Map<IEnumerable<TelemetrieDto>>(telemetries);
    }

    /// <summary>
    /// Récupère les données historiques de télémétrie dans une plage de dates.
    /// </summary>
    /// <param name="startDate">Date de début de la période.</param>
    /// <param name="endDate">Date de fin de la période.</param>
    /// <param name="peripheriqueId">ID du périphérique (facultatif).</param>
    /// <returns>Une collection d'enregistrements de télémétrie.</returns>
    public async Task<IEnumerable<TelemetrieDto>> GetHistoricalDataAsync(DateTime startDate, DateTime endDate, int? peripheriqueId = null)
    {
        var telemetries = await _telemetrieRepository.GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
        return _mapper.Map<IEnumerable<TelemetrieDto>>(telemetries);
    }

    /// <summary>
    /// Stocke une suggestion générée par le moteur IA.
    /// </summary>
    /// <param name="suggestion">L'objet Suggestion à stocker.</param>
    public async Task StoreAISuggestionAsync(Suggestion suggestion)
    {
        var suggestionModel = _mapper.Map<DTOs.Suggestion>(suggestion);
        await _suggestionRepository.AddAsync(suggestionModel);
    }
}
