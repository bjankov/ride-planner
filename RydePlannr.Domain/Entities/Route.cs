using RydePlannr.Domain.Enums;

namespace RydePlannr.Domain.Entities;

public class Route
{
   public int Id { get; set; }
   public string Name { get; set; } =  string.Empty;
   public string? Description { get; set; }
   public double DistanceKm { get; set; }
   public int ElevationGainMeters { get; set; }
   public RouteSurface Surface { get; set; }
   public RouteDifficulty Difficulty { get; set; }
   
   // Foreign keys
   public int StartLocationId { get; set; }
   public int EndLocationId { get; set; }

   // Navigation properties
   public Location StartLocation { get; set; } = null!;
   public Location EndLocation { get; set; } = null!;
   public ICollection<RideEvent> RideEvents { get; set; } = new List<RideEvent>();
}