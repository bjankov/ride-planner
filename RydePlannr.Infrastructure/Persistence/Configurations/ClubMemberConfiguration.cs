using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RydePlannr.Domain.Entities;

namespace RydePlannr.Infrastructure.Persistence.Configurations;

public class ClubMemberConfiguration : IEntityTypeConfiguration<ClubMember>
{
    public void Configure(EntityTypeBuilder<ClubMember> builder)
    {
        builder.ToTable("ClubMembers");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.JoinedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Jedan user ne može biti dva puta u istom klubu
        builder.HasIndex(m => new { m.UserId, m.ClubId })
            .IsUnique();

        builder.HasOne(m => m.User)
            .WithMany(u => u.ClubMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Club)
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.ClubId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}