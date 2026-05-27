namespace GlucoKids.Application.DTOs;

public record FoodItem(string Name, int Calories, float Carbs, float BreadUnits, string? Brand, string? FoodType, string? ServingDescription);

public record FoodSearchResponse(List<FoodItem> Foods, int Total);
