using GlucoKids.Application.DTOs;
using GlucoKids.Domain.Entities;
using GlucoKids.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoKids.Controllers;

[ApiController]
[Route("api/children")]
public class ChildrenController(AppDbContext db, ILogger<ChildrenController> logger) : ControllerBase
{
    private async Task<(User? user, Child? child)> GetChildByUid(string uid, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Child)
            .FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        return (user, user?.Child);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterChildRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FirebaseUid))
            return BadRequest("FirebaseUid is required.");

        var (user, child) = await GetChildByUid(req.FirebaseUid, ct);
        if (child is null)
            return NotFound("Child not found. Authenticate first via /auth/verify.");

        user!.DisplayName  = req.DisplayName;
        user.AvatarUrl     = req.AvatarUrl;
        child.DateOfBirth  = req.DateOfBirth;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Child profile updated uid={Uid}", req.FirebaseUid);

        return Ok(new ChildResponse(child.Id, user.FirebaseUid, user.DisplayName,
            user.AvatarUrl, child.DateOfBirth, user.CreatedAt));
    }

    [HttpGet("{uid}/progress")]
    public async Task<IActionResult> GetProgress(string uid, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var modules = await db.PsychoModules
            .OrderBy(m => m.OrderIndex)
            .Include(m => m.Lessons.OrderBy(l => l.OrderIndex))
            .ToListAsync(ct);

        var progressMap = await db.LessonProgress
            .Where(p => p.ChildId == child.Id)
            .ToDictionaryAsync(p => p.LessonId, ct);

        var result = modules.Select(m => new ModuleProgressResponse(
            m.Id, m.Title, m.Description, m.OrderIndex,
            m.Lessons.Select(l =>
            {
                progressMap.TryGetValue(l.Id, out var p);
                return new LessonProgressResponse(l.Id, l.Title, l.OrderIndex,
                    p?.Status ?? LessonStatus.NotStarted, p?.StartedAt, p?.CompletedAt);
            })
        ));

        return Ok(result);
    }

    [HttpPut("{uid}/progress/{lessonId:int}")]
    public async Task<IActionResult> UpdateProgress(string uid, int lessonId,
        UpdateProgressRequest req, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var lesson = await db.PsychoLessons.FindAsync([lessonId], ct);
        if (lesson is null) return NotFound("Lesson not found.");

        var progress = await db.LessonProgress
            .FirstOrDefaultAsync(p => p.ChildId == child.Id && p.LessonId == lessonId, ct);

        if (progress is null)
        {
            progress = new LessonProgress { ChildId = child.Id, LessonId = lessonId };
            db.LessonProgress.Add(progress);
        }

        progress.Status = req.Status;
        if (req.Status == LessonStatus.InProgress && progress.StartedAt is null)
            progress.StartedAt = DateTime.UtcNow;
        if (req.Status == LessonStatus.Completed)
            progress.CompletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Progress updated uid={Uid} lessonId={LessonId} status={Status}",
            uid, lessonId, req.Status);

        return Ok(new LessonProgressResponse(lessonId, lesson.Title, lesson.OrderIndex,
            progress.Status, progress.StartedAt, progress.CompletedAt));
    }

    [HttpGet("{uid}/duels")]
    public async Task<IActionResult> GetDuelHistory(string uid, CancellationToken ct,
        int page = 0, int pageSize = 20)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var duels = await db.Duels
            .Include(d => d.Challenger).ThenInclude(c => c.User)
            .Include(d => d.Opponent).ThenInclude(c => c.User)
            .Where(d => d.ChallengerChildId == child.Id || d.OpponentChildId == child.Id)
            .OrderByDescending(d => d.DuelDate)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = duels.Select(d =>
        {
            var isChallenger = d.ChallengerChildId == child.Id;
            var myPts  = isChallenger ? d.ChallengerPoints : d.OpponentPoints;
            var oppPts = isChallenger ? d.OpponentPoints   : d.ChallengerPoints;
            var opp    = isChallenger ? d.Opponent : d.Challenger;
            var result = d.Status != DuelStatus.Completed ? d.Status.ToString()
                       : d.WinnerChildId is null           ? "Draw"
                       : d.WinnerChildId == child.Id       ? "Win"
                       :                                     "Loss";
            return new DuelHistoryItem(d.Id, d.DuelDate, opp.User.DisplayName, myPts, oppPts, result);
        });

        return Ok(items);
    }

    [HttpGet("{uid}/stats")]
    public async Task<IActionResult> GetStats(string uid, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var stats = await db.ChildStats.FirstOrDefaultAsync(s => s.ChildId == child.Id, ct);
        if (stats is null)
            return Ok(new ChildStatsResponse(0, 0, 0, 0, 0, 0, 0));

        return Ok(new ChildStatsResponse(stats.TotalDuels, stats.Wins, stats.Losses,
            stats.Draws, stats.WinStreak, stats.BestStreak, stats.TotalPoints));
    }

    [HttpPost("{uid}/records")]
    public async Task<IActionResult> SaveRecord(string uid, SaveHealthRecordRequest req, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var record = new HealthRecord
        {
            ChildId        = child.Id,
            GlucoseMmol    = req.GlucoseMmol,
            MealContext    = req.MealContext,
            InsulinLong    = req.InsulinLong,
            InsulinShort   = req.InsulinShort,
            CarbohydratesG = req.CarbohydratesG,
            Mood           = req.Mood,
            RecordedAt     = req.RecordedAt.ToUniversalTime(),
        };
        db.HealthRecords.Add(record);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("HealthRecord saved uid={Uid} glucose={G}", uid, req.GlucoseMmol);

        return Ok(new HealthRecordResponse(record.Id, record.GlucoseMmol, record.MealContext,
            record.InsulinLong, record.InsulinShort, record.CarbohydratesG,
            record.Mood, record.RecordedAt, record.CreatedAt));
    }

    [HttpGet("{uid}/records")]
    public async Task<IActionResult> GetRecords(string uid, CancellationToken ct,
        int page = 0, int pageSize = 30)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var records = await db.HealthRecords
            .Where(r => r.ChildId == child.Id)
            .OrderByDescending(r => r.RecordedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(r => new HealthRecordResponse(r.Id, r.GlucoseMmol, r.MealContext,
                r.InsulinLong, r.InsulinShort, r.CarbohydratesG,
                r.Mood, r.RecordedAt, r.CreatedAt))
            .ToListAsync(ct);

        return Ok(records);
    }

    [HttpPost("{uid}/accept-invite")]
    public async Task<IActionResult> AcceptInvite(string uid, AcceptInviteRequest req, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var link = await db.ParentChildLinks
            .FirstOrDefaultAsync(l => l.InviteCode == req.InviteCode && l.Status == LinkStatus.Pending, ct);

        if (link is null) return NotFound("Invite code not found or already used.");

        link.ChildId  = child.Id;
        link.Status   = LinkStatus.Active;
        link.LinkedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Parent-child link activated — child={ChildId} code={Code}", child.Id, req.InviteCode);

        return Ok();
    }
}
