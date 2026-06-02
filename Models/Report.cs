using System.ComponentModel.DataAnnotations;

namespace ScoutingAppMvc.Models;

public class Report
{
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }

    [Required]
    [Range(1.0, 5.0)]
    public decimal Rating { get; set; }

    [Range(0, 120)]
    public int MinutesPlayed { get; set; }

    [Range(0, 20)]
    public int GoalsScored { get; set; }

    [Required]
    [MaxLength(20)]
    public string ReceivedCards { get; set; } = "None";

    [Required]
    [MaxLength(20)]
    public string RatedPosition { get; set; } = "";

    [MaxLength(2000)]
    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Player Player { get; set; } = null!;
}
