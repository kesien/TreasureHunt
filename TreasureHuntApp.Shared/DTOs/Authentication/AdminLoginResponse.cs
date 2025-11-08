namespace TreasureHuntApp.Shared.DTOs.Authentication;

public class AdminLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}