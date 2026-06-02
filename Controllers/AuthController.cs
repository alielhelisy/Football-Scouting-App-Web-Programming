using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Helpers;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static bool IsMainAdmin(User user) =>
        user.Id == 1 || string.Equals(user.Username?.Trim(), "admin", StringComparison.OrdinalIgnoreCase);

    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard", "Player");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
        if (user == null || user.PasswordHash != HashHelper.Hash(password))
        {
            TempData["Error"] = "Invalid username/email or password.";
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role),
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return RedirectToAction("Dashboard", "Player");
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Dashboard", "Player");
        return View();
    }

    [HttpPost]
    public IActionResult Register(string username, string name, string surname,
                                   string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(surname)  || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            TempData["Error"] = "All fields are required.";
            return View();
        }
        if (password != confirmPassword)
        {
            TempData["Error"] = "Passwords do not match.";
            return View();
        }
        if (_db.Users.Any(u => u.Username == username))
        {
            TempData["Error"] = "Username already taken.";
            return View();
        }
        if (_db.Users.Any(u => u.Email == email))
        {
            TempData["Error"] = "Email already used.";
            return View();
        }

        _db.Users.Add(new User
        {
            Username     = username,
            Name         = name,
            Surname      = surname,
            Email        = email,
            PasswordHash = HashHelper.Hash(password),
            Role         = "SCOUT"
        });
        _db.SaveChanges();
        TempData["Success"] = "Account created! Please log in.";
        return RedirectToAction("Login");
    }

    [Authorize]
    public IActionResult Account()
    {
        var user = _db.Users.Find(CurrentUserId);
        if (user == null) return RedirectToAction("Logout");

        ViewBag.TotalPlayers = _db.Players.Count(p => p.UserId == CurrentUserId);
        ViewBag.TotalReports = _db.Reports.Count(r => r.Player.UserId == CurrentUserId);
        var recentReports = _db.Reports
            .Where(r => r.Player.UserId == CurrentUserId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new
            {
                r.Rating,
                r.CreatedAt,
                r.Comments,
                PlayerName = r.Player.Name,
                Position = r.Player.Position,
                Birthday = r.Player.Birthday
            })
            .ToList();
        ViewBag.RecentReports = recentReports
            .Select(r => new
            {
                r.Rating,
                r.CreatedAt,
                r.Comments,
                r.PlayerName,
                Position = ScoutingConstants.Positions.GetValueOrDefault(r.Position, r.Position),
                r.Birthday
            })
            .ToList();

        var players = _db.Players
            .Include(p => p.Reports)
            .Where(p => p.UserId == CurrentUserId)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Name,
                p.Birthday,
                p.Foot,
                p.Position,
                p.OtherPosition,
                ReportCount = p.Reports.Count
            })
            .ToList();
        ViewBag.Players = players
            .Select(p => new
            {
                p.Name,
                p.Birthday,
                p.Foot,
                MainPosition = ScoutingConstants.Positions.GetValueOrDefault(p.Position, p.Position),
                OtherPosition = string.IsNullOrEmpty(p.OtherPosition)
                    ? ""
                    : ScoutingConstants.Positions.GetValueOrDefault(p.OtherPosition, p.OtherPosition),
                p.ReportCount
            })
            .ToList();

        return View(user);
    }

    [Authorize]
    public IActionResult ChangePassword() => View();

    [Authorize]
    [HttpPost]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword, bool returnToAccount = false)
    {
        IActionResult PasswordError(string message)
        {
            TempData["Error"] = message;
            return returnToAccount ? RedirectToAction("Account") : View();
        }

        if (string.IsNullOrWhiteSpace(currentPassword) ||
            string.IsNullOrWhiteSpace(newPassword) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            return PasswordError("All password fields are required.");
        }
        if (newPassword != confirmPassword)
        {
            return PasswordError("New passwords do not match.");
        }
        if (newPassword.Length < 6)
        {
            return PasswordError("New password must be at least 6 characters.");
        }

        var user = _db.Users.Find(CurrentUserId);
        if (user == null || user.PasswordHash != HashHelper.Hash(currentPassword))
        {
            return PasswordError("Current password is incorrect.");
        }

        user.PasswordHash = HashHelper.Hash(newPassword);
        _db.SaveChanges();
        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction("Account");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = _db.Users
            .Include(u => u.Players).ThenInclude(p => p.Reports)
            .FirstOrDefault(u => u.Id == CurrentUserId);
        if (user == null) return RedirectToAction("Login");
        if (IsMainAdmin(user))
        {
            TempData["Error"] = "The main admin account cannot be deleted.";
            return RedirectToAction("Account");
        }
        _db.Users.Remove(user);
        _db.SaveChanges();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Your account has been deleted.";
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
