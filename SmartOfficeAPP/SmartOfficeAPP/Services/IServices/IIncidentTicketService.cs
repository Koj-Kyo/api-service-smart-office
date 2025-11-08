using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;

namespace SmartOfficeAPP.Services.IServices;

public interface IIncidentTicketService
{
    Task<IncidentTicketDto> CreateTicketAsync(IncidentTicketCreateDto ticketDto, string createdById);
    Task<IncidentTicketDto?> GetTicketByIdAsync(int id);
    Task<IEnumerable<IncidentTicketDto>> GetAllTicketsAsync();  // ✅ NOUVEAU
    Task<IEnumerable<IncidentTicketDto>> GetTicketsByStatusAsync(string status);
    Task<IEnumerable<IncidentTicketDto>> GetTicketsByUserIdAsync(string userId);
    Task<bool> UpdateTicketStatusAsync(int id, string newStatus);
    Task<bool> AssignTicketAsync(int id, string assignedToId);
    Task<bool> DeleteTicketAsync(int id);
}
