namespace SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;

public interface ICommandeService
{
    // Envoie une commande à un périphérique et l'enregistre dans la base de données.
    Task<CommandeResponseDto> SendCommandAsync(string userId, CommandeRequestDto requestDto);
}
