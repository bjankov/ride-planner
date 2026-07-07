using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Infrastructure.Persistence.Configurations;

public class RideEventConfiguration : IEntityTypeConfiguration<RideEvent>
{
    public void Configure(EntityTypeBuilder<RideEvent> builder)
    {
        builder.ToTable("RideEvents");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.MaxParticipants)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();  // sprema enum kao string u bazu umjesto int

        // RideEvent → User (Organizer)
        builder.HasOne(r => r.Organizer)
            .WithMany(u => u.OrganizedRides)
            .HasForeignKey(r => r.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);

        // RideEvent → Route
        builder.HasOne(r => r.Route)
            .WithMany(ro => ro.RideEvents)
            .HasForeignKey(r => r.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        // RideEvent → RideType
        builder.HasOne(r => r.RideType)
            .WithMany(rt => rt.RideEvents)
            .HasForeignKey(r => r.RideTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // RideEvent → Club (opcionalno)
        builder.HasOne(r => r.Club)
            .WithMany(c => c.RideEvents)
            .HasForeignKey(r => r.ClubId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // RideEvent → RideParticipants
        builder.HasMany(r => r.Participants)
            .WithOne(p => p.RideEvent)
            .HasForeignKey(p => p.RideEventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}