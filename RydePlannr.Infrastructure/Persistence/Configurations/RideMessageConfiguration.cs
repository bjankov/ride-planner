using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Infrastructure.Persistence.Configurations;

public class RideMessageConfiguration : IEntityTypeConfiguration<RideMessage>
{
    public void Configure(EntityTypeBuilder<RideMessage> builder)
    {
        builder.ToTable("RideMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(m => m.SentAt)
            .IsRequired();

        builder.HasOne(m => m.RideEvent)
            .WithMany()
            .HasForeignKey(m => m.RideEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
