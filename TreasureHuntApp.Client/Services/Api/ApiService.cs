using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TreasureHuntApp.Client.Models;

namespace TreasureHuntApp.Client.Services.Api;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IAdminAuthService _adminAuthService;
    private readonly ITeamAuthService _teamAuthService;

    public ApiService(IHttpClientFactory httpClientFactory, IAdminAuthService adminAuthService, ITeamAuthService teamAuthService)
    {
        _httpClient = httpClientFactory.CreateClient("ApiService");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        _adminAuthService = adminAuthService;
        _teamAuthService = teamAuthService;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            await EnsureAuthenticationHeadersAsync();
            var response = await _httpClient.GetAsync(endpoint);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<T>($"GET kérés hiba: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await EnsureAuthenticationHeadersAsync();
            var response = await _httpClient.PostAsync(endpoint, content);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<T>($"POST kérés hiba: {ex.Message}");
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await EnsureAuthenticationHeadersAsync();
            var response = await _httpClient.PutAsync(endpoint, content);
            return await ProcessResponseAsync<T>(response);
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<T>($"PUT kérés hiba: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
    {
        try
        {
            await EnsureAuthenticationHeadersAsync();
            var response = await _httpClient.DeleteAsync(endpoint);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = true
                };
            }

            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                ErrorMessage = $"Törlés sikertelen: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                ErrorMessage = $"DELETE kérés hiba: {ex.Message}"
            };
        }
    }

    private async Task EnsureAuthenticationHeadersAsync()
    {
        // Admin token ellenőrzése és beállítása
        if (await _adminAuthService.IsAuthenticatedAsync())
        {
            var token = await _adminAuthService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                SetAuthenticationHeader(token);
            }
        }
        // Team session ellenőrzése és beállítása
        else if (await _teamAuthService.IsJoinedAsync())
        {
            var sessionToken = await _teamAuthService.GetSessionTokenAsync();
            if (!string.IsNullOrEmpty(sessionToken))
            {
                SetTeamSessionHeader(sessionToken);
            }
        }
    }

    public void SetAuthenticationHeader(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void SetTeamSessionHeader(string sessionToken)
    {
        _httpClient.DefaultRequestHeaders.Remove("X-Team-Session");
        _httpClient.DefaultRequestHeaders.Add("X-Team-Session", sessionToken);
    }

    public void ClearHeaders()
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _httpClient.DefaultRequestHeaders.Remove("X-Team-Session");
    }

    private async Task<ApiResponse<T>> ProcessResponseAsync<T>(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(string))
                {
                    return new ApiResponse<T>
                    {
                        Success = true,
                        Data = (T)(object)content
                    };
                }

                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = data
                };
            }

            // API error response kezelése
            try
            {
                var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, _jsonOptions);
                return new ApiResponse<T>
                {
                    Success = false,
                    ErrorMessage = errorResponse?.Message ?? $"API hiba: {response.StatusCode}",
                    Errors = errorResponse?.Errors ?? new List<string>()
                };
            }
            catch
            {
                return CreateErrorResponse<T>($"API hiba: {response.StatusCode} - {content}");
            }
        }
        catch (Exception ex)
        {
            return CreateErrorResponse<T>($"Válasz feldolgozási hiba: {ex.Message}");
        }
    }

    private static ApiResponse<T> CreateErrorResponse<T>(string errorMessage)
    {
        return new ApiResponse<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            Errors = new List<string> { errorMessage }
        };
    }
}

// API error response model
internal class ApiErrorResponse
{
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}
