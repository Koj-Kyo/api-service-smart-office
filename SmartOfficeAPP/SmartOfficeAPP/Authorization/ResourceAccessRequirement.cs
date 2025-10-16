using Microsoft.AspNetCore.Authorization;
using SmartOfficeAPP.Enums;

namespace SmartOfficeAPP.Authorization;

/// <summary>
/// Requirement pour l'autorisation basée sur les ressources (Zones/Périphériques).
/// </summary>
public class ResourceAccessRequirement : IAuthorizationRequirement
{
    public ResourceAccessRequirement(DroitAccesType requiredAccessType)
    {
        RequiredAccessType = requiredAccessType;
    }

    public DroitAccesType RequiredAccessType { get; }
}
