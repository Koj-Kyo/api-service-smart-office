using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.DTOs.Response;
using Microsoft.AspNetCore.OutputCaching;

namespace SmartOfficeAPP.Controllers;

/// <summary>
/// Contrôleur pour les statistiques et données du Dashboard
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
 private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Récupère les statistiques KPI du Dashboard
    /// GET: api/Dashboard/stats
    /// </summary>
    [HttpGet("stats")]
    [OutputCache(Duration = 60, Tags = new[] { "dashboard-stats" })]
    public async Task<ActionResult<ApiResponse<DashboardStatsDto>>> GetStats()
    {
        try
        {
 // === Statistiques Appareils ===
    var totalDevices = await _context.PeripheriquesLogiques.CountAsync();
            // ? FIX: Pas de propriété Statut, on compte simplement les devices existants comme "actifs"
  var activeDevices = totalDevices; // Tous les devices enregistrés sont considérés comme actifs

       // === Statistiques Utilisateurs ===
            var totalUsers = await _context.Users.CountAsync();
          var activeUsers = await _context.Users
         .CountAsync(u => u.EmailConfirmed);

   // === Statistiques Incidents ===
   var totalIncidents = await _context.TicketsIncidents.CountAsync();
  var openIncidents = await _context.TicketsIncidents
    .CountAsync(i => i.Statut == "Ouvert" || i.Statut == "En cours");

            // === Statistiques Énergie ===
    var now = DateTime.UtcNow;
 var last30Days = now.AddDays(-30);
          
      // ? FIX: Utiliser la propriété Valeur et filtrer par TypeEvenement
            var energyConsumed = await _context.Telemetries
       .Where(t => t.Horodatage >= last30Days && 
      (t.TypeEvenement == "Consommation_Energie" || 
       t.TypeEvenement == "Energy"))
     .SumAsync(t => t.Valeur);

            // TODO: Calculer les pourcentages de changement si historique disponible
            // Pour l'instant, on retourne 0 ou des valeurs factices

            var stats = new DashboardStatsDto
    {
      ActiveDevices = activeDevices,
       TotalDevices = totalDevices,
        DevicesChangePercent = 0, // À calculer avec historique

      ActiveUsers = activeUsers,
  TotalUsers = totalUsers,
      UsersChangePercent = 0, // À calculer avec historique

         OpenIncidents = openIncidents,
                TotalIncidents = totalIncidents,
    IncidentsChangePercent = 0, // À calculer avec historique

           EnergyConsumed = energyConsumed,
   EnergyChangePercent = 0 // À calculer avec historique
            };

            var response = ApiResponse<DashboardStatsDto>.FromDatabase(stats);
  return Ok(response);
        }
   catch (Exception ex)
        {
 _logger.LogError(ex, "Erreur lors de la récupération des statistiques du Dashboard");
            return StatusCode(500, ApiResponse<DashboardStatsDto>.FromDatabase(
        null!, 
        DateTime.UtcNow
            ));
        }
    }

    /// <summary>
    /// Récupère les données de consommation énergétique
    /// GET: api/Dashboard/energy?period=month
    /// </summary>
    /// <param name="period">Période: "day", "week", "month" (défaut: "month")</param>
    [HttpGet("energy")]
    [OutputCache(Duration = 300, VaryByQueryKeys = new[] { "period" }, Tags = new[] { "dashboard-energy" })]
    public async Task<ActionResult<ApiResponse<List<EnergyDataPointDto>>>> GetEnergyData([FromQuery] string period = "month")
    {
        try
        {
  var now = DateTime.UtcNow;
    DateTime startDate;
            string dateFormat;

  // Déterminer la plage de dates et le format selon la période
            switch (period.ToLower())
 {
      case "day":
    startDate = now.AddDays(-1);
           dateFormat = "HH:mm"; // Format heure
    break;
          case "week":
         startDate = now.AddDays(-7);
  dateFormat = "ddd"; // Jour de la semaine
            break;
                case "month":
        default:
    startDate = now.AddDays(-30);
dateFormat = "dd/MM"; // Jour/Mois
break;
      }

            // ? FIX: Utiliser la propriété Valeur et filtrer par TypeEvenement
         var data = await _context.Telemetries
       .Where(t => t.Horodatage >= startDate && 
(t.TypeEvenement == "Consommation_Energie" || 
           t.TypeEvenement == "Energy"))
                .GroupBy(t => t.Horodatage.Date)
    .Select(g => new EnergyDataPointDto
   {
        Label = g.Key.ToString(dateFormat),
         Value = g.Sum(t => t.Valeur)
      })
 .OrderBy(e => e.Label)
        .ToListAsync();

      // Si aucune donnée, retourner un tableau vide plutôt qu'une erreur
 if (!data.Any())
       {
       _logger.LogWarning("Aucune donnée de consommation énergétique trouvée pour la période {Period}", period);
          data = new List<EnergyDataPointDto>();
          }

     var response = ApiResponse<List<EnergyDataPointDto>>.FromDatabase(data);
     return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des données énergétiques pour la période {Period}", period);
            return StatusCode(500, ApiResponse<List<EnergyDataPointDto>>.FromDatabase(
     new List<EnergyDataPointDto>(), 
                DateTime.UtcNow
            ));
        }
    }

    /// <summary>
    /// Récupère l'activité des appareils par type
    /// GET: api/Dashboard/device-activity
    /// </summary>
    [HttpGet("device-activity")]
    [OutputCache(Duration = 120, Tags = new[] { "dashboard-devices" })]
    public async Task<ActionResult<ApiResponse<List<DeviceActivityDataDto>>>> GetDeviceActivity()
    {
        try
        {
     var activity = await _context.PeripheriquesLogiques
    .GroupBy(p => p.Type ?? "Unknown")
         .Select(g => new DeviceActivityDataDto
    {
          DeviceType = g.Key,
    Count = g.Count()
         })
 .OrderByDescending(d => d.Count)
   .ToListAsync();

            // Si aucun appareil, retourner un tableau vide
     if (!activity.Any())
 {
           _logger.LogWarning("Aucun appareil trouvé dans la base de données");
          activity = new List<DeviceActivityDataDto>();
       }

   var response = ApiResponse<List<DeviceActivityDataDto>>.FromDatabase(activity);
return Ok(response);
  }
        catch (Exception ex)
{
          _logger.LogError(ex, "Erreur lors de la récupération de l'activité des appareils");
          return StatusCode(500, ApiResponse<List<DeviceActivityDataDto>>.FromDatabase(
       new List<DeviceActivityDataDto>(), 
                DateTime.UtcNow
));
 }    }
}
