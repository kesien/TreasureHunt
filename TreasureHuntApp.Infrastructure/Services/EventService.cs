using Microsoft.EntityFrameworkCore;
using TreasureHuntApp.Core.Entities;
using TreasureHuntApp.Infrastructure.Data;
using TreasureHuntApp.Shared.DTOs.Events;
using TreasureHuntApp.Shared.DTOs.Locations;
using TreasureHuntApp.Shared.DTOs.Teams;

namespace TreasureHuntApp.Infrastructure.Services;
public class EventService(TreasureHuntDbContext context, ITeamCodeGenerator teamCodeGenerator)
    : IEventService
{
    public async Task<EventResponse> CreateEventAsync(CreateEventRequest request)
    {
        var eventEntity = new EventEntity
        {
            Name = request.Name,
            Description = request.Description,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Type = request.EventType,
            Status = request.StartTime > DateTime.UtcNow ? EventStatus.Created : EventStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        context.Events.Add(eventEntity);
        await context.SaveChangesAsync();

        // Add locations if provided
        if (request.Locations.Any())
        {
            foreach (var locationRequest in request.Locations)
            {
                var location = new LocationEntity
                {
                    Name = locationRequest.Name,
                    Description = locationRequest.Description,
                    Address = locationRequest.Address,
                    Latitude = locationRequest.Latitude,
                    Longitude = locationRequest.Longitude,
                    Order = locationRequest.Order,
                    IsRequired = locationRequest.IsRequired,
                    EventId = eventEntity.Id
                };

                context.Locations.Add(location);
            }

            await context.SaveChangesAsync();
        }

        return MapToEventResponse(eventEntity);
    }

    public async Task<EventDetailResponse?> GetEventDetailAsync(int eventId)
    {
        var eventEntity = await context.Events
            .Include(e => e.Teams)
                .ThenInclude(t => t.Visits)
            .Include(e => e.Locations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            return null;
        }

        var response = new EventDetailResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            StartTime = eventEntity.StartTime,
            EndTime = eventEntity.EndTime,
            EventType = eventEntity.Type,
            Status = eventEntity.Status,
            CreatedAt = eventEntity.CreatedAt,
            LocationCount = eventEntity.Locations.Count,
            TeamCount = eventEntity.Teams.Count,
            Locations = eventEntity.Locations.Select(MapToLocationResponse).OrderBy(l => l.Order).ToList(),
            Teams = eventEntity.Teams.Select(t => MapToTeamResponse(t, eventEntity.Locations.Count)).ToList()
        };

        return response;
    }

    public async Task<List<EventResponse>> GetEventsAsync()
    {
        var events = await context.Events
            .Include(e => e.Teams)
            .Include(e => e.Locations)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return events.Select(MapToEventResponse).ToList();
    }

    public async Task<EventResponse?> UpdateEventAsync(int eventId, UpdateEventRequest request)
    {
        var eventEntity = await context.Events.FindAsync(eventId);
        if (eventEntity == null)
        {
            return null;
        }

        // Don't allow updating active events
        if (eventEntity.Status == EventStatus.Active)
        {
            return null;
        }

        eventEntity.Name = request.Name;
        eventEntity.Description = request.Description;
        eventEntity.StartTime = request.StartTime;
        eventEntity.EndTime = request.EndTime;
        eventEntity.Type = request.EventType;

        // Update status based on new times
        eventEntity.Status = request.StartTime > DateTime.UtcNow ? EventStatus.Created : EventStatus.Active;

        await context.SaveChangesAsync();

        return MapToEventResponse(eventEntity);
    }

    public async Task<bool> DeleteEventAsync(int eventId)
    {
        var eventEntity = await context.Events
            .Include(e => e.Teams)
            .Include(e => e.Locations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            return false;
        }

        // Don't allow deleting active events
        if (eventEntity.Status == EventStatus.Active)
        {
            return false;
        }

        context.Events.Remove(eventEntity);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> StartEventAsync(int eventId)
    {
        var eventEntity = await context.Events.FindAsync(eventId);
        if (eventEntity is not { Status: EventStatus.Created })
        {
            return false;
        }

        eventEntity.Status = EventStatus.Active;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EndEventAsync(int eventId)
    {
        var eventEntity = await context.Events.FindAsync(eventId);
        if (eventEntity == null || eventEntity.Status != EventStatus.Active)
        {
            return false;
        }

        eventEntity.Status = EventStatus.Finished;
        await context.SaveChangesAsync();

        return true;
    }

    public async Task CheckAndUpdateEventStatusesAsync()
    {
        var now = DateTime.UtcNow;

        var eventsToStart = await context.Events
            .Where(e => e.Status == EventStatus.Created && e.StartTime <= now)
            .ToListAsync();

        foreach (var eventEntity in eventsToStart)
        {
            eventEntity.Status = EventStatus.Active;
        }

        var eventsToEnd = await context.Events
            .Where(e => e.Status == EventStatus.Active && e.EndTime.AddHours(1) <= now)
            .ToListAsync();

        foreach (var eventEntity in eventsToEnd)
        {
            eventEntity.Status = EventStatus.Finished;
        }

        if (eventsToStart.Any() || eventsToEnd.Any())
        {
            await context.SaveChangesAsync();
        }
    }

    public async Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request)
    {
        var teamCode = await teamCodeGenerator.GenerateUniqueTeamCodeAsync();

        var team = new TeamEntity
        {
            Name = request.Name,
            AccessCode = teamCode,
            EventId = request.EventId
        };

        context.Teams.Add(team);
        await context.SaveChangesAsync();

        // Load event for team count
        var eventEntity = await context.Events
            .Include(e => e.Locations)
            .FirstOrDefaultAsync(e => e.Id == request.EventId);

        return MapToTeamResponse(team, eventEntity?.Locations.Count ?? 0);
    }

    public async Task<List<TeamResponse>> GetEventTeamsAsync(int eventId)
    {
        var teams = await context.Teams
            .Include(t => t.Visits)
            .Where(t => t.EventId == eventId)
            .ToListAsync();

        var locationCount = await context.Locations
            .CountAsync(l => l.EventId == eventId);

        return teams.Select(t => MapToTeamResponse(t, locationCount)).ToList();
    }

    public async Task<bool> DeleteTeamAsync(int teamId)
    {
        var team = await context.Teams.FindAsync(teamId);
        if (team == null)
        {
            return false;
        }

        context.Teams.Remove(team);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<LocationResponse> AddLocationToEventAsync(int eventId, CreateLocationRequest request)
    {
        var location = new LocationEntity
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Order = request.Order,
            IsRequired = request.IsRequired,
            EventId = eventId
        };

        context.Locations.Add(location);
        await context.SaveChangesAsync();

        return MapToLocationResponse(location);
    }

    public async Task<LocationResponse?> UpdateLocationAsync(int locationId, CreateLocationRequest request)
    {
        var location = await context.Locations.FindAsync(locationId);
        if (location == null)
        {
            return null;
        }

        location.Name = request.Name;
        location.Description = request.Description;
        location.Address = request.Address;
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.Order = request.Order;
        location.IsRequired = request.IsRequired;

        await context.SaveChangesAsync();

        return MapToLocationResponse(location);
    }

    public async Task<bool> DeleteLocationAsync(int locationId)
    {
        var location = await context.Locations.FindAsync(locationId);
        if (location == null)
        {
            return false;
        }

        context.Locations.Remove(location);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<List<LocationResponse>> GetEventLocationsAsync(int eventId)
    {
        var locations = await context.Locations
            .Where(l => l.EventId == eventId)
            .OrderBy(l => l.Order)
            .ToListAsync();

        return locations.Select(MapToLocationResponse).ToList();
    }

    public async Task<EventStatsResponse> GetEventStatsAsync()
    {
        var now = DateTime.UtcNow;

        var stats = new EventStatsResponse
        {
            TotalEvents = await context.Events.CountAsync(),
            ActiveEvents = await context.Events.CountAsync(e => e.Status == EventStatus.Active),
            UpcomingEvents = await context.Events.CountAsync(e => e.Status == EventStatus.Created),
            CompletedEvents = await context.Events.CountAsync(e => e.Status == EventStatus.Finished),
            TotalTeams = await context.Teams.CountAsync(),
            ActiveTeams = await context.Teams.CountAsync(t => t.Event.Status == EventStatus.Active)
        };

        return stats;
    }

    public async Task<List<TeamResponse>> GetEventTeamProgressAsync(int eventId)
    {
        var teams = await context.Teams
            .Include(t => t.Visits)
                .ThenInclude(v => v.Location)
            .Where(t => t.EventId == eventId)
            .ToListAsync();

        var locationCount = await context.Locations
            .CountAsync(l => l.EventId == eventId);

        return teams.Select(t => MapToTeamResponse(t, locationCount))
                   .OrderByDescending(t => t.CompletionPercentage)
                   .ToList();
    }

    private static EventResponse MapToEventResponse(EventEntity eventEntity)
    {
        return new EventResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Description = eventEntity.Description,
            StartTime = eventEntity.StartTime,
            EndTime = eventEntity.EndTime,
            EventType = eventEntity.Type,
            Status = eventEntity.Status,
            CreatedAt = eventEntity.CreatedAt,
            LocationCount = eventEntity.Locations?.Count ?? 0,
            TeamCount = eventEntity.Teams?.Count ?? 0
        };
    }

    private static LocationResponse MapToLocationResponse(LocationEntity location)
    {
        return new LocationResponse
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Order = location.Order,
            IsRequired = location.IsRequired,
            EventId = location.EventId
        };
    }

    private static TeamResponse MapToTeamResponse(TeamEntity team, int totalLocations)
    {
        var completedLocations = team.Visits?.Count(v => v.IsCompleted) ?? 0;
        var completionPercentage = totalLocations > 0 ? (double)completedLocations / totalLocations * 100 : 0;

        return new TeamResponse
        {
            Id = team.Id,
            Name = team.Name,
            AccessCode = team.AccessCode,
            LastLatitude = team.LastLatitude,
            LastLongitude = team.LastLongitude,
            LastLocationUpdate = team.LastActiveAt,
            CompletedLocations = completedLocations,
            TotalLocations = totalLocations,
            CompletionPercentage = Math.Round(completionPercentage, 1)
        };
    }
}
