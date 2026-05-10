namespace GlucoKids.Application.DTOs;

public record CreateInviteResponse(string InviteCode);

public record AcceptInviteRequest(string InviteCode);

public record LinkedChildResponse(int ChildId, string? DisplayName, string? AvatarUrl, DateOnly? DateOfBirth);
