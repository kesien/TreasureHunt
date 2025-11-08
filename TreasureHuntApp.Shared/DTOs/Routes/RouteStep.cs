namespace TreasureHuntApp.Shared.DTOs.Routes;

public class RouteStep
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string Maneuver { get; set; } = string.Empty;
}