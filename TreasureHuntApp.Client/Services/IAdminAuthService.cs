using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;

public interface IAdminAuthService
{
    Task<ApiResponse<AdminLoginResponse>> LoginAsync(AdminLoginRequest request);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
}
