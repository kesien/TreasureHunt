using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;
public interface IEventService
{
    Task<ApiResponse<EventResponse>> CreateEventAsync(CreateEventRequest eventModel);
    Task<ApiResponse<bool>> DeleteEventAsync(int eventId);
    Task<ApiResponse<EventStatsResponse>> GetDashboardStatsAsync();
    Task<ApiResponse<EventResponse>> GetEventAsync(int eventId);
    Task<ApiResponse<EventStatsResponse>> GetEventProgressAsync(int eventId);
    Task<ApiResponse<List<EventResponse>>> GetEventsAsync();
    Task<ApiResponse<EventResponse>> UpdateEventAsync(int eventId, UpdateEventRequest eventModel);
}