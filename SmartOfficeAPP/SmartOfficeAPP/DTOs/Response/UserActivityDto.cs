namespace SmartOfficeAPP.DTOs.Response;

/// <summary>
/// DTO pour l'historique d'activité utilisateur.
/// </summary>
public class UserActivityDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string ActivityType { get; set; } = string.Empty; // "Commande", "Consultation", "Modification"
    public string Description { get; set; } = string.Empty;
    public string? TargetResource { get; set; } // "Zone: Bureau 1", "Périphérique: Lampe LED"
    public int? TargetResourceId { get; set; }
    public string? ResourceType { get; set; } // "Zone", "Périphérique"
    public string Status { get; set; } = string.Empty; // "Succès", "Échec"
    public string? Details { get; set; }
}

/// <summary>
/// DTO pour les statistiques d'activité.
/// </summary>
public class UserActivityStatsDto
{
    public int TotalActivities { get; set; }
    public int CommandesExecutees { get; set; }
    public int CommandesEchouees { get; set; }
    public int ConsultationsZones { get; set; }
    public int ConsultationsPeripheriques { get; set; }
    public Dictionary<string, int> ActivitiesByType { get; set; } = new();
    public List<UserActivityDto> RecentActivities { get; set; } = new();
    public DateTime? LastActivity { get; set; }
    public Dictionary<string, int> ActivitiesByDay { get; set; } = new(); // "2025-01-15": 5
}
