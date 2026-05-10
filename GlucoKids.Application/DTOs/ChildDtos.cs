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

public record UpdateProgressRequest(LessonStatus Status);

public record LessonProgressResponse(
    int LessonId,
    string LessonTitle,
    int OrderIndex,
    LessonStatus Status,
    DateTime? StartedAt,
    DateTime? CompletedAt);

public record ModuleProgressResponse(
    int ModuleId,
    string ModuleTitle,
    string? ModuleDescription,
    int OrderIndex,
    IEnumerable<LessonProgressResponse> Lessons);

public record SaveHealthRecordRequest(
    decimal? GlucoseMmol,
    string? MealContext,
    decimal? InsulinLong,
    decimal? InsulinShort,
    decimal? CarbohydratesG,
    int? Mood,
    DateTime RecordedAt);

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
