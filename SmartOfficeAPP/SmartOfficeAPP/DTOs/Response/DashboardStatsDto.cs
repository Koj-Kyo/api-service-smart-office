namespace SmartOfficeAPP.DTOs.Response;

/// <summary>
/// DTO pour les statistiques KPI du Dashboard
/// </summary>
public class DashboardStatsDto
{
    // === Appareils ===
    public int ActiveDevices { get; set; }
    public int TotalDevices { get; set; }
    public double DevicesChangePercent { get; set; }
    
    // === Utilisateurs ===
    public int ActiveUsers { get; set; }
    public int TotalUsers { get; set; }
  public double UsersChangePercent { get; set; }
    
    // === Incidents ===
    public int OpenIncidents { get; set; }
    public int TotalIncidents { get; set; }
    public double IncidentsChangePercent { get; set; }
    
  // === Énergie ===
    public double EnergyConsumed { get; set; }
    public double EnergyChangePercent { get; set; }
}
