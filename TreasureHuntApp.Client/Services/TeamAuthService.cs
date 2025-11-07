using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;

public class TeamAuthService : ITeamAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string SessionTokenKey = "team_session_token";
    private const string TeamIdKey = "team_id";
    private const string EventIdKey = "event_id";
    private const string TeamNameKey = "team_name";

    public TeamAuthService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
    {
        _httpClient = httpClientFactory.CreateClient("AuthClient");
        _jsRuntime = jsRuntime;
    }

    public async Task<ApiResponse<TeamJoinResponse>> JoinTeamAsync(TeamJoinRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/team/join", request);

            if (response.IsSuccessStatusCode)
            {
                var joinResponse = await response.Content.ReadFromJsonAsync<TeamJoinResponse>();
                if (joinResponse != null)
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", SessionTokenKey, joinResponse.SessionToken);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TeamIdKey, joinResponse.TeamId.ToString());
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", EventIdKey, joinResponse.EventId.ToString());
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TeamNameKey, joinResponse.TeamName);

                    _httpClient.DefaultRequestHeaders.Remove("X-Team-Session");
                    _httpClient.DefaultRequestHeaders.Add("X-Team-Session", joinResponse.SessionToken);

                    return new ApiResponse<TeamJoinResponse>
                    {
                        Success = true,
                        Data = joinResponse
                    };
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse<TeamJoinResponse>
            {
                Success = false,
                ErrorMessage = $"Csatlakozási hiba: {response.StatusCode}",
                Errors = new List<string> { errorContent }
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiResponse<TeamJoinResponse>
            {
                Success = false,
                ErrorMessage = "Hálózati hiba történt",
                Errors = new List<string> { ex.Message }
            };
        }
        catch (JsonException ex)
        {
            return new ApiResponse<TeamJoinResponse>
            {
                Success = false,
                ErrorMessage = "Adatfeldolgozási hiba",
                Errors = new List<string> { ex.Message }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<TeamJoinResponse>
            {
                Success = false,
                ErrorMessage = "Váratlan hiba történt",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task LeaveTeamAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", SessionTokenKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TeamIdKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", EventIdKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TeamNameKey);

            _httpClient.DefaultRequestHeaders.Remove("X-Team-Session");
        }
        catch (JSException)
        {
            // localStorage hiba esetén folytatjuk
        }
    }

    public async Task<bool> IsJoinedAsync()
    {
        try
        {
            var sessionToken = await GetSessionTokenAsync();
            var teamId = await GetTeamIdAsync();
            var eventId = await GetEventIdAsync();

            if (string.IsNullOrEmpty(sessionToken) || !teamId.HasValue || !eventId.HasValue)
                return false;

            // Custom header beállítása, ha még nincs
            if (!_httpClient.DefaultRequestHeaders.Contains("X-Team-Session"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-Team-Session", sessionToken);
            }

            return true;
        }
        catch (JSException)
        {
            return false;
        }
    }

    public async Task<string?> GetSessionTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", SessionTokenKey);
        }
        catch (JSException)
        {
            return null;
        }
    }

    public async Task<int?> GetTeamIdAsync()
    {
        try
        {
            var teamIdString = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TeamIdKey);
            if (int.TryParse(teamIdString, out var teamId))
                return teamId;
            return null;
        }
        catch (JSException)
        {
            return null;
        }
    }

    public async Task<int?> GetEventIdAsync()
    {
        try
        {
            var eventIdString = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", EventIdKey);
            if (int.TryParse(eventIdString, out var eventId))
                return eventId;
            return null;
        }
        catch (JSException)
        {
            return null;
        }
    }

    public async Task<string?> GetTeamNameAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TeamNameKey);
        }
        catch (JSException)
        {
            return null;
        }
    }
}
