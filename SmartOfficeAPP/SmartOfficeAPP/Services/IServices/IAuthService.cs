using Microsoft.AspNetCore.Identity;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;

namespace SmartOfficeAPP.Services.IServices;

public interface IAuthService
{
    // Enregistre un nouvel utilisateur et retourne un booléen de succès.
    Task<IdentityResult> RegisterAsync(RegisterDto registerDto);

    // Authentifie un utilisateur et retourne un JWT s'il réussit, sinon null.
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);


}
