using GlucoKids.Domain.Entities;

namespace GlucoKids.Application.DTOs;

public record JoinQueueRequest(string FirebaseUid);

public record QueueStatusResponse(bool IsMatched, int? DuelId);

public record LogGlucoseRequest(decimal Value, DateTime MeasuredAt);

public record GlucoseReadingResponse(
    int Id,
    int ChildId,
    string? ChildName,
    decimal Value,
    bool IsInRange,
    DateTime MeasuredAt);

public record DuelResponse(
    int Id,
    DateOnly DuelDate,
    DateTime EndsAt,
    DuelStatus Status,
    int MyPoints,
    int OpponentPoints,
    string? OpponentName,
    int? WinnerChildId,
    IEnumerable<GlucoseReadingResponse> MyReadings,
    IEnumerable<GlucoseReadingResponse> OpponentReadings);

public record DuelHistoryItem(
    int Id,
    DateOnly DuelDate,
    string? OpponentName,
    int MyPoints,
    int OpponentPoints,
    string Result);

public record ChildStatsResponse(
    int TotalDuels,
    int Wins,
    int Losses,
    int Draws,
    int WinStreak,
    int BestStreak,
    int TotalPoints);
