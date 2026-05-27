namespace GlucoKids.Domain.Entities;

public class Achievement
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = string.Empty;
    public int XpReward { get; set; }
    public AchievementCategory Category { get; set; }

    public ICollection<ChildAchievement> ChildAchievements { get; set; } = [];
}

public class ChildAchievement
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int AchievementId { get; set; }
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    public Child Child { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}

public enum AchievementCategory { Lessons, Glucose, Battle, Streak }
