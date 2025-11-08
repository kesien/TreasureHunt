using TreasureHuntApp.Shared.DTOs.Locations;

namespace TreasureHuntApp.Shared.DTOs.Routes;

public class OptimizedRouteRequestDto
{
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public List<LocationPointDto> Waypoints { get; set; } = new();
    public TransportMode TransportMode { get; set; }
    public bool ReturnToStart { get; set; } = false;
}