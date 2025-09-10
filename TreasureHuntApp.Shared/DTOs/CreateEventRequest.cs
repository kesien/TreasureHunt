using System.ComponentModel.DataAnnotations;
using TreasureHuntApp.Core.Entities;

namespace TreasureHuntApp.Shared.DTOs;
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

    public bool TeamTrackingEnabled { get; set; }
}

public class EventResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public EventType EventType { get; set; }
    public EventStatus Status { get; set; }
    public bool TeamTrackingEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LocationCount { get; set; }
    public int TeamCount { get; set; }
}

public class EventDetailResponse : EventResponse
{
    public List<LocationResponse> Locations { get; set; } = new();
    public List<TeamResponse> Teams { get; set; } = new();
}

public class CreateLocationRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(0, 999)]
    public int Order { get; set; }

    public bool IsRequired { get; set; } = true;
}

public class LocationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public int EventId { get; set; }
}

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

public class CreateTeamRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int EventId { get; set; }
}

public class EventStatsResponse
{
    public int TotalEvents { get; set; }
    public int ActiveEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int CompletedEvents { get; set; }
    public int TotalTeams { get; set; }
    public int ActiveTeams { get; set; }
}
