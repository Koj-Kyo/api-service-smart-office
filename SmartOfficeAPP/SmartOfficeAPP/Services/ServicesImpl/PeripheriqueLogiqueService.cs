using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl
{
    public class PeripheriqueLogiqueService : IPeripheriqueLogiqueService
    {
        private readonly IPeripheriqueLogiqueRepository _peripheriqueRepository;
        private readonly IIotHubService _iotHubService;
        private readonly IZoneRepository _zoneRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PeripheriqueLogiqueService> _logger;

        public PeripheriqueLogiqueService(
            IPeripheriqueLogiqueRepository peripheriqueRepository, 
            IIotHubService iotHubService,
            IZoneRepository zoneRepository,
            IMapper mapper,
            ILogger<PeripheriqueLogiqueService> logger)
        {
            _peripheriqueRepository = peripheriqueRepository;
            _iotHubService = iotHubService;
            _zoneRepository = zoneRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PeripheriqueLogiqueDto>> GetPeripheriquesByZoneIdAsync(int zoneId)
        {
            var peripheriques = await _peripheriqueRepository.GetByZoneIdAsync(zoneId);
            return _mapper.Map<IEnumerable<PeripheriqueLogiqueDto>>(peripheriques);
        }

        public async Task<IEnumerable<PeripheriqueLogiqueDto>> GetAllAsync()
        {
            var peripheriques = await _peripheriqueRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PeripheriqueLogiqueDto>>(peripheriques);
        }

        public async Task<PeripheriqueLogiqueDto?> GetByIdAsync(int id)
        {
            var peripherique = await _peripheriqueRepository.GetByIdAsync(id);
            return _mapper.Map<PeripheriqueLogiqueDto>(peripherique);
        }

        public async Task<PeripheriqueLogiqueDto> CreateAsync(PeripheriqueLogiqueCreationDto peripheriqueDto)
        {
            var peripherique = _mapper.Map<PeripheriqueLogique>(peripheriqueDto);
            await _peripheriqueRepository.AddAsync(peripherique);
            return _mapper.Map<PeripheriqueLogiqueDto>(peripherique);
        }

        public async Task<bool> UpdateAsync(int id, PeripheriqueLogiqueUpdateDto peripheriqueDto)
        {
            var peripherique = await _peripheriqueRepository.GetByIdAsync(id);
            if (peripherique == null) return false;

            _mapper.Map(peripheriqueDto, peripherique);
            return await _peripheriqueRepository.UpdateAsync(peripherique);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _peripheriqueRepository.DeleteAsync(id);
        }

        /// <summary>
        /// Synchronise les devices depuis Azure IoT Hub vers la base de données
        /// </summary>
        public async Task<int> SyncDevicesFromIoTHubAsync()
        {
            try
            {
                _logger.LogInformation("Démarrage de la synchronisation des devices depuis Azure IoT Hub");

                // Récupérer tous les devices depuis Azure IoT Hub
                var iotHubDevices = await _iotHubService.ListDevicesAsync(1000);
                
                if (!iotHubDevices.Any())
                {
                    _logger.LogInformation("Aucun device trouvé dans Azure IoT Hub");
                    return 0;
                }

                // Récupérer tous les devices existants dans la base de données
                var existingDevices = await _peripheriqueRepository.GetAllAsync();
                var existingDeviceIds = existingDevices.Select(d => d.IotDeviceId).ToHashSet();

                // Récupérer une zone par défaut (première zone disponible ou créer une zone "Non assigné")
                var zones = await _zoneRepository.GetAllZonesAsync();
                var defaultZone = zones.FirstOrDefault();
                
                if (defaultZone == null)
                {
                    _logger.LogWarning("Aucune zone disponible pour assigner les nouveaux devices");
                    return 0;
                }

                int syncedCount = 0;

                foreach (var iotDevice in iotHubDevices)
                {
                    // Vérifier si le device existe déjà dans la base de données
                    if (existingDeviceIds.Contains(iotDevice.DeviceId))
                    {
                        _logger.LogDebug("Device {DeviceId} existe déjà dans la base de données", iotDevice.DeviceId);
                        continue;
                    }

                    try
                    {
                        // Récupérer les informations détaillées du device twin
                        var twinInfo = await _iotHubService.GetDeviceTwinAsync(iotDevice.DeviceId);

                        // Déterminer le type du device depuis les desired ou reported properties
                        string deviceType = "Unknown";
                        if (twinInfo != null)
                        {
                            // Essayer d'extraire le type depuis les properties
                            deviceType = ExtractDeviceTypeFromTwin(twinInfo);
                        }

                        // Créer le nouveau périphérique
                        var newPeripherique = new PeripheriqueLogique
                        {
                            Name = iotDevice.DeviceId, // Utiliser l'ID comme nom par défaut
                            Type = deviceType,
                            IotDeviceId = iotDevice.DeviceId,
                            ZoneId = defaultZone.Id
                        };

                        await _peripheriqueRepository.AddAsync(newPeripherique);
                        syncedCount++;

                        _logger.LogInformation(
                            "Device {DeviceId} synchronisé avec succès (Type: {Type}, Zone: {ZoneName})",
                            iotDevice.DeviceId,
                            deviceType,
                            defaultZone.Name
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erreur lors de la synchronisation du device {DeviceId}", iotDevice.DeviceId);
                    }
                }

                _logger.LogInformation(
                    "Synchronisation terminée : {SyncedCount} devices ajoutés sur {TotalDevices} dans IoT Hub",
                    syncedCount,
                    iotHubDevices.Count()
                );

                return syncedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la synchronisation des devices depuis Azure IoT Hub");
                throw;
            }
        }

        /// <summary>
        /// Extrait le type de device depuis les propriétés du twin
        /// </summary>
        private string ExtractDeviceTypeFromTwin(DeviceTwinInfo twinInfo)
        {
            try
            {
                // Essayer de parser les reported properties
                if (!string.IsNullOrWhiteSpace(twinInfo.ReportedProperties) && twinInfo.ReportedProperties != "{}")
                {
                    var reported = System.Text.Json.JsonDocument.Parse(twinInfo.ReportedProperties);
                    if (reported.RootElement.TryGetProperty("deviceType", out var deviceType))
                    {
                        return deviceType.GetString() ?? "Unknown";
                    }
                    if (reported.RootElement.TryGetProperty("type", out var type))
                    {
                        return type.GetString() ?? "Unknown";
                    }
                }

                // Essayer de parser les desired properties
                if (!string.IsNullOrWhiteSpace(twinInfo.DesiredProperties) && twinInfo.DesiredProperties != "{}")
                {
                    var desired = System.Text.Json.JsonDocument.Parse(twinInfo.DesiredProperties);
                    if (desired.RootElement.TryGetProperty("deviceType", out var deviceType))
                    {
                        return deviceType.GetString() ?? "Unknown";
                    }
                    if (desired.RootElement.TryGetProperty("type", out var type))
                    {
                        return type.GetString() ?? "Unknown";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erreur lors de l'extraction du type de device depuis le twin {DeviceId}", twinInfo.DeviceId);
            }

            return "Generic IoT Device";
        }
    }
}

