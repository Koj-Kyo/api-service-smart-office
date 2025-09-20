using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;


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
}
