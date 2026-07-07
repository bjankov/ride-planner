using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Infrastructure.Persistence.Configurations;

public class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Clubs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.HasMany(c => c.Members)
            .WithOne(m => m.Club)
            .HasForeignKey(m => m.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.RideEvents)
            .WithOne(r => r.Club)
            .HasForeignKey(r => r.ClubId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}