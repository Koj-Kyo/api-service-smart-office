using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Route("api/user/activity")]
[ApiController]
[Authorize]
public class UserActivityController : ControllerBase
{
    private readonly IUserActivityService _activityService;
    private readonly ILogger<UserActivityController> _logger;

    public UserActivityController(
        IUserActivityService activityService,
        ILogger<UserActivityController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère l'historique d'activité de l'utilisateur connecté.
    /// </summary>
    /// <param name="pageNumber">Numéro de page (défaut: 1)</param>
    /// <param name="pageSize">Taille de page (défaut: 20, max: 100)</param>
    /// <param name="startDate">Date de début (optionnel)</param>
    /// <param name="endDate">Date de fin (optionnel)</param>
    /// <param name="activityType">Type d'activité: "Commande", "Audit" (optionnel)</param>
    [HttpGet("history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserActivity(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? activityType = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Utilisateur non authentifié." });
        }

        // Limiter la taille de page
        if (pageSize > 100)
            pageSize = 100;

        var (activities, totalCount) = await _activityService.GetUserActivitiesAsync(
            userId,
            pageNumber,
            pageSize,
            startDate,
            endDate,
            activityType);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            activities,
            pagination = new
            {
                currentPage = pageNumber,
                pageSize,
                totalCount,
                totalPages,
                hasPrevious = pageNumber > 1,
                hasNext = pageNumber < totalPages
            }
        });
    }

    /// <summary>
    /// Récupère les statistiques d'activité de l'utilisateur connecté.
    /// </summary>
    /// <param name="startDate">Date de début (optionnel, défaut: 30 derniers jours)</param>
    /// <param name="endDate">Date de fin (optionnel, défaut: maintenant)</param>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(UserActivityStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserActivityStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Utilisateur non authentifié." });
        }

        // Par défaut: 30 derniers jours
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var stats = await _activityService.GetUserActivityStatsAsync(userId, startDate, endDate);

        return Ok(stats);
    }

    /// <summary>
    /// Récupère les activités récentes (dernières 24h) de l'utilisateur connecté.
    /// </summary>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(List<UserActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentActivity()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Utilisateur non authentifié." });
        }

        var yesterday = DateTime.UtcNow.AddDays(-1);
        var (activities, _) = await _activityService.GetUserActivitiesAsync(
            userId,
            1,
            50,
            yesterday,
            DateTime.UtcNow);

        return Ok(activities);
    }
}
