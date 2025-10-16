using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.Authorization;
using SmartOfficeAPP.Enums;
using SmartOfficeAPP.Extensions;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // L'accès à la télémétrie est réservé à tout utilisateur connecté
public class TelemetriesController : ControllerBase
{
    private readonly ITelemetrieService _telemetrieService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<TelemetriesController> _logger;

    public TelemetriesController(
        ITelemetrieService telemetrieService,
        IAuthorizationService authorizationService,
        ILogger<TelemetriesController> logger)
    {
        _telemetrieService = telemetrieService;
        _authorizationService = authorizationService;
        _logger = logger;
    }


    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique logique spécifique.
    /// Vérifie les droits d'accès en lecture sur le périphérique.
    /// </summary>
    [HttpGet("peripherique")]
    public async Task<IActionResult> GetTelemetriesByPeripheriqueIdAsync([FromQuery] int peripheriqueId)
    {
        // Vérification des droits d'accès en lecture
        var hasAccess = await _authorizationService.CheckPeripheriqueAccessAsync(
            User,
            peripheriqueId,
            DroitAccesType.Lecture);

        if (!hasAccess)
        {
            _logger.LogWarning(
                "Accès refusé à la télémétrie pour périphérique {PeripheriqueId} par userId={UserId}",
                peripheriqueId,
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Forbid();
        }

        var telemetries = await _telemetrieService.GetTelemetriesByPeripheriqueIdAsync(peripheriqueId);
        return Ok(telemetries);
    }

    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique spécifique.
    /// Vérifie les droits d'accès en lecture sur le périphérique.
    /// </summary>
    [HttpGet("by-peripherique/{peripheriqueId}")]
    public async Task<IActionResult> GetTelemetriesByPeripheriqueId(int peripheriqueId)
    {
        // Vérification des droits d'accès en lecture
        var hasAccess = await _authorizationService.CheckPeripheriqueAccessAsync(
            User,
            peripheriqueId,
            DroitAccesType.Lecture);

        if (!hasAccess)
        {
            _logger.LogWarning(
                "Accès refusé à la télémétrie pour périphérique {PeripheriqueId} par userId={UserId}",
                peripheriqueId,
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            return Forbid();
        }

        var telemetries = await _telemetrieService.GetTelemetriesByPeripheriqueIdAsync(peripheriqueId);

        if (telemetries == null)
        {
            return NotFound("Périphérique introuvable.");
        }

        return Ok(telemetries);
    }

    /// <summary>
    /// Ajoute un nouvel enregistrement de télémétrie.
    /// </summary>
    /// <param name="telemetrie">L'objet de télémétrie à ajouter.</param>
    /// <returns>Une réponse de succès (code 201 Created) ou d'erreur.</returns>
    [HttpPost("add")]
    public async Task<IActionResult> AddTelemetryAsync([FromBody] Telemetrie telemetrie)
    {
        if (telemetrie == null)
        {
            return BadRequest("Les données de télémétrie sont invalides.");
        }

        // Assurer que la date est en UTC pour la compatibilité avec PostgreSQL
        if (telemetrie.Horodatage.Kind == DateTimeKind.Unspecified)
        {
            telemetrie.Horodatage = DateTime.SpecifyKind(telemetrie.Horodatage, DateTimeKind.Utc);
        }
        else if (telemetrie.Horodatage.Kind == DateTimeKind.Local)
        {
            telemetrie.Horodatage = telemetrie.Horodatage.ToUniversalTime();
        }

        await _telemetrieService.AddTelemetryAsync(telemetrie);
        return CreatedAtAction(nameof(GetTelemetriesByPeripheriqueIdAsync), new { peripheriqueId = telemetrie.PeripheriqueLogiqueId }, telemetrie);
    }

    /// <summary>
    /// Récupère les données historiques de télémétrie dans une plage de dates.
    /// Exemple de requête : GET /api/telemetrie/history?startDate=2024-01-01&endDate=2024-01-31&peripheriqueId=1
    /// </summary>
    /// <param name="startDate">Date de début de la période.</param>
    /// <param name="endDate">Date de fin de la période.</param>
    /// <param name="peripheriqueId">ID du périphérique (facultatif).</param>
    /// <returns>Une collection de données de télémétrie historiques.</returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistoricalDataAsync(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? peripheriqueId)
    {
        // Convertir les dates en UTC pour la compatibilité avec PostgreSQL
        var startDateUtc = startDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) 
            : startDate.ToUniversalTime();
        
        var endDateUtc = endDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) 
            : endDate.ToUniversalTime();

        var historicalData = await _telemetrieService.GetHistoricalDataAsync(startDateUtc, endDateUtc, peripheriqueId);
        return Ok(historicalData);
    }

    /// <summary>
    /// Stocke une suggestion générée par le moteur IA.
    /// </summary>
    /// <param name="suggestionDto">L'objet SuggestionDto à stocker.</param>
    /// <returns>Une réponse de succès (code 200 Ok) ou d'erreur.</returns>
    [HttpPost("suggestions")]
    public async Task<IActionResult> StoreAISuggestionAsync([FromBody] Suggestion suggestion)
    {
        if (suggestion == null)
        {
            return BadRequest("Les données de suggestion sont invalides.");
        }
        await _telemetrieService.StoreAISuggestionAsync(suggestion);
        return Ok("Suggestion stockée avec succès.");
    }
}
