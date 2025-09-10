using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Infrastructure.Services;
public interface IEventService
{
    // Event CRUD operations
    Task<EventResponse> CreateEventAsync(CreateEventRequest request);
    Task<EventResponse?> GetEventAsync(int eventId);
    Task<EventDetailResponse?> GetEventDetailAsync(int eventId);
    Task<List<EventResponse>> GetEventsAsync();
    Task<EventResponse?> UpdateEventAsync(int eventId, UpdateEventRequest request);
    Task<bool> DeleteEventAsync(int eventId);

    // Event status management
    Task<bool> StartEventAsync(int eventId);
    Task<bool> EndEventAsync(int eventId);
    Task CheckAndUpdateEventStatusesAsync(); // Background service method

    // Team management within events
    Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request);
    Task<List<TeamResponse>> GetEventTeamsAsync(int eventId);
    Task<bool> DeleteTeamAsync(int teamId);

    // Location management within events
    Task<LocationResponse> AddLocationToEventAsync(int eventId, CreateLocationRequest request);
    Task<LocationResponse?> UpdateLocationAsync(int locationId, CreateLocationRequest request);
    Task<bool> DeleteLocationAsync(int locationId);
    Task<List<LocationResponse>> GetEventLocationsAsync(int eventId);

    // Statistics and reporting
    Task<EventStatsResponse> GetEventStatsAsync();
    Task<List<TeamResponse>> GetEventTeamProgressAsync(int eventId);
}
