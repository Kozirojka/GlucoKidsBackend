namespace GlucoKids.Application.DTOs;

public record GoogleUserInfo(string Id, string Email, string? Name, string? Picture);

public record AuthResponse(string UserId, string Email, string? Name, string? Picture, string Role, bool IsAdmin);
