using Microsoft.AspNetCore.Identity;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Models.Identity;

namespace SmartOfficeAPP.Services.IServices;

public interface IUserManagementService
{
    // Gestion des utilisateurs
    Task<IEnumerable<UserDetailsDto>> GetAllUsersAsync();
    Task<UserDetailsDto?> GetUserByIdAsync(string userId);
    Task<UserDetailsDto?> GetUserByEmailAsync(string email);
    Task<IdentityResult> CreateUserAsync(CreateUserDto createUserDto);
    Task<IdentityResult> UpdateUserAsync(string userId, UpdateUserDto updateUserDto);
    Task<IdentityResult> DeleteUserAsync(string userId);
    Task<IdentityResult> ToggleUserActiveStatusAsync(string userId);

    // Gestion des rôles
    Task<IEnumerable<RoleDto>> GetAllRolesAsync();
    Task<IdentityResult> CreateRoleAsync(CreateRoleDto createRoleDto);
    Task<IdentityResult> UpdateRoleAsync(string roleId, UpdateRoleDto updateRoleDto);
    Task<IdentityResult> DeleteRoleAsync(string roleId);

    // Association utilisateurs-rôles
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName);
    Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName);
    Task<IdentityResult> SetUserRolesAsync(string userId, IEnumerable<string> roleNames);

    // Statistiques et recherche
    Task<UserStatisticsDto> GetUserStatisticsAsync();
    Task<IEnumerable<UserDetailsDto>> SearchUsersAsync(string searchTerm);
    Task<IEnumerable<UserDetailsDto>> GetUsersByRoleAsync(string roleName);
}