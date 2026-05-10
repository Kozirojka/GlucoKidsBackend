using GlucoKids.Application.DTOs;
using GlucoKids.Domain.Entities;
using GlucoKids.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoKids.Controllers;

[ApiController]
[Route("api/duels")]
public class DuelsController(AppDbContext db, ILogger<DuelsController> logger) : ControllerBase
{
    private async Task<Child?> GetChildByUid(string uid, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Child)
            .FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        return user?.Child;
    }

    [HttpPost("queue")]
    public async Task<IActionResult> JoinQueue(JoinQueueRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.FirebaseUid))
            return BadRequest("FirebaseUid is required.");

        var child = await GetChildByUid(req.FirebaseUid, ct);
        if (child is null) return NotFound("Child not found. Register first.");

        var hasActive = await db.Duels.AnyAsync(d =>
            (d.ChallengerChildId == child.Id || d.OpponentChildId == child.Id) &&
            d.Status == DuelStatus.Active, ct);
        if (hasActive) return Conflict("Child already has an active duel.");

        var alreadyQueued = await db.MatchmakingQueue.AnyAsync(q => q.ChildId == child.Id, ct);
        if (!alreadyQueued)
            db.MatchmakingQueue.Add(new MatchmakingQueue { ChildId = child.Id });

        var opponent = await db.MatchmakingQueue
            .Where(q => q.ChildId != child.Id)
            .OrderBy(q => q.JoinedAt)
            .FirstOrDefaultAsync(ct);

        if (opponent is null)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Child {Uid} joined queue — waiting", req.FirebaseUid);
            return Ok(new QueueStatusResponse(false, null));
        }

        var duel = new Duel
        {
            ChallengerChildId = opponent.ChildId,
            OpponentChildId   = child.Id,
            DuelDate          = DateOnly.FromDateTime(DateTime.UtcNow),
            EndsAt            = DateTime.UtcNow.Date.AddDays(1)
        };
        db.Duels.Add(duel);
        db.MatchmakingQueue.Remove(opponent);

        foreach (var cid in new[] { opponent.ChildId, child.Id })
            if (!await db.ChildStats.AnyAsync(s => s.ChildId == cid, ct))
                db.ChildStats.Add(new ChildStats { ChildId = cid });

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Duel created — challenger={A} opponent={B}", opponent.ChildId, child.Id);
        return Ok(new QueueStatusResponse(true, duel.Id));
    }

    [HttpDelete("queue/{uid}")]
    public async Task<IActionResult> LeaveQueue(string uid, CancellationToken ct)
    {
        var child = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var entry = await db.MatchmakingQueue.FirstOrDefaultAsync(q => q.ChildId == child.Id, ct);
        if (entry is null) return NotFound("Child is not in queue.");

        db.MatchmakingQueue.Remove(entry);
        await db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet("queue/{uid}")]
    public async Task<IActionResult> GetQueueStatus(string uid, CancellationToken ct)
    {
        var child = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var activeDuel = await db.Duels.FirstOrDefaultAsync(d =>
            (d.ChallengerChildId == child.Id || d.OpponentChildId == child.Id) &&
            d.Status == DuelStatus.Active, ct);

        if (activeDuel is not null)
            return Ok(new QueueStatusResponse(true, activeDuel.Id));

        return Ok(new QueueStatusResponse(false, null));
    }

    [HttpGet("{duelId:int}")]
    public async Task<IActionResult> GetDuel(int duelId, [FromQuery] string uid, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Child)
            .FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        var child = user?.Child;
        if (child is null) return NotFound("Child not found.");

        var duel = await db.Duels
            .Include(d => d.Challenger).ThenInclude(c => c.User)
            .Include(d => d.Opponent).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(d => d.Id == duelId, ct);
        if (duel is null) return NotFound("Duel not found.");

        var isChallenger = duel.ChallengerChildId == child.Id;
        var isOpponent   = duel.OpponentChildId   == child.Id;
        if (!isChallenger && !isOpponent) return Forbid();

        var readings = await db.GlucoseReadings
            .Where(r => r.DuelId == duelId)
            .OrderBy(r => r.MeasuredAt)
            .ToListAsync(ct);

        var opponentEntity = isChallenger ? duel.Opponent : duel.Challenger;
        var myPoints  = isChallenger ? duel.ChallengerPoints : duel.OpponentPoints;
        var oppPoints = isChallenger ? duel.OpponentPoints   : duel.ChallengerPoints;

        GlucoseReadingResponse ToDto(GlucoseReading r, string? name) =>
            new(r.Id, r.ChildId, name, r.Value, r.IsInRange, r.MeasuredAt);

        return Ok(new DuelResponse(
            duel.Id, duel.DuelDate, duel.EndsAt, duel.Status,
            myPoints, oppPoints,
            opponentEntity.User.DisplayName,
            duel.WinnerChildId,
            readings.Where(r => r.ChildId == child.Id).Select(r => ToDto(r, user!.DisplayName)),
            readings.Where(r => r.ChildId == opponentEntity.Id).Select(r => ToDto(r, opponentEntity.User.DisplayName))
        ));
    }

    [HttpPost("{duelId:int}/glucose")]
    public async Task<IActionResult> LogGlucose(int duelId, [FromQuery] string uid,
        LogGlucoseRequest req, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Child)
            .FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        var child = user?.Child;
        if (child is null) return NotFound("Child not found.");

        var duel = await db.Duels.FindAsync([duelId], ct);
        if (duel is null) return NotFound("Duel not found.");
        if (duel.Status != DuelStatus.Active) return BadRequest("Duel is not active.");
        if (DateTime.UtcNow >= duel.EndsAt) return BadRequest("Duel has ended.");

        var isChallenger = duel.ChallengerChildId == child.Id;
        var isOpponent   = duel.OpponentChildId   == child.Id;
        if (!isChallenger && !isOpponent) return Forbid();

        const decimal rangeLow  = 3.9m;
        const decimal rangeHigh = 10.0m;
        var isInRange = req.Value >= rangeLow && req.Value <= rangeHigh;

        var reading = new GlucoseReading
        {
            ChildId    = child.Id,
            DuelId     = duelId,
            Value      = req.Value,
            IsInRange  = isInRange,
            MeasuredAt = req.MeasuredAt.ToUniversalTime()
        };
        db.GlucoseReadings.Add(reading);
        await db.SaveChangesAsync(ct);

        if (isInRange)
        {
            const int pts = 10;
            if (isChallenger) duel.ChallengerPoints += pts;
            else              duel.OpponentPoints   += pts;

            db.DuelPointEvents.Add(new DuelPointEvent
            {
                DuelId           = duelId,
                ChildId          = child.Id,
                Points           = pts,
                Reason           = PointReason.InTargetReading,
                GlucoseReadingId = reading.Id
            });

            var stats = await db.ChildStats.FirstOrDefaultAsync(s => s.ChildId == child.Id, ct);
            if (stats is not null) stats.TotalPoints += pts;

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Glucose {Value} in range → +{Pts} pts uid={Uid}", req.Value, pts, uid);
        }

        return Ok(new GlucoseReadingResponse(reading.Id, reading.ChildId, user!.DisplayName,
            reading.Value, reading.IsInRange, reading.MeasuredAt));
    }
}
