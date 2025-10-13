namespace SmartOfficeAPP.DTOs;

/// <summary>
/// Statistiques de consommation énergétique
/// </summary>
public class ConsommationStatsDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Consommation totale en mWh
    public double ConsommationTotaleMwh { get; set; }
    
    // Puissance moyenne en mW
    public double PuissanceMoyenneMw { get; set; }
    
    // Puissance maximale en mW
    public double PuissanceMaximaleMw { get; set; }
    
    // Puissance minimale en mW
    public double PuissanceMinimaleMw { get; set; }
    
    // Temps de fonctionnement total en heures
    public double TempsFonctionnementHeures { get; set; }
    
    // Efficacité énergétique (ratio consommation/utilisation)
    public double EfficaciteEnergetique { get; set; }
    
    // Nombre de cycles on/off
    public int NombreCycles { get; set; }
    
    // Temps moyen par cycle en minutes
    public double TempsMoyenCycleMinutes { get; set; }
}

/// <summary>
/// Données de consommation horaire pour graphiques
/// </summary>
public class ConsommationHoraireDto
{
    public DateTime Heure { get; set; }
    public double ConsommationMwh { get; set; }
    public double PuissanceMoyenneMw { get; set; }
    public bool EtatActif { get; set; }
    public int NombreActivations { get; set; }
}

/// <summary>
/// Tendances de consommation
/// </summary>
public class ConsommationTrendDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    public int NombreJours { get; set; }
    
    // Consommation quotidienne moyenne
    public double ConsommationQuotidienneMoyenne { get; set; }
    
    // Tendance (positive = augmentation, négative = diminution)
    public double TendancePourcentage { get; set; }
    
    // Prédiction pour les prochains jours
    public double ConsommationPrediteJour { get; set; }
    
    // Données quotidiennes
    public List<ConsommationQuotidienneDto> DonneesQuotidiennes { get; set; } = new();
}

/// <summary>
/// Consommation quotidienne
/// </summary>
public class ConsommationQuotidienneDto
{
    public DateTime Date { get; set; }
    public double ConsommationMwh { get; set; }
    public double TempsFonctionnementHeures { get; set; }
    public double PuissanceMoyenne { get; set; }
}

/// <summary>
/// Anomalie de consommation détectée
/// </summary>
public class AnomalieConsommationDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    public DateTime DateDetection { get; set; }
    public string TypeAnomalie { get; set; } = string.Empty; // "ConsommationElevee", "ConsommationAnormale", "ArretInattendu"
    public string Description { get; set; } = string.Empty;
    public double ValeurMesuree { get; set; }
    public double ValeurAttendue { get; set; }
    public double EcartPourcentage { get; set; }
    public string Severite { get; set; } = "Medium"; // Low, Medium, High
}

/// <summary>
/// Rapport de consommation détaillé
/// </summary>
public class RapportConsommationDto
{
    public int? ZoneId { get; set; }
    public string? ZoneName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DateGeneration { get; set; }
    
    // Statistiques globales
    public double ConsommationTotaleMwh { get; set; }
    public decimal CoutTotalEstime { get; set; }
    public double EconomiesPotentiellesMwh { get; set; }
    public decimal EconomiesPotentiellesEuros { get; set; }
    
    // Par périphérique
    public List<ConsommationStatsDto> StatistiquesParPeripherique { get; set; } = new();
    
    // Recommandations d'optimisation
    public List<RecommandationOptimisationDto> Recommandations { get; set; } = new();
    
    // Comparaison avec période précédente
    public ComparaisonPeriodeDto? ComparaisonPeriodePrecedente { get; set; }
}

/// <summary>
/// Recommandation d'optimisation énergétique
/// </summary>
public class RecommandationOptimisationDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    public string TypeRecommandation { get; set; } = string.Empty; // "Planification", "Remplacement", "Optimisation"
    public string Description { get; set; } = string.Empty;
    public double EconomieEstimeeMwh { get; set; }
    public decimal EconomieEstimeeEuros { get; set; }
    public string Priorite { get; set; } = "Medium"; // Low, Medium, High
}

/// <summary>
/// Comparaison entre périodes
/// </summary>
public class ComparaisonPeriodeDto
{
    public double ConsommationPrecedenteMwh { get; set; }
    public double ConsommationActuelleMwh { get; set; }
    public double VariationMwh { get; set; }
    public double VariationPourcentage { get; set; }
    public string Tendance { get; set; } = string.Empty; // "Amélioration", "Dégradation", "Stable"
}

/// <summary>
/// Coût énergétique calculé
/// </summary>
public class CoutEnergetiqueDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double ConsommationMwh { get; set; }
    public decimal TarifKwh { get; set; }
    public decimal CoutTotal { get; set; }
    public decimal CoutMoyen { get; set; }
    public decimal CoutJournalier { get; set; }
    
    // Répartition par tranche horaire si applicable
    public List<CoutHoraireDto> CoutsHoraires { get; set; } = new();
}

/// <summary>
/// Coût horaire
/// </summary>
public class CoutHoraireDto
{
    public DateTime Heure { get; set; }
    public double ConsommationMwh { get; set; }
    public decimal TarifKwh { get; set; }
    public decimal Cout { get; set; }
}

/// <summary>
/// Top des consommateurs
/// </summary>
public class TopConsommateursDto
{
    public int PeripheriqueId { get; set; }
    public string PeripheriqueName { get; set; } = string.Empty;
    public string PeripheriqueType { get; set; } = string.Empty;
    public int ZoneId { get; set; }
    public string ZoneName { get; set; } = string.Empty;
    public double ConsommationMwh { get; set; }
    public double PourcentageTotal { get; set; }
    public decimal CoutEstime { get; set; }
    public int Rang { get; set; }
}

/// <summary>
/// Comparaison de consommation entre deux périodes
/// </summary>
public class ComparaisonConsommationDto
{
    public int PeripheriqueId { get; set; }
    public string? PeripheriqueName { get; set; }
    
    // Période 1
    public DateTime StartDate1 { get; set; }
    public DateTime EndDate1 { get; set; }
    public double ConsommationPeriode1Mwh { get; set; }
    
    // Période 2
    public DateTime StartDate2 { get; set; }
    public DateTime EndDate2 { get; set; }
    public double ConsommationPeriode2Mwh { get; set; }
    
    // Comparaison
    public double DifferenceMwh { get; set; }
    public double DifferencePourcentage { get; set; }
    public string Tendance { get; set; } = string.Empty;
    public string Interpretation { get; set; } = string.Empty;
}