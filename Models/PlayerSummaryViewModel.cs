namespace ScoutingAppMvc.Models;

public class PlayerSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Position { get; set; } = "";
    public string ScoutName { get; set; } = "";
    public decimal AvgRating { get; set; }
    public string LastReview { get; set; } = "";
}
