namespace RydePlannr.Application.DTOs.Route;

public class RouteResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double DistanceKm { get; set; }
    public double ElevationGainMeters { get; set; }
    public string Surface { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string StartLocationName { get; set; } = string.Empty;
    public string EndLocationName { get; set; } = string.Empty;
}