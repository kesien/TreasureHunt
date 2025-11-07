using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Client.Services.Api;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;

public class EventService : IEventService
{
    private readonly IApiService _apiService;
    private const string BaseEndpoint = "api/events";

    public EventService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<EventResponse>>> GetEventsAsync()
    {
        return await _apiService.GetAsync<List<EventResponse>>(BaseEndpoint);
    }

    public async Task<ApiResponse<EventResponse>> GetEventAsync(int eventId)
    {
        return await _apiService.GetAsync<EventResponse>($"{BaseEndpoint}/{eventId}");
    }

    public async Task<ApiResponse<EventResponse>> CreateEventAsync(CreateEventRequest eventModel)
    {
        return await _apiService.PostAsync<EventResponse>(BaseEndpoint, eventModel);
    }

    public async Task<ApiResponse<EventResponse>> UpdateEventAsync(int eventId, UpdateEventRequest eventModel)
    {
        return await _apiService.PutAsync<EventResponse>($"{BaseEndpoint}/{eventId}", eventModel);
    }

    public async Task<ApiResponse<bool>> DeleteEventAsync(int eventId)
    {
        return await _apiService.DeleteAsync($"{BaseEndpoint}/{eventId}");
    }

    public async Task<ApiResponse<EventStatsResponse>> GetDashboardStatsAsync()
    {
        return await _apiService.GetAsync<EventStatsResponse>($"{BaseEndpoint}/stats");
    }

    public async Task<ApiResponse<EventStatsResponse>> GetEventProgressAsync(int eventId)
    {
        return await _apiService.GetAsync<EventStatsResponse>($"{BaseEndpoint}/{eventId}/progress");
    }
}
