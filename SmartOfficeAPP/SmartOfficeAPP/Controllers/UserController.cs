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

    /// <summary>
    /// ✅ MISE À JOUR: Récupère le profil complet de l'utilisateur connecté avec rôles
    /// </summary>
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

        // ✅ Récupérer les rôles de l'utilisateur
        var roles = await _userManager.GetRolesAsync(user);

        // ✅ Extraire prénom et nom du FullName
        var nameParts = user.FullName?.Split(' ', 2) ?? Array.Empty<string>();
        var firstName = nameParts.Length > 0 ? nameParts[0] : null;
        var lastName = nameParts.Length > 1 ? nameParts[1] : null;

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            FirstName = firstName,
            LastName = lastName,
            Roles = roles.ToList()
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

