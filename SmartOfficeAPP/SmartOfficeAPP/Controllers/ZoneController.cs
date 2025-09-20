using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;


namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")] // Sécurise toutes les actions pour les administrateurs
public class ZonesController : ControllerBase
{
    private readonly IZoneService _zoneService;
    private readonly IMapper _mapper;

    public ZonesController(IZoneService zoneService, IMapper mapper)
    {
        _zoneService = zoneService;
        _mapper = mapper;
    }

    // GET: api/Zones
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ZoneDto>>> GetZones()
    {
        var zones = await _zoneService.GetAllZonesAsync();
        return Ok(zones);
    }

    // GET: api/Zones/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ZoneDto>> GetZone(int id)
    {
        var zone = await _zoneService.GetZoneByIdAsync(id);
        if (zone == null)
        {
            return NotFound();
        }
        return Ok(zone);
    }

    // POST: api/Zones
    [HttpPost]
    public async Task<ActionResult<ZoneDto>> PostZone([FromBody] ZoneCreationDto zoneCreationDto)
    {
        var createdZone = await _zoneService.CreateZoneAsync(zoneCreationDto);
        return CreatedAtAction("GetZone", new { id = createdZone.Id }, createdZone);
    }

    // PUT: api/Zones/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutZone(int id, [FromBody] ZoneUpdateDto zoneUpdateDto)
    {
        var result = await _zoneService.UpdateZoneAsync(id, zoneUpdateDto);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // DELETE: api/Zones/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteZone(int id)
    {
        var result = await _zoneService.DeleteZoneAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
