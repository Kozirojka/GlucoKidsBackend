using System.Security.Claims;

namespace GlucoKids.Application.Interfaces;

public interface IFirebaseTokenService
{
    Task<ClaimsPrincipal> VerifyIdTokenAsync(string idToken, CancellationToken ct = default);
}
