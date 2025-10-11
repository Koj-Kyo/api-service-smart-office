namespace SmartOfficeAPP.Repositories.IRepositories;

public interface ISuggestionRepository
{
    Task AddAsync(DTOs.Suggestion suggestion);

}
