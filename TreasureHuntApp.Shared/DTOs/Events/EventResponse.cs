using TreasureHuntApp.Core.Entities;

namespace TreasureHuntApp.Shared.DTOs.Events;

public class EventResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public EventType EventType { get; set; }
    public EventStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LocationCount { get; set; }
    public int TeamCount { get; set; }
}