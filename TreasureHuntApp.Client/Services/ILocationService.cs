using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;
public interface ILocationService
{
    Task<ApiResponse<LocationResponse>> CreateLocationAsync(int eventId, Location location);
    Task<ApiResponse<bool>> DeleteLocationAsync(int locationId);
    Task<ApiResponse<LocationResponse>> GetLocationAsync(int locationId);
    Task<ApiResponse<List<LocationResponse>>> GetLocationsAsync(int eventId);
    Task<ApiResponse<RouteInfo>> GetRouteAsync(int eventId, double startLat, double startLon);
    Task<ApiResponse<LocationResponse>> UpdateLocationAsync(int locationId, Location location);
}