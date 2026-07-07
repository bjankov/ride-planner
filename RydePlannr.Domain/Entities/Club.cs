namespace RydePlannr.Domain.Entities;

public class Club
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<ClubMember> Members { get; set; } = new List<ClubMember>();
    public ICollection<RideEvent> RideEvents { get; set; } = new List<RideEvent>();
}