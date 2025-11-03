using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Enums;
using System.Text.Json;

namespace SmartOfficeAPP.Services.ServicesImpl
{
    public class CommandeService : ICommandeService
    {
        private readonly ICommandeRepository _commandeRepository;
        private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
        private readonly IIotHubService _iotHubService;
        private readonly IDroitAccesRepository _droitAccesRepository;
        private readonly ISignalRService _signalRService;
        private readonly IDistributedCache _cache;
        private readonly IMapper _mapper;
        private readonly ILogger<CommandeService> _logger;

        public CommandeService(
            ICommandeRepository commandeRepository,
            IPeripheriqueLogiqueRepository peripheriqueRepository,
            IIotHubService iotHubService,
            IDroitAccesRepository droitAccesRepository,
            ISignalRService signalRService,
            IDistributedCache cache,
            IMapper mapper,
            ILogger<CommandeService> logger)
        {
            _commandeRepository = commandeRepository;
            _peripheriqueRepository = peripheriqueRepository;
            _iotHubService = iotHubService;
            _droitAccesRepository = droitAccesRepository;
            _signalRService = signalRService;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommandeResponseDto> SendCommandAsync(string userId, CommandeRequestDto requestDto)
        {
            try
            {
                // 1. Chercher le périphérique pour obtenir son IotDeviceId et sa Zone
                var peripherique = await _peripheriqueRepository.GetByIdAsync(requestDto.PeripheriqueLogiqueId);
                if (peripherique == null)
                {
                    return new CommandeResponseDto { IsSuccess = false, Message = "Périphérique introuvable." };
                }

                // 2. ✅ Vérifier les droits: contrôle direct sur le périphérique OU contrôle sur la zone du périphérique
                var droitsPeripheriques = await _droitAccesRepository.GetDroitsAccesByUserPeripheriquesAsync(userId);
                var hasDeviceControl = droitsPeripheriques.Any(d =>
                    d.PeripheriqueLogiqueId == requestDto.PeripheriqueLogiqueId &&
                    d.Type == DroitAccesType.Controle);

                var droitsZones = await _droitAccesRepository.GetDroitsAccesByUserZonesAsync(userId);
                var hasZoneControl = droitsZones.Any(d =>
                    d.ZoneId == peripherique.ZoneId &&
                    d.Type == DroitAccesType.Controle);

                var canControl = hasDeviceControl || hasZoneControl;

                if (!canControl)
                {
                    var errorMessage = "Accès refusé : vous n'avez pas le droit de contrôler ce périphérique.";
                    _logger.LogWarning("Accès refusé (contrôle) user {UserId} → périphérique {PeriphId} (zone {ZoneId}). " +
                        "Droit direct: {HasDevice}, Droit zone: {HasZone}", 
                        userId, requestDto.PeripheriqueLogiqueId, peripherique.ZoneId, hasDeviceControl, hasZoneControl);
                    
                    return new CommandeResponseDto
                    {
                        IsSuccess = false,
                        Message = errorMessage
                    };
                }

                _logger.LogInformation("Contrôle autorisé pour user {UserId} → périphérique {PeriphId} (zone {ZoneId}). " +
                    "Droit direct: {HasDevice}, Droit zone: {HasZone}", 
                    userId, requestDto.PeripheriqueLogiqueId, peripherique.ZoneId, hasDeviceControl, hasZoneControl);

                // 3. Préparer et envoyer la commande via le service IoT Hub
                var commandSuccess = await _iotHubService.InvokeDeviceMethodAsync(
                    peripherique.IotDeviceId,
                    requestDto.NomCommande,
                    requestDto.Valeur
                );

                // 4. ✅ Write-through: Enregistrer la commande dans la base de données
                var commande = new Commande
                {
                    Horodatage = DateTime.UtcNow,
                    NomCommande = requestDto.NomCommande,
                    Valeur = requestDto.Valeur,
                    PeripheriqueLogiqueId = requestDto.PeripheriqueLogiqueId,
                    Statut = commandSuccess ? "Succès" : "Échec",
                    UserId = userId
                };

                await _commandeRepository.AddCommandeAsync(commande);

                // 5. ✅ Write-through: Invalider le cache après écriture en DB
                await InvalidateCommandeCacheAsync(userId, requestDto.PeripheriqueLogiqueId);

                // 6. ✅ Mettre en cache la nouvelle commande
                var commandeDto = _mapper.Map<CommandeResponseDto>(commande);
                await CacheCommandeAsync(commande.Id.ToString(), commandeDto);

                var response = new CommandeResponseDto
                {
                    IsSuccess = commandSuccess,
                    Message = commandSuccess ? "Commande envoyée avec succès." : "Échec de l'envoi de la commande.",
                    CommandeId = commande.Id
                };

                // 7. ✅ SignalR: Envoyer le résultat de la commande
                await _signalRService.SendCommandExecutionResultAsync(
                    commande.Id,
                    requestDto.PeripheriqueLogiqueId,
                    commande.Statut,
                    response.Message
                );

                // 8. ✅ SignalR: Mettre à jour le statut du périphérique si la commande a réussi
                if (commandSuccess)
                {
                    await _signalRService.SendDeviceStatusUpdateAsync(
                        requestDto.PeripheriqueLogiqueId,
                        "command_executed",
                        new { 
                            command = requestDto.NomCommande, 
                            value = requestDto.Valeur, 
                            timestamp = DateTime.UtcNow 
                        }
                    );
                }

                _logger.LogInformation("Commande {CommandName} {Status} pour le périphérique {PeripheriqueId} par l'utilisateur {UserId}", 
                    requestDto.NomCommande, commandSuccess ? "réussie" : "échouée", requestDto.PeripheriqueLogiqueId, userId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la commande {CommandName} pour le périphérique {PeripheriqueId}", 
                    requestDto.NomCommande, requestDto.PeripheriqueLogiqueId);
                
                return new CommandeResponseDto
                {
                    IsSuccess = false,
                    Message = "Erreur interne lors de l'envoi de la commande."
                };
            }
        }

        /// <summary>
        /// ✅ Write-through: Invalide le cache des commandes après une nouvelle commande
        /// </summary>
        private async Task InvalidateCommandeCacheAsync(string userId, int peripheriqueId)
        {
            try
            {
                var cacheKeys = new[]
                {
                    $"commandes:user:{userId}",
                    $"commandes:peripherique:{peripheriqueId}",
                    "commandes:recent",
                    "api-data" // Tag pour OutputCache
                };

                foreach (var key in cacheKeys)
                {
                    await _cache.RemoveAsync(key);
                }

                _logger.LogDebug("Cache invalidé pour les commandes - User:{UserId}, Périphérique:{PeripheriqueId}", userId, peripheriqueId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors de l'invalidation du cache des commandes");
            }
        }

        /// <summary>
        /// ✅ Mise en cache d'une commande
        /// </summary>
        private async Task CacheCommandeAsync(string commandeId, CommandeResponseDto commande)
        {
            try
            {
                var cacheKey = $"commande:{commandeId}";
                var json = JsonSerializer.Serialize(commande);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };

                await _cache.SetStringAsync(cacheKey, json, options);
                _logger.LogDebug("Commande {CommandeId} mise en cache", commandeId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors de la mise en cache de la commande {CommandeId}", commandeId);
            }
        }
    }
}
