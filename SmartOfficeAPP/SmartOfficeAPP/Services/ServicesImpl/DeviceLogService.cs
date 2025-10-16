using AutoMapper;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class DeviceLogService : IDeviceLogService
{
    private readonly IDeviceLogRepository _deviceLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DeviceLogService> _logger;

    public DeviceLogService(
        IDeviceLogRepository deviceLogRepository,
        IMapper mapper,
        ILogger<DeviceLogService> logger)
    {
        _deviceLogRepository = deviceLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task AddLogAsync(DeviceLogDto logDto)
    {
        var log = _mapper.Map<DeviceLog>(logDto);
        await _deviceLogRepository.AddLogAsync(log);
        _logger.LogDebug("Log ajouté: {DeviceId} - {Level} - {Message}", log.DeviceId, log.Level, log.Message);
    }

    public async Task<IEnumerable<DeviceLogDto>> GetLogsByDeviceIdAsync(string deviceId, int limit = 100)
    {
        var logs = await _deviceLogRepository.GetLogsByDeviceIdAsync(deviceId, limit);
        return _mapper.Map<IEnumerable<DeviceLogDto>>(logs);
    }

    public async Task<IEnumerable<DeviceLogDto>> GetLogsByLevelAsync(string level, int limit = 100)
    {
        var logs = await _deviceLogRepository.GetLogsByLevelAsync(level, limit);
        return _mapper.Map<IEnumerable<DeviceLogDto>>(logs);
    }

    public async Task<IEnumerable<DeviceLogDto>> GetLogsByPeripheralAsync(string peripheral, int limit = 100)
    {
        var logs = await _deviceLogRepository.GetLogsByPeripheralAsync(peripheral, limit);
        return _mapper.Map<IEnumerable<DeviceLogDto>>(logs);
    }

    public async Task<IEnumerable<DeviceLogDto>> GetRecentLogsAsync(int limit = 100)
    {
        var logs = await _deviceLogRepository.GetRecentLogsAsync(limit);
        return _mapper.Map<IEnumerable<DeviceLogDto>>(logs);
    }

    public async Task<IEnumerable<DeviceLogDto>> GetLogsByDeviceIdAndDateRangeAsync(string deviceId, DateTime startDate, DateTime endDate)
    {
        var logs = await _deviceLogRepository.GetLogsByDeviceIdAndDateRangeAsync(deviceId, startDate, endDate);
        return _mapper.Map<IEnumerable<DeviceLogDto>>(logs);
    }
}
