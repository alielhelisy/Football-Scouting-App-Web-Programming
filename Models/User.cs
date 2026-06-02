using System.ComponentModel.DataAnnotations;

namespace ScoutingAppMvc.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = "";

    [MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(50)]
    public string? Surname { get; set; }

    [EmailAddress]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required]
    public string PasswordHash { get; set; } = "";

    [Required]
    [RegularExpression("^(SCOUT|ADMIN)$")]
    public string Role { get; set; } = "SCOUT";

    public ICollection<Player> Players { get; set; } = new List<Player>();
}
