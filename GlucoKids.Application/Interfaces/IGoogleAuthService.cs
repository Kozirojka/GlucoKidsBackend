using GlucoKids.Application.DTOs;

namespace GlucoKids.Application.Interfaces;

public interface IGoogleAuthService
{
    string BuildAuthorizationUrl(string state);
    Task<GoogleUserInfo> ExchangeCodeAsync(string code, CancellationToken ct = default);
}
