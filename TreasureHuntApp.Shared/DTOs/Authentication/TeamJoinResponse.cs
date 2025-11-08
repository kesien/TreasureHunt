namespace TreasureHuntApp.Shared.DTOs.Authentication;

public class TeamJoinResponse
{
    public string SessionToken { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
}