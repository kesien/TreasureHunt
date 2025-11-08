namespace TreasureHuntApp.Shared.DTOs.Locations;

public class LocationPointDto
{
    public int LocationId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = string.Empty;
}