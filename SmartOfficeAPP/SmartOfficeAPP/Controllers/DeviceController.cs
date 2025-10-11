using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using System.Text.Json;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] // Sécurise toutes les actions pour les administrateurs
public class DevicesController : ControllerBase
{
    private readonly IPeripheriqueLogiqueService _peripheriqueService;
    private readonly IIotHubService _iotHubService;
    private readonly IMapper _mapper;

    public DevicesController(IPeripheriqueLogiqueService peripheriqueService, IIotHubService iotHubService, IMapper mapper)
    {
        _peripheriqueService = peripheriqueService;
        _iotHubService = iotHubService;
        _mapper = mapper;
    }

    // GET: api/Devices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PeripheriqueLogiqueDto>>> GetDevices()
    {
        return new ObjectResult("Cette action n'est pas encore implémentée selon le plan.");
    }

    // GET: api/Devices/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PeripheriqueLogiqueDto>> GetDevice(int id)
    {
        var peripherique = await _peripheriqueService.GetByIdAsync(id);
        if (peripherique == null)
        {
            return NotFound();
        }
        return Ok(peripherique);
    }

    // GET: api/zones/{zoneId}/devices
    [HttpGet("/api/zones/{zoneId}/devices")]
    public async Task<ActionResult<IEnumerable<PeripheriqueLogiqueDto>>> GetDevicesByZone(int zoneId)
    {
        var peripheriques = await _peripheriqueService.GetPeripheriquesByZoneIdAsync(zoneId);
        return Ok(peripheriques);
    }

    // POST: api/Devices
    [HttpPost]
    public async Task<ActionResult<PeripheriqueLogiqueDto>> PostDevice([FromBody] PeripheriqueLogiqueCreationDto creationDto)
    {
        var createdPeripherique = await _peripheriqueService.CreateAsync(creationDto);
        return CreatedAtAction("GetDevice", new { id = createdPeripherique.Id }, createdPeripherique);
    }

    // PUT: api/Devices/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDevice(int id, [FromBody] PeripheriqueLogiqueUpdateDto updateDto)
    {
        var result = await _peripheriqueService.UpdateAsync(id, updateDto);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // DELETE: api/Devices/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var result = await _peripheriqueService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // POST: api/devices/{iotDeviceId}/desired
    // Exemple de body JSON:
    // {
    //   "peripherals": [
    //     { "name":"relay4","type":"relay","gpio":33 },
    //     { "name":"mq2","type":"smoke","gpio":32 }
    //   ]
    // }
    [HttpPost("{iotDeviceId}/desired")]
    public async Task<IActionResult> SetDesiredProperties(string iotDeviceId, [FromBody] object desired)
    {
        if (desired is null) return BadRequest("Payload manquant.");
        var desiredJson = JsonSerializer.Serialize(desired);
        var ok = await _iotHubService.UpdateDesiredPropertiesAsync(iotDeviceId, desiredJson);
        if (!ok) return StatusCode(500, "Échec de mise à jour des desired properties.");
        return Ok(new { message = "Desired properties mises à jour." });
    }
}
