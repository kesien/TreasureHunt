namespace TreasureHuntApp.Shared.DTOs;
public class AdminLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AdminLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class TeamJoinRequest
{
    public string TeamCode { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
}

public class TeamJoinResponse
{
    public string SessionToken { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
}

public class QrCodeResponse
{
    public string QrCodeBase64 { get; set; } = string.Empty;
    public string JoinUrl { get; set; } = string.Empty;
    public string TeamCode { get; set; } = string.Empty;
}
