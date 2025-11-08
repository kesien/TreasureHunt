namespace TreasureHuntApp.Shared.DTOs.Teams;

public class TeamResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime? LastLocationUpdate { get; set; }
    public int CompletedLocations { get; set; }
    public int TotalLocations { get; set; }
    public double CompletionPercentage { get; set; }
}