namespace TreasureHuntApp.Core.Entities;
public class VisitEntity
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public TeamEntity Team { get; set; } = null!;

    public int LocationId { get; set; }
    public LocationEntity Location { get; set; } = null!;

    public DateTime VisitedAt { get; set; }

    public bool IsCompleted { get; set; }
}
