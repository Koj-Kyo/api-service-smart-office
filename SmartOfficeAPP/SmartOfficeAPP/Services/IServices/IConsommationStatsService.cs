using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Models;

namespace SmartOfficeAPP.Services.IServices;

/// <summary>
/// Service pour gérer les statistiques et analyses de consommation énergétique
/// </summary>
public interface IConsommationStatsService
{
    /// <summary>
    /// Calcule les statistiques de consommation pour un périphérique sur une période
    /// </summary>
    Task<ConsommationStatsDto> GetConsommationStatsAsync(int peripheriqueId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Calcule les statistiques de consommation pour une zone
    /// </summary>
    Task<ConsommationStatsDto> GetConsommationStatsByZoneAsync(int zoneId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Obtient les données de consommation horaire pour des graphiques
    /// </summary>
    Task<IEnumerable<ConsommationHoraireDto>> GetConsommationHoraireAsync(int peripheriqueId, DateTime date);

    /// <summary>
    /// Calcule les tendances de consommation
    /// </summary>
    Task<ConsommationTrendDto> GetConsommationTrendAsync(int peripheriqueId, int nombreJours);

    /// <summary>
    /// Détecte les anomalies de consommation
    /// </summary>
    Task<IEnumerable<AnomalieConsommationDto>> DetectAnomaliesConsommationAsync(int peripheriqueId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Génère un rapport de consommation détaillé
    /// </summary>
    Task<RapportConsommationDto> GenererRapportConsommationAsync(int? zoneId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Calcule les coûts énergétiques basés sur les tarifs
    /// </summary>
    Task<CoutEnergetiqueDto> CalculerCoutEnergetiqueAsync(int peripheriqueId, DateTime startDate, DateTime endDate, decimal tarifKwh);

    /// <summary>
    /// Obtient les périphériques les plus consommateurs
    /// </summary>
    Task<IEnumerable<TopConsommateursDto>> GetTopConsommateursAsync(int? zoneId, DateTime startDate, DateTime endDate, int top = 10);

    /// <summary>
    /// Compare la consommation entre deux périodes
    /// </summary>
    Task<ComparaisonConsommationDto> ComparerConsommationAsync(int peripheriqueId, DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2);
}