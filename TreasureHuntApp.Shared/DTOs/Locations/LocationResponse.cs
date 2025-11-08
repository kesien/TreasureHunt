namespace TreasureHuntApp.Shared.DTOs.Locations;

public class LocationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public int EventId { get; set; }
}