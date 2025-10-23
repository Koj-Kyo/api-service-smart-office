using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;
using System.Text.Json;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class DevicePeripheralService : IDevicePeripheralService
{
    private readonly IDevicePeripheralRepository _repository;
    private readonly IIotHubService _iotHubService;
    private readonly IMapper _mapper;
    private readonly ILogger<DevicePeripheralService> _logger;

    public DevicePeripheralService(
        IDevicePeripheralRepository repository,
        IIotHubService iotHubService,
        IMapper mapper,
        ILogger<DevicePeripheralService> logger)
    {
        _repository = repository;
        _iotHubService = iotHubService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<DevicePeripheralDto>> GetAllAsync()
    {
        var peripherals = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<DevicePeripheralDto>>(peripherals);
    }

    public async Task<DevicePeripheralDto?> GetByIdAsync(int id)
    {
        var peripheral = await _repository.GetByIdAsync(id);
        return peripheral == null ? null : _mapper.Map<DevicePeripheralDto>(peripheral);
    }

    public async Task<IEnumerable<DevicePeripheralDto>> GetByIotDeviceIdAsync(string iotDeviceId)
    {
        var peripherals = await _repository.GetByIotDeviceIdAsync(iotDeviceId);
        return _mapper.Map<IEnumerable<DevicePeripheralDto>>(peripherals);
    }

    public async Task<DevicePeripheralDto?> GetByNameAsync(string iotDeviceId, string name)
    {
        var peripheral = await _repository.GetByNameAsync(iotDeviceId, name);
        return peripheral == null ? null : _mapper.Map<DevicePeripheralDto>(peripheral);
    }

    public async Task<DevicePeripheralDto> CreateAsync(DevicePeripheralCreationDto dto)
    {
        // Vérifier si existe déjà
        if (await _repository.ExistsAsync(dto.IotDeviceId, dto.Name))
        {
            throw new InvalidOperationException($"Un périphérique nommé '{dto.Name}' existe déjà pour le device '{dto.IotDeviceId}'.");
        }

        var peripheral = _mapper.Map<DevicePeripheral>(dto);
        peripheral.Status = "active";
        peripheral.CreatedAt = DateTime.UtcNow;

        var created = await _repository.AddAsync(peripheral);

        // Synchroniser avec IoT Hub (desired properties)
        try
        {
            await SyncWithDesiredPropertiesAsync(dto.IotDeviceId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de synchroniser avec IoT Hub après création du périphérique {Name}", dto.Name);
        }

        return _mapper.Map<DevicePeripheralDto>(created);
    }

    public async Task<bool> UpdateAsync(int id, DevicePeripheralUpdateDto dto)
    {
        var peripheral = await _repository.GetByIdAsync(id);
        if (peripheral == null) return false;

        // Mise à jour des champs fournis
        if (dto.Name != null) peripheral.Name = dto.Name;
        if (dto.Type != null) peripheral.Type = dto.Type;
        if (dto.Gpio.HasValue) peripheral.Gpio = dto.Gpio.Value;
        if (dto.Notes != null) peripheral.Notes = dto.Notes;
        if (dto.Unit != null) peripheral.Unit = dto.Unit;
        if (dto.WiringDiagram != null) peripheral.WiringDiagram = dto.WiringDiagram;
        if (dto.InstalledAt.HasValue) peripheral.InstalledAt = dto.InstalledAt.Value;
        if (dto.Status != null) peripheral.Status = dto.Status;
        if (dto.PeripheriqueLogiqueId.HasValue) peripheral.PeripheriqueLogiqueId = dto.PeripheriqueLogiqueId;

        peripheral.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(peripheral);

        // Synchroniser avec IoT Hub
        if (updated)
        {
            try
            {
                await SyncWithDesiredPropertiesAsync(peripheral.IotDeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible de synchroniser avec IoT Hub après mise à jour du périphérique {Id}", id);
            }
        }

        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var peripheral = await _repository.GetByIdAsync(id);
        if (peripheral == null) return false;

        var iotDeviceId = peripheral.IotDeviceId;
        var deleted = await _repository.DeleteAsync(id);

        // Synchroniser avec IoT Hub
        if (deleted)
        {
            try
            {
                await SyncWithDesiredPropertiesAsync(iotDeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Impossible de synchroniser avec IoT Hub après suppression du périphérique {Id}", id);
            }
        }

        return deleted;
    }

    public async Task<bool> SyncWithDesiredPropertiesAsync(string iotDeviceId)
    {
        try
        {
            // Récupérer tous les périphériques du device depuis la DB
            var peripherals = await _repository.GetByIotDeviceIdAsync(iotDeviceId);

            // Construire le payload desired
            var peripheralsList = peripherals.Select(p => new
            {
                name = p.Name,
                type = p.Type,
                gpio = p.Gpio,
                unit = p.Unit,
                notes = p.Notes
            }).ToList();

            var desiredJson = JsonSerializer.Serialize(new { peripherals = peripheralsList });

            // Envoyer à IoT Hub
            var success = await _iotHubService.UpdateDesiredPropertiesAsync(iotDeviceId, desiredJson);

            if (success)
            {
                _logger.LogInformation("Synchronisation réussie des périphériques pour le device {DeviceId}", iotDeviceId);
            }
            else
            {
                _logger.LogWarning("Échec de la synchronisation des périphériques pour le device {DeviceId}", iotDeviceId);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation avec IoT Hub pour {DeviceId}", iotDeviceId);
            return false;
        }
    }

    /// <summary>
    /// Synchronise les périphériques physiques depuis IoT Hub (reported properties) vers la base de données
    /// </summary>
    public async Task<int> SyncPeripheralsFromIoTHubAsync(string iotDeviceId)
    {
        try
        {
            _logger.LogInformation("Démarrage de la synchronisation des périphériques pour {DeviceId}", iotDeviceId);

            // Récupérer le device twin depuis IoT Hub
            var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
            if (twin == null)
            {
                _logger.LogWarning("Device twin introuvable pour {DeviceId}", iotDeviceId);
                return 0;
            }

            // Parser les reported properties pour extraire les périphériques
            var reportedPeripherals = ParsePeripheralsFromJson(twin.ReportedProperties);
            
            // Si aucun périphérique dans reported, essayer desired
            if (reportedPeripherals.Count == 0)
            {
                _logger.LogInformation("Aucun périphérique dans reported properties, essai avec desired properties");
                reportedPeripherals = ParsePeripheralsFromJson(twin.DesiredProperties);
            }

            if (reportedPeripherals.Count == 0)
            {
                _logger.LogInformation("Aucun périphérique trouvé dans le twin pour {DeviceId}", iotDeviceId);
                return 0;
            }

            // Récupérer les périphériques existants en DB
            var existingPeripherals = await _repository.GetByIotDeviceIdAsync(iotDeviceId);
            var existingNames = existingPeripherals.Select(p => p.Name).ToHashSet();

            int syncedCount = 0;

            foreach (var reported in reportedPeripherals)
            {
                try
                {
                    // Si le périphérique n'existe pas en DB, le créer
                    if (!existingNames.Contains(reported.Name))
                    {
                        var newPeripheral = new DevicePeripheral
                        {
                            Name = reported.Name,
                            Type = reported.Type,
                            Gpio = reported.Gpio,
                            IotDeviceId = iotDeviceId,
                            Unit = reported.Unit,
                            Status = "active",
                            LastUpdateReceived = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Serializer l'état si présent
                        if (reported.State != null)
                        {
                            newPeripheral.LastKnownState = JsonSerializer.Serialize(reported.State);
                        }

                        await _repository.AddAsync(newPeripheral);
                        syncedCount++;

                        _logger.LogInformation(
                            "Périphérique {Name} (Type: {Type}, GPIO: {Gpio}) synchronisé pour {DeviceId}",
                            reported.Name,
                            reported.Type,
                            reported.Gpio,
                            iotDeviceId
                        );
                    }
                    else
                    {
                        // Mettre à jour l'état du périphérique existant
                        var existing = existingPeripherals.First(p => p.Name == reported.Name);
                        
                        if (reported.State != null)
                        {
                            existing.LastKnownState = JsonSerializer.Serialize(reported.State);
                        }
                        
                        existing.LastUpdateReceived = DateTime.UtcNow;
                        existing.UpdatedAt = DateTime.UtcNow;
                        
                        await _repository.UpdateAsync(existing);
                        
                        _logger.LogDebug("État mis à jour pour le périphérique {Name} de {DeviceId}", reported.Name, iotDeviceId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la synchronisation du périphérique {Name} pour {DeviceId}", reported.Name, iotDeviceId);
                }
            }

            _logger.LogInformation(
                "Synchronisation terminée pour {DeviceId}: {SyncedCount} nouveaux périphériques ajoutés",
                iotDeviceId,
                syncedCount
            );

            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation des périphériques pour {DeviceId}", iotDeviceId);
            throw;
        }
    }

    /// <summary>
    /// Synchronise tous les périphériques de tous les devices IoT Hub
    /// </summary>
    public async Task<int> SyncAllPeripheralsFromIoTHubAsync()
    {
        try
        {
            _logger.LogInformation("Démarrage de la synchronisation globale des périphériques depuis IoT Hub");

            // Récupérer tous les devices depuis IoT Hub
            var iotHubDevices = await _iotHubService.ListDevicesAsync(1000);
            
            if (!iotHubDevices.Any())
            {
                _logger.LogInformation("Aucun device trouvé dans IoT Hub");
                return 0;
            }

            int totalSynced = 0;

            foreach (var device in iotHubDevices)
            {
                try
                {
                    var synced = await SyncPeripheralsFromIoTHubAsync(device.DeviceId);
                    totalSynced += synced;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la synchronisation des périphériques pour {DeviceId}", device.DeviceId);
                }
            }

            _logger.LogInformation(
                "Synchronisation globale terminée: {TotalSynced} périphériques ajoutés sur {TotalDevices} devices",
                totalSynced,
                iotHubDevices.Count()
            );

            return totalSynced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation globale des périphériques");
            throw;
        }
    }

    /// <summary>
    /// Parse les périphériques depuis le JSON des properties
    /// </summary>
    private List<PeripheralFromIoTHub> ParsePeripheralsFromJson(string json)
    {
        var result = new List<PeripheralFromIoTHub>();

        try
        {
            if (string.IsNullOrWhiteSpace(json) || json == "{}")
            {
                return result;
            }

            var doc = JsonDocument.Parse(json);
            
            // Chercher la propriété "peripherals"
            if (doc.RootElement.TryGetProperty("peripherals", out var peripheralsArray))
            {
                foreach (var item in peripheralsArray.EnumerateArray())
                {
                    var peripheral = new PeripheralFromIoTHub();

                    if (item.TryGetProperty("name", out var name))
                        peripheral.Name = name.GetString() ?? "";

                    if (item.TryGetProperty("type", out var type))
                        peripheral.Type = type.GetString() ?? "";

                    if (item.TryGetProperty("gpio", out var gpio))
                        peripheral.Gpio = gpio.GetInt32();

                    if (item.TryGetProperty("unit", out var unit))
                        peripheral.Unit = unit.GetString();

                    if (item.TryGetProperty("state", out var state))
                        peripheral.State = state;

                    // Valider que les champs requis sont présents
                    if (!string.IsNullOrWhiteSpace(peripheral.Name) && 
                        !string.IsNullOrWhiteSpace(peripheral.Type))
                    {
                        result.Add(peripheral);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors du parsing des périphériques depuis JSON");
        }

        return result;
    }

    /// <summary>
    /// Classe helper pour parser les périphériques depuis IoT Hub
    /// </summary>
    private class PeripheralFromIoTHub
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Gpio { get; set; }
        public string? Unit { get; set; }
        public JsonElement? State { get; set; }
    }
}
