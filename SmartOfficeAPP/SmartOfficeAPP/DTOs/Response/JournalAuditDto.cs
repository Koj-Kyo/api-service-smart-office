namespace SmartOfficeAPP.DTOs.Response;

/// <summary>
/// DTO pour l'historique des actions utilisateur (journal d'audit)
/// </summary>
public class JournalAuditDto
{
    /// <summary>
    /// Date et heure de l'action
    /// </summary>
    public DateTime Horodatage { get; set; }

    /// <summary>
    /// Action effectuée (ex: "POST /api/Zones", "DELETE /api/Devices/123")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Contexte de l'action (ex: "API Request", "IoT Hub Command")
    /// </summary>
    public string Contexte { get; set; } = string.Empty;
}

/// <summary>
/// DTO enrichi pour l'admin incluant les détails utilisateur
/// </summary>
public class JournalAuditDetailDto : JournalAuditDto
{
    /// <summary>
    /// ID de l'utilisateur ayant effectué l'action
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Nom d'utilisateur
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Email de l'utilisateur
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// Nom complet de l'utilisateur
    /// </summary>
    public string? UserFullName { get; set; }
}
