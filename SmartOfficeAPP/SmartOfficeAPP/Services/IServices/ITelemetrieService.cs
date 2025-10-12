using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;
namespace SmartOfficeAPP.Services.IServices;

/// <summary>
/// Définit le contrat pour le service de gestion de la télémétrie et des suggestions de l'IA.
/// </summary>
public interface ITelemetrieService
{
    /// <summary>
    /// Ajoute un nouvel enregistrement de télémétrie.
    /// </summary>
    /// <param name="telemetrie">L'objet Telemetrie à ajouter.</param>
    /// <returns>La tâche asynchrone.</returns>
    Task AddTelemetryAsync(Telemetrie telemetrie);

    /// <summary>
    /// Récupère les données de télémétrie pour un périphérique spécifique.
    /// </summary>
    /// <param name="peripheriqueId">L'ID du périphérique logique.</param>
    /// <returns>Une collection de DTO de télémétrie.</returns>
    Task<IEnumerable<TelemetrieDto>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId);

    /// <summary>
    /// Récupère les données historiques de télémétrie dans une plage de dates.
    /// </summary>
    /// <param name="startDate">Date de début de la période.</param>
    /// <param name="endDate">Date de fin de la période.</param>
    /// <param name="peripheriqueId">ID du périphérique (facultatif).</param>
    /// <returns>Une collection d'enregistrements de télémétrie.</returns>
    Task<IEnumerable<TelemetrieDto>> GetHistoricalDataAsync(DateTime startDate, DateTime endDate, int? peripheriqueId = null);

    /// <summary>
    /// Stocke une suggestion générée par le moteur IA.
    /// </summary>
    /// <param name="suggestion">L'objet Suggestion à stocker.</param>
    /// <returns>La tâche asynchrone.</returns>
    Task StoreAISuggestionAsync(Suggestion suggestion);
}

