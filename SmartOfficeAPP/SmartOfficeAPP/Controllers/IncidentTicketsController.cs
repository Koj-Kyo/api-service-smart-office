using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.Services.IServices;
using Microsoft.AspNetCore.Authorization;

namespace SmartOfficeAPP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentTicketsController : ControllerBase
{
    private readonly IIncidentTicketService _incidentTicketService;

    public IncidentTicketsController(IIncidentTicketService incidentTicketService)
    {
        _incidentTicketService = incidentTicketService;
    }

    /// <summary>
    /// Crée un nouveau ticket d'incident.
    /// </summary>
    /// <param name="ticketDto">Les données du ticket à créer.</param>
    /// <returns>Le ticket créé avec son ID.</returns>
    [HttpPost]
    // [Authorize] // Ajouter cet attribut pour sécuriser la méthode
    public async Task<IActionResult> CreateTicket([FromBody] IncidentTicketCreateDto ticketDto)
    {
        // Note: L'ID de l'utilisateur qui crée le ticket doit être récupéré
        // du contexte de l'utilisateur authentifié.
        var createdById = "current-user-id"; // Remplacer par l'ID réel de l'utilisateur
        var createdTicket = await _incidentTicketService.CreateTicketAsync(ticketDto, createdById);

        return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, createdTicket);
    }

    /// <summary>
    /// Récupère un ticket d'incident par son ID.
    /// </summary>
    /// <param name="id">L'ID du ticket.</param>
    /// <returns>Le ticket d'incident correspondant.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicketById(int id)
    {
        var ticket = await _incidentTicketService.GetTicketByIdAsync(id);
        if (ticket == null)
        {
            return NotFound();
        }
        return Ok(ticket);
    }

    /// <summary>
    /// Récupère tous les tickets avec un statut spécifique.
    /// </summary>
    /// <param name="status">Le statut des tickets (par exemple, "Ouvert", "En cours", "Fermé").</param>
    /// <returns>Une liste de tickets d'incident.</returns>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetTicketsByStatus(string status)
    {
        var tickets = await _incidentTicketService.GetTicketsByStatusAsync(status);
        return Ok(tickets);
    }

    /// <summary>
    /// Récupère tous les tickets créés ou assignés à un utilisateur.
    /// </summary>
    /// <param name="userId">L'ID de l'utilisateur.</param>
    /// <returns>Une liste de tickets d'incident.</returns>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetTicketsByUserId(string userId)
    {
        var tickets = await _incidentTicketService.GetTicketsByUserIdAsync(userId);
        return Ok(tickets);
    }

    /// <summary>
    /// Met à jour le statut d'un ticket d'incident.
    /// </summary>
    /// <param name="id">L'ID du ticket.</param>
    /// <param name="newStatus">Le nouveau statut.</param>
    /// <returns>Statut de la mise à jour.</returns>
    [HttpPut("{id}/status")]
    // [Authorize] // Ajouter cet attribut pour sécuriser la méthode
    public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] string newStatus)
    {
        var result = await _incidentTicketService.UpdateTicketStatusAsync(id, newStatus);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Assigne un ticket à un autre utilisateur.
    /// </summary>
    /// <param name="id">L'ID du ticket.</param>
    /// <param name="assignedToId">L'ID de l'utilisateur à qui le ticket est assigné.</param>
    /// <returns>Statut de l'assignation.</returns>
    [HttpPut("{id}/assign")]
    // [Authorize(Roles = "Manager, Admin")] // Exemple: Restreindre l'accès à certains rôles
    public async Task<IActionResult> AssignTicket(int id, [FromBody] string assignedToId)
    {
        var result = await _incidentTicketService.AssignTicketAsync(id, assignedToId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Supprime un ticket d'incident.
    /// </summary>
    /// <param name="id">L'ID du ticket à supprimer.</param>
    /// <returns>Statut de la suppression.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] 
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var result = await _incidentTicketService.DeleteTicketAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
