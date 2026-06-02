using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers;

[Authorize]
public class PlayerController : Controller
{
    private readonly AppDbContext _db;
    public PlayerController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("ADMIN");

    public IActionResult Dashboard()
    {
        var query = _db.Players.Include(p => p.User).AsQueryable();
        if (!IsAdmin) query = query.Where(p => p.UserId == CurrentUserId);

        var players = query.OrderBy(p => p.Position).ThenBy(p => p.Name).ToList();
        var byPos = ScoutingConstants.Positions.Keys
            .ToDictionary(k => k, k => players.Where(p => p.Position == k).ToList());

        ViewBag.Positions = ScoutingConstants.Positions;
        ViewBag.ByPos     = byPos;
        ViewBag.IsAdmin   = IsAdmin;
        return View();
    }

    public IActionResult ByPosition(string posKey)
    {
        posKey = (posKey ?? "").ToUpper();
        if (!ScoutingConstants.Positions.ContainsKey(posKey))
        {
            TempData["Error"] = "Invalid position.";
            return RedirectToAction("Dashboard");
        }

        var query = _db.Players
            .Include(p => p.User)
            .Include(p => p.Reports)
            .Where(p => p.Position == posKey);
        if (!IsAdmin) query = query.Where(p => p.UserId == CurrentUserId);

        var result = query.OrderBy(p => p.Name).ToList().Select(p => new PlayerSummaryViewModel
        {
            Id         = p.Id,
            Name       = p.Name,
            Position   = p.Position,
            ScoutName  = p.User?.Username ?? "",
            AvgRating  = ScoutingConstants.ComputeAverageRating(p.Reports),
            LastReview = p.Reports.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.Comments ?? ""
        }).ToList();

        ViewBag.PosKey     = posKey;
        ViewBag.PosDisplay = ScoutingConstants.Positions[posKey];
        ViewBag.IsAdmin    = IsAdmin;
        return View(result);
    }

    public IActionResult Details(int id)
    {
        var player = _db.Players
            .Include(p => p.User)
            .Include(p => p.Reports)
            .FirstOrDefault(p => p.Id == id);

        if (player == null || (!IsAdmin && player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Player not found.";
            return RedirectToAction("Dashboard");
        }

        var reports    = player.Reports.OrderByDescending(r => r.CreatedAt).ToList();
        var avg        = ScoutingConstants.ComputeAverageRating(reports);
        ViewBag.Avg   = avg;
        ViewBag.Stars = ScoutingConstants.StarsDisplay(avg);
        ViewBag.PosDisplay = ScoutingConstants.Positions.GetValueOrDefault(player.Position, player.Position);
        ViewBag.Positions  = ScoutingConstants.Positions;
        ViewBag.IsAdmin    = IsAdmin;
        ViewBag.Reports    = reports;
        return View(player);
    }

    public IActionResult Add(string? position)
    {
        ViewBag.Positions    = ScoutingConstants.Positions;
        ViewBag.FootOptions  = ScoutingConstants.FootOptions;
        ViewBag.GenderOptions= ScoutingConstants.GenderOptions;
        ViewBag.PresetPos    = (position ?? "").ToUpper();
        ViewBag.Teams        = _db.Players
            .Where(p => !string.IsNullOrEmpty(p.Team))
            .Select(p => p.Team!)
            .Distinct()
            .OrderBy(t => t)
            .ToList();
        return View(new Player());
    }

    [HttpPost]
    public IActionResult Add(Player player)
    {
        player.UserId        = CurrentUserId;
        player.Position      = (player.Position      ?? "").ToUpper();
        player.OtherPosition = (player.OtherPosition ?? "").ToUpper();
        player.Team          = player.Team        ?? "";
        player.FirstName     = player.FirstName   ?? "";
        player.LastName      = player.LastName    ?? "";
        player.Nationality   = player.Nationality ?? "";
        player.Foot          = player.Foot        ?? "";
        player.Gender        = player.Gender      ?? "Male";

        if (string.IsNullOrWhiteSpace(player.Name))
        {
            TempData["Error"] = "Display Name cannot be empty.";
            ViewBag.Positions    = ScoutingConstants.Positions;
            ViewBag.FootOptions  = ScoutingConstants.FootOptions;
            ViewBag.GenderOptions= ScoutingConstants.GenderOptions;
            ViewBag.PresetPos    = player.Position;
            return View(player);
        }

        _db.Players.Add(player);
        _db.SaveChanges();
        TempData["Success"] = $"Player '{player.Name}' created.";
        var redirectPos = ScoutingConstants.Positions.ContainsKey(player.Position)
            ? player.Position : ScoutingConstants.Positions.Keys.First();
        return RedirectToAction("ByPosition", new { posKey = redirectPos });
    }

    public IActionResult Edit(int id)
    {
        var player = _db.Players.FirstOrDefault(p => p.Id == id);
        if (player == null || (!IsAdmin && player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Player not found.";
            return RedirectToAction("Dashboard");
        }
        ViewBag.Positions    = ScoutingConstants.Positions;
        ViewBag.FootOptions  = ScoutingConstants.FootOptions;
        ViewBag.GenderOptions= ScoutingConstants.GenderOptions;
        return View(player);
    }

    [HttpPost]
    public IActionResult Edit(int id, Player updated)
    {
        var player = _db.Players.FirstOrDefault(p => p.Id == id);
        if (player == null || (!IsAdmin && player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Player not found.";
            return RedirectToAction("Dashboard");
        }
        if (string.IsNullOrWhiteSpace(updated.Name))
        {
            TempData["Error"] = "Display Name cannot be empty.";
            ViewBag.Positions    = ScoutingConstants.Positions;
            ViewBag.FootOptions  = ScoutingConstants.FootOptions;
            ViewBag.GenderOptions= ScoutingConstants.GenderOptions;
            return View(player);
        }

        player.Name          = updated.Name;
        player.Gender        = updated.Gender;
        player.Team          = updated.Team ?? "";
        player.Position      = (updated.Position ?? "").ToUpper();
        player.OtherPosition = (updated.OtherPosition ?? "").ToUpper();
        player.FirstName     = updated.FirstName ?? "";
        player.LastName      = updated.LastName ?? "";
        player.Nationality   = updated.Nationality ?? "";
        player.Birthday      = updated.Birthday;
        player.Height        = updated.Height;
        player.Weight        = updated.Weight;
        player.Foot          = updated.Foot ?? "";

        _db.SaveChanges();
        TempData["Success"] = "Player updated.";
        var redirectPos = ScoutingConstants.Positions.ContainsKey(player.Position)
            ? player.Position : ScoutingConstants.Positions.Keys.First();
        return RedirectToAction("ByPosition", new { posKey = redirectPos });
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var player = _db.Players.FirstOrDefault(p => p.Id == id);
        if (player == null || (!IsAdmin && player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Player not found.";
            return RedirectToAction("Dashboard");
        }
        var pos = player.Position;
        _db.Players.Remove(player);
        _db.SaveChanges();
        TempData["Success"] = $"Player '{player.Name}' deleted.";
        var redirectPos = ScoutingConstants.Positions.ContainsKey(pos) ? pos : ScoutingConstants.Positions.Keys.First();
        return RedirectToAction("ByPosition", new { posKey = redirectPos });
    }
}
