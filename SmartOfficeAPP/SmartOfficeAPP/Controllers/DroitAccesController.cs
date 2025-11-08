using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using SmartOfficeAPP.Enums;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Route("api")]
[ApiController]
public class DroitAccesController : ControllerBase
{
    private readonly IDroitAccesService _droitAccesService;
    private readonly ILogger<DroitAccesController> _logger;

    public DroitAccesController(
        IDroitAccesService droitAccesService,
        ILogger<DroitAccesController> logger)
    {
        _droitAccesService = droitAccesService;
        _logger = logger;
    }

    // Points de terminaison pour l'administration des droits d'accès (CRUD)

    // GET: api/admin/access-rights
    [HttpGet("admin/access-rights")]
    [Authorize(Roles = "Admin")] // Seuls les administrateurs peuvent accéder
    public async Task<ActionResult<IEnumerable<DroitAccesDto>>> GetDroitsAcces()
    {
        try
        {
            var droitsAcces = await _droitAccesService.GetDroitsAccesAsync();
            _logger.LogInformation("✅ {Count} droits d'accès retournés", droitsAcces.Count());
            return Ok(droitsAcces);  // ✅ Retour direct sans wrapper
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur GetDroitsAcces");
            return StatusCode(500, "Erreur serveur");
        }
    }

    // GET: api/admin/access-rights/{id}
    [HttpGet("admin/access-rights/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DroitAccesDto>> GetDroitAccesById(int id)
    {
        var droitAcces = await _droitAccesService.GetDroitAccesByIdAsync(id);
        if (droitAcces == null)
        {
            return NotFound();
        }
        return Ok(droitAcces);  // ✅ Retour direct
    }

    // POST: api/admin/access-rights
    [HttpPost("admin/access-rights")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DroitAccesDto>> AddDroitAcces([FromBody] DroitAccesCreationDto droitAccesDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newDroitAcces = await _droitAccesService.AddDroitAccesAsync(droitAccesDto);
        return CreatedAtAction(nameof(GetDroitAccesById), new { id = newDroitAcces.Id }, newDroitAcces);
    }

    // PUT: api/admin/access-rights/{id}
    [HttpPut("admin/access-rights/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateDroitAcces(int id, [FromBody] DroitAccesCreationDto droitAccesDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _droitAccesService.UpdateDroitAccesAsync(id, droitAccesDto);
        if (!success)
        {
            return NotFound("Droit d'accès non trouvé ou impossible de mettre à jour.");
        }
        return NoContent();
    }

    // DELETE: api/admin/access-rights/{id}
    [HttpDelete("admin/access-rights/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDroitAcces(int id)
    {
        var success = await _droitAccesService.DeleteDroitAccesAsync(id);
        if (!success)
        {
            return NotFound("Droit d'accès non trouvé.");
        }
        return NoContent();
    }

    // ✅ NOUVEAU: Endpoint spécialisé pour affecter une zone à un utilisateur
    [HttpPost("admin/assign-zone-to-user")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DroitAccesDto>> AssignZoneToUser([FromBody] AssignZoneToUserDto assignDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var droitAccesDto = new DroitAccesCreationDto
            {
                UserId = assignDto.UserId,
                ZoneId = assignDto.ZoneId,
                Type = ((int)assignDto.AccessType).ToString()
            };

            var newDroitAcces = await _droitAccesService.AddDroitAccesAsync(droitAccesDto);
            
            _logger.LogInformation("Zone {ZoneId} affectée à l'utilisateur {UserId} avec type {AccessType}",
                assignDto.ZoneId, assignDto.UserId, assignDto.AccessType);

            return Ok(newDroitAcces);  // ✅ Retour direct
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'affectation de zone {ZoneId} à l'utilisateur {UserId}",
                assignDto.ZoneId, assignDto.UserId);
            return BadRequest("Erreur lors de l'affectation");
        }
    }

    // ✅ NOUVEAU: Endpoint pour affecter un périphérique spécifique à un utilisateur
    [HttpPost("admin/assign-device-to-user")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DroitAccesDto>> AssignDeviceToUser([FromBody] AssignDeviceToUserDto assignDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var droitAccesDto = new DroitAccesCreationDto
            {
                UserId = assignDto.UserId,
                PeripheriqueLogiqueId = assignDto.PeripheriqueLogiqueId,
                Type = ((int)assignDto.AccessType).ToString()
            };

            var newDroitAcces = await _droitAccesService.AddDroitAccesAsync(droitAccesDto);
            
            _logger.LogInformation("Périphérique {PeripheriqueId} affecté à l'utilisateur {UserId} avec type {AccessType}",
                assignDto.PeripheriqueLogiqueId, assignDto.UserId, assignDto.AccessType);

            return Ok(newDroitAcces);  // ✅ Retour direct
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'affectation de périphérique {PeripheriqueId} à l'utilisateur {UserId}",
                assignDto.PeripheriqueLogiqueId, assignDto.UserId);
            return BadRequest("Erreur lors de l'affectation");
        }
    }

    // Points de terminaison pour les utilisateurs finaux

    // GET: api/user/access-rights/zones
    [HttpGet("user/access-rights/zones")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DroitAccesDto>>> GetUserZoneAccessRights()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Impossible de déterminer l'utilisateur.");
        }

        var droits = await _droitAccesService.GetUserZoneAccessRightsAsync(userId);
        return Ok(droits);  // ✅ Retour direct
    }

    // GET: api/user/access-rights/peripheriques
    [HttpGet("user/access-rights/peripheriques")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<DroitAccesDto>>> GetUserPeripheriqueAccessRights()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Impossible de déterminer l'utilisateur.");
        }

        var droits = await _droitAccesService.GetUserPeripheriqueAccessRightsAsync(userId);
        return Ok(droits);  // ✅ Retour direct
    }
}

// ✅ DTOs pour simplifier l'affectation
public class AssignZoneToUserDto
{
    public required string UserId { get; set; }
    public int ZoneId { get; set; }
    public DroitAccesType AccessType { get; set; } = DroitAccesType.Controle;
}

public class AssignDeviceToUserDto
{
    public required string UserId { get; set; }
    public int PeripheriqueLogiqueId { get; set; }
    public DroitAccesType AccessType { get; set; } = DroitAccesType.Controle;
}
