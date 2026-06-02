using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers.Api;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public ReportsApiController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("ADMIN");

    // GET /api/reports
    [HttpGet]
    public IActionResult GetAll()
    {
        var query = _db.Reports.Include(r => r.Player).AsQueryable();
        if (!IsAdmin) query = query.Where(r => r.Player.UserId == CurrentUserId);

        var reports = query.Select(r => new
        {
            r.Id, r.PlayerId,
            PlayerName = r.Player.Name,
            r.Rating, r.MinutesPlayed, r.GoalsScored,
            r.ReceivedCards, r.RatedPosition, r.Comments, r.CreatedAt
        }).ToList();

        return Ok(reports);
    }

    // GET /api/reports/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var r = _db.Reports.Include(r => r.Player).FirstOrDefault(r => r.Id == id);
        if (r == null) return NotFound(new { message = "Report not found." });
        if (!IsAdmin && r.Player.UserId != CurrentUserId) return Forbid();

        return Ok(new
        {
            r.Id, r.PlayerId,
            PlayerName = r.Player.Name,
            r.Rating, r.MinutesPlayed, r.GoalsScored,
            r.ReceivedCards, r.RatedPosition, r.Comments, r.CreatedAt
        });
    }

    // POST /api/reports
    [HttpPost]
    public IActionResult Create([FromBody] Report report)
    {
        var player = _db.Players.Find(report.PlayerId);
        if (player == null) return NotFound(new { message = "Player not found." });
        if (!IsAdmin && player.UserId != CurrentUserId) return Forbid();

        report.CreatedAt = DateTime.Now;
        _db.Reports.Add(report);
        _db.SaveChanges();

        return StatusCode(201, new { report.Id, report.PlayerId, report.Rating, message = "Report created." });
    }

    // DELETE /api/reports/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var report = _db.Reports.Include(r => r.Player).FirstOrDefault(r => r.Id == id);
        if (report == null) return NotFound(new { message = "Report not found." });
        if (!IsAdmin && report.Player.UserId != CurrentUserId) return Forbid();

        _db.Reports.Remove(report);
        _db.SaveChanges();

        return Ok(new { message = "Report deleted." });
    }
}
