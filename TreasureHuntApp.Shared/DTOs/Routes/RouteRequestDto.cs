using System.Text.Json.Serialization;

namespace TreasureHuntApp.Shared.DTOs.Routes;

public class RouteRequestDto
{
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public TransportMode TransportMode { get; set; }
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