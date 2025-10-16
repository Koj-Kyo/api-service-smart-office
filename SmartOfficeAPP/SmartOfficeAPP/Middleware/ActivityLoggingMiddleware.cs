using System.Security.Claims;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Middleware;

/// <summary>
/// Middleware optionnel pour enregistrer automatiquement les activités utilisateur.
/// Enregistre les actions importantes dans le JournalAudit.
/// </summary>
public class ActivityLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ActivityLoggingMiddleware> _logger;

    public ActivityLoggingMiddleware(RequestDelegate next, ILogger<ActivityLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLower();
        var method = context.Request.Method;

        // Ne logger que certaines actions importantes
        if (ShouldLog(path, method))
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var audit = new JournalAudit
                    {
                        UserId = userId,
                        Action = $"{method} {path}",
                        Contexte = $"IP: {context.Connection.RemoteIpAddress}, UserAgent: {context.Request.Headers["User-Agent"]}",
                        Horodatage = DateTime.UtcNow
                    };

                    dbContext.JournalAudits.Add(audit);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'enregistrement de l'audit pour userId={UserId}", userId);
                }
            }
        }

        await _next(context);
    }

    private bool ShouldLog(string? path, string method)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        // Ignorer les requêtes non-importantes
        if (path.Contains("/health") || path.Contains("/swagger"))
            return false;

        // Logger les actions de lecture/écriture importantes
        var importantPaths = new[]
        {
            "/api/zones",
            "/api/devices",
            "/api/commandes",
            "/api/admin/access-rights",
            "/api/user/profile",
            "/api/telemetries",
            "/api/devicelogs"
        };

        return importantPaths.Any(p => path.StartsWith(p));
    }
}

/// <summary>
/// Extension pour faciliter l'enregistrement du middleware.
/// </summary>
public static class ActivityLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseActivityLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ActivityLoggingMiddleware>();
    }
}
