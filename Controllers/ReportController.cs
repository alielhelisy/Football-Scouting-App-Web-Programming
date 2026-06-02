using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly AppDbContext _db;
    public ReportController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("ADMIN");

    private IQueryable<Player> VisiblePlayers()
    {
        var query = _db.Players.Include(p => p.User).AsQueryable();
        if (!IsAdmin) query = query.Where(p => p.UserId == CurrentUserId);
        return query;
    }

    private void PopulateReportForm(int? presetPlayerId = null, bool isEdit = false)
    {
        ViewBag.Players            = VisiblePlayers().OrderBy(p => p.Name).ToList();
        ViewBag.Positions          = ScoutingConstants.Positions;
        ViewBag.CardOptions        = ScoutingConstants.CardOptions;
        ViewBag.RatingOptions      = ScoutingConstants.RatingOptions;
        ViewBag.RatingDescriptions = ScoutingConstants.RatingDescriptions;
        ViewBag.PresetPlayerId     = presetPlayerId;
        ViewBag.IsEdit             = isEdit;
    }

    public IActionResult Index()
    {
        var query = _db.Reports
            .Include(r => r.Player).ThenInclude(p => p.User)
            .AsQueryable();

        if (!IsAdmin)
        {
            query = query.Where(r => r.Player.UserId == CurrentUserId);
        }

        var reports = query
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        ViewBag.Positions = ScoutingConstants.Positions;
        return View(reports);
    }

    public IActionResult Create(int? playerId)
    {
        PopulateReportForm(playerId);
        return View(new Report());
    }

    [HttpPost]
    public IActionResult Create(int playerId, decimal rating, int minutesPlayed,
                                int goalsScored, string receivedCards, string ratedPosition, string? comments)
    {
        var player = _db.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null || (!IsAdmin && player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Player not found.";
            return RedirectToAction("Create");
        }
        if (!ScoutingConstants.RatingOptions.Contains(rating))
        {
            TempData["Error"] = "Invalid rating.";
            return RedirectToAction("Create");
        }

        _db.Reports.Add(new Report
        {
            PlayerId      = playerId,
            Rating        = rating,
            MinutesPlayed = minutesPlayed,
            GoalsScored   = goalsScored,
            ReceivedCards = receivedCards,
            RatedPosition = (ratedPosition ?? "").ToUpper(),
            Comments      = comments?.Trim(),
            CreatedAt     = DateTime.Now
        });
        _db.SaveChanges();
        TempData["Success"] = "Report saved.";
        return RedirectToAction("Details", "Player", new { id = playerId });
    }

    public IActionResult Edit(int id)
    {
        var report = _db.Reports
            .Include(r => r.Player).ThenInclude(p => p.User)
            .FirstOrDefault(r => r.Id == id);

        if (report == null || (!IsAdmin && report.Player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Report not found.";
            return RedirectToAction("Dashboard", "Player");
        }

        PopulateReportForm(report.PlayerId, true);
        return View("Create", report);
    }

    [HttpPost]
    public IActionResult Edit(int id, decimal rating, int minutesPlayed,
                              int goalsScored, string receivedCards, string ratedPosition, string? comments)
    {
        var report = _db.Reports
            .Include(r => r.Player)
            .FirstOrDefault(r => r.Id == id);

        if (report == null || (!IsAdmin && report.Player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Report not found.";
            return RedirectToAction("Dashboard", "Player");
        }

        if (!ScoutingConstants.RatingOptions.Contains(rating))
        {
            TempData["Error"] = "Invalid rating.";
            return RedirectToAction("Edit", new { id });
        }

        report.Rating        = rating;
        report.MinutesPlayed = minutesPlayed;
        report.GoalsScored   = goalsScored;
        report.ReceivedCards = receivedCards;
        report.RatedPosition = (ratedPosition ?? "").ToUpper();
        report.Comments      = comments?.Trim();

        _db.SaveChanges();
        TempData["Success"] = "Report updated.";
        return RedirectToAction("Details", "Player", new { id = report.PlayerId });
    }

    [HttpPost]
    public IActionResult DeleteReport(int playerId, int reportId)
    {
        var report = _db.Reports.Include(r => r.Player).FirstOrDefault(r => r.Id == reportId && r.PlayerId == playerId);
        if (report == null || (!IsAdmin && report.Player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Report not found.";
            return RedirectToAction("Details", "Player", new { id = playerId });
        }
        _db.Reports.Remove(report);
        _db.SaveChanges();
        TempData["Success"] = "Report deleted.";
        return RedirectToAction("Details", "Player", new { id = playerId });
    }

    public IActionResult EditComment(int playerId)
    {
        var player = _db.Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null || (!IsAdmin && player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Player not found.";
            return RedirectToAction("Dashboard", "Player");
        }
        var latest = _db.Reports
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault();
        if (latest == null)
        {
            TempData["Error"] = "No report to edit yet.";
            return RedirectToAction("Details", "Player", new { id = playerId });
        }
        ViewBag.Player     = player;
        ViewBag.PosDisplay = ScoutingConstants.Positions.GetValueOrDefault(player.Position, player.Position);
        return View(latest);
    }

    [HttpPost]
    public IActionResult EditComment(int playerId, int reportId, string comment)
    {
        var report = _db.Reports.Include(r => r.Player).FirstOrDefault(r => r.Id == reportId);
        if (report == null || (!IsAdmin && report.Player.UserId != CurrentUserId))
        {
            TempData["Error"] = "Report not found.";
            return RedirectToAction("Details", "Player", new { id = playerId });
        }
        report.Comments = comment?.Trim();
        _db.SaveChanges();
        TempData["Success"] = "Comment updated.";
        return RedirectToAction("Details", "Player", new { id = playerId });
    }
}
