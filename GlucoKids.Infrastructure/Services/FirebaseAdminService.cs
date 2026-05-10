using FirebaseAdmin.Auth;
using GlucoKids.Application.Interfaces;

namespace GlucoKids.Infrastructure.Services;

public class FirebaseAdminService : IFirebaseAdminService
{
    public async Task SetAdminClaimAsync(string uid, bool isAdmin, CancellationToken ct = default)
    {
        var claims = new Dictionary<string, object>
        {
            { "admin", isAdmin }
        };
        await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);
    }
}
