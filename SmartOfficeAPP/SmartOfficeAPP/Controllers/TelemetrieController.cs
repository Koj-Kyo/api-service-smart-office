using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Distributed;
using SmartOfficeAPP.Authorization;
using SmartOfficeAPP.Enums;
using SmartOfficeAPP.Extensions;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;
using System.Text.Json;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // L'accès à la télémétrie est réservé à tout utilisateur connecté
public class TelemetriesController : ControllerBase
{
    private readonly ITelemetrieService _telemetrieService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TelemetriesController> _logger;

    public TelemetriesController(
        ITelemetrieService telemetrieService,
        IAuthorizationService authorizationService,
        IDistributedCache cache,
     ILogger<TelemetriesController> logger)
    {
        _telemetrieService = telemetrieService;
        _authorizationService = authorizationService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique logique spécifique.
    /// Vérifie les droits d'accès en lecture sur le périphérique.
    /// ✅ Support ?fresh=true et retourne ApiResponse<T>
    /// </summary>
    [HttpGet("peripherique")]
    [OutputCache(PolicyName = "TelemetryData")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TelemetrieDto>>>> GetTelemetriesByPeripheriqueIdAsync(
        [FromQuery] int peripheriqueId,
   [FromQuery] bool fresh = false)
    {
        // Vérification des droits d'accès en lecture
        var hasAccess = await _authorizationService.CheckPeripheriqueAccessAsync(
            User,
      peripheriqueId,
        DroitAccesType.Lecture);

 if (!hasAccess)
        {
          _logger.LogWarning(
   "Accès refusé à la télémétrie pour périphérique {PeripheriqueId} par userId={UserId}",
        peripheriqueId,
    User.FindFirstValue(ClaimTypes.NameIdentifier));

    return Forbid();
        }

        var cacheKey = $"telemetries:peripherique:{peripheriqueId}";

        // ✅ Si fresh=true, bypasser le cache et aller directement en DB
 if (!fresh)
    {
    var cachedData = await GetFromCacheAsync<IEnumerable<TelemetrieDto>>(cacheKey);
  if (cachedData != null)
            {
     return Ok(ApiResponse<IEnumerable<TelemetrieDto>>.FromCache(
  cachedData, 
 DateTime.UtcNow.AddMinutes(-1) // Approximation du dernier update
      ));
            }
    }

        // Aller chercher les données fraîches en DB
        var telemetries = await _telemetrieService.GetTelemetriesByPeripheriqueIdAsync(peripheriqueId);
        
        // Mise en cache pour les prochaines requêtes
    if (!fresh)
        {
          await SetCacheAsync(cacheKey, telemetries, TimeSpan.FromMinutes(1));
    }

  var response = fresh 
            ? ApiResponse<IEnumerable<TelemetrieDto>>.FromDatabase(telemetries)
   : ApiResponse<IEnumerable<TelemetrieDto>>.FromDatabase(telemetries);

      return Ok(response);
    }

    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique spécifique.
    /// ✅ Support ?fresh=true et retourne ApiResponse<T>
    /// </summary>
    [HttpGet("by-peripherique/{peripheriqueId}")]
    [OutputCache(PolicyName = "TelemetryData")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TelemetrieDto>>>> GetTelemetriesByPeripheriqueId(
        int peripheriqueId,
        [FromQuery] bool fresh = false)
    {
        return await GetTelemetriesByPeripheriqueIdAsync(peripheriqueId, fresh);
    }

/// <summary>
 /// Ajoute un nouvel enregistrement de télémétrie.
    /// ✅ Retourne ApiResponse<T>
    /// </summary>
    [HttpPost("add")]
    public async Task<ActionResult<ApiResponse<Telemetrie>>> AddTelemetryAsync([FromBody] Telemetrie telemetrie)
    {
        if (telemetrie == null)
        {
    return BadRequest(ApiResponse<Telemetrie>.FromDatabase(null!, DateTime.UtcNow));
        }

 // Assurer que la date est en UTC pour la compatibilité avec PostgreSQL
        if (telemetrie.Horodatage.Kind == DateTimeKind.Unspecified)
        {
   telemetrie.Horodatage = DateTime.SpecifyKind(telemetrie.Horodatage, DateTimeKind.Utc);
     }
     else if (telemetrie.Horodatage.Kind == DateTimeKind.Local)
{
  telemetrie.Horodatage = telemetrie.Horodatage.ToUniversalTime();
  }

        await _telemetrieService.AddTelemetryAsync(telemetrie);
  
        // ✅ Invalider le cache après insertion
        await InvalidateTelemetryCacheAsync(telemetrie.PeripheriqueLogiqueId);

   var response = ApiResponse<Telemetrie>.FromDatabase(telemetrie);
        return CreatedAtAction(nameof(GetTelemetriesByPeripheriqueIdAsync), 
      new { peripheriqueId = telemetrie.PeripheriqueLogiqueId }, response);
    }

    /// <summary>
    /// Récupère les données historiques de télémétrie dans une plage de dates.
    /// ✅ Support ?fresh=true et retourne ApiResponse<T>
    /// </summary>
    [HttpGet("history")]
    [OutputCache(PolicyName = "TelemetryData")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TelemetrieDto>>>> GetHistoricalDataAsync(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int? peripheriqueId,
        [FromQuery] bool fresh = false)
    {
        // Convertir les dates en UTC pour la compatibilité avec PostgreSQL
        var startDateUtc = startDate.Kind == DateTimeKind.Unspecified 
         ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) 
     : startDate.ToUniversalTime();
        
   var endDateUtc = endDate.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) 
            : endDate.ToUniversalTime();

        var cacheKey = $"telemetries:history:{startDateUtc:yyyyMMdd}:{endDateUtc:yyyyMMdd}:{peripheriqueId}";

 // ✅ Si fresh=true, bypasser le cache
        if (!fresh)
        {
    var cachedData = await GetFromCacheAsync<IEnumerable<TelemetrieDto>>(cacheKey);
      if (cachedData != null)
         {
   return Ok(ApiResponse<IEnumerable<TelemetrieDto>>.FromCache(
              cachedData, 
     DateTime.UtcNow.AddMinutes(-2)
     ));
       }
        }

     var historicalData = await _telemetrieService.GetHistoricalDataAsync(startDateUtc, endDateUtc, peripheriqueId);
     
        // Mise en cache pour les données historiques (plus longue durée)
      if (!fresh)
        {
            await SetCacheAsync(cacheKey, historicalData, TimeSpan.FromMinutes(5));
     }

var response = ApiResponse<IEnumerable<TelemetrieDto>>.FromDatabase(historicalData);
        return Ok(response);
    }

    /// <summary>
    /// Stocke une suggestion générée par le moteur IA.
    /// ✅ Retourne ApiResponse<T>
    /// </summary>
    [HttpPost("suggestions")]
    public async Task<ActionResult<ApiResponse<string>>> StoreAISuggestionAsync([FromBody] Suggestion suggestion)
  {
        if (suggestion == null)
     {
    return BadRequest(ApiResponse<string>.FromDatabase("Les données de suggestion sont invalides."));
  }
        
        await _telemetrieService.StoreAISuggestionAsync(suggestion);
        
        var response = ApiResponse<string>.FromDatabase("Suggestion stockée avec succès.");
        return Ok(response);
    }

    // ✅ Helper methods pour gestion du cache
    private async Task<T?> GetFromCacheAsync<T>(string key) where T : class
    {
        try
        {
            var cached = await _cache.GetStringAsync(key);
    if (cached != null)
  {
     return JsonSerializer.Deserialize<T>(cached);
            }
        }
     catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de la lecture du cache pour la clé {CacheKey}", key);
        }
        return null;
  }

    private async Task SetCacheAsync<T>(string key, T data, TimeSpan expiration)
    {
        try
        {
   var json = JsonSerializer.Serialize(data);
     var options = new DistributedCacheEntryOptions
       {
     AbsoluteExpirationRelativeToNow = expiration
     };
    await _cache.SetStringAsync(key, json, options);
        }
        catch (Exception ex)
      {
      _logger.LogWarning(ex, "Erreur lors de la mise en cache pour la clé {CacheKey}", key);
        }
    }

    private async Task InvalidateTelemetryCacheAsync(int peripheriqueId)
    {
     try
   {
            var keysToRemove = new[]
       {
      $"telemetries:peripherique:{peripheriqueId}",
       "telemetries:history:*" // Pattern pour les données historiques
     };

            foreach (var key in keysToRemove)
 {
        await _cache.RemoveAsync(key);
          }
      }
        catch (Exception ex)
        {
         _logger.LogWarning(ex, "Erreur lors de l'invalidation du cache télémétrie");
        }
}
}

