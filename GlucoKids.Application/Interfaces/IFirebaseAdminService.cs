namespace GlucoKids.Application.Interfaces;

public interface IFirebaseAdminService
{
    Task SetAdminClaimAsync(string uid, bool isAdmin, CancellationToken ct = default);
}
