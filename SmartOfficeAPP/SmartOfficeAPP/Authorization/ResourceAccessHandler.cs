using Microsoft.AspNetCore.Authorization;
using SmartOfficeAPP.Enums;
using SmartOfficeAPP.Repositories.IRepositories;
using System.Security.Claims;

namespace SmartOfficeAPP.Authorization;

/// <summary>
/// Handler pour l'autorisation basée sur les ressources.
/// Vérifie si l'utilisateur a les droits d'accès requis sur une Zone ou un Périphérique.
/// Inclut un fallback: si l'utilisateur a le droit sur la Zone, il l'a sur tous ses périphériques.
/// </summary>
public class ResourceAccessHandler : AuthorizationHandler<ResourceAccessRequirement, ResourceAccessContext>
{
    private readonly IDroitAccesRepository _droitAccesRepository;
    private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
    private readonly ILogger<ResourceAccessHandler> _logger;

    public ResourceAccessHandler(
        IDroitAccesRepository droitAccesRepository,
        IPeripheriqueLogiqueRepository peripheriqueRepository,
        ILogger<ResourceAccessHandler> logger)
    {
        _droitAccesRepository = droitAccesRepository;
        _peripheriqueRepository = peripheriqueRepository;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceAccessRequirement requirement,
        ResourceAccessContext resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Utilisateur non identifié lors de la vérification des droits d'accès");
            return;
        }

        // Bypass pour les admins
        if (context.User.IsInRole("Admin"))
        {
            _logger.LogDebug("Admin bypass pour userId={UserId}", userId);
            context.Succeed(requirement);
            return;
        }

        try
        {
            bool hasAccess = false;

            if (resource.ZoneId.HasValue)
            {
                hasAccess = await CheckZoneAccessAsync(userId, resource.ZoneId.Value, requirement.RequiredAccessType);
            }
            else if (resource.PeripheriqueLogiqueId.HasValue)
            {
                hasAccess = await CheckPeripheriqueAccessAsync(userId, resource.PeripheriqueLogiqueId.Value, requirement.RequiredAccessType);
            }

            if (hasAccess)
            {
                _logger.LogDebug(
                    "Accès accordé pour userId={UserId} sur {ResourceType}={ResourceId} avec type={AccessType}",
                    userId,
                    resource.ZoneId.HasValue ? "Zone" : "Périphérique",
                    resource.ZoneId ?? resource.PeripheriqueLogiqueId,
                    requirement.RequiredAccessType);

                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning(
                    "Accès refusé pour userId={UserId} sur {ResourceType}={ResourceId} avec type={AccessType}",
                    userId,
                    resource.ZoneId.HasValue ? "Zone" : "Périphérique",
                    resource.ZoneId ?? resource.PeripheriqueLogiqueId,
                    requirement.RequiredAccessType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification des droits d'accès pour userId={UserId}", userId);
        }
    }

    private async Task<bool> CheckZoneAccessAsync(string userId, int zoneId, DroitAccesType requiredType)
    {
        var droits = await _droitAccesRepository.GetDroitsAccesByUserZonesAsync(userId);
        var droit = droits.FirstOrDefault(d => d.ZoneId == zoneId);

        if (droit == null)
            return false;

        // Si on demande Lecture, Lecture ou Controle suffisent
        // Si on demande Controle, seul Controle suffit
        return requiredType == DroitAccesType.Lecture
            ? droit.Type == DroitAccesType.Lecture || droit.Type == DroitAccesType.Controle
            : droit.Type == DroitAccesType.Controle;
    }

    private async Task<bool> CheckPeripheriqueAccessAsync(string userId, int peripheriqueId, DroitAccesType requiredType)
    {
        // 1) Vérifier le droit direct sur le périphérique
        var droitsPeripheriques = await _droitAccesRepository.GetDroitsAccesByUserPeripheriquesAsync(userId);
        var droitDirect = droitsPeripheriques.FirstOrDefault(d => d.PeripheriqueLogiqueId == peripheriqueId);

        if (droitDirect != null)
        {
            bool directAccess = requiredType == DroitAccesType.Lecture
                ? droitDirect.Type == DroitAccesType.Lecture || droitDirect.Type == DroitAccesType.Controle
                : droitDirect.Type == DroitAccesType.Controle;

            if (directAccess)
                return true;
        }

        // 2) Fallback: vérifier le droit sur la zone parente
        var peripherique = await _peripheriqueRepository.GetByIdAsync(peripheriqueId);
        if (peripherique == null)
        {
            _logger.LogWarning("Périphérique {PeripheriqueId} introuvable pour fallback zone", peripheriqueId);
            return false;
        }

        var droitsZones = await _droitAccesRepository.GetDroitsAccesByUserZonesAsync(userId);
        var droitZone = droitsZones.FirstOrDefault(d => d.ZoneId == peripherique.ZoneId);

        if (droitZone == null)
            return false;

        bool zoneAccess = requiredType == DroitAccesType.Lecture
            ? droitZone.Type == DroitAccesType.Lecture || droitZone.Type == DroitAccesType.Controle
            : droitZone.Type == DroitAccesType.Controle;

        if (zoneAccess)
        {
            _logger.LogDebug(
                "Accès accordé via fallback zone {ZoneId} pour périphérique {PeripheriqueId}",
                peripherique.ZoneId,
                peripheriqueId);
        }

        return zoneAccess;
    }
}

/// <summary>
/// Contexte de ressource passé au handler.
/// </summary>
public class ResourceAccessContext
{
    public int? ZoneId { get; set; }
    public int? PeripheriqueLogiqueId { get; set; }
}
