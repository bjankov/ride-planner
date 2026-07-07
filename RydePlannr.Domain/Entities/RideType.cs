namespace RydePlannr.Domain.Entities;

public class RideType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<RideEvent> RideEvents { get; set; } = new List<RideEvent>();
}