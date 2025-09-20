using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Repositories.IRepositories;

public interface IDroitAccesRepository
{
    // Récupère tous les droits d'accès.
    Task<IEnumerable<DroitAcces>> GetDroitsAccesAsync();


    // Récupère un seul droit d'accès par son identifiant.
    Task<DroitAcces> GetDroitAccesByIdAsync(int id);

    // Récupère tous les droits d'accès d'un utilisateur sur les zones.
    Task<IEnumerable<DroitAcces>> GetDroitsAccesByUserZonesAsync(string userId);

    // Récupère tous les droits d'accès d'un utilisateur sur les périphériques.
    Task<IEnumerable<DroitAcces>> GetDroitsAccesByUserPeripheriquesAsync(string userId);

    // Ajoute un nouveau droit d'accès.
    Task<DroitAcces> AddDroitAccesAsync(DroitAcces droitAcces);

    // Met à jour un droit d'accès existant.
    Task<bool> UpdateDroitAccesAsync(DroitAcces droitAcces);

    // Supprime un droit d'accès par son identifiant.
    Task<bool> DeleteDroitAccesAsync(int id);
}
