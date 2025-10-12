using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;


namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(registerDto);

        if (!result.Succeeded)
        {
            // Nous retournons maintenant les messages d'erreur précis d'Identity.
            return BadRequest(result.Errors.Select(e => e.Description));
        }

        return Ok(new { message = "L'utilisateur a été enregistré avec succès. Veuillez vous connecter." });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var authResponse = await _authService.LoginAsync(loginDto);

        if (authResponse == null)
        {
            return Unauthorized("Identifiants de connexion invalides.");
        }

        return Ok(authResponse);
    }

    /// <summary>
    /// Endpoint de test pour vérifier l'autorisation
    /// </summary>
    [HttpGet("test-auth")]
    [Authorize]
    public IActionResult TestAuth()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        return Ok(new
        {
            Message = "Autorisation réussie !",
            UserId = userId,
            Email = email,
            Roles = roles,
            IsAuthenticated = User.Identity?.IsAuthenticated,
            AuthenticationType = User.Identity?.AuthenticationType,
            AllClaims = allClaims
        });
    }

    /// <summary>
    /// Endpoint de test pour vérifier l'autorisation Admin
    /// </summary>
    [HttpGet("test-admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult TestAdmin()
    {
        return Ok(new { Message = "Vous êtes bien Admin !" });
    }
}
