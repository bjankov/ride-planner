using RydePlannr.Domain.Enums;
namespace RydePlannr.Domain.Entities;

public class RideEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? CutoffMinutes { get; set; }
    public int MaxParticipants { get; set; }
    public RideStatus Status { get; set; }
    public decimal? EntryFee { get; set; }

    // Foreign keys
    public int OrganizerId { get; set; }
    public int RouteId { get; set; }
    public int RideTypeId { get; set; }
    public int? ClubId { get; set; }

    // Navigation properties
    public User Organizer { get; set; } = null!;
    public Route Route { get; set; } = null!;
    public RideType RideType { get; set; } = null!;
    public Club? Club { get; set; }
    public ICollection<RideParticipant> Participants { get; set; } = new List<RideParticipant>();
}