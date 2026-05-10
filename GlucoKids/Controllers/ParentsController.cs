using GlucoKids.Application.DTOs;
using GlucoKids.Domain.Entities;
using GlucoKids.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlucoKids.Controllers;

[ApiController]
[Route("api/parents")]
public class ParentsController(AppDbContext db, ILogger<ParentsController> logger) : ControllerBase
{
    private async Task<(User? user, Parent? parent)> GetParentByUid(string uid, CancellationToken ct)
    {
        var user = await db.Users.Include(u => u.Parent)
            .FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
        return (user, user?.Parent);
    }

    [HttpPost("{parentUid}/invite")]
    public async Task<IActionResult> CreateInvite(string parentUid, CancellationToken ct)
    {
        var (_, parent) = await GetParentByUid(parentUid, ct);
        if (parent is null) return NotFound("Parent not found.");

        var code = GenerateInviteCode();

        db.ParentChildLinks.Add(new ParentChildLink
        {
            ParentId   = parent.Id,
            InviteCode = code,
            Status     = LinkStatus.Pending
        });
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Invite created — parentId={ParentId} code={Code}", parent.Id, code);

        return Ok(new CreateInviteResponse(code));
    }

    [HttpGet("{parentUid}/children")]
    public async Task<IActionResult> GetLinkedChildren(string parentUid, CancellationToken ct)
    {
        var (_, parent) = await GetParentByUid(parentUid, ct);
        if (parent is null) return NotFound("Parent not found.");

        var children = await db.ParentChildLinks
            .Where(l => l.ParentId == parent.Id && l.Status == LinkStatus.Active)
            .Include(l => l.ChildUser).ThenInclude(u => u.Child)
            .Select(l => new LinkedChildResponse(
                l.ChildId,
                l.ChildUser.DisplayName,
                l.ChildUser.AvatarUrl,
                l.ChildUser.Child!.DateOfBirth))
            .ToListAsync(ct);

        return Ok(children);
    }

    [HttpDelete("{parentUid}/children/{childId:int}")]
    public async Task<IActionResult> UnlinkChild(string parentUid, int childId, CancellationToken ct)
    {
        var (_, parent) = await GetParentByUid(parentUid, ct);
        if (parent is null) return NotFound("Parent not found.");

        var link = await db.ParentChildLinks
            .FirstOrDefaultAsync(l => l.ParentId == parent.Id && l.ChildId == childId && l.Status == LinkStatus.Active, ct);

        if (link is null) return NotFound("Active link not found.");

        db.ParentChildLinks.Remove(link);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Parent-child link removed — parentId={ParentId} childId={ChildId}", parent.Id, childId);

        return Ok();
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 8)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}
