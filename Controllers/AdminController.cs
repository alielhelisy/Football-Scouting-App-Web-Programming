using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Helpers;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers;

[Authorize(Roles = "ADMIN")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private static bool IsMainAdmin(User user) =>
        user.Id == 1 || string.Equals(user.Username?.Trim(), "admin", StringComparison.OrdinalIgnoreCase);

    public IActionResult Accounts()
    {
        var users = _db.Users
            .Where(u => u.Id != CurrentUserId)
            .Select(u => new
            {
                u.Id, u.Username, u.Name, u.Surname, u.Email, u.Role,
                IsMainAdmin = u.Id == 1 || u.Username.ToLower() == "admin",
                PlayerCount = _db.Players.Count(p => p.UserId == u.Id)
            })
            .OrderBy(u => u.Username)
            .ToList();
        return View(users as object);
    }

    public IActionResult AddAccount() => View();

    [HttpPost]
    public IActionResult AddAccount(string username, string name, string surname,
                                     string email, string password, string confirmPassword, string role)
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
        if (role != "SCOUT" && role != "ADMIN") role = "SCOUT";

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
            Role         = role
        });
        _db.SaveChanges();
        TempData["Success"] = $"Account '{username}' created.";
        return RedirectToAction("Accounts");
    }

    [HttpPost]
    public IActionResult UpdateRole(int userId, string role)
    {
        if (role != "SCOUT" && role != "ADMIN")
        {
            TempData["Error"] = "Invalid role.";
            return RedirectToAction("Accounts");
        }
        var user = _db.Users.Find(userId);
        if (user == null)
        {
            TempData["Error"] = "Account not found.";
            return RedirectToAction("Accounts");
        }
        if (IsMainAdmin(user))
        {
            TempData["Error"] = "The main admin account cannot be changed.";
            return RedirectToAction("Accounts");
        }
        user.Role = role;
        _db.SaveChanges();
        TempData["Success"] = $"'{user.Username}' role changed to {role}.";
        return RedirectToAction("Accounts");
    }

    [HttpPost]
    public IActionResult DeleteAccount(int userId)
    {
        if (userId == CurrentUserId)
        {
            TempData["Error"] = "You cannot delete your own account here.";
            return RedirectToAction("Accounts");
        }
        var user = _db.Users
            .Include(u => u.Players).ThenInclude(p => p.Reports)
            .FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            TempData["Error"] = "Account not found.";
            return RedirectToAction("Accounts");
        }
        if (IsMainAdmin(user))
        {
            TempData["Error"] = "The main admin account cannot be deleted.";
            return RedirectToAction("Accounts");
        }
        _db.Users.Remove(user);
        _db.SaveChanges();
        TempData["Success"] = $"Account '{user.Username}' deleted.";
        return RedirectToAction("Accounts");
    }
}
