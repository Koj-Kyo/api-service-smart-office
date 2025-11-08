using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class DroitAccesRepository : IDroitAccesRepository
{
    private readonly ApplicationDbContext _context;

    public DroitAccesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Récupère tous les droits d'accès de manière asynchrone.
    public async Task<IEnumerable<DroitAcces>> GetDroitsAccesAsync()
    {
        return await _context.DroitsAcces
      .Include(da => da.User)       // ✅ Charger l'utilisateur
         .Include(da => da.Zone)
          .Include(da => da.PeripheriqueLogique)
            .OrderByDescending(da => da.CreatedAt)     // ✅ Trier par date de création
   .ToListAsync();
    }

    // Récupère un droit d'accès par son identifiant.
    public async Task<DroitAcces> GetDroitAccesByIdAsync(int id)
    {
        return await _context.DroitsAcces
          .Include(da => da.User)  // ✅ Charger l'utilisateur
   .Include(da => da.Zone)
  .Include(da => da.PeripheriqueLogique)
        .FirstOrDefaultAsync(da => da.Id == id);
    }

    // Récupère tous les droits d'accès d'un utilisateur sur les zones.
    public async Task<IEnumerable<DroitAcces>> GetDroitsAccesByUserZonesAsync(string userId)
    {
        return await _context.DroitsAcces
            .Include(da => da.User)           // ✅ Charger l'utilisateur
.Include(da => da.Zone)
     .Where(da => da.UserId == userId && da.ZoneId != null)
            .OrderByDescending(da => da.CreatedAt)
   .ToListAsync();
    }

    // Récupère tous les droits d'accès d'un utilisateur sur les périphériques.
    public async Task<IEnumerable<DroitAcces>> GetDroitsAccesByUserPeripheriquesAsync(string userId)
    {
        return await _context.DroitsAcces
    .Include(da => da.User)                // ✅ Charger l'utilisateur
            .Include(da => da.PeripheriqueLogique)
            .Where(da => da.UserId == userId && da.PeripheriqueLogiqueId != null)
         .OrderByDescending(da => da.CreatedAt)
       .ToListAsync();
    }

    // Ajoute un nouveau droit d'accès et le retourne.
    public async Task<DroitAcces> AddDroitAccesAsync(DroitAcces droitAcces)
    {
        droitAcces.CreatedAt = DateTime.UtcNow;        // ✅ Définir la date de création
        _context.DroitsAcces.Add(droitAcces);
      await _context.SaveChangesAsync();
      
      // ✅ Recharger avec les relations pour retourner un objet complet
        return await GetDroitAccesByIdAsync(droitAcces.Id);
    }

    // Met à jour un droit d'accès existant.
    public async Task<bool> UpdateDroitAccesAsync(DroitAcces droitAcces)
    {
        var existingDroitAcces = await _context.DroitsAcces.FindAsync(droitAcces.Id);
        if (existingDroitAcces == null)
        {
            return false;
        }

droitAcces.UpdatedAt = DateTime.UtcNow;        // ✅ Définir la date de modification
        _context.Entry(existingDroitAcces).CurrentValues.SetValues(droitAcces);
        await _context.SaveChangesAsync();
        return true;
    }

    // Supprime un droit d'accès par son identifiant.
    public async Task<bool> DeleteDroitAccesAsync(int id)
    {
  var droitAcces = await _context.DroitsAcces.FindAsync(id);
        if (droitAcces == null)
        {
       return false;
        }

        _context.DroitsAcces.Remove(droitAcces);
  await _context.SaveChangesAsync();
        return true;
    }
}
