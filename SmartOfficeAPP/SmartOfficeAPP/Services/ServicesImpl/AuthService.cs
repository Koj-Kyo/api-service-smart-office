using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Models.Identity;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace SmartOfficeAPP.Services.ServicesImpl;


public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(IAuthRepository authRepository, UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _authRepository = authRepository;
        _userManager = userManager;
        _configuration = configuration;
    }

    // Mise à jour pour retourner IdentityResult qui contient des erreurs détaillées
    public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
    {
        var user = new ApplicationUser 
        { 
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            EmailConfirmed = false // Sera confirmé par email ou Admin
        };
        var result = await _authRepository.RegisterAsync(user, registerDto.Password);
        return result;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _authRepository.FindUserByEmailAsync(loginDto.Email);
        if (user == null || !await _authRepository.CheckPasswordAsync(user, loginDto.Password))
        {
            return null;
        }

        // Génération du JWT d'accès et du jeton de rafraîchissement.
        var authResponse = await GenerateAuthTokensAsync(user);

        // Sauvegarde le jeton de rafraîchissement dans la base de données.
        await _authRepository.SaveRefreshTokenAsync(user, authResponse.RefreshToken, DateTime.UtcNow.AddDays(7));

        return authResponse;
    }

    private async Task<AuthResponseDto> GenerateAuthTokensAsync(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]);

        // Récupérer les rôles de l'utilisateur
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

        // Ajouter les rôles aux claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15), // Jeton d'accès de courte durée
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"]
        };

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = Guid.NewGuid().ToString(); // Génération d'un jeton de rafraîchissement simple

        return new AuthResponseDto
        {
            AccessToken = tokenHandler.WriteToken(accessToken),
            RefreshToken = refreshToken,
            UserId = user.Id
        };
    }
}