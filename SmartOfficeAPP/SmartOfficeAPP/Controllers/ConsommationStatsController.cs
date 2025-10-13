using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Services.ServicesImpl;

namespace SmartOfficeAPP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsommationStatsController : ControllerBase
{
    private readonly IConsommationStatsService _consommationStatsService;
    private readonly ILogger<ConsommationStatsController> _logger;

    public ConsommationStatsController(
        IConsommationStatsService consommationStatsService,
        ILogger<ConsommationStatsController> logger)
    {
        _consommationStatsService = consommationStatsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtient les statistiques de consommation pour un périphérique
    /// </summary>
    [HttpGet("peripherique/{peripheriqueId}")]
    public async Task<ActionResult<ConsommationStatsDto>> GetConsommationStats(
        int peripheriqueId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin");
            }

            var stats = await _consommationStatsService.GetConsommationStatsAsync(peripheriqueId, startDate, endDate);
            return Ok(stats);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des statistiques pour le périphérique {PeripheriqueId}", peripheriqueId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Obtient les statistiques de consommation pour une zone
    /// </summary>
    [HttpGet("zone/{zoneId}")]
    public async Task<ActionResult<ConsommationStatsDto>> GetConsommationStatsByZone(
        int zoneId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin");
            }

            var stats = await _consommationStatsService.GetConsommationStatsByZoneAsync(zoneId, startDate, endDate);
            return Ok(stats);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des statistiques pour la zone {ZoneId}", zoneId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Obtient les données de consommation horaire pour un périphérique
    /// </summary>
    [HttpGet("peripherique/{peripheriqueId}/horaire")]
    public async Task<ActionResult<IEnumerable<ConsommationHoraireDto>>> GetConsommationHoraire(
        int peripheriqueId,
        [FromQuery] DateTime date)
    {
        try
        {
            var data = await _consommationStatsService.GetConsommationHoraireAsync(peripheriqueId, date);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des données horaires pour le périphérique {PeripheriqueId}", peripheriqueId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Obtient les tendances de consommation
    /// </summary>
    [HttpGet("peripherique/{peripheriqueId}/tendances")]
    public async Task<ActionResult<ConsommationTrendDto>> GetConsommationTrend(
        int peripheriqueId,
        [FromQuery] int nombreJours = 30)
    {
        try
        {
            if (nombreJours <= 0 || nombreJours > 365)
            {
                return BadRequest("Le nombre de jours doit être entre 1 et 365");
            }

            var trend = await _consommationStatsService.GetConsommationTrendAsync(peripheriqueId, nombreJours);
            return Ok(trend);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des tendances pour le périphérique {PeripheriqueId}", peripheriqueId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Détecte les anomalies de consommation
    /// </summary>
    [HttpGet("peripherique/{peripheriqueId}/anomalies")]
    public async Task<ActionResult<IEnumerable<AnomalieConsommationDto>>> DetectAnomalies(
        int peripheriqueId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin");
            }

            var anomalies = await _consommationStatsService.DetectAnomaliesConsommationAsync(peripheriqueId, startDate, endDate);
            return Ok(anomalies);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection d'anomalies pour le périphérique {PeripheriqueId}", peripheriqueId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Génère un rapport de consommation détaillé
    /// </summary>
    [HttpGet("rapport")]
    public async Task<ActionResult<RapportConsommationDto>> GenererRapportConsommation(
        [FromQuery] int? zoneId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin");
            }

            var rapport = await _consommationStatsService.GenererRapportConsommationAsync(zoneId, startDate, endDate);
            return Ok(rapport);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du rapport de consommation");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Calcule le coût énergétique
    /// </summary>
    [HttpGet("peripherique/{peripheriqueId}/cout")]
    public async Task<ActionResult<CoutEnergetiqueDto>> CalculerCoutEnergetique(
        int peripheriqueId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] decimal tarifKwh)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin");
            }

            if (tarifKwh <= 0)
            {
                return BadRequest("Le tarif kWh doit être positif");
            }

            var cout = await _consommationStatsService.CalculerCoutEnergetiqueAsync(peripheriqueId, startDate, endDate, tarifKwh);
            return Ok(cout);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul du coût énergétique pour le périphérique {PeripheriqueId}", peripheriqueId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Obtient le top des consommateurs
    /// </summary>
    [HttpGet("top-consommateurs")]
    public async Task<ActionResult<IEnumerable<TopConsommateursDto>>> GetTopConsommateurs(
        [FromQuery] int? zoneId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int top = 10)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("La date de début doit être antérieure à la date de fin");
            }

            if (top <= 0 || top > 100)
            {
                return BadRequest("Le paramètre 'top' doit être entre 1 et 100");
            }

            var topConsommateurs = await _consommationStatsService.GetTopConsommateursAsync(zoneId, startDate, endDate, top);
            return Ok(topConsommateurs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du top des consommateurs");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    /// <summary>
    /// Compare la consommation entre deux périodes
    /// </summary>
    [HttpGet("peripherique/{peripheriqueId}/comparaison")]
    public async Task<ActionResult<ComparaisonConsommationDto>> ComparerConsommation(
        int peripheriqueId,
        [FromQuery] DateTime startDate1,
        [FromQuery] DateTime endDate1,
        [FromQuery] DateTime startDate2,
        [FromQuery] DateTime endDate2)
    {
        try
        {
            if (startDate1 >= endDate1 || startDate2 >= endDate2)
            {
                return BadRequest("Les dates de début doivent être antérieures aux dates de fin");
            }

            var comparaison = await _consommationStatsService.ComparerConsommationAsync(
                peripheriqueId, startDate1, endDate1, startDate2, endDate2);
            return Ok(comparaison);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la comparaison de consommation pour le périphérique {PeripheriqueId}", peripheriqueId);
            return StatusCode(500, "Erreur interne du serveur");
        }
    }
}