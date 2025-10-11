using SmartOfficeAPP.Repositories.IRepositories;
namespace SmartOfficeAPP.Repositories.Repos;

/// <summary>
/// Implémentation concrète du dépôt pour le modèle Suggestion.
/// Pour des raisons de démonstration, cette version est en mémoire.
/// Vous la remplacerez par une logique de base de données (ex: Entity Framework) plus tard.
/// </summary>
public class SuggestionRepository : ISuggestionRepository
{
    public Task AddAsync(DTOs.Suggestion suggestion)
    {
        // TODO: Remplacez cette ligne par votre logique d'ajout à la base de données.
        // Par exemple, avec Entity Framework Core :
        // _dbContext.Suggestions.Add(suggestion);
        // await _dbContext.SaveChangesAsync();

        Console.WriteLine($"Suggestion de l'IA stockée : {suggestion.ContenuSuggestion}");
        return Task.CompletedTask;
    }
}
