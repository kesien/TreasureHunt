namespace TreasureHuntApp.Shared.DTOs.Routes;

public class OptimizedRouteResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<int> OptimalOrder { get; set; } = new(); // Location IDs in optimal order
    public double TotalDistance { get; set; }
    public double TotalDuration { get; set; }
    public List<RouteSegment> RouteSegments { get; set; } = new();
}