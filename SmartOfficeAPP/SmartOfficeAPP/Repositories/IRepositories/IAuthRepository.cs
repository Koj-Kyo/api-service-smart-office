using Microsoft.AspNetCore.Identity;
using SmartOfficeAPP.Models.Identity;
namespace SmartOfficeAPP.Repositories.IRepositories;

public interface IAuthRepository
{
    // L'interface IAuthRepository définit les contrats pour la gestion des utilisateurs.
   

        // Enregistre un nouvel utilisateur.
        Task<IdentityResult> RegisterAsync(ApplicationUser user, string password);

        // Trouve un utilisateur par son adresse e-mail.
        Task<ApplicationUser> FindUserByEmailAsync(string email);

        // Vérifie les identifiants de l'utilisateur.
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

    // Enregistre le jeton de rafraîchissement d'un utilisateur.
    Task<bool> SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, DateTime expiryTime);

}
