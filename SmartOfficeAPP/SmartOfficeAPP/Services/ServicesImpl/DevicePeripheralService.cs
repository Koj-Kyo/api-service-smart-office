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
}
