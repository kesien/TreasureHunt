namespace TreasureHuntApp.Core.Entities;
public class TeamSessionEntity
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public TeamEntity Team { get; set; } = null!;
    public string SessionToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}