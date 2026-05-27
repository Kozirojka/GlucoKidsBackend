using GlucoKids.Application.Interfaces;
using GlucoKids.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoKids.Controllers;

[ApiController]
[Route("api/achievements")]
public class AchievementsController(
    AppDbContext db,
    IAchievementService achievementService) : ControllerBase
{
    [HttpGet("{uid}")]
    public async Task<IActionResult> GetAchievements(string uid, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Child)
            .FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        if (user?.Child is null) return NotFound("Child not found.");

        var response = await achievementService.GetAllWithStatusAsync(user.Child.Id);
        return Ok(response);
    }
}
