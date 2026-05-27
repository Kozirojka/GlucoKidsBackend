using GlucoKids.Domain.Entities;

namespace GlucoKids.Application.DTOs;

public record RegisterChildRequest(
    string FirebaseUid,
    string? DisplayName,
    string? AvatarUrl,
    DateOnly? DateOfBirth);

public record ChildResponse(
    int Id,
    string FirebaseUid,
    string? DisplayName,
    string? AvatarUrl,
    DateOnly? DateOfBirth,
    DateTime CreatedAt);

public record UpdateProgressRequest(string LessonKey, LessonStatus Status);

public record LessonProgressResponse(
    string LessonKey,
    LessonStatus Status,
    DateTime? StartedAt,
    DateTime? CompletedAt);

public record FoodEntryRequest(
    string Name,
    string? Brand,
    int Calories,
    decimal CarbohydratesG,
    decimal BreadUnits,
    decimal? WeightG);

public record SaveHealthRecordRequest(
    decimal? GlucoseMmol,
    string? MealContext,
    decimal? InsulinLong,
    decimal? InsulinShort,
    decimal? CarbohydratesG,
    int? Mood,
    DateTime RecordedAt,
    List<FoodEntryRequest>? Foods = null);

public record HealthRecordResponse(
    int Id,
    decimal? GlucoseMmol,
    string? MealContext,
    decimal? InsulinLong,
    decimal? InsulinShort,
    decimal? CarbohydratesG,
    int? Mood,
    DateTime RecordedAt,
    DateTime CreatedAt);
