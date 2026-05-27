namespace GlucoKids.Domain.Entities;

public class Child
{
    public int Id { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int TotalXp { get; set; } = 0;

    public User User { get; set; } = null!;
    public MedicalProfile? MedicalProfile { get; set; }
    public ICollection<LessonProgress> Progress { get; set; } = [];
    public ICollection<XpLog> XpLogs { get; set; } = [];
}

public class LessonProgress
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public string LessonKey { get; set; } = string.Empty;  // Firebase lesson ID
    public LessonStatus Status { get; set; } = LessonStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Child Child { get; set; } = null!;
}

public enum LessonStatus { NotStarted, InProgress, Completed }

public class HealthRecord
{
    public int Id { get; set; }
    public int ChildId { get; set; }

    public decimal? GlucoseMmol { get; set; }
    public string? MealContext { get; set; }
    public decimal? InsulinLong { get; set; }
    public decimal? InsulinShort { get; set; }
    public decimal? CarbohydratesG { get; set; }
    public int? Mood { get; set; }

    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Child Child { get; set; } = null!;
    public ICollection<FoodEntry> Foods { get; set; } = [];
}
