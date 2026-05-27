using GlucoKids.Domain.Entities;

namespace GlucoKids.Application.DTOs;

public record MedicalProfileResponse(
    DiabetesType DiabetesType,
    DateOnly? DiagnosedAt,
    decimal TargetGlucoseMin,
    decimal TargetGlucoseMax,
    string? InsulinBrand,
    DateTime UpdatedAt);

public record UpsertMedicalProfileRequest(
    DiabetesType DiabetesType,
    DateOnly? DiagnosedAt,
    decimal TargetGlucoseMin,
    decimal TargetGlucoseMax,
    string? InsulinBrand);