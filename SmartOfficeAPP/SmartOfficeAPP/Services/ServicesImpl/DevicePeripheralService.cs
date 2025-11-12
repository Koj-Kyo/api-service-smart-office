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
    private readonly IZoneRepository _zoneRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DevicePeripheralService> _logger;

    public DevicePeripheralService(
        IDevicePeripheralRepository repository,
        IIotHubService iotHubService,
        IZoneRepository zoneRepository,
        IMapper mapper,
        ILogger<DevicePeripheralService> logger)
    {
        _repository = repository;
        _iotHubService = iotHubService;
        _zoneRepository = zoneRepository;
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

    // =============================
    // ? NOUVEAUX: Méthodes enrichies pour app mobile
    // =============================

    /// <summary>
    /// Récupère tous les périphériques enrichis (DB + IoT Hub + Zone)
    /// </summary>
    public async Task<IEnumerable<DevicePeripheralEnrichedDto>> GetAllEnrichedAsync()
    {
        try
        {
            var peripherals = await _repository.GetAllAsync();
            var enrichedList = new List<DevicePeripheralEnrichedDto>();

            // Grouper par IotDeviceId pour optimiser les appels IoT Hub
            var groupedByDevice = peripherals.GroupBy(p => p.IotDeviceId);

            foreach (var deviceGroup in groupedByDevice)
            {
                var iotDeviceId = deviceGroup.Key;
                
                // Récupérer le twin une seule fois par device
                var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
                var reportedPeripherals = twin != null ? ParsePeripheralsFromJson(twin.ReportedProperties) : new List<PeripheralFromIoTHub>();

                foreach (var peripheral in deviceGroup)
                {
                    var enriched = await EnrichPeripheralAsync(peripheral, twin, reportedPeripherals);
                    enrichedList.Add(enriched);
                }
            }

            return enrichedList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des périphériques enrichis");
            throw;
        }
    }

    /// <summary>
    /// Récupère un périphérique enrichi par ID
    /// </summary>
    public async Task<DevicePeripheralEnrichedDto?> GetEnrichedByIdAsync(int id)
    {
        try
        {
            var peripheral = await _repository.GetByIdAsync(id);
            if (peripheral == null) return null;

            var twin = await _iotHubService.GetDeviceTwinAsync(peripheral.IotDeviceId);
            var reportedPeripherals = twin != null ? ParsePeripheralsFromJson(twin.ReportedProperties) : new List<PeripheralFromIoTHub>();

            return await EnrichPeripheralAsync(peripheral, twin, reportedPeripherals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du périphérique enrichi {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Récupère tous les périphériques enrichis d'un device IoT Hub
    /// </summary>
    public async Task<IEnumerable<DevicePeripheralEnrichedDto>> GetEnrichedByIotDeviceIdAsync(string iotDeviceId)
    {
        try
        {
            var peripherals = await _repository.GetByIotDeviceIdAsync(iotDeviceId);
            var enrichedList = new List<DevicePeripheralEnrichedDto>();

            // Récupérer le twin une seule fois
            var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
            var reportedPeripherals = twin != null ? ParsePeripheralsFromJson(twin.ReportedProperties) : new List<PeripheralFromIoTHub>();

            foreach (var peripheral in peripherals)
            {
                var enriched = await EnrichPeripheralAsync(peripheral, twin, reportedPeripherals);
                enrichedList.Add(enriched);
            }

            return enrichedList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des périphériques enrichis pour {DeviceId}", iotDeviceId);
            throw;
        }
    }

    /// <summary>
    /// Récupère les périphériques enrichis par zone
    /// </summary>
    public async Task<IEnumerable<DevicePeripheralEnrichedDto>> GetEnrichedByZoneIdAsync(int zoneId)
    {
        try
        {
            var allPeripherals = await _repository.GetAllAsync();
            var peripheralsInZone = allPeripherals
                .Where(p => p.PeripheriqueLogique?.ZoneId == zoneId)
                .ToList();

            var enrichedList = new List<DevicePeripheralEnrichedDto>();

            // Grouper par IotDeviceId pour optimiser
            var groupedByDevice = peripheralsInZone.GroupBy(p => p.IotDeviceId);

            foreach (var deviceGroup in groupedByDevice)
            {
                var iotDeviceId = deviceGroup.Key;
                var twin = await _iotHubService.GetDeviceTwinAsync(iotDeviceId);
                var reportedPeripherals = twin != null ? ParsePeripheralsFromJson(twin.ReportedProperties) : new List<PeripheralFromIoTHub>();

                foreach (var peripheral in deviceGroup)
                {
                    var enriched = await EnrichPeripheralAsync(peripheral, twin, reportedPeripherals);
                    enrichedList.Add(enriched);
                }
            }

            return enrichedList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des périphériques enrichis pour la zone {ZoneId}", zoneId);
            throw;
        }
    }

    /// <summary>
    /// Récupère les statistiques agrégées des périphériques
    /// </summary>
    public async Task<DevicePeripheralsStatsDto> GetStatsAsync()
    {
        try
        {
            var peripherals = await _repository.GetAllAsync();
            var peripheralsList = peripherals.ToList();

            var stats = new DevicePeripheralsStatsDto
            {
                TotalDevices = peripheralsList.Count,
                ActiveDevicesCount = peripheralsList.Count(p => p.Status == "active" && p.LastUpdateReceived.HasValue),
                OfflineDevicesCount = peripheralsList.Count(p => p.Status == "offline" || !p.LastUpdateReceived.HasValue),
                ErrorDevicesCount = peripheralsList.Count(p => p.Status == "error"),
                ActionableDevicesCount = peripheralsList.Count(p => IsActionableType(p.Type)),
                GeneratedAtUtc = DateTime.UtcNow
            };

            // Compter les zones distinctes
            var zoneIds = peripheralsList
                .Where(p => p.PeripheriqueLogique?.ZoneId != null)
                .Select(p => p.PeripheriqueLogique!.ZoneId)
                .Distinct();
            stats.ZonesCount = zoneIds.Count();

            // Compter par type
            stats.CountsByType = peripheralsList
                .GroupBy(p => NormalizeType(p.Type))
                .ToDictionary(g => g.Key, g => g.Count());

            // Récupérer les données énergétiques depuis IoT Hub
            var deviceIds = peripheralsList.Select(p => p.IotDeviceId).Distinct();
            
            foreach (var deviceId in deviceIds)
            {
                try
                {
                    var twin = await _iotHubService.GetDeviceTwinAsync(deviceId);
                    if (twin != null)
                    {
                        var reported = ParsePeripheralsFromJson(twin.ReportedProperties);
                        
                        foreach (var reportedPeripheral in reported)
                        {
                            if (reportedPeripheral.State != null)
                            {
                                // Extraire energyMWh et lastPowerMW si disponibles
                                if (reportedPeripheral.State.Value.TryGetProperty("energyMWh", out var energyMWh))
                                {
                                    stats.TotalEnergyTodayWh += energyMWh.GetDouble() / 1000.0;
                                }
                                
                                if (reportedPeripheral.State.Value.TryGetProperty("lastPowerMW", out var lastPowerMW))
                                {
                                    stats.TotalCurrentPowerW += lastPowerMW.GetDouble() / 1000.0;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erreur lors de la récupération des données énergétiques pour {DeviceId}", deviceId);
                }
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul des statistiques");
            throw;
        }
    }

    // =============================
    // HELPERS PRIVÉS
    // =============================

    /// <summary>
    /// Enrichit un périphérique avec les données IoT Hub et Zone
    /// </summary>
    private async Task<DevicePeripheralEnrichedDto> EnrichPeripheralAsync(
        DevicePeripheral peripheral,
        DeviceTwinInfo? twin,
        List<PeripheralFromIoTHub> reportedPeripherals)
    {
        var enriched = new DevicePeripheralEnrichedDto
        {
            Id = peripheral.Id,
            Name = peripheral.Name,
            Type = NormalizeType(peripheral.Type),
            IotDeviceId = peripheral.IotDeviceId,
            Gpio = peripheral.Gpio,
            Unit = peripheral.Unit,
            Notes = peripheral.Notes,
            InstalledAt = peripheral.InstalledAt,
            Status = peripheral.Status,
            CreatedAt = peripheral.CreatedAt,
            UpdatedAt = peripheral.UpdatedAt,
            IsActive = peripheral.Status == "active" && peripheral.LastUpdateReceived.HasValue,
            IsActionable = IsActionableType(peripheral.Type)
        };

        // Récupérer la zone si disponible
        if (peripheral.PeripheriqueLogique?.ZoneId != null)
        {
            var zone = await _zoneRepository.GetZoneByIdAsync(peripheral.PeripheriqueLogique.ZoneId);
            if (zone != null)
            {
                enriched.Zone = _mapper.Map<ZoneDto>(zone);
                enriched.ZoneId = zone.Id;
            }
        }

        // Fusionner avec l'état temps réel depuis IoT Hub
        var reported = reportedPeripherals.FirstOrDefault(p => p.Name == peripheral.Name);
        if (reported != null && reported.State != null)
        {
            enriched.LastUpdatedUtc = DateTime.UtcNow;
            enriched.ConnectivityStatus = "Online";
            enriched.HealthStatus = "OK";

            // Extraire les capabilities depuis l'état
            enriched.Capabilities = ExtractCapabilities(reported.State.Value, peripheral.Type);

            // Extraire l'état spécifique par type
            enriched.TypeSpecificState = ExtractTypeSpecificState(reported.State.Value, peripheral.Type);

            // ? VÉRIFIER QUE STATE EST UN OBJET AVANT D'EXTRAIRE LES PROPRIÉTÉS
            if (reported.State.Value.ValueKind == JsonValueKind.Object)
            {
                // Extraire les données énergétiques
                if (reported.State.Value.TryGetProperty("metering", out var metering))
                {
                    enriched.Metering = metering.GetString();
                }
                if (reported.State.Value.TryGetProperty("ratedWatt", out var ratedWatt))
                {
                    enriched.RatedWatt = (float)ratedWatt.GetDouble();
                }
                if (reported.State.Value.TryGetProperty("energyMWh", out var energyMWh))
                {
                    enriched.EnergyTodayWh = energyMWh.GetDouble() / 1000.0;
                }
                if (reported.State.Value.TryGetProperty("lastPowerMW", out var lastPowerMW))
                {
                    enriched.CurrentPowerW = (float)(lastPowerMW.GetDouble() / 1000.0);
                }

                // Extraire le GUID si présent
                if (reported.State.Value.TryGetProperty("id", out var id))
                {
                    enriched.Guid = id.GetString();
                }

                // Vérifier l'état de santé
                if (reported.State.Value.TryGetProperty("error", out var error) && !string.IsNullOrEmpty(error.GetString()))
                {
                    enriched.HealthStatus = "Error";
                    enriched.HealthMessage = error.GetString();
                }
            }
            else
            {
                _logger.LogDebug("State est de type {ValueKind} pour {PeripheralName}, impossible d'extraire metering/energy/guid", 
                    reported.State.Value.ValueKind, peripheral.Name);
            }
        }
        else
        {
            // Pas de données temps réel
            enriched.ConnectivityStatus = peripheral.LastUpdateReceived.HasValue ? "Degraded" : "Offline";
            enriched.HealthStatus = "Unknown";
            enriched.LastUpdatedUtc = peripheral.LastUpdateReceived;
        }

        // Vérifier la connexion du device parent
        if (twin != null)
        {
            if (twin.ConnectionState == "Disconnected")
            {
                enriched.ConnectivityStatus = "Offline";
                enriched.IsActive = false;
            }
            enriched.LastUpdatedUtc = twin.LastActivityTime ?? enriched.LastUpdatedUtc;
        }

        return enriched;
    }

    /// <summary>
    /// Extrait les capabilities d'un périphérique depuis son état
    /// </summary>
    private List<string> ExtractCapabilities(JsonElement state, string type)
    {
        var capabilities = new List<string>();
        var normalizedType = NormalizeType(type);

        // ? VÉRIFIER SI STATE EST UN OBJET JSON AVANT D'ACCÉDER AUX PROPRIÉTÉS
        if (state.ValueKind != JsonValueKind.Object)
        {
            // Si state est une valeur simple (string, bool, number), ajouter juste "State"
            _logger.LogDebug("State est de type {ValueKind}, pas un objet JSON", state.ValueKind);
            
            if (state.ValueKind == JsonValueKind.String || 
                state.ValueKind == JsonValueKind.True || 
                state.ValueKind == JsonValueKind.False ||
                state.ValueKind == JsonValueKind.Number)
            {
                capabilities.Add("State");
            }
            
            return capabilities;
        }

        // Capabilities communes (seulement si state est un objet)
        if (state.TryGetProperty("state", out _) || state.TryGetProperty("value", out _))
        {
            capabilities.Add("State");
        }

        // Capabilities par type
        switch (normalizedType)
        {
            case "Lighting":
                if (state.TryGetProperty("brightness", out _)) capabilities.Add("Brightness");
                if (state.TryGetProperty("color", out _)) capabilities.Add("Color");
                if (state.TryGetProperty("colorTemperature", out _)) capabilities.Add("ColorTemperature");
                break;

            case "Climate":
                if (state.TryGetProperty("targetTemperature", out _)) capabilities.Add("Temperature");
                if (state.TryGetProperty("fanSpeed", out _)) capabilities.Add("FanSpeed");
                if (state.TryGetProperty("mode", out _)) capabilities.Add("Mode");
                break;

            case "Security":
                if (state.TryGetProperty("armed", out _)) capabilities.Add("Armed");
                if (state.TryGetProperty("lock", out _)) capabilities.Add("Lock");
                if (state.TryGetProperty("batteryPercent", out _)) capabilities.Add("Battery");
                break;

            case "Media":
                if (state.TryGetProperty("volume", out _)) capabilities.Add("Volume");
                if (state.TryGetProperty("playing", out _)) capabilities.Add("PlayPause");
                if (state.TryGetProperty("mute", out _)) capabilities.Add("Mute");
                break;

            case "Cleaner":
                if (state.TryGetProperty("mode", out _)) capabilities.Add("Mode");
                if (state.TryGetProperty("batteryPercent", out _)) capabilities.Add("Battery");
                if (state.TryGetProperty("docked", out _)) capabilities.Add("Dock");
                break;

            case "Door":
                if (state.TryGetProperty("locked", out _)) capabilities.Add("Lock");
                if (state.TryGetProperty("open", out _)) capabilities.Add("DoorSensor");
                break;
        }

        // Capability énergétique
        if (state.TryGetProperty("metering", out var metering) && metering.GetString() != "none")
        {
            capabilities.Add("Energy");
        }

        return capabilities;
    }

    /// <summary>
    /// Extrait l'état spécifique par type
    /// </summary>
    private object? ExtractTypeSpecificState(JsonElement state, string type)
    {
        var normalizedType = NormalizeType(type);

        // ? VÉRIFIER SI STATE EST UN OBJET JSON AVANT D'ACCÉDER AUX PROPRIÉTÉS
        if (state.ValueKind != JsonValueKind.Object)
        {
            // Si state est une valeur simple, retourner directement
            _logger.LogDebug("State est de type {ValueKind}, retour de la valeur brute", state.ValueKind);
            
            try
            {
                return state.ValueKind switch
                {
                    JsonValueKind.String => state.GetString(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Number => state.GetDouble(),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors de l'extraction de la valeur simple du state");
                return null;
            }
        }

        // Extraction selon le type normalisé (seulement si state est un objet)
        switch (normalizedType)
        {
            case "Lighting":
                return new
                {
                    brightness = GetJsonProperty(state, "brightness")?.GetInt32(),
                    lightColor = GetJsonProperty(state, "color")?.GetString(),
                    colorTemperature = GetJsonProperty(state, "colorTemperature")?.GetInt32(),
                    powerConsumptionW = GetJsonProperty(state, "lastPowerMW")?.GetDouble() / 1000.0,
                    energyTodayWh = GetJsonProperty(state, "energyMWh")?.GetDouble() / 1000.0
                };

            case "Climate":
                return new
                {
                    targetTemperature = GetJsonProperty(state, "targetTemperature")?.GetDouble(),
                    currentTemperature = GetJsonProperty(state, "value")?.GetDouble(),
                    climateMode = GetJsonProperty(state, "mode")?.GetString(),
                    fanSpeed = GetJsonProperty(state, "fanSpeed")?.GetInt32(),
                    humidityPercent = GetJsonProperty(state, "humidity")?.GetInt32()
                };

            case "Security":
                return new
                {
                    armedState = GetJsonProperty(state, "armed")?.GetBoolean() == true ? "Armed" : "Disarmed",
                    lastAlarmUtc = GetJsonProperty(state, "lastAlarm")?.GetString(),
                    batteryPercent = GetJsonProperty(state, "batteryPercent")?.GetInt32()
                };

            case "Media":
                return new
                {
                    volume = GetJsonProperty(state, "volume")?.GetInt32(),
                    isPlaying = GetJsonProperty(state, "playing")?.GetBoolean(),
                    currentMediaTitle = GetJsonProperty(state, "media")?.GetString(),
                    mute = GetJsonProperty(state, "mute")?.GetBoolean()
                };

            case "Cleaner":
                return new
                {
                    cleanerMode = GetJsonProperty(state, "mode")?.GetString(),
                    batteryPercent = GetJsonProperty(state, "batteryPercent")?.GetInt32(),
                    docked = GetJsonProperty(state, "docked")?.GetBoolean(),
                    progressPercent = GetJsonProperty(state, "progress")?.GetInt32()
                };

            case "Door":
                return new
                {
                    isLocked = GetJsonProperty(state, "locked")?.GetBoolean(),
                    doorOpen = GetJsonProperty(state, "open")?.GetBoolean(),
                    lockBatteryPercent = GetJsonProperty(state, "batteryPercent")?.GetInt32()
                };

            default:
                // Type générique: retourner l'état brut si c'est un objet
                try
                {
                    return JsonSerializer.Deserialize<object>(state.GetRawText());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erreur lors de la désérialisation de l'état générique");
                    return null;
                }
        }
    }

    /// <summary>
    /// Helper pour récupérer une propriété JSON
    /// </summary>
    private JsonElement? GetJsonProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var prop) ? prop : null;
    }

    /// <summary>
    /// Normalise le type de périphérique
    /// </summary>
    private string NormalizeType(string type)
    {
        var lowerType = type.ToLowerInvariant();
        
        // Mapping vers les catégories standard
        if (lowerType.Contains("light") || lowerType.Contains("lamp") || lowerType == "relay")
            return "Lighting";
        if (lowerType.Contains("temp") || lowerType.Contains("climate") || lowerType.Contains("hvac"))
            return "Climate";
        if (lowerType.Contains("security") || lowerType.Contains("motion") || lowerType.Contains("smoke") || lowerType.Contains("alarm"))
            return "Security";
        if (lowerType.Contains("media") || lowerType.Contains("speaker") || lowerType.Contains("tv"))
            return "Media";
        if (lowerType.Contains("clean") || lowerType.Contains("vacuum") || lowerType.Contains("robot"))
            return "Cleaner";
        if (lowerType.Contains("door") || lowerType.Contains("lock") || lowerType.Contains("gate"))
            return "Door";
        if (lowerType.Contains("sensor"))
            return "Sensor";
        if (lowerType.Contains("actuator") || lowerType.Contains("motor") || lowerType.Contains("servo"))
            return "Actuator";
        
        return type; // Retourner le type original si pas de mapping
    }

    /// <summary>
    /// Détermine si un type de périphérique peut recevoir des commandes
    /// </summary>
    private bool IsActionableType(string type)
    {
        var lowerType = type.ToLowerInvariant();
        return lowerType.Contains("relay") 
            || lowerType.Contains("actuator") 
            || lowerType.Contains("light")
            || lowerType.Contains("motor")
            || lowerType.Contains("servo")
            || lowerType.Contains("lock")
            || lowerType.Contains("clean")
            || lowerType.Contains("media");
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
