namespace TreasureHuntApp.Shared.DTOs.Routes;

public class RouteData
{
    public double Distance { get; set; } // meters
    public double Duration { get; set; } // seconds
    public List<List<double>> Coordinates { get; set; } = new(); // [longitude, latitude] pairs
    public List<RouteStep> Steps { get; set; } = new();
}