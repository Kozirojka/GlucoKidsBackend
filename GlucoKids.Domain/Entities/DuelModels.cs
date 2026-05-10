namespace GlucoKids.Domain.Entities;

public class MatchmakingQueue
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Child Child { get; set; } = null!;
}

public class Duel
{
    public int Id { get; set; }
    public int ChallengerChildId { get; set; }
    public int OpponentChildId { get; set; }
    public DuelStatus Status { get; set; } = DuelStatus.Active;
    public DateOnly DuelDate { get; set; }
    public DateTime EndsAt { get; set; }
    public int? WinnerChildId { get; set; }
    public int ChallengerPoints { get; set; }
    public int OpponentPoints { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Child Challenger { get; set; } = null!;
    public Child Opponent { get; set; } = null!;
    public Child? Winner { get; set; }
    public ICollection<GlucoseReading> GlucoseReadings { get; set; } = [];
    public ICollection<DuelPointEvent> PointEvents { get; set; } = [];
}

public class GlucoseReading
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int? DuelId { get; set; }
    public decimal Value { get; set; }
    public bool IsInRange { get; set; }
    public DateTime MeasuredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Child Child { get; set; } = null!;
    public Duel? Duel { get; set; }
}

public class DuelPointEvent
{
    public int Id { get; set; }
    public int DuelId { get; set; }
    public int ChildId { get; set; }
    public int Points { get; set; }
    public PointReason Reason { get; set; }
    public int? GlucoseReadingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Duel Duel { get; set; } = null!;
    public Child Child { get; set; } = null!;
    public GlucoseReading? GlucoseReading { get; set; }
}

public class ChildStats
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public int TotalDuels { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int WinStreak { get; set; }
    public int BestStreak { get; set; }
    public int TotalPoints { get; set; }

    public Child Child { get; set; } = null!;
}

public enum DuelStatus { Active, Completed, Cancelled }
public enum PointReason { InTargetReading }
