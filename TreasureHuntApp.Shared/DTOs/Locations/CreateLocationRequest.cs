using System.ComponentModel.DataAnnotations;

namespace TreasureHuntApp.Shared.DTOs.Locations;

public class CreateLocationRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(0, 999)]
    public int Order { get; set; }

    public bool IsRequired { get; set; } = true;
}