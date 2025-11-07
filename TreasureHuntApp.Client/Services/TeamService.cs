using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Client.Services.Api;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;

public class TeamService : ITeamService
{
    private readonly IApiService _apiService;
    private const string BaseEndpoint = "api/teams";

    public TeamService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<ApiResponse<List<TeamResponse>>> GetTeamsAsync(int eventId)
    {
        return await _apiService.GetAsync<List<TeamResponse>>($"api/events/{eventId}/teams");
    }

    public async Task<ApiResponse<TeamResponse>> CreateTeamAsync(int eventId, Team team)
    {
        return await _apiService.PostAsync<TeamResponse>($"api/events/{eventId}/teams", team);
    }

    public async Task<ApiResponse<bool>> DeleteTeamAsync(int teamId)
    {
        return await _apiService.DeleteAsync($"{BaseEndpoint}/{teamId}");
    }

    public async Task<ApiResponse<string>> GetQrCodeAsync(string teamCode)
    {
        return await _apiService.GetAsync<string>($"api/auth/team/qr/{teamCode}");
    }

    public async Task<ApiResponse<List<TeamResponse>>> GetTeamProgressAsync(int eventId)
    {
        return await _apiService.GetAsync<List<TeamResponse>>($"api/events/{eventId}/teams/progress");
    }

    public async Task<ApiResponse<TeamResponse>> GetTeamAsync(int teamId)
    {
        return await _apiService.GetAsync<TeamResponse>($"{BaseEndpoint}/{teamId}");
    }

    public async Task<ApiResponse<TeamResponse>> UpdateTeamAsync(int teamId, Team team)
    {
        return await _apiService.PutAsync<TeamResponse>($"{BaseEndpoint}/{teamId}", team);
    }

    public async Task<ApiResponse<bool>> CheckInLocationAsync(int teamId, int locationId, CheckInRequest request)
    {
        return await _apiService.PostAsync<bool>($"{BaseEndpoint}/{teamId}/checkin/{locationId}", request);
    }

    public async Task<ApiResponse<List<TeamResponse>>> GetTeamVisitsAsync(int teamId)
    {
        return await _apiService.GetAsync<List<TeamResponse>>($"{BaseEndpoint}/{teamId}/visits");
    }
}

// Check-in request model
public class CheckInRequest
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PhotoBase64 { get; set; }
    public string? Notes { get; set; }
}
