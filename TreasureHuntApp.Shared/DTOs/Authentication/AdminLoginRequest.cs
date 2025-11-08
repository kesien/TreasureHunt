namespace TreasureHuntApp.Shared.DTOs.Authentication;
public class AdminLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}