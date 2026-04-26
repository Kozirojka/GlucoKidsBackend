namespace GlucoKids.Models;

public record FoodItem(string Name, int Calories, float Carbs, float BreadUnits, string? Brand);
public record FoodSearchResponse(List<FoodItem> Foods, int Total);
