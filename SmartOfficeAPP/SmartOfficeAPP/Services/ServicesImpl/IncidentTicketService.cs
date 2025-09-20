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

    public IncidentTicketService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IncidentTicketDto> CreateTicketAsync(IncidentTicketCreateDto ticketDto, string createdById)
    {
        var ticket = new IncidentTicket
        {
            Title = ticketDto.Title,
            Description = ticketDto.Description,
            Status = ticketDto.Status,
            Priority = ticketDto.Priority,
            CreatedById = createdById,
            AssignedToId = ticketDto.AssignedToId,
            CreatedAt = DateTime.UtcNow
        };

        _context.TicketsIncidents.Add(ticket);
        await _context.SaveChangesAsync();

        return new IncidentTicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedById = ticket.CreatedById,
            AssignedToId = ticket.AssignedToId,
            CreatedAt = ticket.CreatedAt
        };
    }

    public async Task<IncidentTicketDto> GetTicketByIdAsync(int id)
    {
        var ticket = await _context.TicketsIncidents.FindAsync(id);
        if (ticket == null)
        {
            return null;
        }

        return new IncidentTicketDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            CreatedById = ticket.CreatedById,
            AssignedToId = ticket.AssignedToId,
            CreatedAt = ticket.CreatedAt
        };
    }

    public async Task<IEnumerable<IncidentTicketDto>> GetTicketsByStatusAsync(string status)
    {
        var tickets = _context.TicketsIncidents.Where(t => t.Status == status).ToList();
        return await Task.FromResult(tickets.Select(t => new IncidentTicketDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            CreatedById = t.CreatedById,
            AssignedToId = t.AssignedToId,
            CreatedAt = t.CreatedAt
        }));
    }

    public async Task<IEnumerable<IncidentTicketDto>> GetTicketsByUserIdAsync(string userId)
    {
        var tickets = _context.TicketsIncidents
            .Where(t => t.CreatedById == userId || t.AssignedToId == userId)
            .ToList();

        return await Task.FromResult(tickets.Select(t => new IncidentTicketDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            CreatedById = t.CreatedById,
            AssignedToId = t.AssignedToId,
            CreatedAt = t.CreatedAt
        }));
    }

    public async Task<bool> UpdateTicketStatusAsync(int id, string newStatus)
    {
        var ticket = await _context.TicketsIncidents.FindAsync(id);
        if (ticket == null)
        {
            return false;
        }

        ticket.Status = newStatus;
        _context.TicketsIncidents.Update(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignTicketAsync(int id, string assignedToId)
    {
        var ticket = await _context.TicketsIncidents.FindAsync(id);
        if (ticket == null)
        {
            return false;
        }

        ticket.AssignedToId = assignedToId;
        _context.TicketsIncidents.Update(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTicketAsync(int id)
    {
        var ticket = await _context.TicketsIncidents.FindAsync(id);
        if (ticket == null)
        {
            return false;
        }

        _context.TicketsIncidents.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }
}
