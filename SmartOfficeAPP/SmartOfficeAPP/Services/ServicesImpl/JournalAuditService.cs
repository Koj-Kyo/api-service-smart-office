using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class JournalAuditService : IJournalAuditService
{
    private readonly IJournalAuditRepository _journalAuditRepository;

    public JournalAuditService(IJournalAuditRepository journalAuditRepository)
    {
        _journalAuditRepository = journalAuditRepository;
    }

    /// <inheritdoc/>
    public async Task LogActionAsync(string userId, string action, string contexte)
    {
        var auditLog = new JournalAudit
        {
            UserId = userId,
            Horodatage = DateTime.UtcNow,
            Action = action,
            Contexte = contexte
        };
        await _journalAuditRepository.AddJournalAuditAsync(auditLog);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<JournalAudit>> GetAllJournalAuditsAsync()
    {
        return await _journalAuditRepository.GetAllJournalAuditsAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<JournalAudit>> GetJournalAuditsByUserIdAsync(string userId)
    {
        return await _journalAuditRepository.GetJournalAuditsByUserIdAsync(userId);
    }
}
