namespace RydePlannr.Application.DTOs.RideEvent;

public class RideEventResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? CutoffMinutes { get; set; }
    public DateTime? CutoffAt { get; set; }
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OrganizerUsername { get; set; } = string.Empty;
    public string RouteName { get; set; } = string.Empty;
    public string RideTypeName { get; set; } = string.Empty;
    public string? ClubName { get; set; } 
}