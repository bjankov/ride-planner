namespace RydePlannr.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;
    public ICollection<ClubMember> ClubMemberships { get; set; } = new List<ClubMember>();
    public ICollection<RideEvent> OrganizedRides { get; set; } = new List<RideEvent>();
    public ICollection<RideParticipant> Participations { get; set; } = new List<RideParticipant>();
}