using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
