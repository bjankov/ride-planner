namespace RydePlannr.Domain.Entities;

public class Location
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Navigation properties
    public ICollection<Route> RoutesAsStart { get; set; } = new List<Route>();
    public ICollection<Route> RoutesAsEnd { get; set; } = new List<Route>();
}