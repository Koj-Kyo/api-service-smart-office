using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class PeripheriqueLogiqueRepository : IPeripheriqueLogiqueRepository
{
    private readonly ApplicationDbContext _context;

    public PeripheriqueLogiqueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PeripheriqueLogique> GetByIdAsync(int id)
    {
        return await _context.PeripheriquesLogiques.FindAsync(id);
    }

    public async Task<IEnumerable<PeripheriqueLogique>> GetAllAsync()
    {
        return await _context.PeripheriquesLogiques.ToListAsync();
    }

    public async Task AddAsync(PeripheriqueLogique peripherique)
    {
        await _context.PeripheriquesLogiques.AddAsync(peripherique);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(PeripheriqueLogique peripherique)
    {
        _context.PeripheriquesLogiques.Update(peripherique);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var peripherique = await GetByIdAsync(id);
        if (peripherique == null) return false;

        _context.PeripheriquesLogiques.Remove(peripherique);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PeripheriqueLogique>> GetByZoneIdAsync(int zoneId)
    {
        return await _context.PeripheriquesLogiques.Where(p => p.ZoneId == zoneId).ToListAsync();
    }

    public async Task<PeripheriqueLogique?> GetByIotDeviceIdAndNameAsync(string iotDeviceId, string name)
    {
        return await _context.PeripheriquesLogiques
            .Where(p => p.IotDeviceId == iotDeviceId && p.Name == name)
            .FirstOrDefaultAsync();
    }
}

