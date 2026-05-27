namespace GlucoKids.Domain.Entities;

public class FoodEntry
{
    public int Id { get; set; }
    public int HealthRecordId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public int Calories { get; set; }
    public decimal CarbohydratesG { get; set; }
    public decimal BreadUnits { get; set; }
    public decimal? WeightG { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public HealthRecord HealthRecord { get; set; } = null!;
}
