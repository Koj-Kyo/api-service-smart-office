namespace SmartOfficeAPP.DTOs;

public class UserProfileDto
{
    // Identifiant unique de l'utilisateur
    public string Id { get; set; } = string.Empty;

    // Adresse e-mail de l'utilisateur
    public string? Email { get; set; }

    // Nom complet de l'utilisateur
    public string? FullName { get; set; }

    // ✅ NOUVEAU: Prénom (extrait de FullName ou séparé)
    public string? FirstName { get; set; }

    // ✅ NOUVEAU: Nom de famille (extrait de FullName ou séparé)
    public string? LastName { get; set; }

    // ✅ NOUVEAU: Liste des rôles de l'utilisateur
    public List<string> Roles { get; set; } = new();
}
