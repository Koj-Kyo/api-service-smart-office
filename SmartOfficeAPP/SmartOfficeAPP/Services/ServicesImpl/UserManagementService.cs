using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Models.Identity;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    #region Gestion des utilisateurs

    public async Task<IEnumerable<UserDetailsDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDetailsDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDetailsDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles.ToList()
            });
        }

        return userDtos.OrderBy(u => u.UserName);
    }

    public async Task<UserDetailsDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount,
            Roles = roles.ToList()
        };
    }

    public async Task<UserDetailsDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        return await GetUserByIdAsync(user.Id);
    }

    public async Task<IdentityResult> CreateUserAsync(CreateUserDto createUserDto)
    {
        var user = new ApplicationUser
        {
            UserName = createUserDto.UserName,
            Email = createUserDto.Email,
            FullName = createUserDto.FullName,
            PhoneNumber = createUserDto.PhoneNumber,
            EmailConfirmed = createUserDto.EmailConfirmed
        };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);
        
        if (result.Succeeded && createUserDto.Roles.Any())
        {
            var roleResult = await _userManager.AddToRolesAsync(user, createUserDto.Roles);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Utilisateur créé mais erreur lors de l'assignation des rôles: {Errors}", 
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        if (result.Succeeded)
        {
            _logger.LogInformation("Utilisateur créé: {Email} par le système de gestion", createUserDto.Email);
        }

        return result;
    }

    public async Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Utilisateur introuvable." });
        }

        user.UserName = updateUserDto.UserName;
        user.Email = updateUserDto.Email;
        user.FullName = updateUserDto.FullName;
        user.PhoneNumber = updateUserDto.PhoneNumber;
        user.EmailConfirmed = updateUserDto.EmailConfirmed;
        user.LockoutEnabled = updateUserDto.LockoutEnabled;

        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Utilisateur mis à jour: {UserId}", userId);
        }

        return result;
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Utilisateur introuvable." });
        }

        var result = await _userManager.DeleteAsync(user);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Utilisateur supprimé: {UserId}", userId);
        }

        return result;
    }

    public async Task<IdentityResult> ToggleUserActiveStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Utilisateur introuvable." });
        }

        if (user.LockoutEnabled && (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow))
        {
            // Désactiver l'utilisateur
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.LockoutEnabled = true;
        }
        else
        {
            // Activer l'utilisateur
            user.LockoutEnd = null;
            user.LockoutEnabled = false;
        }

        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            var status = user.LockoutEnd == null ? "activé" : "désactivé";
            _logger.LogInformation("Utilisateur {Status}: {UserId}", status, userId);
        }

        return result;
    }

    #endregion

    #region Gestion des rôles

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles.ToListAsync();
        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                UserCount = usersInRole.Count
            });
        }

        return roleDtos.OrderBy(r => r.Name);
    }

    public async Task<IdentityResult> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        var role = new IdentityRole(createRoleDto.Name);
        var result = await _roleManager.CreateAsync(role);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Rôle créé: {RoleName}", createRoleDto.Name);
        }

        return result;
    }

    public async Task<IdentityResult> UpdateRoleAsync(string roleId, UpdateRoleDto updateRoleDto)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Rôle introuvable." });
        }

        role.Name = updateRoleDto.Name;
        var result = await _roleManager.UpdateAsync(role);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Rôle mis à jour: {RoleId}", roleId);
        }

        return result;
    }

    public async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Rôle introuvable." });
        }

        // Vérifier si des utilisateurs ont ce rôle
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            return IdentityResult.Failed(new IdentityError 
            { 
                Description = $"Impossible de supprimer le rôle. {usersInRole.Count} utilisateur(s) l'utilisent encore." 
            });
        }

        var result = await _roleManager.DeleteAsync(role);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Rôle supprimé: {RoleId}", roleId);
        }

        return result;
    }

    #endregion

    #region Association utilisateurs-rôles

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Utilisateur introuvable." });
        }

        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Rôle introuvable." });
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Rôle {RoleName} assigné à l'utilisateur {UserId}", roleName, userId);
        }

        return result;
    }

    public async Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Utilisateur introuvable." });
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Rôle {RoleName} retiré de l'utilisateur {UserId}", roleName, userId);
        }

        return result;
    }

    public async Task<IdentityResult> SetUserRolesAsync(string userId, IEnumerable<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Utilisateur introuvable." });
        }

        // Retirer tous les rôles actuels
        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        
        if (!removeResult.Succeeded)
        {
            return removeResult;
        }

        // Ajouter les nouveaux rôles
        var addResult = await _userManager.AddToRolesAsync(user, roleNames);
        
        if (addResult.Succeeded)
        {
            _logger.LogInformation("Rôles mis à jour pour l'utilisateur {UserId}: {Roles}", 
                userId, string.Join(", ", roleNames));
        }

        return addResult;
    }

    #endregion

    #region Statistiques et recherche

    public async Task<UserStatisticsDto> GetUserStatisticsAsync()
    {
        var allUsers = await _userManager.Users.ToListAsync();
        var activeUsers = allUsers.Where(u => !u.LockoutEnabled || (u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow)).Count();
        var lockedUsers = allUsers.Count - activeUsers;
        var unconfirmedEmails = allUsers.Where(u => !u.EmailConfirmed).Count();

        var roles = await _roleManager.Roles.ToListAsync();
        var usersByRole = new Dictionary<string, int>();

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            usersByRole[role.Name!] = usersInRole.Count;
        }

        return new UserStatisticsDto
        {
            TotalUsers = allUsers.Count,
            ActiveUsers = activeUsers,
            LockedUsers = lockedUsers,
            UnconfirmedEmails = unconfirmedEmails,
            TotalRoles = roles.Count,
            UsersByRole = usersByRole
        };
    }

    public async Task<IEnumerable<UserDetailsDto>> SearchUsersAsync(string searchTerm)
    {
        var users = await _userManager.Users
            .Where(u => u.UserName!.Contains(searchTerm) || 
                       u.Email!.Contains(searchTerm) || 
                       (u.FullName != null && u.FullName.Contains(searchTerm)))
            .ToListAsync();

        var userDtos = new List<UserDetailsDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDetailsDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            });
        }

        return userDtos;
    }

    public async Task<IEnumerable<UserDetailsDto>> GetUsersByRoleAsync(string roleName)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        var userDtos = new List<UserDetailsDto>();

        foreach (var user in usersInRole)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDetailsDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            });
        }

        return userDtos.OrderBy(u => u.UserName);
    }

    #endregion
}