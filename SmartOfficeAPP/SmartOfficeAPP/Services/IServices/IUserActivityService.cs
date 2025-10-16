using SmartOfficeAPP.DTOs.Response;

namespace SmartOfficeAPP.Services.IServices;

public interface IUserActivityService
{
    /// <summary>
    /// Récupère l'historique d'activité d'un utilisateur avec pagination.
    /// </summary>
    Task<(List<UserActivityDto> Activities, int TotalCount)> GetUserActivitiesAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? activityType = null);

    /// <summary>
    /// Récupère les statistiques d'activité d'un utilisateur.
    /// </summary>
    Task<UserActivityStatsDto> GetUserActivityStatsAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null);
}
