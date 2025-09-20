using AutoMapper;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl
{
    public class CommandeService : ICommandeService
    {
        private readonly ICommandeRepository _commandeRepository;
        private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
        private readonly IIotHubService _iotHubService;
        private readonly IMapper _mapper;

        public CommandeService(ICommandeRepository commandeRepository, IPeripheriqueLogiqueRepository peripheriqueRepository, IIotHubService iotHubService, IMapper mapper)
        {
            _commandeRepository = commandeRepository;
            _peripheriqueRepository = peripheriqueRepository;
            _iotHubService = iotHubService;
            _mapper = mapper;
        }

        public async Task<CommandeResponseDto> SendCommandAsync(string userId, CommandeRequestDto requestDto)
        {
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

            return new CommandeResponseDto
            {
                IsSuccess = commandSuccess,
                Message = commandSuccess ? "Commande envoyée avec succès." : "Échec de l'envoi de la commande.",
                CommandeId = commande.Id
            };
        }
    }
}
