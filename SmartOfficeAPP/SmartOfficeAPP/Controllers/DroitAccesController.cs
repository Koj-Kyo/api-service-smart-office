using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Route("api")]
[ApiController]
public class DroitAccesController : ControllerBase
{
    private readonly IDroitAccesService _droitAccesService;

    public DroitAccesController(IDroitAccesService droitAccesService)
    {
        _droitAccesService = droitAccesService;
    }

    // Points de terminaison pour l'administration des droits d'accès (CRUD)

    // GET: api/admin/access-rights
    [HttpGet("admin/access-rights")]
    [Authorize(Roles = "Admin")] // Seuls les administrateurs peuvent accéder
    public async Task<IActionResult> GetDroitsAcces()
    {
        var droitsAcces = await _droitAccesService.GetDroitsAccesAsync();
        return Ok(droitsAcces);
    }

    // GET: api/admin/access-rights/{id}
    [HttpGet("admin/access-rights/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetDroitAccesById(int id)
    {
        var droitAcces = await _droitAccesService.GetDroitAccesByIdAsync(id);
        if (droitAcces == null)
        {
            return NotFound("Droit d'accès non trouvé.");
        }
        return Ok(droitAcces);
    }

    // POST: api/admin/access-rights
    [HttpPost("admin/access-rights")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddDroitAcces([FromBody] DroitAccesCreationDto droitAccesDto)
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

    // Points de terminaison pour les utilisateurs finaux

    // GET: api/user/access-rights/zones
    [HttpGet("user/access-rights/zones")]
    [Authorize] // Les utilisateurs authentifiés peuvent accéder à leurs propres droits
    public async Task<IActionResult> GetUserZoneAccessRights()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Impossible de déterminer l'utilisateur.");
        }

        var droits = await _droitAccesService.GetUserZoneAccessRightsAsync(userId);
        return Ok(droits);
    }

    // GET: api/user/access-rights/peripheriques
    [HttpGet("user/access-rights/peripheriques")]
    [Authorize]
    public async Task<IActionResult> GetUserPeripheriqueAccessRights()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Impossible de déterminer l'utilisateur.");
        }

        var droits = await _droitAccesService.GetUserPeripheriqueAccessRightsAsync(userId);
        return Ok(droits);
    }
}
