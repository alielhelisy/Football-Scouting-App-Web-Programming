using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutingAppMvc.Data;
using ScoutingAppMvc.Models;
using System.Security.Claims;

namespace ScoutingAppMvc.Controllers.Api;

[ApiController]
[Route("api/players")]
[Authorize]
public class PlayersApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public PlayersApiController(AppDbContext db) => _db = db;

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin => User.IsInRole("ADMIN");

    // GET /api/players
    [HttpGet]
    public IActionResult GetAll()
    {
        var query = _db.Players.AsQueryable();
        if (!IsAdmin) query = query.Where(p => p.UserId == CurrentUserId);

        var players = query.Select(p => new
        {
            p.Id, p.Name, p.FirstName, p.LastName,
            p.Team, p.Position, p.Nationality,
            p.Gender, p.Foot, p.Height, p.Weight, p.Birthday
        }).ToList();

        return Ok(players);
    }

    // GET /api/players/{id}
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var p = _db.Players.Include(p => p.Reports).FirstOrDefault(p => p.Id == id);
        if (p == null) return NotFound(new { message = "Player not found." });
        if (!IsAdmin && p.UserId != CurrentUserId) return Forbid();

        return Ok(new
        {
            p.Id, p.Name, p.FirstName, p.LastName,
            p.Team, p.Position, p.Nationality,
            p.Gender, p.Foot, p.Height, p.Weight, p.Birthday,
            Reports = p.Reports.Select(r => new
            {
                r.Id, r.Rating, r.MinutesPlayed, r.GoalsScored,
                r.ReceivedCards, r.RatedPosition, r.Comments, r.CreatedAt
            })
        });
    }

    // POST /api/players
    [HttpPost]
    public IActionResult Create([FromBody] Player player)
    {
        if (string.IsNullOrWhiteSpace(player.Name))
            return BadRequest(new { message = "Player name is required." });

        player.UserId = CurrentUserId;
        _db.Players.Add(player);
        _db.SaveChanges();

        return StatusCode(201, new { player.Id, player.Name, message = "Player created." });
    }

    // PUT /api/players/{id}
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Player updated)
    {
        var player = _db.Players.Find(id);
        if (player == null) return NotFound(new { message = "Player not found." });
        if (!IsAdmin && player.UserId != CurrentUserId) return Forbid();

        player.Name        = updated.Name;
        player.FirstName   = updated.FirstName;
        player.LastName    = updated.LastName;
        player.Team        = updated.Team;
        player.Position    = updated.Position;
        player.OtherPosition = updated.OtherPosition;
        player.Nationality = updated.Nationality;
        player.Gender      = updated.Gender;
        player.Foot        = updated.Foot;
        player.Height      = updated.Height;
        player.Weight      = updated.Weight;
        player.Birthday    = updated.Birthday;
        _db.SaveChanges();

        return Ok(new { message = "Player updated." });
    }

    // DELETE /api/players/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var player = _db.Players.Include(p => p.Reports).FirstOrDefault(p => p.Id == id);
        if (player == null) return NotFound(new { message = "Player not found." });
        if (!IsAdmin && player.UserId != CurrentUserId) return Forbid();

        _db.Players.Remove(player);
        _db.SaveChanges();

        return Ok(new { message = "Player deleted." });
    }
}
