using TreasureHuntApp.Client.Models;

namespace TreasureHuntApp.Client.Services.Api;

public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse<bool>> DeleteAsync(string endpoint);
    void SetAuthenticationHeader(string token);
    void SetTeamSessionHeader(string sessionToken);
    void ClearHeaders();
}
