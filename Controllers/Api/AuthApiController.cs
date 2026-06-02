using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Helpers;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthApiController(AppDbContext db) => _db = db;

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Username and password are required." });

        var user = _db.Users.FirstOrDefault(u => u.Username == req.Username || u.Email == req.Username);
        if (user == null || user.PasswordHash != HashHelper.Hash(req.Password))
            return Unauthorized(new { message = "Invalid username or password." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.Username),
            new(ClaimTypes.Role,           user.Role),
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new { message = "Login successful.", user.Id, user.Username, user.Role });
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Username and password are required." });

        if (_db.Users.Any(u => u.Username == req.Username))
            return BadRequest(new { message = "Username already taken." });

        if (!string.IsNullOrWhiteSpace(req.Email) && _db.Users.Any(u => u.Email == req.Email))
            return BadRequest(new { message = "Email already used." });

        _db.Users.Add(new User
        {
            Username     = req.Username,
            Name         = req.Name,
            Surname      = req.Surname,
            Email        = req.Email,
            PasswordHash = HashHelper.Hash(req.Password),
            Role         = "SCOUT"
        });
        _db.SaveChanges();

        return StatusCode(201, new { message = "Account created successfully." });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out." });
    }

    // GET /api/auth/me
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user   = _db.Users.Find(userId);
        if (user == null) return NotFound(new { message = "User not found." });

        return Ok(new { user.Id, user.Username, user.Name, user.Surname, user.Email, user.Role });
    }
}

public record LoginRequest(string Username, string Password);
public record RegisterRequest(string Username, string Password, string? Name, string? Surname, string? Email);
