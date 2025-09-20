using Microsoft.AspNetCore.Identity;

namespace SmartOfficeAPP.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    // Un champ pour stocker le jeton de rafraîchissement de l'utilisateur.
    public string? RefreshToken { get; set; }

    // Un champ pour stocker la date d'expiration du jeton de rafraîchissement.
    public DateTime? RefreshTokenExpiryTime { get; set; }

}
