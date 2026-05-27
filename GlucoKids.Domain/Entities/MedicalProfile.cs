namespace GlucoKids.Domain.Entities;

public class MedicalProfile
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public DiabetesType DiabetesType { get; set; } = DiabetesType.T1;
    public DateOnly? DiagnosedAt { get; set; }
    public decimal TargetGlucoseMin { get; set; } = 3.9m;
    public decimal TargetGlucoseMax { get; set; } = 10.0m;
    public string? InsulinBrand { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Child Child { get; set; } = null!;
}

public enum DiabetesType { T1, T2, LADA, Other }
