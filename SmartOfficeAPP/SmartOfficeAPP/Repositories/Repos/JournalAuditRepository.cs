using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class JournalAuditRepository : IJournalAuditRepository
{
    private readonly ApplicationDbContext _context;

    public JournalAuditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task AddJournalAuditAsync(JournalAudit journalAudit)
    {
        await _context.JournalAudits.AddAsync(journalAudit);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<JournalAudit>> GetAllJournalAuditsAsync()
    {
        return await _context.JournalAudits.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<JournalAudit>> GetJournalAuditsByUserIdAsync(string userId)
    {
        return await _context.JournalAudits
            .Where(ja => ja.UserId == userId)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<JournalAudit>> GetJournalAuditsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.JournalAudits
            .Where(ja => ja.Horodatage >= startDate && ja.Horodatage <= endDate)
            .ToListAsync();
    }
}
