using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;
public interface ITeamService
{
    Task<ApiResponse<bool>> CheckInLocationAsync(int teamId, int locationId, CheckInRequest request);
    Task<ApiResponse<TeamResponse>> CreateTeamAsync(int eventId, Team team);
    Task<ApiResponse<bool>> DeleteTeamAsync(int teamId);
    Task<ApiResponse<string>> GetQrCodeAsync(string teamCode);
    Task<ApiResponse<TeamResponse>> GetTeamAsync(int teamId);
    Task<ApiResponse<List<TeamResponse>>> GetTeamProgressAsync(int eventId);
    Task<ApiResponse<List<TeamResponse>>> GetTeamsAsync(int eventId);
    Task<ApiResponse<List<TeamResponse>>> GetTeamVisitsAsync(int teamId);
    Task<ApiResponse<TeamResponse>> UpdateTeamAsync(int teamId, Team team);
}