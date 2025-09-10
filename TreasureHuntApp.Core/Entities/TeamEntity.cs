using System.ComponentModel.DataAnnotations;

namespace TreasureHuntApp.Core.Entities;
public class TeamEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string AccessCode { get; set; } = string.Empty;

    public int EventId { get; set; }
    public EventEntity Event { get; set; } = null!;

    public DateTime? LastActiveAt { get; set; }
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }

    // Navigation properties
    public ICollection<VisitEntity> Visits { get; set; } = new List<VisitEntity>();
    public ICollection<PhotoEntity> Photos { get; set; } = new List<PhotoEntity>();
}