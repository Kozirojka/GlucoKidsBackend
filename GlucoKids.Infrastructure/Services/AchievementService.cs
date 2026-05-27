using GlucoKids.Application.DTOs;
using GlucoKids.Application.Interfaces;
using GlucoKids.Domain.Entities;
using GlucoKids.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GlucoKids.Infrastructure.Services;

public class AchievementService(AppDbContext db, ILogger<AchievementService> logger) : IAchievementService
{
    public async Task<AchievementsResponse> GetAllWithStatusAsync(int childId)
    {
        var all = await db.Achievements
            .OrderBy(a => a.Category)
            .ThenBy(a => a.Id)
            .ToListAsync();

        var earned = await db.ChildAchievements
            .Where(ca => ca.ChildId == childId)
            .ToDictionaryAsync(ca => ca.AchievementId);

        var dtos = all.Select(a =>
        {
            earned.TryGetValue(a.Id, out var ca);
            return new AchievementDto(a.Id, a.Key, a.Title, a.Description,
                a.IconEmoji, a.XpReward, a.Category, ca?.EarnedAt);
        }).ToList();

        return new AchievementsResponse(dtos, earned.Count);
    }

    public async Task CheckAndAwardAsync(int childId, AchievementTrigger trigger, string? referenceId = null)
    {
        switch (trigger)
        {
            case AchievementTrigger.HealthRecordSaved:
                await AddXpLog(childId, 5, XpReason.HealthRecordSaved, referenceId);
                await CheckHealthRecordAchievements(childId);
                break;
            case AchievementTrigger.LessonCompleted:
                await AddXpLog(childId, 10, XpReason.LessonCompleted, referenceId);
                await CheckLessonAchievements(childId);
                break;
        }
    }

    private async Task AddXpLog(int childId, int amount, XpReason reason, string? referenceId)
    {
        var child = await db.Children.FindAsync(childId);
        if (child is null) return;

        db.XpLogs.Add(new XpLog
        {
            ChildId     = childId,
            Amount      = amount,
            Reason      = reason,
            ReferenceId = referenceId,
            EarnedAt    = DateTime.UtcNow
        });
        child.TotalXp += amount;
        await db.SaveChangesAsync();
    }

    private async Task CheckHealthRecordAchievements(int childId)
    {
        var count = await db.HealthRecords.CountAsync(r => r.ChildId == childId);
        if (count >= 1)  await AwardIfNotYet(childId, "first_record");
        if (count >= 7)  await AwardIfNotYet(childId, "records_7");
        if (count >= 30) await AwardIfNotYet(childId, "records_30");

        var inRangeCount = await db.HealthRecords.CountAsync(r =>
            r.ChildId == childId && r.GlucoseMmol >= 3.9m && r.GlucoseMmol <= 10.0m);
        if (inRangeCount >= 5) await AwardIfNotYet(childId, "in_range_5");

        var streak = await ComputeStreak(childId);
        if (streak >= 3)  await AwardIfNotYet(childId, "streak_3");
        if (streak >= 7)  await AwardIfNotYet(childId, "streak_7");
        if (streak >= 30) await AwardIfNotYet(childId, "streak_30");
    }

    private const int TotalLessons = 10;

    private async Task CheckLessonAchievements(int childId)
    {
        var completed = await db.LessonProgress
            .CountAsync(p => p.ChildId == childId && p.Status == LessonStatus.Completed);

        if (completed >= 1) await AwardIfNotYet(childId, "first_lesson");
        if (completed >= 5) await AwardIfNotYet(childId, "lessons_5");
        if (completed >= TotalLessons) await AwardIfNotYet(childId, "all_lessons");
    }

    private async Task<int> ComputeStreak(int childId)
    {
        var dates = await db.HealthRecords
            .Where(r => r.ChildId == childId)
            .OrderByDescending(r => r.RecordedAt)
            .Take(60)
            .Select(r => r.RecordedAt.Date)
            .ToListAsync();

        var distinctDates = dates.Distinct().OrderByDescending(d => d).ToList();
        if (distinctDates.Count == 0) return 0;

        var today = DateTime.UtcNow.Date;
        if (distinctDates[0] != today && distinctDates[0] != today.AddDays(-1))
            return 0;

        int streak = 1;
        for (int i = 1; i < distinctDates.Count; i++)
        {
            if (distinctDates[i] == distinctDates[i - 1].AddDays(-1))
                streak++;
            else
                break;
        }
        return streak;
    }

    private async Task AwardIfNotYet(int childId, string key)
    {
        var achievement = await db.Achievements.FirstOrDefaultAsync(a => a.Key == key);
        if (achievement is null) return;

        var already = await db.ChildAchievements.AnyAsync(ca =>
            ca.ChildId == childId && ca.AchievementId == achievement.Id);
        if (already) return;

        db.ChildAchievements.Add(new ChildAchievement
        {
            ChildId       = childId,
            AchievementId = achievement.Id,
            EarnedAt      = DateTime.UtcNow
        });

        var child = await db.Children.FindAsync(childId);
        if (child is not null)
        {
            db.XpLogs.Add(new XpLog
            {
                ChildId     = childId,
                Amount      = achievement.XpReward,
                Reason      = XpReason.AchievementEarned,
                ReferenceId = achievement.Key,
                EarnedAt    = DateTime.UtcNow
            });
            child.TotalXp += achievement.XpReward;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Achievement awarded: {Key} → child {ChildId}", key, childId);
    }
}
