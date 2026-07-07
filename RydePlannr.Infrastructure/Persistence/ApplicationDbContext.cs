using Microsoft.EntityFrameworkCore;
using RydePlannr.Domain.Entities;
using RydePlannr.Domain.Enums;

namespace RydePlannr.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Club> Clubs { get; set; }
    public DbSet<RideEvent> RideEvents { get; set; }
    public DbSet<RideParticipant> RideParticipants { get; set; }
    public DbSet<RideType> RideTypes { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<RideMessage> RideMessages { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<RideType>().HasData(
            new RideType { Id = 1, Name = "Leisure",    Description = "Opuštena vožnja bez vremenskog pritiska" },
            new RideType { Id = 2, Name = "Training",   Description = "Strukturirana vožnja s ciljem poboljšanja forme" },
            new RideType { Id = 3, Name = "Race",       Description = "Kompetitivna vožnja s mjerenjem vremena" },
            new RideType { Id = 4, Name = "Gran Fondo", Description = "Masovna sportska vožnja na duge relacije" },
            new RideType { Id = 5, Name = "Night Ride", Description = "Noćna vožnja" },
            new RideType { Id = 6, Name = "Charity Ride", Description = "Vožnja organizirana u dobrotvorne svrhe s prikupljanjem donacija"} 
        );

        base.OnModelCreating(modelBuilder);
    }
}