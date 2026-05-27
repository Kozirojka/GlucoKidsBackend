using GlucoKids.Application.DTOs;

namespace GlucoKids.Application.Interfaces;

public enum AchievementTrigger
{
    HealthRecordSaved,
    LessonCompleted
}

public interface IAchievementService
{
    Task<AchievementsResponse> GetAllWithStatusAsync(int childId);
    Task CheckAndAwardAsync(int childId, AchievementTrigger trigger, string? referenceId = null);
}
