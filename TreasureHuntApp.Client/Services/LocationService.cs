using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Client.Services.Api;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;

public class LocationService : ILocationService
{
    private readonly IApiService _apiService;
    private const string BaseEndpoint = "api/locations";

    public LocationService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<LocationResponse>>> GetLocationsAsync(int eventId)
    {
        return await _apiService.GetAsync<List<LocationResponse>>($"api/events/{eventId}/locations");
    }

    public async Task<ApiResponse<LocationResponse>> GetLocationAsync(int locationId)
    {
        return await _apiService.GetAsync<LocationResponse>($"{BaseEndpoint}/{locationId}");
    }

    public async Task<ApiResponse<LocationResponse>> CreateLocationAsync(int eventId, Location location)
    {
        return await _apiService.PostAsync<LocationResponse>($"api/events/{eventId}/locations", location);
    }

    public async Task<ApiResponse<LocationResponse>> UpdateLocationAsync(int locationId, Location location)
    {
        return await _apiService.PutAsync<LocationResponse>($"{BaseEndpoint}/{locationId}", location);
    }

    public async Task<ApiResponse<bool>> DeleteLocationAsync(int locationId)
    {
        return await _apiService.DeleteAsync($"{BaseEndpoint}/{locationId}");
    }

    public async Task<ApiResponse<RouteInfo>> GetRouteAsync(int eventId, double startLat, double startLon)
    {
        var query = $"?startLat={startLat}&startLon={startLon}";
        return await _apiService.GetAsync<RouteInfo>($"api/events/{eventId}/locations/route{query}");
    }
}

// Helper models
public class LocationOrder
{
    public int LocationId { get; set; }
    public int Order { get; set; }
}

public class RouteInfo
{
    public List<RoutePoint> Route { get; set; } = new();
    public double TotalDistance { get; set; }
    public int EstimatedDuration { get; set; }
}

public class RoutePoint
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int? LocationId { get; set; }
}
