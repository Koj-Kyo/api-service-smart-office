using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // L'envoi de commandes est réservé à tout utilisateur connecté
public class CommandesController : ControllerBase
{
    private readonly ICommandeService _commandeService;

    public CommandesController(ICommandeService commandeService)
    {
        _commandeService = commandeService;
    }

    // POST: api/Commandes
    /// <summary>
    /// Envoie une commande à un périphérique IoT.
    /// </summary>
    /// <param name="requestDto">Les informations de la commande à envoyer.</param>
    /// <returns>Le résultat de l'opération d'envoi de commande.</returns>
    [HttpPost]
    public async Task<ActionResult<CommandeResponseDto>> PostCommand([FromBody] CommandeRequestDto requestDto)
    {
        // Récupère l'ID de l'utilisateur à partir du jeton JWT.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new CommandeResponseDto
            {
                IsSuccess = false,
                Message = "Utilisateur non authentifié."
            });
        }

        var response = await _commandeService.SendCommandAsync(userId, requestDto);

        if (!response.IsSuccess)
        {
            // Retourne un BadRequest si la commande a échoué.
            return BadRequest(response);
        }

        // Retourne un Ok si la commande a réussi.
        return Ok(response);
    }
}