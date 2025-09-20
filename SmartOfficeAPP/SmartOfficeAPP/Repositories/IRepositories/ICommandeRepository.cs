using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface ICommandeRepository
{
    // Enregistre une nouvelle commande dans la base de données.
    Task<Commande> AddCommandeAsync(Commande commande);
}
