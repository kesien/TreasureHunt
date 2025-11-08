using System.ComponentModel.DataAnnotations;

namespace TreasureHuntApp.Shared.DTOs.Teams;

public class CreateTeamRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int EventId { get; set; }
}