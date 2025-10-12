using Microsoft.AspNetCore.Identity;
using SmartOfficeAPP.Models.Identity;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class DataSeedingService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DataSeedingService> _logger;

    public DataSeedingService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DataSeedingService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Créer les rôles de base
            await CreateRoleAsync("Admin", "Administrateur système avec tous les droits");
            await CreateRoleAsync("Manager", "Gestionnaire avec droits étendus");
            await CreateRoleAsync("User", "Utilisateur standard");
            await CreateRoleAsync("Technician", "Technicien pour la maintenance IoT");

            // Créer un utilisateur Admin par défaut
            await CreateDefaultAdminAsync();
            await CreateDefaultManagerAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du seeding des données initiales");
            throw;
        }
    }

    private async Task CreateRoleAsync(string roleName, string? description = null)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Rôle '{RoleName}' créé avec succès", roleName);
            }
            else
            {
                _logger.LogError("Erreur lors de la création du rôle '{RoleName}': {Errors}", 
                    roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            _logger.LogDebug("Rôle '{RoleName}' existe déjà", roleName);
        }
    }

    private async Task CreateDefaultAdminAsync()
    {
        const string adminEmail = "adminer@smartoffice.com";
        const string adminPassword = "Admin1234!";
        const string adminUserName = "admin";

        var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                FullName = "Administrateur Système",
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(adminUser, adminPassword);
            if (createResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(adminUser, "Admin");
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Utilisateur Admin par défaut créé: {Email}", adminEmail);
                }
                else
                {
                    _logger.LogError("Erreur lors de l'assignation du rôle Admin à {Email}: {Errors}", 
                        adminEmail, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogError("Erreur lors de la création de l'utilisateur Admin: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            _logger.LogDebug("Utilisateur Admin existe déjà: {Email}", adminEmail);
        }
    }

    private async Task CreateDefaultManagerAsync()
    {
        const string managerEmail = "manager@smartoffice.com";
        const string managerPassword = "Manager123!";
        const string managerUserName = "manager";

        var existingManager = await _userManager.FindByEmailAsync(managerEmail);
        if (existingManager == null)
        {
            var managerUser = new ApplicationUser
            {
                UserName = managerUserName,
                Email = managerEmail,
                FullName = "Gestionnaire Système",
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(managerUser, managerPassword);
            if (createResult.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(managerUser, "Manager");
                if (roleResult.Succeeded)
                {
                    _logger.LogInformation("Utilisateur Manager par défaut créé: {Email}", managerEmail);
                }
                else
                {
                    _logger.LogError("Erreur lors de l'assignation du rôle Manager à {Email}: {Errors}", 
                        managerEmail, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogError("Erreur lors de la création de l'utilisateur Manager: {Errors}", 
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            _logger.LogDebug("Utilisateur Manager existe déjà: {Email}", managerEmail);
        }
    }
}