using System.ComponentModel.DataAnnotations;

namespace TreasureHuntApp.Core.Entities;
public class EventEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public EventType Type { get; set; }

    public EventStatus Status { get; set; }

    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<TeamEntity> Teams { get; set; } = new List<TeamEntity>();
    public ICollection<LocationEntity> Locations { get; set; } = new List<LocationEntity>();
}

public enum EventType
{
    Easter,
    Halloween
}

public enum EventStatus
{
    Created,
    Active,
    Finished
}