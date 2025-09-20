using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.Models.Identity;
using SmartOfficeAPP.DTOs;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Utilisateur non trouvé.");
        }

        var profileDto = new UserProfileDto
        {
            Email = user.Email,
            // Ajoutez d'autres champs de profil si nécessaire (ex: Nom, Prénom)
        };

        return Ok(profileDto);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound("Utilisateur non trouvé.");
        }

        user.Email = updateDto.Email;
        // Mettre à jour d'autres champs si nécessaire

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return NoContent();
    }

    // TODO: Implémenter le point de terminaison pour /api/admin/users/{id}/roles
    // Point de terminaison pour les administrateurs pour attribuer un rôle
    [HttpPost("admin/users/{id}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(string id, [FromBody] RoleUpdateDto roleUpdateDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("Utilisateur non trouvé.");
        }

        var result = await _userManager.AddToRoleAsync(user, roleUpdateDto.RoleName);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return Ok(new { message = $"Rôle '{roleUpdateDto.RoleName}' attribué à l'utilisateur." });
    }

    // Point de terminaison pour les administrateurs pour retirer un rôle
    [HttpDelete("admin/users/{id}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole(string id, [FromBody] RoleUpdateDto roleUpdateDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("Utilisateur non trouvé.");
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleUpdateDto.RoleName);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return Ok(new { message = $"Rôle '{roleUpdateDto.RoleName}' retiré de l'utilisateur." });
    }
}

