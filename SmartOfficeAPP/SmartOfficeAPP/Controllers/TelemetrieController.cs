using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // L'accès à la télémétrie est réservé à tout utilisateur connecté
public class TelemetriesController : ControllerBase
{
    private readonly ITelemetrieService _telemetrieService;

    public TelemetriesController(ITelemetrieService telemetrieService)
    {
        _telemetrieService = telemetrieService;
    }


    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique logique spécifique.
    /// Exemple de requête : GET /api/telemetrie/peripherique?peripheriqueId=1
    /// </summary>
    /// <param name="peripheriqueId">L'ID du périphérique.</param>
    /// <returns>Une collection de données de télémétrie.</returns>
    [HttpGet("peripherique")]
    public async Task<IActionResult> GetTelemetriesByPeripheriqueIdAsync([FromQuery] int peripheriqueId)
    {
        var telemetries = await _telemetrieService.GetTelemetriesByPeripheriqueIdAsync(peripheriqueId);
        return Ok(telemetries);
    }

    // GET: api/Telemetries/by-peripherique/{peripheriqueId}
    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique spécifique.
    /// </summary>
    /// <param name="peripheriqueId">L'ID du périphérique.</param>
    /// <returns>Une liste de données de télémétrie.</returns>
    [HttpGet("by-peripherique/{peripheriqueId}")]
    public async Task<IActionResult> GetTelemetriesByPeripheriqueId(int peripheriqueId)
    {
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
        var historicalData = await _telemetrieService.GetHistoricalDataAsync(startDate, endDate, peripheriqueId);
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
