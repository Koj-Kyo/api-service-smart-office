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
    //public ApplicationDbContext()
    //{
    //}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // NOTE: Cette configuration est nécessaire pour que les outils de migration puissent fonctionner.
        // La chaîne de connexion doit correspondre à celle de votre fichier appsettings.json.
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=BureauIntelligentDB;Username=postgres;Password=246824682468");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
