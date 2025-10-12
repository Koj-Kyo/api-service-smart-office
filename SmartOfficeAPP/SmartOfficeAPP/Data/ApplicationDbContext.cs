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
        // Gardé UNIQUEMENT pour les outils EF si le DI n’a pas configuré le contexte.
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

        // Index utile pour requêtes temporelles/filtrage par périphérique
        builder.Entity<Telemetrie>()
               .HasIndex(t => new { t.PeripheriqueLogiqueId, t.Horodatage });
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
