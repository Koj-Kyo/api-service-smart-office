using Microsoft.AspNetCore.Identity;
using SmartOfficeAPP.Models.Identity;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Utilise UserManager pour créer un utilisateur avec un mot de passe.
    public async Task<IdentityResult> RegisterAsync(ApplicationUser user, string password)
    {
        return await _userManager.CreateAsync(user, password);
    }

    // Utilise UserManager pour trouver un utilisateur par son adresse e-mail.
    public async Task<ApplicationUser> FindUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    // Utilise UserManager pour vérifier le mot de passe de l'utilisateur.
    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    // Sauvegarde le jeton de rafraîchissement et sa date d'expiration pour l'utilisateur.
    public async Task<bool> SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expiryTime)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiryTime;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
