using System.ComponentModel.DataAnnotations;

namespace ScoutingAppMvc.Models;

public class Player
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [Required]
    [MaxLength(10)]
    public string Gender { get; set; } = "Male";

    [MaxLength(100)]
    public string Team { get; set; } = "";

    [MaxLength(20)]
    public string Position { get; set; } = "";

    [MaxLength(20)]
    public string OtherPosition { get; set; } = "";

    [MaxLength(50)]
    public string FirstName { get; set; } = "";

    [MaxLength(50)]
    public string LastName { get; set; } = "";

    [MaxLength(60)]
    public string Nationality { get; set; } = "";

    public DateTime? Birthday { get; set; }

    [Range(100, 250)]
    public int? Height { get; set; }

    [Range(30, 200)]
    public int? Weight { get; set; }

    [MaxLength(10)]
    public string Foot { get; set; } = "";

    public User User { get; set; } = null!;
    public ICollection<Report> Reports { get; set; } = new List<Report>();
}
