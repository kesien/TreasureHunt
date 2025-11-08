using TreasureHuntApp.Shared.DTOs.Locations;
using TreasureHuntApp.Shared.DTOs.Teams;

namespace TreasureHuntApp.Shared.DTOs.Events;

public class EventDetailResponse : EventResponse
{
    public List<LocationResponse> Locations { get; set; } = new();
    public List<TeamResponse> Teams { get; set; } = new();
}