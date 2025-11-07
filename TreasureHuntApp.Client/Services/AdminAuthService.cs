using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using TreasureHuntApp.Client.Models;
using TreasureHuntApp.Shared.DTOs;

namespace TreasureHuntApp.Client.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private const string TokenKey = "admin_token";
    private const string ExpiryKey = "admin_token_expiry";

    public AdminAuthService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime)
    {
        _httpClient = httpClientFactory.CreateClient("AuthClient");
        _jsRuntime = jsRuntime;
    }

    public async Task<ApiResponse<AdminLoginResponse>> LoginAsync(AdminLoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/admin/login", request);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<AdminLoginResponse>();
                if (loginResponse != null)
                {
                    // Token tárolása localStorage-ban
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, loginResponse.Token);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ExpiryKey, loginResponse.ExpiresAt.ToString("O"));

                    // Authorization header beállítása
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

                    return new ApiResponse<AdminLoginResponse>
                    {
                        Success = true,
                        Data = loginResponse
                    };
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            return new ApiResponse<AdminLoginResponse>
            {
                Success = false,
                ErrorMessage = $"Bejelentkezési hiba: {response.StatusCode}",
                Errors = new List<string> { errorContent }
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiResponse<AdminLoginResponse>
            {
                Success = false,
                ErrorMessage = "Hálózati hiba történt",
                Errors = new List<string> { ex.Message }
            };
        }
        catch (JsonException ex)
        {
            return new ApiResponse<AdminLoginResponse>
            {
                Success = false,
                ErrorMessage = "Adatfeldolgozási hiba",
                Errors = new List<string> { ex.Message }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AdminLoginResponse>
            {
                Success = false,
                ErrorMessage = "Váratlan hiba történt",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            // Token eltávolítása localStorage-ból
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", ExpiryKey);

            // Authorization header törlése
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        catch (JSException)
        {
            // localStorage hiba esetén folytatjuk
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            var expiryString = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ExpiryKey);
            if (string.IsNullOrEmpty(expiryString))
                return false;

            if (DateTime.TryParse(expiryString, out var expiryDate))
            {
                if (DateTime.UtcNow >= expiryDate)
                {
                    await LogoutAsync();
                    return false;
                }

                // Authorization header beállítása, ha még nincs
                if (_httpClient.DefaultRequestHeaders.Authorization == null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                return true;
            }

            return false;
        }
        catch (JSException)
        {
            return false;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch (JSException)
        {
            return null;
        }
    }
}
