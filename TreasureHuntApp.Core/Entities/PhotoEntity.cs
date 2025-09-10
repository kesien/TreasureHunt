using System.ComponentModel.DataAnnotations;

namespace TreasureHuntApp.Core.Entities;
public class PhotoEntity
{
    public int Id { get; set; }

    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; }

    public int TeamId { get; set; }
    public TeamEntity Team { get; set; } = null!;

    public int LocationId { get; set; }
    public LocationEntity Location { get; set; } = null!;
}
