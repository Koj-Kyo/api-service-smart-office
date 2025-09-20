using SmartOfficeAPP.Data;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Repositories.IRepositories;

namespace SmartOfficeAPP.Repositories.Repos;

public class CommandeRepository : ICommandeRepository
{
    private readonly ApplicationDbContext _context;

    public CommandeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Ajoute une nouvelle commande à la base de données.
    public async Task<Commande> AddCommandeAsync(Commande commande)
    {
        await _context.Commandes.AddAsync(commande);
        await _context.SaveChangesAsync();
        return commande;
    }
}
