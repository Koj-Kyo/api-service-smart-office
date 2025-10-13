using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Data;
using SmartOfficeAPP.DTOs;
using SmartOfficeAPP.Services.IServices;

namespace SmartOfficeAPP.Services.ServicesImpl;

public class ConsommationStatsService : IConsommationStatsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConsommationStatsService> _logger;
    private readonly ISignalRService _signalRService;

    public ConsommationStatsService(
        ApplicationDbContext context,
        ILogger<ConsommationStatsService> logger,
        ISignalRService signalRService)
    {
        _context = context;
        _logger = logger;
        _signalRService = signalRService;
    }

    public async Task<ConsommationStatsDto> GetConsommationStatsAsync(int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var peripherique = await _context.PeripheriquesLogiques
                .Include(p => p.Zone)
                .FirstOrDefaultAsync(p => p.Id == peripheriqueId);

            if (peripherique == null)
            {
                throw new ArgumentException($"Périphérique avec l'ID {peripheriqueId} non trouvé");
            }

            // Récupérer les données de télémétrie énergétique
            var telemetries = await _context.Telemetries
                .Where(t => t.PeripheriqueLogiqueId == peripheriqueId &&
                           t.Horodatage >= startDate &&
                           t.Horodatage <= endDate &&
                           (t.TypeEvenement == "energy_mwh" || t.TypeEvenement == "power_mw" || t.TypeEvenement == "state"))
                .OrderBy(t => t.Horodatage)
                .ToListAsync();

            // Calculer les statistiques
            var energyData = telemetries.Where(t => t.TypeEvenement == "energy_mwh").ToList();
            var powerData = telemetries.Where(t => t.TypeEvenement == "power_mw").ToList();
            var stateData = telemetries.Where(t => t.TypeEvenement == "state").ToList();

            var stats = new ConsommationStatsDto
            {
                PeripheriqueId = peripheriqueId,
                PeripheriqueName = peripherique.Name,
                ZoneId = peripherique.ZoneId,
                ZoneName = peripherique.Zone?.Name,
                StartDate = startDate,
                EndDate = endDate
            };

            if (energyData.Any())
            {
                var firstEnergy = energyData.First().Valeur;
                var lastEnergy = energyData.Last().Valeur;
                stats.ConsommationTotaleMwh = Math.Max(0, lastEnergy - firstEnergy);
            }

            if (powerData.Any())
            {
                stats.PuissanceMoyenneMw = powerData.Average(p => p.Valeur);
                stats.PuissanceMaximaleMw = powerData.Max(p => p.Valeur);
                stats.PuissanceMinimaleMw = powerData.Min(p => p.Valeur);
            }

            // Calculer le temps de fonctionnement
            if (stateData.Any())
            {
                var tempsActiveTicks = CalculerTempsFonctionnement(stateData);
                stats.TempsFonctionnementHeures = TimeSpan.FromTicks(tempsActiveTicks).TotalHours;
                stats.NombreCycles = CalculerNombreCycles(stateData);
                
                if (stats.NombreCycles > 0)
                {
                    stats.TempsMoyenCycleMinutes = (stats.TempsFonctionnementHeures * 60) / stats.NombreCycles;
                }
            }

            // Calculer l'efficacité énergétique
            var totalHours = (endDate - startDate).TotalHours;
            if (totalHours > 0 && stats.TempsFonctionnementHeures > 0)
            {
                var ratioUtilisation = stats.TempsFonctionnementHeures / totalHours;
                var ratioConsommation = stats.ConsommationTotaleMwh / (stats.PuissanceMoyenneMw * totalHours);
                stats.EfficaciteEnergetique = ratioUtilisation > 0 ? ratioConsommation / ratioUtilisation : 0;
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul des statistiques de consommation pour le périphérique {PeripheriqueId}", peripheriqueId);
            throw;
        }
    }

    public async Task<ConsommationStatsDto> GetConsommationStatsByZoneAsync(int zoneId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var zone = await _context.Zones
                .Include(z => z.Peripheriques)
                .FirstOrDefaultAsync(z => z.Id == zoneId);

            if (zone == null)
            {
                throw new ArgumentException($"Zone avec l'ID {zoneId} non trouvée");
            }

            var peripheriqueIds = zone.Peripheriques.Select(p => p.Id).ToList();
            
            var telemetries = await _context.Telemetries
                .Where(t => peripheriqueIds.Contains(t.PeripheriqueLogiqueId) &&
                           t.Horodatage >= startDate &&
                           t.Horodatage <= endDate &&
                           (t.TypeEvenement == "energy_mwh" || t.TypeEvenement == "power_mw" || t.TypeEvenement == "state"))
                .ToListAsync();

            // Calculer les statistiques agrégées pour la zone
            var energyData = telemetries.Where(t => t.TypeEvenement == "energy_mwh").ToList();
            var powerData = telemetries.Where(t => t.TypeEvenement == "power_mw").ToList();
            var stateData = telemetries.Where(t => t.TypeEvenement == "state").ToList();

            var stats = new ConsommationStatsDto
            {
                ZoneId = zoneId,
                ZoneName = zone.Name,
                StartDate = startDate,
                EndDate = endDate
            };

            // Calculer consommation totale (somme de tous les périphériques)
            var consommationTotale = 0.0;
            foreach (var peripheriqueId in peripheriqueIds)
            {
                var peripheriqueEnergyData = energyData.Where(e => e.PeripheriqueLogiqueId == peripheriqueId).ToList();
                if (peripheriqueEnergyData.Any())
                {
                    var firstEnergy = peripheriqueEnergyData.First().Valeur;
                    var lastEnergy = peripheriqueEnergyData.Last().Valeur;
                    consommationTotale += Math.Max(0, lastEnergy - firstEnergy);
                }
            }
            stats.ConsommationTotaleMwh = consommationTotale;

            if (powerData.Any())
            {
                stats.PuissanceMoyenneMw = powerData.Average(p => p.Valeur);
                stats.PuissanceMaximaleMw = powerData.Max(p => p.Valeur);
                stats.PuissanceMinimaleMw = powerData.Min(p => p.Valeur);
            }

            // Temps de fonctionnement moyen
            var tempsTotalTicks = 0L;
            var nombreCyclesTotal = 0;
            
            foreach (var peripheriqueId in peripheriqueIds)
            {
                var peripheriqueStateData = stateData.Where(s => s.PeripheriqueLogiqueId == peripheriqueId).ToList();
                if (peripheriqueStateData.Any())
                {
                    tempsTotalTicks += CalculerTempsFonctionnement(peripheriqueStateData);
                    nombreCyclesTotal += CalculerNombreCycles(peripheriqueStateData);
                }
            }

            stats.TempsFonctionnementHeures = TimeSpan.FromTicks(tempsTotalTicks).TotalHours;
            stats.NombreCycles = nombreCyclesTotal;

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul des statistiques de consommation pour la zone {ZoneId}", zoneId);
            throw;
        }
    }

    public async Task<IEnumerable<ConsommationHoraireDto>> GetConsommationHoraireAsync(int peripheriqueId, DateTime date)
    {
        try
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var telemetries = await _context.Telemetries
                .Where(t => t.PeripheriqueLogiqueId == peripheriqueId &&
                           t.Horodatage >= startDate &&
                           t.Horodatage < endDate &&
                           (t.TypeEvenement == "energy_mwh" || t.TypeEvenement == "power_mw" || t.TypeEvenement == "state"))
                .OrderBy(t => t.Horodatage)
                .ToListAsync();

            var result = new List<ConsommationHoraireDto>();

            for (int hour = 0; hour < 24; hour++)
            {
                var hourStart = startDate.AddHours(hour);
                var hourEnd = hourStart.AddHours(1);

                var hourTelemetries = telemetries.Where(t => t.Horodatage >= hourStart && t.Horodatage < hourEnd).ToList();
                
                var energyData = hourTelemetries.Where(t => t.TypeEvenement == "energy_mwh").ToList();
                var powerData = hourTelemetries.Where(t => t.TypeEvenement == "power_mw").ToList();
                var stateData = hourTelemetries.Where(t => t.TypeEvenement == "state").ToList();

                var hourlyData = new ConsommationHoraireDto
                {
                    Heure = hourStart
                };

                if (energyData.Count >= 2)
                {
                    var firstEnergy = energyData.First().Valeur;
                    var lastEnergy = energyData.Last().Valeur;
                    hourlyData.ConsommationMwh = Math.Max(0, lastEnergy - firstEnergy);
                }

                if (powerData.Any())
                {
                    hourlyData.PuissanceMoyenneMw = powerData.Average(p => p.Valeur);
                }

                if (stateData.Any())
                {
                    hourlyData.EtatActif = stateData.Any(s => s.Valeur > 0);
                    hourlyData.NombreActivations = CalculerNombreCycles(stateData);
                }

                result.Add(hourlyData);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des données horaires pour le périphérique {PeripheriqueId}", peripheriqueId);
            throw;
        }
    }

    public async Task<ConsommationTrendDto> GetConsommationTrendAsync(int peripheriqueId, int nombreJours)
    {
        try
        {
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-nombreJours);

            var peripherique = await _context.PeripheriquesLogiques
                .FirstOrDefaultAsync(p => p.Id == peripheriqueId);

            if (peripherique == null)
            {
                throw new ArgumentException($"Périphérique avec l'ID {peripheriqueId} non trouvé");
            }

            var telemetries = await _context.Telemetries
                .Where(t => t.PeripheriqueLogiqueId == peripheriqueId &&
                           t.Horodatage >= startDate &&
                           t.Horodatage <= endDate &&
                           t.TypeEvenement == "energy_mwh")
                .OrderBy(t => t.Horodatage)
                .ToListAsync();

            var trend = new ConsommationTrendDto
            {
                PeripheriqueId = peripheriqueId,
                PeripheriqueName = peripherique.Name,
                NombreJours = nombreJours
            };

            var donneesQuotidiennes = new List<ConsommationQuotidienneDto>();

            for (int i = 0; i < nombreJours; i++)
            {
                var currentDate = startDate.AddDays(i);
                var nextDate = currentDate.AddDays(1);

                var dayTelemetries = telemetries
                    .Where(t => t.Horodatage >= currentDate && t.Horodatage < nextDate)
                    .ToList();

                var dayData = new ConsommationQuotidienneDto
                {
                    Date = currentDate
                };

                if (dayTelemetries.Count >= 2)
                {
                    var firstEnergy = dayTelemetries.First().Valeur;
                    var lastEnergy = dayTelemetries.Last().Valeur;
                    dayData.ConsommationMwh = Math.Max(0, lastEnergy - firstEnergy);
                }

                donneesQuotidiennes.Add(dayData);
            }

            trend.DonneesQuotidiennes = donneesQuotidiennes;

            if (donneesQuotidiennes.Any())
            {
                trend.ConsommationQuotidienneMoyenne = donneesQuotidiennes.Average(d => d.ConsommationMwh);

                // Calculer la tendance (régression linéaire simple)
                if (donneesQuotidiennes.Count > 1)
                {
                    var premiereMoitie = donneesQuotidiennes.Take(donneesQuotidiennes.Count / 2).Average(d => d.ConsommationMwh);
                    var deuxiemeMoitie = donneesQuotidiennes.Skip(donneesQuotidiennes.Count / 2).Average(d => d.ConsommationMwh);
                    
                    if (premiereMoitie > 0)
                    {
                        trend.TendancePourcentage = ((deuxiemeMoitie - premiereMoitie) / premiereMoitie) * 100;
                    }

                    // Prédiction simple basée sur la tendance
                    trend.ConsommationPrediteJour = trend.ConsommationQuotidienneMoyenne * (1 + trend.TendancePourcentage / 100);
                }
            }

            return trend;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul des tendances pour le périphérique {PeripheriqueId}", peripheriqueId);
            throw;
        }
    }

    public async Task<IEnumerable<AnomalieConsommationDto>> DetectAnomaliesConsommationAsync(int peripheriqueId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var peripherique = await _context.PeripheriquesLogiques
                .FirstOrDefaultAsync(p => p.Id == peripheriqueId);

            if (peripherique == null)
            {
                throw new ArgumentException($"Périphérique avec l'ID {peripheriqueId} non trouvé");
            }

            var telemetries = await _context.Telemetries
                .Where(t => t.PeripheriqueLogiqueId == peripheriqueId &&
                           t.Horodatage >= startDate &&
                           t.Horodatage <= endDate &&
                           (t.TypeEvenement == "power_mw" || t.TypeEvenement == "energy_mwh"))
                .OrderBy(t => t.Horodatage)
                .ToListAsync();

            var anomalies = new List<AnomalieConsommationDto>();

            if (!telemetries.Any()) return anomalies;

            // Calculer les seuils basés sur l'historique
            var powerData = telemetries.Where(t => t.TypeEvenement == "power_mw").Select(t => t.Valeur).ToList();
            
            if (powerData.Any())
            {
                var average = powerData.Average();
                var stdDev = CalculateStandardDeviation(powerData, average);
                var seuilHaut = average + (2 * stdDev);
                var seuilBas = Math.Max(0, average - (2 * stdDev));

                foreach (var telemetry in telemetries.Where(t => t.TypeEvenement == "power_mw"))
                {
                    if (telemetry.Valeur > seuilHaut)
                    {
                        anomalies.Add(new AnomalieConsommationDto
                        {
                            PeripheriqueId = peripheriqueId,
                            PeripheriqueName = peripherique.Name,
                            DateDetection = telemetry.Horodatage,
                            TypeAnomalie = "ConsommationElevee",
                            Description = $"Consommation anormalement élevée détectée",
                            ValeurMesuree = telemetry.Valeur,
                            ValeurAttendue = average,
                            EcartPourcentage = ((telemetry.Valeur - average) / average) * 100,
                            Severite = telemetry.Valeur > (seuilHaut * 1.5) ? "High" : "Medium"
                        });
                    }
                    else if (telemetry.Valeur < seuilBas && average > 10) // Éviter les fausses alertes pour de très faibles consommations
                    {
                        anomalies.Add(new AnomalieConsommationDto
                        {
                            PeripheriqueId = peripheriqueId,
                            PeripheriqueName = peripherique.Name,
                            DateDetection = telemetry.Horodatage,
                            TypeAnomalie = "ConsommationAnormale",
                            Description = $"Consommation anormalement faible détectée",
                            ValeurMesuree = telemetry.Valeur,
                            ValeurAttendue = average,
                            EcartPourcentage = ((average - telemetry.Valeur) / average) * 100,
                            Severite = "Low"
                        });
                    }
                }
            }

            return anomalies.OrderByDescending(a => a.DateDetection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la détection d'anomalies pour le périphérique {PeripheriqueId}", peripheriqueId);
            throw;
        }
    }

    public async Task<RapportConsommationDto> GenererRapportConsommationAsync(int? zoneId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var rapport = new RapportConsommationDto
            {
                ZoneId = zoneId,
                StartDate = startDate,
                EndDate = endDate,
                DateGeneration = DateTime.UtcNow
            };

            List<int> peripheriqueIds;

            if (zoneId.HasValue)
            {
                var zone = await _context.Zones
                    .Include(z => z.Peripheriques)
                    .FirstOrDefaultAsync(z => z.Id == zoneId.Value);

                if (zone == null)
                {
                    throw new ArgumentException($"Zone avec l'ID {zoneId} non trouvée");
                }

                rapport.ZoneName = zone.Name;
                peripheriqueIds = zone.Peripheriques.Select(p => p.Id).ToList();
            }
            else
            {
                peripheriqueIds = await _context.PeripheriquesLogiques.Select(p => p.Id).ToListAsync();
            }

            // Calculer les statistiques par périphérique
            var statistiquesParPeripherique = new List<ConsommationStatsDto>();
            foreach (var peripheriqueId in peripheriqueIds)
            {
                var stats = await GetConsommationStatsAsync(peripheriqueId, startDate, endDate);
                statistiquesParPeripherique.Add(stats);
            }

            rapport.StatistiquesParPeripherique = statistiquesParPeripherique;
            rapport.ConsommationTotaleMwh = statistiquesParPeripherique.Sum(s => s.ConsommationTotaleMwh);

            // Générer des recommandations simples
            var recommandations = new List<RecommandationOptimisationDto>();
            
            var grosConsommateurs = statistiquesParPeripherique
                .Where(s => s.ConsommationTotaleMwh > 0)
                .OrderByDescending(s => s.ConsommationTotaleMwh)
                .Take(3);

            foreach (var grosConsommateur in grosConsommateurs)
            {
                if (grosConsommateur.EfficaciteEnergetique < 0.7) // Seuil d'efficacité
                {
                    recommandations.Add(new RecommandationOptimisationDto
                    {
                        PeripheriqueId = grosConsommateur.PeripheriqueId,
                        PeripheriqueName = grosConsommateur.PeripheriqueName,
                        TypeRecommandation = "Optimisation",
                        Description = $"Optimiser la programmation de {grosConsommateur.PeripheriqueName} pour améliorer l'efficacité énergétique",
                        EconomieEstimeeMwh = grosConsommateur.ConsommationTotaleMwh * 0.15, // Estimation 15% d'économie
                        Priorite = grosConsommateur.ConsommationTotaleMwh > (rapport.ConsommationTotaleMwh * 0.2) ? "High" : "Medium"
                    });
                }
            }

            rapport.Recommandations = recommandations;
            rapport.EconomiesPotentiellesMwh = recommandations.Sum(r => r.EconomieEstimeeMwh);

            return rapport;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du rapport de consommation");
            throw;
        }
    }

    public async Task<CoutEnergetiqueDto> CalculerCoutEnergetiqueAsync(int peripheriqueId, DateTime startDate, DateTime endDate, decimal tarifKwh)
    {
        try
        {
            var stats = await GetConsommationStatsAsync(peripheriqueId, startDate, endDate);
            
            var consommationKwh = stats.ConsommationTotaleMwh / 1000; // Conversion mWh en kWh
            var coutTotal = (decimal)consommationKwh * tarifKwh;
            var nombreJours = (endDate - startDate).TotalDays;

            return new CoutEnergetiqueDto
            {
                PeripheriqueId = peripheriqueId,
                PeripheriqueName = stats.PeripheriqueName,
                StartDate = startDate,
                EndDate = endDate,
                ConsommationMwh = stats.ConsommationTotaleMwh,
                TarifKwh = tarifKwh,
                CoutTotal = coutTotal,
                CoutMoyen = nombreJours > 0 ? coutTotal / (decimal)nombreJours : 0,
                CoutJournalier = nombreJours > 0 ? coutTotal / (decimal)nombreJours : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul du coût énergétique pour le périphérique {PeripheriqueId}", peripheriqueId);
            throw;
        }
    }

    public async Task<IEnumerable<TopConsommateursDto>> GetTopConsommateursAsync(int? zoneId, DateTime startDate, DateTime endDate, int top = 10)
    {
        try
        {
            var query = _context.PeripheriquesLogiques.Include(p => p.Zone).AsQueryable();

            if (zoneId.HasValue)
            {
                query = query.Where(p => p.ZoneId == zoneId.Value);
            }

            var peripheriques = await query.ToListAsync();
            var topConsommateurs = new List<TopConsommateursDto>();

            foreach (var peripherique in peripheriques)
            {
                var stats = await GetConsommationStatsAsync(peripherique.Id, startDate, endDate);
                
                if (stats.ConsommationTotaleMwh > 0)
                {
                    topConsommateurs.Add(new TopConsommateursDto
                    {
                        PeripheriqueId = peripherique.Id,
                        PeripheriqueName = peripherique.Name,
                        PeripheriqueType = peripherique.Type,
                        ZoneId = peripherique.ZoneId,
                        ZoneName = peripherique.Zone?.Name ?? "",
                        ConsommationMwh = stats.ConsommationTotaleMwh
                    });
                }
            }

            var ordered = topConsommateurs.OrderByDescending(t => t.ConsommationMwh).Take(top).ToList();
            var consommationTotale = topConsommateurs.Sum(t => t.ConsommationMwh);

            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Rang = i + 1;
                ordered[i].PourcentageTotal = consommationTotale > 0 ? (ordered[i].ConsommationMwh / consommationTotale) * 100 : 0;
            }

            return ordered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du top des consommateurs");
            throw;
        }
    }

    public async Task<ComparaisonConsommationDto> ComparerConsommationAsync(int peripheriqueId, DateTime startDate1, DateTime endDate1, DateTime startDate2, DateTime endDate2)
    {
        try
        {
            var stats1 = await GetConsommationStatsAsync(peripheriqueId, startDate1, endDate1);
            var stats2 = await GetConsommationStatsAsync(peripheriqueId, startDate2, endDate2);

            var difference = stats2.ConsommationTotaleMwh - stats1.ConsommationTotaleMwh;
            var differencePourcentage = stats1.ConsommationTotaleMwh > 0 ? (difference / stats1.ConsommationTotaleMwh) * 100 : 0;

            string tendance, interpretation;
            if (Math.Abs(differencePourcentage) < 5)
            {
                tendance = "Stable";
                interpretation = "La consommation est restée relativement stable entre les deux périodes.";
            }
            else if (differencePourcentage > 0)
            {
                tendance = "Augmentation";
                interpretation = $"La consommation a augmenté de {differencePourcentage:F1}% entre les deux périodes.";
            }
            else
            {
                tendance = "Diminution";
                interpretation = $"La consommation a diminué de {Math.Abs(differencePourcentage):F1}% entre les deux périodes.";
            }

            return new ComparaisonConsommationDto
            {
                PeripheriqueId = peripheriqueId,
                PeripheriqueName = stats1.PeripheriqueName,
                StartDate1 = startDate1,
                EndDate1 = endDate1,
                ConsommationPeriode1Mwh = stats1.ConsommationTotaleMwh,
                StartDate2 = startDate2,
                EndDate2 = endDate2,
                ConsommationPeriode2Mwh = stats2.ConsommationTotaleMwh,
                DifferenceMwh = difference,
                DifferencePourcentage = differencePourcentage,
                Tendance = tendance,
                Interpretation = interpretation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la comparaison de consommation pour le périphérique {PeripheriqueId}", peripheriqueId);
            throw;
        }
    }

    #region Méthodes utilitaires privées

    private long CalculerTempsFonctionnement(List<Models.Telemetrie> stateData)
    {
        if (!stateData.Any()) return 0;

        long totalTicks = 0;
        DateTime? debutActivation = null;

        foreach (var data in stateData.OrderBy(d => d.Horodatage))
        {
            if (data.Valeur > 0 && debutActivation == null)
            {
                debutActivation = data.Horodatage;
            }
            else if (data.Valeur == 0 && debutActivation.HasValue)
            {
                totalTicks += (data.Horodatage - debutActivation.Value).Ticks;
                debutActivation = null;
            }
        }

        // Si encore actif à la fin
        if (debutActivation.HasValue)
        {
            totalTicks += (stateData.Last().Horodatage - debutActivation.Value).Ticks;
        }

        return totalTicks;
    }

    private int CalculerNombreCycles(List<Models.Telemetrie> stateData)
    {
        if (!stateData.Any()) return 0;

        int cycles = 0;
        bool? previousState = null;

        foreach (var data in stateData.OrderBy(d => d.Horodatage))
        {
            bool currentState = data.Valeur > 0;
            
            if (previousState.HasValue && !previousState.Value && currentState)
            {
                cycles++;
            }
            
            previousState = currentState;
        }

        return cycles;
    }

    private double CalculateStandardDeviation(List<double> values, double average)
    {
        if (values.Count <= 1) return 0;

        var sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
        return Math.Sqrt(sumOfSquaresOfDifferences / (values.Count - 1));
    }

    #endregion
}