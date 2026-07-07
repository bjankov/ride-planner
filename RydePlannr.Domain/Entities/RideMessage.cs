namespace RydePlannr.Domain.Entities;

public class RideMessage
{
    public long Id { get; set; }
    public string Content { get; set; } =  string.Empty;
    public DateTime SentAt { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int RideEventId { get; set; }
    public RideEvent RideEvent { get; set; } = null!;
}