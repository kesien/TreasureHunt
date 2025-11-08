using TreasureHuntApp.Core.Entities;
using TreasureHuntApp.Shared.DTOs.Authentication;

namespace TreasureHuntApp.Infrastructure.Services;
public interface IAuthenticationService
{
    Task<AdminLoginResponse?> LoginAdminAsync(string username, string password);
    Task<TeamJoinResponse?> JoinTeamAsync(string teamCode, string? teamName = null);
    Task<bool> ValidateAdminTokenAsync(string token);
    Task<bool> ValidateTeamSessionAsync(string sessionToken);
    Task<TeamEntity?> GetTeamFromSessionAsync(string sessionToken);
    string GenerateTeamCode();
    Task<QrCodeResponse> GenerateTeamQrCodeAsync(string teamCode, string baseUrl);
}