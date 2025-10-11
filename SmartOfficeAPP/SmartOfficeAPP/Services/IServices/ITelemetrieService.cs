using SmartOfficeAPP.DTOs;

namespace SmartOfficeAPP.Services.IServices;

public interface ITelemetrieService
{
    // Récupère les données de télémétrie pour un périphérique spécifique.
    Task<IEnumerable<TelemetrieDto>> GetTelemetriesByPeripheriqueIdAsync(int peripheriqueId);
    /// <summary>
    /// Représente les suggestions générées par le moteur IA.
    /// Ce modèle simple peut être étendu ultérieurement.
    /// </summary>

    /// <summary>
    /// Définit le contrat pour le service de gestion de la télémétrie et des suggestions de l'IA.
    /// </summary>
     /// <summary>
        /// Ajoute un nouvel enregistrement de télémétrie.
        /// </summary>
        /// <param name="telemetrie">L'objet Telemetrie à ajouter.</param>
        /// <returns>La tâche asynchrone.</returns>
        Task AddTelemetryAsync(Models.Telemetrie telemetrie);

        /// <summary>
        /// Récupère les données historiques de télémétrie dans une plage de dates pour un périphérique donné.
        /// </summary>
        /// <param name="startDate">Date de début de la période.</param>
        /// <param name="endDate">Date de fin de la période.</param>
        /// <param name="peripheriqueId">ID du périphérique (facultatif).</param>
        /// <returns>Une collection d'enregistrements de télémétrie.</returns>
        Task<IEnumerable<Models.Telemetrie>> GetHistoricalDataAsync(DateTime startDate, DateTime endDate, int? peripheriqueId = null);

        /// <summary>
        /// Stocke une suggestion générée par le moteur IA.
        /// </summary>
        /// <param name="suggestion">L'objet Suggestion à stocker.</param>
        /// <returns>La tâche asynchrone.</returns>
        Task StoreAISuggestionAsync(Suggestion suggestion);
    }

