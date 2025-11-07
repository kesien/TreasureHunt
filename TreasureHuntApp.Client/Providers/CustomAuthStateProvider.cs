using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TreasureHuntApp.Client.Services;

namespace TreasureHuntApp.Client.Providers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IAdminAuthService _adminAuthService;
    private readonly ITeamAuthService _teamAuthService;
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CustomAuthStateProvider(IAdminAuthService adminAuthService, ITeamAuthService teamAuthService)
    {
        _adminAuthService = adminAuthService;
        _teamAuthService = teamAuthService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = await GetClaimsIdentity();
        _currentUser = new ClaimsPrincipal(identity);
        return new AuthenticationState(_currentUser);
    }

    public async Task UpdateAuthenticationState()
    {
        var identity = await GetClaimsIdentity();
        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    public async Task MarkAdminAsAuthenticated()
    {
        var adminToken = await _adminAuthService.GetTokenAsync();
        if (!string.IsNullOrEmpty(adminToken))
        {
            var identity = new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.Name, "admin"),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("token", adminToken)
                }, "admin");

            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }

    public async Task MarkTeamAsJoined()
    {
        var sessionToken = await _teamAuthService.GetSessionTokenAsync();
        var teamId = await _teamAuthService.GetTeamIdAsync();
        var eventId = await _teamAuthService.GetEventIdAsync();
        var teamName = await _teamAuthService.GetTeamNameAsync();

        if (!string.IsNullOrEmpty(sessionToken) && teamId.HasValue && eventId.HasValue)
        {
            var identity = new ClaimsIdentity(new[]
            {
                    new Claim(ClaimTypes.Name, teamName ?? "Team"),
                    new Claim(ClaimTypes.Role, "Team"),
                    new Claim("sessionToken", sessionToken),
                    new Claim("teamId", teamId.Value.ToString()),
                    new Claim("eventId", eventId.Value.ToString())
                }, "team");

            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }

    public async Task MarkAsLoggedOut()
    {
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    private async Task<ClaimsIdentity> GetClaimsIdentity()
    {
        // Ellenőrizzük az admin autentikációt
        if (await _adminAuthService.IsAuthenticatedAsync())
        {
            var adminToken = await _adminAuthService.GetTokenAsync();
            if (!string.IsNullOrEmpty(adminToken))
            {
                return new ClaimsIdentity(new[]
                {
                        new Claim(ClaimTypes.Name, "admin"),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("token", adminToken)
                    }, "admin");
            }
        }

        // Ellenőrizzük a csapat csatlakozást
        if (await _teamAuthService.IsJoinedAsync())
        {
            var sessionToken = await _teamAuthService.GetSessionTokenAsync();
            var teamId = await _teamAuthService.GetTeamIdAsync();
            var eventId = await _teamAuthService.GetEventIdAsync();
            var teamName = await _teamAuthService.GetTeamNameAsync();

            if (!string.IsNullOrEmpty(sessionToken) && teamId.HasValue && eventId.HasValue)
            {
                return new ClaimsIdentity(new[]
                {
                        new Claim(ClaimTypes.Name, teamName ?? "Team"),
                        new Claim(ClaimTypes.Role, "Team"),
                        new Claim("sessionToken", sessionToken),
                        new Claim("teamId", teamId.Value.ToString()),
                        new Claim("eventId", eventId.Value.ToString())
                    }, "team");
            }
        }

        return new ClaimsIdentity();
    }

    public bool IsAdmin => _currentUser.IsInRole("Admin");
    public bool IsTeam => _currentUser.IsInRole("Team");
    public bool IsAuthenticated => _currentUser.Identity?.IsAuthenticated ?? false;

    public string? GetUserName() => _currentUser.FindFirst(ClaimTypes.Name)?.Value;
    public string? GetToken() => _currentUser.FindFirst("token")?.Value;
    public string? GetSessionToken() => _currentUser.FindFirst("sessionToken")?.Value;
    public int? GetTeamId()
    {
        var teamIdClaim = _currentUser.FindFirst("teamId")?.Value;
        return int.TryParse(teamIdClaim, out var teamId) ? teamId : null;
    }
    public int? GetEventId()
    {
        var eventIdClaim = _currentUser.FindFirst("eventId")?.Value;
        return int.TryParse(eventIdClaim, out var eventId) ? eventId : null;
    }
}