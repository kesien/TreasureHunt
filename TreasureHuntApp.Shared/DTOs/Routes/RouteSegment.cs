namespace TreasureHuntApp.Shared.DTOs.Routes;

public class RouteSegment
{
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public double Distance { get; set; }
    public double Duration { get; set; }
    public List<List<double>> Coordinates { get; set; } = new();
}