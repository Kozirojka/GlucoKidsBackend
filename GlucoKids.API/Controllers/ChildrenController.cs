using GlucoKids.Application.DTOs;
using GlucoKids.Application.Interfaces;
using GlucoKids.Domain.Entities;
using GlucoKids.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoKids.Controllers;

[ApiController]
[Route("api/children")]
public class ChildrenController(
    AppDbContext db,
    ILogger<ChildrenController> logger,
    IAchievementService achievementService) : ControllerBase
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

        user!.DisplayName = req.DisplayName;
        user.AvatarUrl    = req.AvatarUrl;
        child.DateOfBirth = req.DateOfBirth;

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

        var progress = await db.LessonProgress
            .Where(p => p.ChildId == child.Id)
            .OrderBy(p => p.LessonKey)
            .Select(p => new LessonProgressResponse(p.LessonKey, p.Status, p.StartedAt, p.CompletedAt))
            .ToListAsync(ct);

        return Ok(progress);
    }

    [HttpPut("{uid}/progress/{lessonKey}")]
    public async Task<IActionResult> UpdateProgress(string uid, string lessonKey,
        UpdateProgressRequest req, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var progress = await db.LessonProgress
            .FirstOrDefaultAsync(p => p.ChildId == child.Id && p.LessonKey == lessonKey, ct);

        if (progress is null)
        {
            progress = new LessonProgress { ChildId = child.Id, LessonKey = lessonKey };
            db.LessonProgress.Add(progress);
        }

        progress.Status = req.Status;
        if (req.Status == LessonStatus.InProgress && progress.StartedAt is null)
            progress.StartedAt = DateTime.UtcNow;
        if (req.Status == LessonStatus.Completed)
            progress.CompletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        if (req.Status == LessonStatus.Completed)
            await achievementService.CheckAndAwardAsync(child.Id, AchievementTrigger.LessonCompleted);

        logger.LogInformation("Progress updated uid={Uid} lessonKey={Key} status={Status}",
            uid, lessonKey, req.Status);

        return Ok(new LessonProgressResponse(lessonKey, progress.Status, progress.StartedAt, progress.CompletedAt));
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

        if (req.Foods is { Count: > 0 })
        {
            var foods = req.Foods.Select(f => new FoodEntry
            {
                HealthRecordId  = record.Id,
                Name            = f.Name,
                Brand           = f.Brand,
                Calories        = f.Calories,
                CarbohydratesG  = f.CarbohydratesG,
                BreadUnits      = f.BreadUnits,
                WeightG         = f.WeightG,
            });
            db.FoodEntries.AddRange(foods);
            await db.SaveChangesAsync(ct);
        }

        await achievementService.CheckAndAwardAsync(child.Id, AchievementTrigger.HealthRecordSaved);

        logger.LogInformation("HealthRecord saved uid={Uid} glucose={G}", uid, req.GlucoseMmol);

        return Ok(new HealthRecordResponse(record.Id, record.GlucoseMmol, record.MealContext,
            record.InsulinLong, record.InsulinShort, record.CarbohydratesG,
            record.Mood, record.RecordedAt, record.CreatedAt));
    }

    [HttpGet("{uid}/medical-profile")]
    public async Task<IActionResult> GetMedicalProfile(string uid, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var profile = await db.MedicalProfiles.FirstOrDefaultAsync(p => p.ChildId == child.Id, ct);
        if (profile is null) return NotFound("Medical profile not set.");

        return Ok(new MedicalProfileResponse(profile.DiabetesType, profile.DiagnosedAt,
            profile.TargetGlucoseMin, profile.TargetGlucoseMax, profile.InsulinBrand, profile.UpdatedAt));
    }

    [HttpPut("{uid}/medical-profile")]
    public async Task<IActionResult> UpsertMedicalProfile(string uid,
        UpsertMedicalProfileRequest req, CancellationToken ct)
    {
        var (_, child) = await GetChildByUid(uid, ct);
        if (child is null) return NotFound("Child not found.");

        var profile = await db.MedicalProfiles.FirstOrDefaultAsync(p => p.ChildId == child.Id, ct);
        if (profile is null)
        {
            profile = new MedicalProfile { ChildId = child.Id };
            db.MedicalProfiles.Add(profile);
        }

        profile.DiabetesType      = req.DiabetesType;
        profile.DiagnosedAt       = req.DiagnosedAt;
        profile.TargetGlucoseMin  = req.TargetGlucoseMin;
        profile.TargetGlucoseMax  = req.TargetGlucoseMax;
        profile.InsulinBrand      = req.InsulinBrand;
        profile.UpdatedAt         = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Ok(new MedicalProfileResponse(profile.DiabetesType, profile.DiagnosedAt,
            profile.TargetGlucoseMin, profile.TargetGlucoseMax, profile.InsulinBrand, profile.UpdatedAt));
    }

    [HttpGet("{uid}/xp")]
    public async Task<IActionResult> GetXp(string uid, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Child)
            .FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        var child = user?.Child;
        if (child is null) return NotFound("Child not found.");

        var recent = await db.XpLogs
            .Where(x => x.ChildId == child.Id)
            .OrderByDescending(x => x.EarnedAt)
            .Take(20)
            .Select(x => new XpLogEntry(x.Amount, x.Reason, x.ReferenceId, x.EarnedAt))
            .ToListAsync(ct);

        return Ok(new XpSummaryResponse(child.TotalXp, recent));
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

}
