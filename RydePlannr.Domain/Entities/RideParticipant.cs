namespace RydePlannr.Domain.Entities;

public class RideParticipant
{
    public int Id { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public TimeSpan FinishTime { get; set; }
    public double? AverageSpeed { get; set; }
    // Foreign keys
    public int UserId { get; set; }
    public int RideEventId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public RideEvent RideEvent { get; set; } = null!; 
}