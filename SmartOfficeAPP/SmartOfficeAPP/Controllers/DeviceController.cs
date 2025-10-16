using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartOfficeAPP.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DevicesController : ControllerBase
{
    private readonly IPeripheriqueLogiqueService _peripheriqueService;
    private readonly IIotHubService _iotHubService;
    private readonly IMapper _mapper;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        IPeripheriqueLogiqueService peripheriqueService, 
        IIotHubService iotHubService, 
        IMapper mapper,
        ILogger<DevicesController> logger)
    {
        _peripheriqueService = peripheriqueService;
        _iotHubService = iotHubService;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: api/Devices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PeripheriqueLogiqueDto>>> GetDevices()
    {
        var devices = await _peripheriqueService.GetAllAsync();
        return Ok(devices);
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

    // =============================
    // Gestion ciblée des périphériques
    // =============================

    // GET: api/Devices/{iotDeviceId}/peripherals
    // Renvoie en priorité les "reported.peripherals", sinon "desired.peripherals".
    [HttpGet("{iotDeviceId}/peripherals")]
    public async Task<ActionResult<IEnumerable<PeripheralDto>>> GetPeripherals(string iotDeviceId)
    {
        var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
        if (twin is null) return NotFound(new { message = $"Device {iotDeviceId} introuvable dans IoT Hub." });

        var reported = TryExtractPeripherals(twin.ReportedProperties);
        if (reported.Count > 0) return Ok(reported);

        var desired = TryExtractPeripherals(twin.DesiredProperties);
        return Ok(desired);
    }

    // GET: api/Devices/{iotDeviceId}/peripherals/{name}
    // Retourne l'état d'un périphérique (reported si dispo, sinon desired).
    [HttpGet("{iotDeviceId}/peripherals/{name}")]
    public async Task<ActionResult<PeripheralDto>> GetPeripheral(string iotDeviceId, string name)
    {
        var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
        if (twin is null) return NotFound(new { message = $"Device {iotDeviceId} introuvable." });

        var reported = TryGetPeripheral(twin.ReportedProperties, name);
        if (reported is not null) return Ok(reported);

        var desired = TryGetPeripheral(twin.DesiredProperties, name);
        if (desired is not null) return Ok(desired);

        return NotFound(new { message = $"Périphérique '{name}' non trouvé." });
    }

    // POST: api/Devices/{iotDeviceId}/peripherals
    // Ajoute un périphérique dans desired.peripherals (merge du desired).
    [HttpPost("{iotDeviceId}/peripherals")]
    public async Task<IActionResult> AddPeripheral(string iotDeviceId, [FromBody] PeripheralDto peripheral)
    {
        if (peripheral is null || string.IsNullOrWhiteSpace(peripheral.Name) || string.IsNullOrWhiteSpace(peripheral.Type))
            return BadRequest("Champs requis: name, type, gpio.");

        var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
        if (twin is null) return NotFound(new { message = $"Device {iotDeviceId} introuvable dans IoT Hub." });

        var desired = TryExtractPeripherals(twin.DesiredProperties);
        if (desired.Any(p => p.Name.Equals(peripheral.Name, StringComparison.OrdinalIgnoreCase)))
            return Conflict(new { message = $"Un périphérique nommé '{peripheral.Name}' existe déjà dans desired." });

        desired.Add(peripheral);

        var mergedDesired = UpsertPeripheralsIntoDesired(twin.DesiredProperties, desired);
        var ok = await _iotHubService.UpdateDesiredPropertiesAsync(iotDeviceId, mergedDesired);
        if (!ok) return StatusCode(500, "Échec de mise à jour des desired properties.");

        return CreatedAtAction(nameof(GetPeripherals), new { iotDeviceId }, desired);
    }

    // PUT: api/Devices/{iotDeviceId}/peripherals
    // Remplace uniquement desired.peripherals (conserve les autres clés desired).
    [HttpPut("{iotDeviceId}/peripherals")]
    public async Task<IActionResult> ReplacePeripherals(string iotDeviceId, [FromBody] PeripheralsPayload body)
    {
        if (body?.Peripherals is null) return BadRequest("Payload invalide. Utilise { \"peripherals\": [ ... ] }.");

        var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
        if (twin is null) return NotFound(new { message = $"Device {iotDeviceId} introuvable dans IoT Hub." });

        var mergedDesired = UpsertPeripheralsIntoDesired(twin.DesiredProperties, body.Peripherals);
        var ok = await _iotHubService.UpdateDesiredPropertiesAsync(iotDeviceId, mergedDesired);
        if (!ok) return StatusCode(500, "Échec de mise à jour des desired properties.");

        return Ok(body.Peripherals);
    }

    // DELETE: api/Devices/{iotDeviceId}/peripherals/{name}
    // Supprime un périphérique par son nom dans desired.peripherals.
    [HttpDelete("{iotDeviceId}/peripherals/{name}")]
    public async Task<IActionResult> DeletePeripheral(string iotDeviceId, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Nom requis.");

        var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
        if (twin is null) return NotFound(new { message = $"Device {iotDeviceId} introuvable dans IoT Hub." });

        var desired = TryExtractPeripherals(twin.DesiredProperties);
        var countBefore = desired.Count;
        desired = desired.Where(p => !p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();

        if (desired.Count == countBefore)
            return NotFound(new { message = $"Aucun périphérique nommé '{name}' dans desired." });

        var mergedDesired = UpsertPeripheralsIntoDesired(twin.DesiredProperties, desired);
        var ok = await _iotHubService.UpdateDesiredPropertiesAsync(iotDeviceId, mergedDesired);
        if (!ok) return StatusCode(500, "Échec de mise à jour des desired properties.");

        return NoContent();
    }

    // POST: api/Devices/{iotDeviceId}/peripherals/{name}/commands/{command}
    // Envoie une méthode directe 'peripheralCommand' au device ciblant un périphérique.
    [HttpPost("{iotDeviceId}/peripherals/{name}/commands/{command}")]
    public async Task<IActionResult> ExecutePeripheralCommand(string iotDeviceId, string name, string command, [FromBody] PeripheralCommandPayload? payload = null)
    {
        var body = new
        {
            name,
            command,
            parameters = payload?.Parameters ?? new Dictionary<string, object>()
        };
        var success = await _iotHubService.InvokeDeviceMethodAsync(iotDeviceId, "peripheralCommand", JsonSerializer.Serialize(body));
        if (!success) return BadRequest(new { message = $"Échec d'exécution de la commande '{command}' sur '{name}'." });
        return Ok(new { message = $"Commande '{command}' exécutée sur '{name}'." });
    }

    // GET: api/Devices/{iotDeviceId}/peripherals/{name}/merged
    // Retourne une vue fusionnée (desired + reported) d'un périphérique.
    [HttpGet("{iotDeviceId}/peripherals/{name}/merged")]
    public async Task<ActionResult<MergedPeripheralDto>> GetMergedPeripheral(string iotDeviceId, string name)
    {
        var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
        if (twin is null) return NotFound(new { message = $"Device {iotDeviceId} introuvable." });

        var desired = TryGetPeripheral(twin.DesiredProperties, name);
        var reported = TryGetPeripheral(twin.ReportedProperties, name);

        if (desired is null && reported is null)
            return NotFound(new { message = $"Périphérique '{name}' non trouvé." });

        var merged = new MergedPeripheralDto
        {
            Name = name,
            DesiredConfig = desired,
            ReportedState = reported,
            IsSynchronized = desired != null && reported != null && ArePeripheralsSynchronized(desired, reported)
        };

        return Ok(merged);
    }

    // GET: api/Devices/{iotDeviceId}/twin/full
    // Retourne le twin complet avec métadonnées.
    [HttpGet("{iotDeviceId}/twin/full")]
    public async Task<ActionResult<DeviceTwinFullDto>> GetFullTwin(string iotDeviceId)
    {
        var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
        if (twin is null) return NotFound(new { message = $"Device {iotDeviceId} introuvable." });

        var desiredPeripherals = TryExtractPeripherals(twin.DesiredProperties);
        var reportedPeripherals = TryExtractPeripherals(twin.ReportedProperties);

        var fullTwin = new DeviceTwinFullDto
        {
            DeviceId = twin.DeviceId,
            Status = twin.Status,
            ConnectionState = twin.ConnectionState,
            LastActivityTime = twin.LastActivityTime,
            Version = twin.Version,
            DesiredPeripherals = desiredPeripherals,
            ReportedPeripherals = reportedPeripherals,
            RawDesiredProperties = twin.DesiredProperties,
            RawReportedProperties = twin.ReportedProperties
        };

        return Ok(fullTwin);
    }

    // ---- Helpers JSON ----

    private static List<PeripheralDto> TryExtractPeripherals(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return new();
            var container = JsonSerializer.Deserialize<PeripheralsPayload>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return container?.Peripherals?.Where(ValidPeripheral).ToList() ?? new();
        }
        catch
        {
            return new();
        }
    }

    private static PeripheralDto? TryGetPeripheral(string json, string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            var container = JsonSerializer.Deserialize<PeripheralsPayload>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return container?.Peripherals?.FirstOrDefault(p => p != null && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    // Merge: ne remplace que desired.peripherals et conserve les autres desired.*
    private static string UpsertPeripheralsIntoDesired(string existingDesiredJson, List<PeripheralDto> peripherals)
    {
        JsonNode root;
        try
        {
            root = string.IsNullOrWhiteSpace(existingDesiredJson)
                ? new JsonObject()
                : (JsonNode.Parse(existingDesiredJson) as JsonObject) ?? new JsonObject();
        }
        catch
        {
            root = new JsonObject();
        }

        root["peripherals"] = JsonSerializer.SerializeToNode(peripherals);
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private static bool ValidPeripheral(PeripheralDto p)
        => p is not null
           && !string.IsNullOrWhiteSpace(p.Name)
           && !string.IsNullOrWhiteSpace(p.Type);

    private static bool ArePeripheralsSynchronized(PeripheralDto desired, PeripheralDto reported)
        => desired.Name.Equals(reported.Name, StringComparison.OrdinalIgnoreCase)
           && desired.Type.Equals(reported.Type, StringComparison.OrdinalIgnoreCase)
           && desired.Gpio == reported.Gpio;
}

#region DTOs pour peripherals

public class PeripheralDto
{
    public string Name { get; set; } = string.Empty; // ex: "relay4"
    public string Type { get; set; } = string.Empty; // ex: "relay", "smoke", "temp", ...
    public int Gpio { get; set; }                    // ex: 33

    // Métadonnées admin
    public string? Notes { get; set; }              // Notes de câblage
    public string? Unit { get; set; }               // "°C", "%", "W", ...
    public string? WiringDiagram { get; set; }      // URL ou texte du schéma
    public DateTime? InstalledAt { get; set; }      // Date d'installation

    // État temps réel (reporté par firmware)
    public object? State { get; set; }              // État dynamique (on/off, valeur, etc.)
    public DateTime? LastUpdate { get; set; }       // Dernier rapport
    public string? Status { get; set; }             // "active", "error", "offline"
}

public class PeripheralsPayload
{
    public List<PeripheralDto>? Peripherals { get; set; }
}

public class PeripheralCommandPayload
{
    public Dictionary<string, object>? Parameters { get; set; }
}

public class MergedPeripheralDto
{
    public string Name { get; set; } = string.Empty;
    public PeripheralDto? DesiredConfig { get; set; }
    public PeripheralDto? ReportedState { get; set; }
    public bool IsSynchronized { get; set; }
}

public class DeviceTwinFullDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? ConnectionState { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public long Version { get; set; }
    public List<PeripheralDto> DesiredPeripherals { get; set; } = new();
    public List<PeripheralDto> ReportedPeripherals { get; set; } = new();
    public string RawDesiredProperties { get; set; } = "{}";
    public string RawReportedProperties { get; set; } = "{}";
}

#endregion
