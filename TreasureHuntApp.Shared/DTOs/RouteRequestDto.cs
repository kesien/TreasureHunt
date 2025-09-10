using System.Text.Json.Serialization;

namespace TreasureHuntApp.Shared.DTOs;

public class RouteRequestDto
{
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public TransportMode TransportMode { get; set; }
}

public class RouteResponseDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public RouteData? Route { get; set; }
}

public class RouteData
{
    public double Distance { get; set; } // meters
    public double Duration { get; set; } // seconds
    public List<List<double>> Coordinates { get; set; } = new(); // [longitude, latitude] pairs
    public List<RouteStep> Steps { get; set; } = new();
}

public class RouteStep
{
    public double Distance { get; set; }
    public double Duration { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public string Maneuver { get; set; } = string.Empty;
}

public enum TransportMode
{
    [JsonPropertyName("driving")]
    Driving,

    [JsonPropertyName("cycling")]
    Cycling,

    [JsonPropertyName("walking")]
    Walking
}