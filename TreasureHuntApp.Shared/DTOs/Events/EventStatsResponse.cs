namespace TreasureHuntApp.Shared.DTOs.Events;

public class EventStatsResponse
{
    public int TotalEvents { get; set; }
    public int ActiveEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int CompletedEvents { get; set; }
    public int TotalTeams { get; set; }
    public int ActiveTeams { get; set; }
}