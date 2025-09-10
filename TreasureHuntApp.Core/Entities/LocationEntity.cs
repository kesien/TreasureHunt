using System.ComponentModel.DataAnnotations;

namespace TreasureHuntApp.Core.Entities;
public class LocationEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public int EventId { get; set; }
    public EventEntity Event { get; set; } = null!;
    public string Description { get; set; } = string.Empty; // Add if missing
    public int Order { get; set; } // Add if missing - for ordering locations
    public bool IsRequired { get; set; } = true; // Add if missing
    public ICollection<PhotoEntity> Photos { get; set; } = [];

    // Navigation properties
    public ICollection<VisitEntity> Visits { get; set; } = new List<VisitEntity>();
}
