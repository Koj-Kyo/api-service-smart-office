using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementService userManagementService,
        ILogger<UserManagementController> logger)
    {
        _userManagementService = userManagementService;
        _logger = logger;
    }

    #region Gestion des utilisateurs

    /// <summary>
    /// Récupère tous les utilisateurs avec leurs rôles
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetAllUsers()
    {
        var users = await _userManagementService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<ActionResult<UserDetailsDto>> GetUserById(string userId)
    {
        var user = await _userManagementService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Utilisateur avec l'ID {userId} non trouvé");
        }
        return Ok(user);
    }

    /// <summary>
    /// Recherche des utilisateurs par terme de recherche
    /// </summary>
    [HttpGet("users/search")]
    public async Task<ActionResult<IEnumerable<UserDetailsDto>>> SearchUsers([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Le terme de recherche ne peut pas être vide");
        }

        var users = await _userManagementService.SearchUsersAsync(searchTerm);
        return Ok(users);
    }

    /// <summary>
    /// Crée un nouvel utilisateur avec rôles
    /// </summary>
    [HttpPost("users")]
    public async Task<ActionResult<UserDetailsDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var result = await _userManagementService.CreateUserAsync(createUserDto);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de la création de l'utilisateur",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        var createdUser = await _userManagementService.GetUserByEmailAsync(createUserDto.Email);
        _logger.LogInformation("Utilisateur créé: {Email} par Admin: {AdminId}", 
            createUserDto.Email, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return CreatedAtAction(nameof(GetUserById), new { userId = createdUser!.Id }, createdUser);
    }

    /// <summary>
    /// Met à jour un utilisateur existant
    /// </summary>
    [HttpPut("users/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserDto updateUserDto)
    {
        var result = await _userManagementService.UpdateUserAsync(userId, updateUserDto);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de la mise à jour de l'utilisateur",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        _logger.LogInformation("Utilisateur mis à jour: {UserId} par Admin: {AdminId}", 
            userId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return NoContent();
    }

    /// <summary>
    /// Active/Désactive un utilisateur
    /// </summary>
    [HttpPatch("users/{userId}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == currentUserId)
        {
            return BadRequest("Vous ne pouvez pas désactiver votre propre compte");
        }

        var result = await _userManagementService.ToggleUserActiveStatusAsync(userId);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors du changement de statut",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Supprime un utilisateur (à utiliser avec précaution)
    /// </summary>
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == currentUserId)
        {
            return BadRequest("Vous ne pouvez pas supprimer votre propre compte");
        }

        var result = await _userManagementService.DeleteUserAsync(userId);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de la suppression de l'utilisateur",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        _logger.LogWarning("Utilisateur supprimé: {UserId} par Admin: {AdminId}", 
            userId, currentUserId);

        return NoContent();
    }

    #endregion

    #region Gestion des rôles

    /// <summary>
    /// Récupère tous les rôles avec statistiques
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
    {
        var roles = await _userManagementService.GetAllRolesAsync();
        return Ok(roles);
    }

    /// <summary>
    /// Crée un nouveau rôle
    /// </summary>
    [HttpPost("roles")]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        var result = await _userManagementService.CreateRoleAsync(createRoleDto);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de la création du rôle",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        _logger.LogInformation("Rôle créé: {RoleName} par Admin: {AdminId}", 
            createRoleDto.Name, User.FindFirstValue(ClaimTypes.NameIdentifier));

        var roles = await _userManagementService.GetAllRolesAsync();
        var createdRole = roles.FirstOrDefault(r => r.Name == createRoleDto.Name);

        return CreatedAtAction(nameof(GetAllRoles), createdRole);
    }

    /// <summary>
    /// Supprime un rôle (seulement si aucun utilisateur ne l'utilise)
    /// </summary>
    [HttpDelete("roles/{roleId}")]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        var result = await _userManagementService.DeleteRoleAsync(roleId);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de la suppression du rôle",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        _logger.LogInformation("Rôle supprimé: {RoleId} par Admin: {AdminId}", 
            roleId, User.FindFirstValue(ClaimTypes.NameIdentifier));

        return NoContent();
    }

    #endregion

    #region Association utilisateurs-rôles

    /// <summary>
    /// Récupère les rôles d'un utilisateur
    /// </summary>
    [HttpGet("users/{userId}/roles")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string userId)
    {
        var roles = await _userManagementService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    /// <summary>
    /// Assigne des rôles à un utilisateur (remplace tous les rôles existants)
    /// </summary>
    [HttpPut("users/{userId}/roles")]
    public async Task<IActionResult> SetUserRoles(string userId, [FromBody] AssignRolesDto assignRolesDto)
    {
        if (userId != assignRolesDto.UserId)
        {
            return BadRequest("L'ID utilisateur dans l'URL ne correspond pas à celui dans le body");
        }

        var result = await _userManagementService.SetUserRolesAsync(userId, assignRolesDto.RoleNames);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de l'assignation des rôles",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        _logger.LogInformation("Rôles mis à jour pour utilisateur {UserId}: {Roles} par Admin: {AdminId}", 
            userId, string.Join(", ", assignRolesDto.RoleNames), User.FindFirstValue(ClaimTypes.NameIdentifier));

        return NoContent();
    }

    /// <summary>
    /// Ajoute un rôle spécifique à un utilisateur
    /// </summary>
    [HttpPost("users/{userId}/roles/{roleName}")]
    public async Task<IActionResult> AddRoleToUser(string userId, string roleName)
    {
        var result = await _userManagementService.AddUserToRoleAsync(userId, roleName);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de l'ajout du rôle",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Retire un rôle spécifique d'un utilisateur
    /// </summary>
    [HttpDelete("users/{userId}/roles/{roleName}")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleName)
    {
        var result = await _userManagementService.RemoveUserFromRoleAsync(userId, roleName);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { 
                Message = "Erreur lors de la suppression du rôle",
                Errors = result.Errors.Select(e => e.Description)
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Récupère tous les utilisateurs ayant un rôle spécifique
    /// </summary>
    [HttpGet("roles/{roleName}/users")]
    public async Task<ActionResult<IEnumerable<UserDetailsDto>>> GetUsersByRole(string roleName)
    {
        var users = await _userManagementService.GetUsersByRoleAsync(roleName);
        return Ok(users);
    }

    #endregion

    #region Statistiques

    /// <summary>
    /// Récupère les statistiques des utilisateurs et rôles
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
    {
        var statistics = await _userManagementService.GetUserStatisticsAsync();
        return Ok(statistics);
    }

    #endregion
}