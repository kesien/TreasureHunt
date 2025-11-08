using System.ComponentModel.DataAnnotations;
using TreasureHuntApp.Core.Entities;
using TreasureHuntApp.Shared.DTOs.Locations;

namespace TreasureHuntApp.Shared.DTOs.Events;
public class CreateEventRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Required]
    public EventType EventType { get; set; }

    public bool TeamTrackingEnabled { get; set; } = true;

    public List<CreateLocationRequest> Locations { get; set; } = new();
}