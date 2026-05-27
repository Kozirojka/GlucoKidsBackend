using GlucoKids.Domain.Entities;

namespace GlucoKids.Application.DTOs;

public record AchievementDto(
    int Id,
    string Key,
    string Title,
    string Description,
    string IconEmoji,
    int XpReward,
    AchievementCategory Category,
    DateTime? EarnedAt);

public record AchievementsResponse(
    List<AchievementDto> All,
    int EarnedCount);
