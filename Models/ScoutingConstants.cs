namespace ScoutingAppMvc.Models;

public static class ScoutingConstants
{
    public static readonly Dictionary<string, string> Positions = new()
    {
        ["CB"]         = "CB",
        ["FB"]         = "FB (Walker)",
        ["FB_CANCELO"] = "FB (Cancelo)",
        ["FB_DELPH"]   = "FB (Delph)",
        ["6ER"]        = "6er",
        ["8ER"]        = "8er",
        ["WIDE"]       = "Wide player",
        ["CF"]         = "CF",
    };

    public static readonly List<decimal> RatingOptions = new() { 1, 1.5m, 2, 2.5m, 3, 3.5m, 4, 4.5m, 5 };

    public static readonly Dictionary<decimal, string> RatingDescriptions = new()
    {
        [1]    = "Very good performer & very suitable for our system.",
        [1.5m] = "Good performer & suitable for our system.",
        [2]    = "Promising performer & very suitable for our system.",
        [2.5m] = "Very suitable & could become promising if he changes position.",
        [3]    = "Very suitable player.",
        [3.5m] = "Has one desired quality, might become very suitable if he changes position.",
        [4]    = "Not very suitable but has one or more desired qualities.",
        [4.5m] = "Might have a desired quality if he changes position.",
        [5]    = "Not suitable, no desired qualities & no prospect of changing position.",
    };

    public static readonly List<string> FootOptions   = new() { "Right", "Left", "Both" };
    public static readonly List<string> GenderOptions = new() { "Male", "Female" };
    public static readonly List<string> CardOptions   = new() { "None", "Yellow", "Red" };

    public static decimal ComputeAverageRating(IEnumerable<Report> reports)
    {
        var list = reports.ToList();
        if (!list.Any()) return 0;
        return Math.Round(list.Average(r => r.Rating), 1);
    }

    public static string StarsDisplay(decimal avg)
    {
        int filled = Math.Clamp((int)Math.Round(avg), 0, 5);
        return new string('★', filled) + new string('☆', 5 - filled);
    }
}
