using AutoMapper;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Data;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class IncidentTicketService : IIncidentTicketService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISignalRService _signalRService;
    private readonly ILogger<IncidentTicketService> _logger;

    public IncidentTicketService(
        ApplicationDbContext context, 
        IMapper mapper,
        ISignalRService signalRService,
        ILogger<IncidentTicketService> logger)
    {
        _context = context;
        _mapper = mapper;
        _signalRService = signalRService;
        _logger = logger;
    }

    public async Task<IncidentTicketDto> CreateTicketAsync(IncidentTicketCreateDto ticketDto, string createdById)
    {
        try
        {
            var ticket = _mapper.Map<IncidentTicket>(ticketDto);
            ticket.UserId = createdById;
            ticket.DateCreation = DateTime.UtcNow;

            _context.TicketsIncidents.Add(ticket);
            await _context.SaveChangesAsync();

            // ? Recharger avec les relations
            var createdTicket = await _context.TicketsIncidents
                .Include(t => t.User)
                .Include(t => t.Zone)
                .Include(t => t.PeripheriqueLogique)
                .FirstOrDefaultAsync(t => t.Id == ticket.Id);

            var result = MapToDto(createdTicket!);

            // Envoyer une alerte d'incident via SignalR
            await _signalRService.SendIncidentAlertAsync(
                ticket.Id, 
                ticket.Sujet, 
                ticket.Description, 
                GetPriorityFromStatus(ticket.Statut)
            );

            _logger.LogInformation("Nouveau ticket d'incident créé avec ID {TicketId} et alerte envoyée via SignalR", ticket.Id);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du ticket d'incident");
            throw;
        }
    }

    public async Task<IncidentTicketDto?> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.TicketsIncidents
            .Include(t => t.User)
            .Include(t => t.Zone)
            .Include(t => t.PeripheriqueLogique)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket == null)
        {
            return null;
        }

        return MapToDto(ticket);
    }

    // ? NOUVEAU: Récupère TOUS les tickets avec relations
    public async Task<IEnumerable<IncidentTicketDto>> GetAllTicketsAsync()
    {
        var tickets = await _context.TicketsIncidents
            .Include(t => t.User)
            .Include(t => t.Zone)
            .Include(t => t.PeripheriqueLogique)
            .OrderByDescending(t => t.DateCreation)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<IEnumerable<IncidentTicketDto>> GetTicketsByStatusAsync(string status)
    {
        var tickets = await _context.TicketsIncidents
            .Include(t => t.User)
            .Include(t => t.Zone)
            .Include(t => t.PeripheriqueLogique)
            .Where(t => t.Statut == status)
            .OrderByDescending(t => t.DateCreation)
            .ToListAsync();
        
        return tickets.Select(MapToDto);
    }

    public async Task<IEnumerable<IncidentTicketDto>> GetTicketsByUserIdAsync(string userId)
    {
        var tickets = await _context.TicketsIncidents
            .Include(t => t.User)
            .Include(t => t.Zone)
            .Include(t => t.PeripheriqueLogique)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.DateCreation)
            .ToListAsync();

        return tickets.Select(MapToDto);
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, string newStatus)
    {
        try
        {
            var ticket = await _context.TicketsIncidents.FindAsync(id);
            if (ticket == null)
            {
                return false;
            }

            var oldStatus = ticket.Statut;
            ticket.Statut = newStatus;
            
            if (newStatus.ToLowerInvariant() == "résolu" || newStatus.ToLowerInvariant() == "fermé")
            {
                ticket.DateResolution = DateTime.UtcNow;
            }

            _context.TicketsIncidents.Update(ticket);
            await _context.SaveChangesAsync();

            // Envoyer une notification de changement de statut via SignalR
            await _signalRService.SendBroadcastNotificationAsync(
                "Mise à jour de ticket",
                $"Le ticket #{id} - {ticket.Sujet} est passé de '{oldStatus}' à '{newStatus}'"
            );

            _logger.LogInformation("Statut du ticket {TicketId} mis à jour de '{OldStatus}' vers '{NewStatus}'", id, oldStatus, newStatus);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du statut du ticket {TicketId}", id);
            throw;
        }
    }

    public async Task<bool> AssignTicketAsync(int id, string assignedToId)
    {
        // Note: Cette méthode est dépréciée car le modèle n'a plus AssignedToId
        // On pourrait l'implémenter différemment ou la retirer
        return await Task.FromResult(false);
    }

    public async Task<bool> DeleteTicketAsync(int id)
    {
        try
        {
            var ticket = await _context.TicketsIncidents.FindAsync(id);
            if (ticket == null)
            {
                return false;
            }

            _context.TicketsIncidents.Remove(ticket);
            await _context.SaveChangesAsync();

            // Envoyer une notification de suppression via SignalR
            await _signalRService.SendBroadcastNotificationAsync(
                "Ticket supprimé",
                $"Le ticket #{id} - {ticket.Sujet} a été supprimé"
            );

            _logger.LogInformation("Ticket {TicketId} supprimé avec succès", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du ticket {TicketId}", id);
            throw;
        }
    }

    // ? MÉTHODE UTILITAIRE: Mapping enrichi avec relations
    private IncidentTicketDto MapToDto(IncidentTicket ticket)
    {
        return new IncidentTicketDto
        {
            Id = ticket.Id,
            
            // Informations du ticket
            Sujet = ticket.Sujet,
            Description = ticket.Description,
            Statut = ticket.Statut,
            Priorite = GetPriorityFromStatus(ticket.Statut),
            
            // Informations utilisateur (créateur)
            CreatedById = ticket.UserId,
            CreatedByName = ticket.User?.UserName,
            CreatedByEmail = ticket.User?.Email,
            
            // Assignation (pour l'instant null car pas dans le modèle)
            AssignedToId = null,
            AssignedToName = null,
            
            // Informations zone
            ZoneId = ticket.ZoneId,
            ZoneName = ticket.Zone?.Name,
            
            // Informations périphérique
            PeripheriqueLogiqueId = ticket.PeripheriqueLogiqueId,
            PeripheriqueLogiqueName = ticket.PeripheriqueLogique?.Name,
            PeripheriqueLogiqueType = ticket.PeripheriqueLogique?.Type,
            
            // Dates
            CreatedAt = ticket.DateCreation,
            UpdatedAt = null,  // Pas de champ UpdatedAt dans le modèle actuel
            ResolvedAt = ticket.DateResolution
        };
    }

    private static string GetPriorityFromStatus(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "urgent" => "Haute",
            "critique" => "Haute",
            "ouvert" => "Moyenne",
            "en cours" => "Moyenne",
            _ => "Basse"
        };
    }
}
