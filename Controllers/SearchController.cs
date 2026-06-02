using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly AppDbContext _db;
    public SearchController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("ADMIN");

    [HttpGet]
    public IActionResult Index(string? name, string? club, string? positions)
    {
        ViewBag.Positions  = ScoutingConstants.Positions;
        ViewBag.NameQuery  = name ?? "";
        ViewBag.ClubQuery  = club ?? "";
        ViewBag.Searched   = false;

        var selectedPos = (positions ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim().ToUpper())
            .Where(p => ScoutingConstants.Positions.ContainsKey(p))
            .ToList();
        ViewBag.Selected = selectedPos;

        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(club) && !selectedPos.Any())
            return View(new List<PlayerSummaryViewModel>());

        ViewBag.Searched = true;
        var query = _db.Players.Include(p => p.User).Include(p => p.Reports).AsQueryable();
        if (!IsAdmin) query = query.Where(p => p.UserId == CurrentUserId);
        if (selectedPos.Any()) query = query.Where(p => selectedPos.Contains(p.Position));
        if (!string.IsNullOrWhiteSpace(name)) query = query.Where(p => p.Name.Contains(name));
        if (!string.IsNullOrWhiteSpace(club)) query = query.Where(p => p.Team.Contains(club));

        var results = query.OrderBy(p => p.Name).ToList().Select(p => new PlayerSummaryViewModel
        {
            Id         = p.Id,
            Name       = p.Name,
            Position   = ScoutingConstants.Positions.GetValueOrDefault(p.Position, p.Position),
            ScoutName  = p.User?.Username ?? "",
            AvgRating  = ScoutingConstants.ComputeAverageRating(p.Reports),
            LastReview = p.Reports.OrderByDescending(r => r.CreatedAt).FirstOrDefault()?.Comments ?? ""
        }).ToList();

        return View(results);
    }
}
