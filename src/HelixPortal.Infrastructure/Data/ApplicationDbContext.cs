using HelixPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelixPortal.Infrastructure.Data;

/// <summary>
/// Main DbContext for the application. Contains all entity sets and configurations.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ClientOrganisation> ClientOrganisations { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestComment> RequestComments { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

