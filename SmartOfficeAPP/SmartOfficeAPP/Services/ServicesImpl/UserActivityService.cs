using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class UserActivityService : IUserActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(
        ApplicationDbContext context,
        ILogger<UserActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<UserActivityDto> Activities, int TotalCount)> GetUserActivitiesAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? activityType = null)
    {
        try
        {
            // Commandes exécutées
            var commandesQuery = _context.Commandes
                .Include(c => c.PeripheriqueLogique)
                .Where(c => c.UserId == userId);

            if (startDate.HasValue)
                commandesQuery = commandesQuery.Where(c => c.Horodatage >= startDate.Value);
            if (endDate.HasValue)
                commandesQuery = commandesQuery.Where(c => c.Horodatage <= endDate.Value);

            var commandes = await commandesQuery
                .OrderByDescending(c => c.Horodatage)
                .Select(c => new UserActivityDto
                {
                    Id = c.Id,
                    Timestamp = c.Horodatage,
                    ActivityType = "Commande",
                    Description = $"Commande {c.NomCommande}",
                    TargetResource = c.PeripheriqueLogique != null ? $"Périphérique: {c.PeripheriqueLogique.Name}" : null,
                    TargetResourceId = c.PeripheriqueLogiqueId,
                    ResourceType = "Périphérique",
                    Status = c.Statut,
                    Details = $"Valeur: {c.Valeur}"
                })
                .ToListAsync();

            // Journal d'audit
            var auditQuery = _context.JournalAudits
                .Where(ja => ja.UserId == userId);

            if (startDate.HasValue)
                auditQuery = auditQuery.Where(ja => ja.Horodatage >= startDate.Value);
            if (endDate.HasValue)
                auditQuery = auditQuery.Where(ja => ja.Horodatage <= endDate.Value);

            var audits = await auditQuery
                .OrderByDescending(ja => ja.Horodatage)
                .Select(ja => new UserActivityDto
                {
                    Id = ja.Id,
                    Timestamp = ja.Horodatage,
                    ActivityType = "Audit",
                    Description = ja.Action,
                    Status = "Succès",
                    Details = ja.Contexte
                })
                .ToListAsync();

            // Fusion et tri
            var allActivities = commandes.Concat(audits)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            // Filtrage par type si spécifié
            if (!string.IsNullOrEmpty(activityType))
            {
                allActivities = allActivities
                    .Where(a => a.ActivityType.Equals(activityType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var totalCount = allActivities.Count;

            // Pagination
            var paginatedActivities = allActivities
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginatedActivities, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'historique d'activité pour userId={UserId}", userId);
            return (new List<UserActivityDto>(), 0);
        }
    }

    public async Task<UserActivityStatsDto> GetUserActivityStatsAsync(
        string userId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var stats = new UserActivityStatsDto();

            // Statistiques des commandes
            var commandesQuery = _context.Commandes.Where(c => c.UserId == userId);
            if (startDate.HasValue)
                commandesQuery = commandesQuery.Where(c => c.Horodatage >= startDate.Value);
            if (endDate.HasValue)
                commandesQuery = commandesQuery.Where(c => c.Horodatage <= endDate.Value);

            var commandes = await commandesQuery.ToListAsync();
            stats.CommandesExecutees = commandes.Count(c => c.Statut == "Succès");
            stats.CommandesEchouees = commandes.Count(c => c.Statut != "Succès");

            // Statistiques du journal d'audit
            var auditQuery = _context.JournalAudits.Where(ja => ja.UserId == userId);
            if (startDate.HasValue)
                auditQuery = auditQuery.Where(ja => ja.Horodatage >= startDate.Value);
            if (endDate.HasValue)
                auditQuery = auditQuery.Where(ja => ja.Horodatage <= endDate.Value);

            var audits = await auditQuery.ToListAsync();

            // Total d'activités
            stats.TotalActivities = commandes.Count + audits.Count;

            // Activités par type
            stats.ActivitiesByType["Commandes"] = commandes.Count;
            stats.ActivitiesByType["Audit"] = audits.Count;

            // Activités récentes (dernières 10)
            var recentCommandes = commandes
                .OrderByDescending(c => c.Horodatage)
                .Take(10)
                .Select(c => new UserActivityDto
                {
                    Id = c.Id,
                    Timestamp = c.Horodatage,
                    ActivityType = "Commande",
                    Description = $"Commande {c.NomCommande}",
                    TargetResourceId = c.PeripheriqueLogiqueId,
                    ResourceType = "Périphérique",
                    Status = c.Statut,
                    Details = $"Valeur: {c.Valeur}"
                });

            var recentAudits = audits
                .OrderByDescending(ja => ja.Horodatage)
                .Take(10)
                .Select(ja => new UserActivityDto
                {
                    Id = ja.Id,
                    Timestamp = ja.Horodatage,
                    ActivityType = "Audit",
                    Description = ja.Action,
                    Status = "Succès",
                    Details = ja.Contexte
                });

            stats.RecentActivities = recentCommandes.Concat(recentAudits)
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            // Dernière activité
            var allActivities = commandes.Select(c => c.Horodatage)
                .Concat(audits.Select(ja => ja.Horodatage));
            stats.LastActivity = allActivities.Any() ? allActivities.Max() : null;

            // Activités par jour (30 derniers jours)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var activitiesByDay = commandes
                .Where(c => c.Horodatage >= thirtyDaysAgo)
                .GroupBy(c => c.Horodatage.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);

            var auditsByDay = audits
                .Where(ja => ja.Horodatage >= thirtyDaysAgo)
                .GroupBy(ja => ja.Horodatage.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);

            // Fusion
            foreach (var kvp in auditsByDay)
            {
                if (activitiesByDay.ContainsKey(kvp.Key))
                    activitiesByDay[kvp.Key] += kvp.Value;
                else
                    activitiesByDay[kvp.Key] = kvp.Value;
            }

            stats.ActivitiesByDay = activitiesByDay;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des statistiques d'activité pour userId={UserId}", userId);
            return new UserActivityStatsDto();
        }
    }
}
