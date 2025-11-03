using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Distributed;
using SmartOfficeAPP.DTOs.Request;
using SmartOfficeAPP.DTOs.Response;
using SmartOfficeAPP.Services.IServices;
using System.Security.Claims;
using System.Text.Json;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // L'envoi de commandes est réservé à tout utilisateur connecté
public class CommandesController : ControllerBase
{
    private readonly ICommandeService _commandeService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CommandesController> _logger;

    public CommandesController(
        ICommandeService commandeService,
        IDistributedCache cache,
        ILogger<CommandesController> logger)
    {
        _commandeService = commandeService;
        _cache = cache;
        _logger = logger;
    }

    // POST: api/Commandes
    /// <summary>
    /// Envoie une commande à un périphérique IoT.
    /// ✅ Retourne ApiResponse<CommandeResponseDto>
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CommandeResponseDto>>> PostCommand([FromBody] CommandeRequestDto requestDto)
    {
        // Récupère l'ID de l'utilisateur à partir du jeton JWT.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            var errorResponse = new CommandeResponseDto
            {
                IsSuccess = false,
                Message = "Utilisateur non authentifié."
            };

            return Unauthorized(ApiResponse<CommandeResponseDto>.FromDatabase(errorResponse));
        }

        var response = await _commandeService.SendCommandAsync(userId, requestDto);

        // ✅ Envelopper dans ApiResponse
        var apiResponse = ApiResponse<CommandeResponseDto>.FromDatabase(response);

        if (!response.IsSuccess)
        {
            // Retourne un BadRequest si la commande a échoué.
            return BadRequest(apiResponse);
        }

        // Retourne un Ok si la commande a réussi.
        return Ok(apiResponse);
    }

    /// <summary>
    /// ✅ NOUVEAU: Récupère l'historique des commandes pour un utilisateur
    /// Support ?fresh=true et retourne ApiResponse<T>
    /// </summary>
    [HttpGet("my-commands")]
    [OutputCache(PolicyName = "DefaultPolicy")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CommandeResponseDto>>>> GetMyCommands(
        [FromQuery] bool fresh = false,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<IEnumerable<CommandeResponseDto>>.FromDatabase(
                Enumerable.Empty<CommandeResponseDto>()));
        }

        var cacheKey = $"commandes:user:{userId}:limit:{limit}:offset:{offset}";

        // ✅ Si fresh=true, bypasser le cache
        if (!fresh)
        {
            var cachedData = await GetFromCacheAsync<IEnumerable<CommandeResponseDto>>(cacheKey);
            if (cachedData != null)
            {
                return Ok(ApiResponse<IEnumerable<CommandeResponseDto>>.FromCache(
                    cachedData,
                    DateTime.UtcNow.AddMinutes(-3)
                ));
            }
        }

        // TODO: Implémenter ICommandeService.GetCommandsByUserAsync()
        // var commands = await _commandeService.GetCommandsByUserAsync(userId, limit, offset);
        var commands = Enumerable.Empty<CommandeResponseDto>(); // Placeholder

        // Mise en cache
        if (!fresh)
        {
            await SetCacheAsync(cacheKey, commands, TimeSpan.FromMinutes(3));
        }

        var response = ApiResponse<IEnumerable<CommandeResponseDto>>.FromDatabase(commands);
        return Ok(response);
    }

    /// <summary>
    /// ✅ NOUVEAU: Récupère l'historique des commandes pour un périphérique
    /// Support ?fresh=true et retourne ApiResponse<T>
    /// </summary>
    [HttpGet("by-peripherique/{peripheriqueId}")]
    [OutputCache(PolicyName = "DefaultPolicy")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CommandeResponseDto>>>> GetCommandsByPeripherique(
        int peripheriqueId,
        [FromQuery] bool fresh = false,
        [FromQuery] int limit = 50)
    {
        var cacheKey = $"commandes:peripherique:{peripheriqueId}:limit:{limit}";

        // ✅ Si fresh=true, bypasser le cache
        if (!fresh)
        {
            var cachedData = await GetFromCacheAsync<IEnumerable<CommandeResponseDto>>(cacheKey);
            if (cachedData != null)
            {
                return Ok(ApiResponse<IEnumerable<CommandeResponseDto>>.FromCache(
                    cachedData,
                    DateTime.UtcNow.AddMinutes(-3)
                ));
            }
        }

        // TODO: Implémenter ICommandeService.GetCommandsByPeripheriqueAsync()
        // var commands = await _commandeService.GetCommandsByPeripheriqueAsync(peripheriqueId, limit);
        var commands = Enumerable.Empty<CommandeResponseDto>(); // Placeholder

        // Mise en cache
        if (!fresh)
        {
            await SetCacheAsync(cacheKey, commands, TimeSpan.FromMinutes(3));
        }

        var response = ApiResponse<IEnumerable<CommandeResponseDto>>.FromDatabase(commands);
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
}