using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DeviceLogsController : ControllerBase
{
    private readonly IDeviceLogService _deviceLogService;
    private readonly ILogger<DeviceLogsController> _logger;

    public DeviceLogsController(IDeviceLogService deviceLogService, ILogger<DeviceLogsController> logger)
    {
        _deviceLogService = deviceLogService;
        _logger = logger;
    }

    // GET: api/DeviceLogs/recent?limit=100
    [HttpGet("recent")]
    public async Task<ActionResult<IEnumerable<DeviceLogDto>>> GetRecentLogs([FromQuery] int limit = 100)
    {
        if (limit <= 0 || limit > 1000)
        {
            return BadRequest("Limit must be between 1 and 1000");
        }

        var logs = await _deviceLogService.GetRecentLogsAsync(limit);
        return Ok(logs);
    }

    // GET: api/DeviceLogs/device/{deviceId}?limit=100
    [HttpGet("device/{deviceId}")]
    public async Task<ActionResult<IEnumerable<DeviceLogDto>>> GetLogsByDevice(string deviceId, [FromQuery] int limit = 100)
    {
        if (limit <= 0 || limit > 1000)
        {
            return BadRequest("Limit must be between 1 and 1000");
        }

        var logs = await _deviceLogService.GetLogsByDeviceIdAsync(deviceId, limit);
        return Ok(logs);
    }

    // GET: api/DeviceLogs/device/{deviceId}/range?startDate=2025-01-01&endDate=2025-01-31
    [HttpGet("device/{deviceId}/range")]
    public async Task<ActionResult<IEnumerable<DeviceLogDto>>> GetLogsByDeviceAndDateRange(
        string deviceId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        if (startDate >= endDate)
        {
            return BadRequest("Start date must be before end date");
        }

        var logs = await _deviceLogService.GetLogsByDeviceIdAndDateRangeAsync(deviceId, startDate, endDate);
        return Ok(logs);
    }

    // GET: api/DeviceLogs/level/{level}?limit=100
    [HttpGet("level/{level}")]
    public async Task<ActionResult<IEnumerable<DeviceLogDto>>> GetLogsByLevel(string level, [FromQuery] int limit = 100)
    {
        if (limit <= 0 || limit > 1000)
        {
            return BadRequest("Limit must be between 1 and 1000");
        }

        var validLevels = new[] { "info", "warn", "error", "debug" };
        if (!validLevels.Contains(level.ToLowerInvariant()))
        {
            return BadRequest($"Invalid log level. Valid levels: {string.Join(", ", validLevels)}");
        }

        var logs = await _deviceLogService.GetLogsByLevelAsync(level, limit);
        return Ok(logs);
    }

    // GET: api/DeviceLogs/peripheral/{peripheralName}?limit=100
    [HttpGet("peripheral/{peripheralName}")]
    public async Task<ActionResult<IEnumerable<DeviceLogDto>>> GetLogsByPeripheral(string peripheralName, [FromQuery] int limit = 100)
    {
        if (limit <= 0 || limit > 1000)
        {
            return BadRequest("Limit must be between 1 and 1000");
        }

        var logs = await _deviceLogService.GetLogsByPeripheralAsync(peripheralName, limit);
        return Ok(logs);
    }

    // POST: api/DeviceLogs (pour tests manuels)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AddLog([FromBody] DeviceLogDto logDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _deviceLogService.AddLogAsync(logDto);
        return CreatedAtAction(nameof(GetLogsByDevice), new { deviceId = logDto.DeviceId }, logDto);
    }
}
