using RydePlannr.Domain.Enums;

namespace RydePlannr.Application.DTOs.Route;

public class CreateRouteDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double DistanceKm { get; set; }
    public double ElevationGainMeters { get; set; }
    public RouteSurface Surface { get; set; }
    public RouteDifficulty Difficulty { get; set; }
    public int StartLocationId { get; set; }
    public int EndLocationId { get; set; }
}