namespace TreasureHuntApp.Shared.DTOs.Authentication;

public class QrCodeResponse
{
    public string QrCodeBase64 { get; set; } = string.Empty;
    public string JoinUrl { get; set; } = string.Empty;
    public string TeamCode { get; set; } = string.Empty;
}