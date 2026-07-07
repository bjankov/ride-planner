using RydePlannr.Domain.Enums;

namespace RydePlannr.Application.DTOs.RideEvent;

public class UpdateRideEventDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? CutoffMinutes { get; set; }
    public int? MaxParticipants { get; set; }
    public RideStatus? Status { get; set; } 
}