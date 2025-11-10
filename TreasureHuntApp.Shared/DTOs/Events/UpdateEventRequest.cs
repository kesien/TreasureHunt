using System.ComponentModel.DataAnnotations;
using TreasureHuntApp.Core.Entities;

namespace TreasureHuntApp.Shared.DTOs.Events;

public class UpdateEventRequest
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

}