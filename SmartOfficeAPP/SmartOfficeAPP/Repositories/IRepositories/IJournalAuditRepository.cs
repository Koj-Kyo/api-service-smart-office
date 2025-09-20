using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface IJournalAuditRepository
{
    /// <summary>
    /// Asynchronously adds a new audit log entry to the database.
    /// </summary>
    /// <param name="journalAudit">The audit log entry to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddJournalAuditAsync(JournalAudit journalAudit);

    /// <summary>
    /// Asynchronously retrieves all audit logs.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, returning a collection of all audit logs.</returns>
    Task<IEnumerable<JournalAudit>> GetAllJournalAuditsAsync();

    /// <summary>
    /// Asynchronously retrieves audit logs by a specific user ID.
    /// </summary>
    /// <param name="userId">The ID of the user whose audit logs to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, returning a collection of audit logs for the specified user.</returns>
    Task<IEnumerable<JournalAudit>> GetJournalAuditsByUserIdAsync(string userId);

    /// <summary>
    /// Asynchronously retrieves audit logs within a specific date range.
    /// </summary>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns>A task that represents the asynchronous operation, returning a collection of audit logs within the date range.</returns>
    Task<IEnumerable<JournalAudit>> GetJournalAuditsByDateRangeAsync(DateTime startDate, DateTime endDate);
}
