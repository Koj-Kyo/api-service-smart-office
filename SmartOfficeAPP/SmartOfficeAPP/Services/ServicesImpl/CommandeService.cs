using AutoMapper;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Enums;

namespace SmartOfficeAPP.Services.ServicesImpl
{
    public class CommandeService : ICommandeService
    {
        private readonly ICommandeRepository _commandeRepository;
        private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
        private readonly IIotHubService _iotHubService;
        private readonly IDroitAccesRepository _droitAccesRepository;
<<<<<<< HEAD
        private readonly ISignalRService _signalRService;
=======
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
        private readonly IMapper _mapper;
        private readonly ILogger<CommandeService> _logger;

        public CommandeService(
            ICommandeRepository commandeRepository,
            IPeripheriqueLogiqueRepository peripheriqueRepository,
            IIotHubService iotHubService,
            IDroitAccesRepository droitAccesRepository,
<<<<<<< HEAD
            ISignalRService signalRService,
            IMapper mapper,
            ILogger<CommandeService> logger)
=======
            IMapper mapper)
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
        {
            _commandeRepository = commandeRepository;
            _peripheriqueRepository = peripheriqueRepository;
            _iotHubService = iotHubService;
            _droitAccesRepository = droitAccesRepository;
<<<<<<< HEAD
            _signalRService = signalRService;
=======
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CommandeResponseDto> SendCommandAsync(string userId, CommandeRequestDto requestDto)
        {
<<<<<<< HEAD
            try
=======
            // 0. Vérifier les droits de l'utilisateur sur le périphérique (doit avoir "Controle")
            var droits = await _droitAccesRepository.GetDroitsAccesByUserPeripheriquesAsync(userId);
            var canControl = droits.Any(d =>
                d.PeripheriqueLogiqueId == requestDto.PeripheriqueLogiqueId &&
                d.Type == DroitAccesType.Controle);

            if (!canControl)
            {
                return new CommandeResponseDto
                {
                    IsSuccess = false,
                    Message = "Accès refusé : vous n'avez pas le droit de contrôler ce périphérique."
                };
            }

            // 1. Chercher le périphérique pour obtenir son IotDeviceId
            var peripherique = await _peripheriqueRepository.GetByIdAsync(requestDto.PeripheriqueLogiqueId);
            if (peripherique == null)
>>>>>>> ff4d8ffaaee34962fcb64b6be9f90c74f9ba03e0
            {
                // 0. Vérifier les droits de l'utilisateur sur le périphérique (doit avoir "Controle")
                var droits = await _droitAccesRepository.GetDroitsAccesByUserPeripheriquesAsync(userId);
                var canControl = droits.Any(d =>
                    d.PeripheriqueLogiqueId == requestDto.PeripheriqueLogiqueId &&
                    d.Type == DroitAccesType.Controle);

                if (!canControl)
                {
                    var errorMessage = "Accès refusé : vous n'avez pas le droit de contrôler ce périphérique.";
                    _logger.LogWarning("Tentative de commande refusée pour l'utilisateur {UserId} sur le périphérique {PeripheriqueId}", userId, requestDto.PeripheriqueLogiqueId);
                    
                    return new CommandeResponseDto
                    {
                        IsSuccess = false,
                        Message = errorMessage
                    };
                }

                // 1. Chercher le périphérique pour obtenir son IotDeviceId
                var peripherique = await _peripheriqueRepository.GetByIdAsync(requestDto.PeripheriqueLogiqueId);
                if (peripherique == null)
                {
                    return new CommandeResponseDto { IsSuccess = false, Message = "Périphérique introuvable." };
                }

                // 2. Préparer et envoyer la commande via le service IoT Hub
                var commandSuccess = await _iotHubService.InvokeDeviceMethodAsync(
                    peripherique.IotDeviceId,
                    requestDto.NomCommande,
                    requestDto.Valeur
                );

                // 3. Enregistrer la commande dans la base de données
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

                var response = new CommandeResponseDto
                {
                    IsSuccess = commandSuccess,
                    Message = commandSuccess ? "Commande envoyée avec succès." : "Échec de l'envoi de la commande.",
                    CommandeId = commande.Id
                };

                // 4. Envoyer le résultat de la commande via SignalR
                await _signalRService.SendCommandExecutionResultAsync(
                    commande.Id,
                    requestDto.PeripheriqueLogiqueId,
                    commande.Statut,
                    response.Message
                );

                // 5. Mettre à jour le statut du périphérique si la commande a réussi
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
    }
}
