using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Services.IServices;

public interface IJournalAuditService
{
    /// <summary>
    /// Asynchronously logs an action to the audit journal.
    /// </summary>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="action">The action being performed.</param>
    /// <param name="contexte">The context of the action.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LogActionAsync(string userId, string action, string contexte);

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
}
