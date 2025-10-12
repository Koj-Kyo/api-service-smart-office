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
    }

    public DbSet<Zone> Zones { get; set; }
    public DbSet<PeripheriqueLogique> PeripheriquesLogiques { get; set; }
    public DbSet<DroitAcces> DroitsAcces { get; set; }
    public DbSet<Commande> Commandes { get; set; }
    public DbSet<Telemetrie> Telemetries { get; set; }
    public DbSet<JournalAudit> JournalAudits { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<IncidentTicket> TicketsIncidents { get; set; }
    public DbSet<RapportUsage> RapportsUsage { get; set; }
}
