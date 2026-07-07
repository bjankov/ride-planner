namespace RydePlannr.Application.DTOs.RideEvent;

public class CreateRideEventDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? CutoffMinutes { get; set; }
    public int MaxParticipants { get; set; }
    public int RouteId { get; set; }
    public int RideTypeId { get; set; }
    public int? ClubId { get; set; } 
}