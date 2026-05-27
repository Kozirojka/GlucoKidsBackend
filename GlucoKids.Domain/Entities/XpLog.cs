namespace GlucoKids.Domain.Entities;

public class XpLog
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int Amount { get; set; }
    public XpReason Reason { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    public Child Child { get; set; } = null!;
}

public enum XpReason { LessonCompleted, HealthRecordSaved, StreakDay, AchievementEarned }
