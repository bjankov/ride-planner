using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Infrastructure.Persistence.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.DistanceKm)
            .IsRequired();

        builder.Property(r => r.ElevationGainMeters)
            .IsRequired();

        builder.Property(r => r.Surface)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Difficulty)
            .IsRequired()
            .HasConversion<string>();

        // Route → StartLocation
        builder.HasOne(r => r.StartLocation)
            .WithMany(l => l.RoutesAsStart)
            .HasForeignKey(r => r.StartLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Route → EndLocation
        builder.HasOne(r => r.EndLocation)
            .WithMany(l => l.RoutesAsEnd)
            .HasForeignKey(r => r.EndLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}