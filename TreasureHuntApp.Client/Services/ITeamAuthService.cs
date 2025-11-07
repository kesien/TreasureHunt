using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;

public interface ITeamAuthService
{
    Task<ApiResponse<TeamJoinResponse>> JoinTeamAsync(TeamJoinRequest request);
    Task LeaveTeamAsync();
    Task<bool> IsJoinedAsync();
    Task<string?> GetSessionTokenAsync();
    Task<int?> GetTeamIdAsync();
    Task<int?> GetEventIdAsync();
    Task<string?> GetTeamNameAsync();
}
