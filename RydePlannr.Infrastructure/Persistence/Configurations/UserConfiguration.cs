using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RydePlannr.Domain.Entities;
namespace RydePlannr.Infrastructure.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(u => u.Email)
            .IsUnique();
        builder.HasIndex(u => u.Username)
            .IsUnique();
        builder.Property(u => u.PasswordHash)
            .IsRequired();
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .ValueGeneratedOnAdd();
        // User → Role (N:1)
        builder.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
        // User → ClubMemberships (1:N)
        builder.HasMany(u => u.ClubMemberships)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        // User → RideEvents kao organizator (1:N)
        builder.HasMany(u => u.OrganizedRides)
            .WithOne(r => r.Organizer)
            .HasForeignKey(r => r.OrganizerId)
            .OnDelete(DeleteBehavior.Restrict);
        // User → RideParticipants (1:N)
        builder.HasMany(u => u.Participations)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}