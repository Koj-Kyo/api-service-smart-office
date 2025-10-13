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
    private readonly ISignalRService _signalRService;
    private readonly ILogger<TelemetrieService> _logger;

    /// <summary>
    /// Constructeur avec injection de dépendances pour les dépôts et le mapper.
    /// </summary>
    public TelemetrieService(
        ITelemetrieRepository telemetrieRepository,
        IPeripheriqueLogiqueRepository peripheriqueRepository,
        ISuggestionRepository suggestionRepository,
        IMapper mapper,
        ISignalRService signalRService,
        ILogger<TelemetrieService> logger)
    {
        _telemetrieRepository = telemetrieRepository;
        _peripheriqueRepository = peripheriqueRepository;
        _suggestionRepository = suggestionRepository;
        _mapper = mapper;
        _signalRService = signalRService;
        _logger = logger;
    }

    /// <summary>
    /// Ajoute un nouvel enregistrement de télémétrie.
    /// </summary>
    /// <param name="telemetrie">L'objet Telemetrie à ajouter.</param>
    public async Task AddTelemetryAsync(Telemetrie telemetrie)
    {
        try
        {
            await _telemetrieRepository.AddTelemetrieAsync(telemetrie);
            
            // Envoyer une mise à jour en temps réel via SignalR
            var telemetrieDto = _mapper.Map<TelemetrieDto>(telemetrie);
            await _signalRService.SendTelemetryUpdateAsync(telemetrie.PeripheriqueLogiqueId, telemetrieDto);
            
            _logger.LogInformation("Télémétrie ajoutée et diffusée via SignalR pour le périphérique {PeripheriqueId}", telemetrie.PeripheriqueLogiqueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'ajout de télémétrie pour le périphérique {PeripheriqueId}", telemetrie.PeripheriqueLogiqueId);
            throw;
        }
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
        try
        {
            var suggestionDto = _mapper.Map<DTOs.Suggestion>(suggestion);
            await _suggestionRepository.AddAsync(suggestionDto);
            
            _logger.LogInformation("Suggestion IA stockée : {ContenuSuggestion}", suggestion.ContenuSuggestion);
            
            // Optionnel : Envoyer une notification aux utilisateurs concernés
            await _signalRService.SendBroadcastNotificationAsync("Nouvelle suggestion IA", suggestion.ContenuSuggestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du stockage de la suggestion IA");
            throw;
        }
    }
}
