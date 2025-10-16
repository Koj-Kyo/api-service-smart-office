using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
