namespace TreasureHuntApp.Shared.DTOs;

public class OptimizedRouteRequestDto
{
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public List<LocationPointDto> Waypoints { get; set; } = new();
    public TransportMode TransportMode { get; set; }
    public bool ReturnToStart { get; set; } = false;
}

public class LocationPointDto
{
    public int LocationId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class OptimizedRouteResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<int> OptimalOrder { get; set; } = new(); // Location IDs in optimal order
    public double TotalDistance { get; set; }
    public double TotalDuration { get; set; }
    public List<RouteSegment> RouteSegments { get; set; } = new();
}

public class RouteSegment
{
    public int FromLocationId { get; set; }
    public int ToLocationId { get; set; }
    public double Distance { get; set; }
    public double Duration { get; set; }
    public List<List<double>> Coordinates { get; set; } = new();
}