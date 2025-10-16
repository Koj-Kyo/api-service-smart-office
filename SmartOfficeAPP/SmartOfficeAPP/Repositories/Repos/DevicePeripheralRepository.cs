using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class DevicePeripheralRepository : IDevicePeripheralRepository
{
    private readonly ApplicationDbContext _context;

    public DevicePeripheralRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DevicePeripheral>> GetAllAsync()
    {
        return await _context.DevicePeripherals
            .Include(p => p.PeripheriqueLogique)
            .OrderBy(p => p.IotDeviceId)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<DevicePeripheral?> GetByIdAsync(int id)
    {
        return await _context.DevicePeripherals
            .Include(p => p.PeripheriqueLogique)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<DevicePeripheral>> GetByIotDeviceIdAsync(string iotDeviceId)
    {
        return await _context.DevicePeripherals
            .Include(p => p.PeripheriqueLogique)
            .Where(p => p.IotDeviceId == iotDeviceId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<DevicePeripheral?> GetByNameAsync(string iotDeviceId, string name)
    {
        return await _context.DevicePeripherals
            .Include(p => p.PeripheriqueLogique)
            .FirstOrDefaultAsync(p => p.IotDeviceId == iotDeviceId && p.Name == name);
    }

    public async Task<DevicePeripheral> AddAsync(DevicePeripheral peripheral)
    {
        peripheral.CreatedAt = DateTime.UtcNow;
        _context.DevicePeripherals.Add(peripheral);
        await _context.SaveChangesAsync();
        return peripheral;
    }

    public async Task<bool> UpdateAsync(DevicePeripheral peripheral)
    {
        peripheral.UpdatedAt = DateTime.UtcNow;
        _context.DevicePeripherals.Update(peripheral);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var peripheral = await _context.DevicePeripherals.FindAsync(id);
        if (peripheral == null) return false;

        _context.DevicePeripherals.Remove(peripheral);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(string iotDeviceId, string name)
    {
        return await _context.DevicePeripherals
            .AnyAsync(p => p.IotDeviceId == iotDeviceId && p.Name == name);
    }
}
