using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Infrastructure.Persistence.Configurations;

public class RideParticipantConfiguration : IEntityTypeConfiguration<RideParticipant>
{
    public void Configure(EntityTypeBuilder<RideParticipant> builder)
    {
        builder.ToTable("RideParticipants");

        builder.HasKey(p => p.Id);

        // Jedan user ne može se dva puta prijaviti na isti događaj
        builder.HasIndex(p => new { p.RideEventId, p.UserId })
            .IsUnique();
    }
}