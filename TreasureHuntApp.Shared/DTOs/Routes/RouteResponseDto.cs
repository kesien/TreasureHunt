namespace TreasureHuntApp.Shared.DTOs.Routes;

public class RouteResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public RouteData? Route { get; set; }
}