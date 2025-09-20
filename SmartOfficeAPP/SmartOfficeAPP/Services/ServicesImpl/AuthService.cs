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
    private readonly IConfiguration _configuration;

    public AuthService(IAuthRepository authRepository, IConfiguration configuration)
    {
        _authRepository = authRepository;
        _configuration = configuration;
    }

    // Mise à jour pour retourner IdentityResult qui contient des erreurs détaillées
    public async Task<IdentityResult> RegisterAsync(RegisterDto registerDto)
    {
        var user = new ApplicationUser { UserName = registerDto.Email, Email = registerDto.Email };
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
        var authResponse = GenerateAuthTokens(user);

        // Sauvegarde le jeton de rafraîchissement dans la base de données.
        await _authRepository.SaveRefreshTokenAsync(user, authResponse.RefreshToken, DateTime.UtcNow.AddDays(7));

        return authResponse;
    }

    private AuthResponseDto GenerateAuthTokens(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Secret"]);

        var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
            };

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