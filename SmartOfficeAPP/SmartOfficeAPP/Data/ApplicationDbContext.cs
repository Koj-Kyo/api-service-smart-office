using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartOfficeAPP.Models;
using SmartOfficeAPP.Models.Identity;

namespace SmartOfficeAPP.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Fallback pour les outils EF uniquement (quand le DI n’a pas injecté d’options).
        if (!optionsBuilder.IsConfigured)
        {
            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            if (!string.IsNullOrWhiteSpace(conn))
            {
                optionsBuilder.UseNpgsql(conn);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Index composite utile pour lectures par périphérique et plage temporelle
        builder.Entity<Telemetrie>()
               .HasIndex(t => new { t.PeripheriqueLogiqueId, t.Horodatage })
               .HasDatabaseName("IX_Telemetries_PeripheriqueLogiqueId_Horodatage");

        // Index pour les lectures par statut de ticket
        builder.Entity<IncidentTicket>()
               .HasIndex(t => t.Statut)
               .HasDatabaseName("IX_TicketsIncidents_Statut");

        // Index composite pour recherche des périphériques physiques par device IoT
        builder.Entity<DevicePeripheral>()
               .HasIndex(p => new { p.IotDeviceId, p.Name })
               .HasDatabaseName("IX_DevicePeripherals_IotDeviceId_Name")
               .IsUnique();

        // Index pour recherche par GPIO
        builder.Entity<DevicePeripheral>()
               .HasIndex(p => new { p.IotDeviceId, p.Gpio })
               .HasDatabaseName("IX_DevicePeripherals_IotDeviceId_Gpio");

        // Index pour les logs de device par deviceId et timestamp
        builder.Entity<DeviceLog>()
               .HasIndex(l => new { l.DeviceId, l.Timestamp })
               .HasDatabaseName("IX_DeviceLogs_DeviceId_Timestamp");

        // Index pour les logs par niveau
        builder.Entity<DeviceLog>()
               .HasIndex(l => l.Level)
               .HasDatabaseName("IX_DeviceLogs_Level");

        // Index pour les logs par périphérique
        builder.Entity<DeviceLog>()
               .HasIndex(l => l.Peripheral)
               .HasDatabaseName("IX_DeviceLogs_Peripheral");
    }

    public DbSet<Zone> Zones { get; set; }
    public DbSet<PeripheriqueLogique> PeripheriquesLogiques { get; set; }
    public DbSet<DevicePeripheral> DevicePeripherals { get; set; }
    public DbSet<DroitAcces> DroitsAcces { get; set; }
    public DbSet<Commande> Commandes { get; set; }
    public DbSet<Telemetrie> Telemetries { get; set; }
    public DbSet<JournalAudit> JournalAudits { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<IncidentTicket> TicketsIncidents { get; set; }
    public DbSet<RapportUsage> RapportsUsage { get; set; }
    public DbSet<DeviceLog> DeviceLogs { get; set; }
}
