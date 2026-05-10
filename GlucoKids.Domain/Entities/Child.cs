namespace GlucoKids.Domain.Entities;

public class Child
{
    public int Id { get; set; }
    public DateOnly? DateOfBirth { get; set; }

    public User User { get; set; } = null!;
    public ICollection<LessonProgress> Progress { get; set; } = [];
}

public class PsychoModule
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }

    public ICollection<PsychoLesson> Lessons { get; set; } = [];
}

public class PsychoLesson
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int OrderIndex { get; set; }

    public PsychoModule Module { get; set; } = null!;
    public ICollection<LessonProgress> Progress { get; set; } = [];
}

public class LessonProgress
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int LessonId { get; set; }
    public LessonStatus Status { get; set; } = LessonStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Child Child { get; set; } = null!;
    public PsychoLesson Lesson { get; set; } = null!;
}

public enum LessonStatus { NotStarted, InProgress, Completed }

// Manual health log — no DuelId so never affects battle scoring.
public class HealthRecord
{
    public int Id { get; set; }
    public int ChildId { get; set; }

    public decimal? GlucoseMmol { get; set; }
    public string? MealContext { get; set; }   // "AfterMeal" | "Fasting"
    public decimal? InsulinLong { get; set; }
    public decimal? InsulinShort { get; set; }
    public decimal? CarbohydratesG { get; set; }
    public int? Mood { get; set; }             // 1 (worst) – 5 (best)

    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Child Child { get; set; } = null!;
}
