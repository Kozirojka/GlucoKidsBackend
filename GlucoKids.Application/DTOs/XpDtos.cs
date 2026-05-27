using GlucoKids.Domain.Entities;

namespace GlucoKids.Application.DTOs;

public record XpSummaryResponse(int TotalXp, List<XpLogEntry> Recent);

public record XpLogEntry(
    int Amount,
    XpReason Reason,
    string? ReferenceId,
    DateTime EarnedAt);