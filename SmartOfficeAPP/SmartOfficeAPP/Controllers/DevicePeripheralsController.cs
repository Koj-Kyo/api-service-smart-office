using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DevicePeripheralsController : ControllerBase
{
    private readonly IDevicePeripheralService _service;
    private readonly ILogger<DevicePeripheralsController> _logger;

    public DevicePeripheralsController(
        IDevicePeripheralService service,
        ILogger<DevicePeripheralsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// GET: api/DevicePeripherals
    /// Récupère tous les périphériques physiques enregistrés
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DevicePeripheralDto>>> GetAll()
    {
        var peripherals = await _service.GetAllAsync();
        return Ok(peripherals);
    }

    /// <summary>
    /// GET: api/DevicePeripherals/{id}
    /// Récupère un périphérique par ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DevicePeripheralDto>> GetById(int id)
    {
        var peripheral = await _service.GetByIdAsync(id);
        if (peripheral == null)
            return NotFound(new { message = $"Périphérique {id} introuvable." });

        return Ok(peripheral);
    }

    /// <summary>
    /// GET: api/DevicePeripherals/device/{iotDeviceId}
    /// Récupère tous les périphériques d'un device IoT
    /// </summary>
    [HttpGet("device/{iotDeviceId}")]
    public async Task<ActionResult<IEnumerable<DevicePeripheralDto>>> GetByDevice(string iotDeviceId)
    {
        var peripherals = await _service.GetByIotDeviceIdAsync(iotDeviceId);
        return Ok(peripherals);
    }

    /// <summary>
    /// GET: api/DevicePeripherals/device/{iotDeviceId}/name/{name}
    /// Récupère un périphérique par nom sur un device
    /// </summary>
    [HttpGet("device/{iotDeviceId}/name/{name}")]
    public async Task<ActionResult<DevicePeripheralDto>> GetByName(string iotDeviceId, string name)
    {
        var peripheral = await _service.GetByNameAsync(iotDeviceId, name);
        if (peripheral == null)
            return NotFound(new { message = $"Périphérique '{name}' introuvable sur device '{iotDeviceId}'." });

        return Ok(peripheral);
    }

    /// <summary>
    /// POST: api/DevicePeripherals
    /// Crée un nouveau périphérique physique et synchronise avec IoT Hub
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DevicePeripheralDto>> Create([FromBody] DevicePeripheralCreationDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du périphérique {Name}", dto.Name);
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// PUT: api/DevicePeripherals/{id}
    /// Met à jour un périphérique et synchronise avec IoT Hub
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DevicePeripheralUpdateDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = $"Périphérique {id} introuvable." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du périphérique {Id}", id);
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// DELETE: api/DevicePeripherals/{id}
    /// Supprime un périphérique et synchronise avec IoT Hub
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Périphérique {id} introuvable." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du périphérique {Id}", id);
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// POST: api/DevicePeripherals/device/{iotDeviceId}/sync
    /// Force la synchronisation des périphériques avec IoT Hub desired properties
    /// </summary>
    [HttpPost("device/{iotDeviceId}/sync")]
    public async Task<IActionResult> SyncWithIotHub(string iotDeviceId)
    {
        try
        {
            var success = await _service.SyncWithDesiredPropertiesAsync(iotDeviceId);
            if (!success)
                return StatusCode(500, new { message = "Échec de la synchronisation avec IoT Hub." });

            return Ok(new { message = $"Synchronisation réussie pour device '{iotDeviceId}'." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation du device {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    // =============================
    // ? NOUVEAUX: Endpoints enrichis pour app mobile
    // =============================

    /// <summary>
    /// GET: api/DevicePeripherals/enriched
    /// ?? MOBILE: Récupère tous les périphériques enrichis (DB + IoT Hub + Zone)
    /// Support: ?fresh=true pour bypass cache
    /// </summary>
    [HttpGet("enriched")]
    [AllowAnonymous] // Accessible à tous les utilisateurs authentifiés (pas seulement Admin)
    [Authorize]
    [OutputCache(PolicyName = "DeviceData")]
    public async Task<ActionResult<IEnumerable<DevicePeripheralEnrichedDto>>> GetAllEnriched()
    {
        try
        {
            var peripherals = await _service.GetAllEnrichedAsync();
            return Ok(peripherals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des périphériques enrichis");
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// GET: api/DevicePeripherals/enriched/{id}
    /// ?? MOBILE: Récupère un périphérique enrichi par ID
    /// </summary>
    [HttpGet("enriched/{id}")]
    [AllowAnonymous]
    [Authorize]
    [OutputCache(PolicyName = "DeviceData")]
    public async Task<ActionResult<DevicePeripheralEnrichedDto>> GetEnrichedById(int id)
    {
        try
        {
            var peripheral = await _service.GetEnrichedByIdAsync(id);
            if (peripheral == null)
                return NotFound(new { message = $"Périphérique {id} introuvable." });

            return Ok(peripheral);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du périphérique enrichi {Id}", id);
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// GET: api/DevicePeripherals/enriched/device/{iotDeviceId}
    /// ?? MOBILE: Récupère tous les périphériques enrichis d'un device IoT Hub
    /// </summary>
    [HttpGet("enriched/device/{iotDeviceId}")]
    [AllowAnonymous]
    [Authorize]
    [OutputCache(PolicyName = "DeviceData")]
    public async Task<ActionResult<IEnumerable<DevicePeripheralEnrichedDto>>> GetEnrichedByDevice(string iotDeviceId)
    {
        try
        {
            var peripherals = await _service.GetEnrichedByIotDeviceIdAsync(iotDeviceId);
            return Ok(peripherals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des périphériques enrichis pour {DeviceId}", iotDeviceId);
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// GET: api/DevicePeripherals/enriched/zone/{zoneId}
    /// ?? MOBILE: Récupère tous les périphériques enrichis d'une zone
    /// </summary>
    [HttpGet("enriched/zone/{zoneId}")]
    [AllowAnonymous]
    [Authorize]
    [OutputCache(PolicyName = "DeviceData")]
    public async Task<ActionResult<IEnumerable<DevicePeripheralEnrichedDto>>> GetEnrichedByZone(int zoneId)
    {
        try
        {
            var peripherals = await _service.GetEnrichedByZoneIdAsync(zoneId);
            return Ok(peripherals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des périphériques enrichis pour la zone {ZoneId}", zoneId);
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }

    /// <summary>
    /// GET: api/DevicePeripherals/stats
    /// ?? MOBILE: Récupère les statistiques agrégées des périphériques
    /// Retourne: totalDevices, activeDevicesCount, zonesCount, countsByType, actionableDevicesCount, etc.
    /// </summary>
    [HttpGet("stats")]
    [AllowAnonymous]
    [Authorize]
    [OutputCache(PolicyName = "DeviceData")]
    public async Task<ActionResult<DevicePeripheralsStatsDto>> GetStats()
    {
        try
        {
            var stats = await _service.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul des statistiques");
            return StatusCode(500, new { message = "Erreur interne du serveur." });
        }
    }
}
