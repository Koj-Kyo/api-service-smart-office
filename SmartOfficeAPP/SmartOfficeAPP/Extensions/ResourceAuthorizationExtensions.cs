using Microsoft.AspNetCore.Authorization;
using SmartOfficeAPP.Authorization;
using SmartOfficeAPP.Enums;
using System.Security.Claims;

namespace SmartOfficeAPP.Extensions;

/// <summary>
/// Extensions pour simplifier l'utilisation de l'autorisation basée sur les ressources.
/// </summary>
public static class ResourceAuthorizationExtensions
{
    /// <summary>
    /// Vérifie si l'utilisateur a le droit d'accès requis sur une zone.
    /// </summary>
    public static async Task<bool> CheckZoneAccessAsync(
        this IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        int zoneId,
        DroitAccesType requiredAccessType)
    {
        var resource = new ResourceAccessContext { ZoneId = zoneId };
        var requirement = new ResourceAccessRequirement(requiredAccessType);

        var result = await authorizationService.AuthorizeAsync(user, resource, requirement);
        return result.Succeeded;
    }

    /// <summary>
    /// Vérifie si l'utilisateur a le droit d'accès requis sur un périphérique.
    /// </summary>
    public static async Task<bool> CheckPeripheriqueAccessAsync(
        this IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        int peripheriqueId,
        DroitAccesType requiredAccessType)
    {
        var resource = new ResourceAccessContext { PeripheriqueLogiqueId = peripheriqueId };
        var requirement = new ResourceAccessRequirement(requiredAccessType);

        var result = await authorizationService.AuthorizeAsync(user, resource, requirement);
        return result.Succeeded;
    }
}
