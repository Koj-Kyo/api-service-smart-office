using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.DTOs.Response;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

/// <summary>
/// Contrôleur pour consulter l'historique des actions utilisateur (journal d'audit)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalAuditController : ControllerBase
{
    private readonly IJournalAuditService _journalAuditService;
    private readonly ILogger<JournalAuditController> _logger;

    public JournalAuditController(
        IJournalAuditService journalAuditService,
        ILogger<JournalAuditController> logger)
    {
        _journalAuditService = journalAuditService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère l'historique des actions de l'utilisateur connecté
    /// </summary>
    /// <param name="start">Date de début (optionnel)</param>
    /// <param name="end">Date de fin (optionnel)</param>
    /// <param name="limit">Nombre maximum de résultats (max 1000, défaut 100)</param>
    /// <returns>Liste des actions effectuées par l'utilisateur</returns>
    /// <response code="200">Historique récupéré avec succès</response>
    /// <response code="400">Paramètres invalides</response>
    /// <response code="401">Non authentifié</response>
    [HttpGet("my")]
    [ProducesResponseType(typeof(IEnumerable<JournalAuditDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<JournalAuditDto>>> GetMyAuditTrail(
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end,
        [FromQuery] int limit = 100)
    {
        // Validation
        if (limit <= 0 || limit > 1000)
        {
            return BadRequest("La limite doit être entre 1 et 1000.");
        }

        if (start.HasValue && end.HasValue && start.Value >= end.Value)
        {
            return BadRequest("La date de début doit être antérieure à la date de fin.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Impossible de déterminer l'utilisateur.");
        }

        try
        {
            var audits = await _journalAuditService.GetJournalAuditsByUserIdAsync(userId);

            // Filtrage par dates
            if (start.HasValue)
            {
                audits = audits.Where(a => a.Horodatage >= start.Value);
            }
            if (end.HasValue)
            {
                audits = audits.Where(a => a.Horodatage <= end.Value);
            }

            // Tri et limitation
            var result = audits
                .OrderByDescending(a => a.Horodatage)
                .Take(limit)
                .Select(a => new JournalAuditDto
                {
                    Horodatage = a.Horodatage,
                    Action = a.Action,
                    Contexte = a.Contexte
                });

            _logger.LogInformation(
                "Utilisateur {UserId} a consulté son historique ({Count} entrées)",
                userId,
                result.Count());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'historique pour {UserId}", userId);
            return StatusCode(500, "Erreur serveur lors de la récupération de l'historique.");
        }
    }

    /// <summary>
    /// [ADMIN] Récupère l'historique de tous les utilisateurs
    /// </summary>
    /// <param name="userId">Filtrer par utilisateur (optionnel)</param>
    /// <param name="start">Date de début (optionnel)</param>
    /// <param name="end">Date de fin (optionnel)</param>
    /// <param name="limit">Nombre maximum de résultats (max 2000, défaut 500)</param>
    /// <returns>Liste de toutes les actions avec détails utilisateur</returns>
    /// <response code="200">Historique récupéré avec succès</response>
    /// <response code="400">Paramètres invalides</response>
    /// <response code="403">Accès refusé (Admin uniquement)</response>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<JournalAuditDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<JournalAuditDetailDto>>> GetAllAuditTrail(
        [FromQuery] string? userId,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end,
        [FromQuery] int limit = 500)
    {
        // Validation
        if (limit <= 0 || limit > 2000)
        {
            return BadRequest("La limite doit être entre 1 et 2000.");
        }

        if (start.HasValue && end.HasValue && start.Value >= end.Value)
        {
            return BadRequest("La date de début doit être antérieure à la date de fin.");
        }

        try
        {
            IEnumerable<Models.JournalAudit> audits;

            // Filtrage par utilisateur si spécifié
            if (!string.IsNullOrEmpty(userId))
            {
                audits = await _journalAuditService.GetJournalAuditsByUserIdAsync(userId);
            }
            else
            {
                audits = await _journalAuditService.GetAllJournalAuditsAsync();
            }

            // Filtrage par dates
            if (start.HasValue)
            {
                audits = audits.Where(a => a.Horodatage >= start.Value);
            }
            if (end.HasValue)
            {
                audits = audits.Where(a => a.Horodatage <= end.Value);
            }

            // Tri et limitation
            var result = audits
                .OrderByDescending(a => a.Horodatage)
                .Take(limit)
                .Select(a => new JournalAuditDetailDto
                {
                    Horodatage = a.Horodatage,
                    Action = a.Action,
                    Contexte = a.Contexte,
                    UserId = a.UserId,
                    UserName = a.User?.UserName,
                    UserEmail = a.User?.Email,
                    UserFullName = a.User?.FullName
                });

            _logger.LogInformation(
                "Admin {AdminId} a consulté l'historique complet ({Count} entrées)",
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                result.Count());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'historique complet");
            return StatusCode(500, "Erreur serveur lors de la récupération de l'historique.");
        }
    }

    /// <summary>
    /// [ADMIN] Récupère l'historique d'un utilisateur spécifique
    /// </summary>
    /// <param name="userId">ID de l'utilisateur</param>
    /// <param name="start">Date de début (optionnel)</param>
    /// <param name="end">Date de fin (optionnel)</param>
    /// <param name="limit">Nombre maximum de résultats (max 1000, défaut 200)</param>
    /// <returns>Historique de l'utilisateur spécifié</returns>
    /// <response code="200">Historique récupéré avec succès</response>
    /// <response code="400">Paramètres invalides</response>
    /// <response code="403">Accès refusé (Admin uniquement)</response>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<JournalAuditDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<JournalAuditDetailDto>>> GetUserAuditTrail(
        string userId,
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end,
        [FromQuery] int limit = 200)
    {
        // Validation
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("L'ID utilisateur est requis.");
        }

        if (limit <= 0 || limit > 1000)
        {
            return BadRequest("La limite doit être entre 1 et 1000.");
        }

        if (start.HasValue && end.HasValue && start.Value >= end.Value)
        {
            return BadRequest("La date de début doit être antérieure à la date de fin.");
        }

        try
        {
            var audits = await _journalAuditService.GetJournalAuditsByUserIdAsync(userId);

            // Filtrage par dates
            if (start.HasValue)
            {
                audits = audits.Where(a => a.Horodatage >= start.Value);
            }
            if (end.HasValue)
            {
                audits = audits.Where(a => a.Horodatage <= end.Value);
            }

            // Tri et limitation
            var result = audits
                .OrderByDescending(a => a.Horodatage)
                .Take(limit)
                .Select(a => new JournalAuditDetailDto
                {
                    Horodatage = a.Horodatage,
                    Action = a.Action,
                    Contexte = a.Contexte,
                    UserId = a.UserId,
                    UserName = a.User?.UserName,
                    UserEmail = a.User?.Email,
                    UserFullName = a.User?.FullName
                });

            _logger.LogInformation(
                "Admin {AdminId} a consulté l'historique de {UserId} ({Count} entrées)",
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                userId,
                result.Count());

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'historique pour {UserId}", userId);
            return StatusCode(500, "Erreur serveur lors de la récupération de l'historique.");
        }
    }

    /// <summary>
    /// [ADMIN] Récupère des statistiques sur l'historique d'audit
    /// </summary>
    /// <param name="days">Nombre de jours à analyser (défaut 30)</param>
    /// <returns>Statistiques d'utilisation</returns>
    /// <response code="200">Statistiques récupérées avec succès</response>
    /// <response code="403">Accès refusé (Admin uniquement)</response>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AuditStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditStatsDto>> GetAuditStats([FromQuery] int days = 30)
    {
        if (days <= 0 || days > 365)
        {
            return BadRequest("Le nombre de jours doit être entre 1 et 365.");
        }

        try
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var audits = await _journalAuditService.GetAllJournalAuditsAsync();
            var recentAudits = audits.Where(a => a.Horodatage >= startDate).ToList();

            var stats = new AuditStatsDto
            {
                TotalActions = recentAudits.Count,
                UniqueUsers = recentAudits.Select(a => a.UserId).Distinct().Count(),
                PeriodDays = days,
                StartDate = startDate,
                EndDate = DateTime.UtcNow,
                TopActions = recentAudits
                    .GroupBy(a => a.Action)
                    .Select(g => new ActionCount { Action = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList(),
                ActionsByDay = recentAudits
                    .GroupBy(a => a.Horodatage.Date)
                    .Select(g => new DailyActionCount
                    {
                        Date = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des statistiques d'audit");
            return StatusCode(500, "Erreur serveur lors de la récupération des statistiques.");
        }
    }
}

/// <summary>
/// DTO pour les statistiques d'audit
/// </summary>
public class AuditStatsDto
{
    public int TotalActions { get; set; }
    public int UniqueUsers { get; set; }
    public int PeriodDays { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ActionCount> TopActions { get; set; } = new();
    public List<DailyActionCount> ActionsByDay { get; set; } = new();
}

public class ActionCount
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailyActionCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}
