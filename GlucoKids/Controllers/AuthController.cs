using GlucoKids.Application.DTOs;
using GlucoKids.Application.Interfaces;
using GlucoKids.Domain.Entities;
using GlucoKids.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRole = GlucoKids.Domain.Entities.UserRole;

namespace GlucoKids.Controllers;

[ApiController]
public class AuthController(
    IFirebaseTokenService firebaseTokenService,
    IFirebaseAdminService firebaseAdminService,
    IGoogleAuthService googleAuthService,
    AppDbContext db,
    IConfiguration configuration,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("/auth/verify")]
    public async Task<IActionResult> VerifyFirebaseToken(CancellationToken ct)
    {
        var body = await new StreamReader(Request.Body).ReadToEndAsync(ct);
        using var doc = System.Text.Json.JsonDocument.Parse(body);

        if (!doc.RootElement.TryGetProperty("idToken", out var tokenEl))
            return BadRequest("Field 'idToken' is required.");

        var idToken = tokenEl.GetString();
        if (string.IsNullOrWhiteSpace(idToken))
            return BadRequest("idToken cannot be empty.");

        var roleStr = doc.RootElement.TryGetProperty("role", out var roleEl) ? roleEl.GetString() : null;
        var role = roleStr?.Equals("Parent", StringComparison.OrdinalIgnoreCase) == true
            ? UserRole.Parent : UserRole.Child;

        var adminUid = configuration["Auth:AdminUid"];

        try
        {
            var principal = await firebaseTokenService.VerifyIdTokenAsync(idToken, ct);
            var uid     = principal.FindFirst("user_id")?.Value ?? principal.FindFirst("sub")?.Value;
            var email   = principal.FindFirst("email")?.Value;
            var name    = principal.FindFirst("name")?.Value;
            var picture = principal.FindFirst("picture")?.Value;

            if (!string.IsNullOrEmpty(uid))
            {
                var user  = await db.Users.FirstOrDefaultAsync(u => u.FirebaseUid == uid, ct);
                var isNew = user is null;

                if (isNew)
                {
                    user = new User { FirebaseUid = uid, Role = role, CreatedAt = DateTime.UtcNow };
                    db.Users.Add(user);
                    await db.SaveChangesAsync(ct);

                    if (role == UserRole.Child)
                        db.Children.Add(new Child { Id = user.Id });
                    else
                        db.Parents.Add(new Parent { Id = user.Id });
                }

                user!.Email      = email;
                user.DisplayName = name;
                user.AvatarUrl   = picture;

                // Визначаємо чи користувач — адмін (з конфігу або вже помічений в DB)
                if (!string.IsNullOrEmpty(adminUid) && uid == adminUid)
                    user.IsAdmin = true;

                await db.SaveChangesAsync(ct);

                // Встановлюємо Firebase custom claim — підписується Google, клієнт не може підробити
                await firebaseAdminService.SetAdminClaimAsync(uid, user.IsAdmin, ct);

                logger.LogInformation("Auth OK — uid={Uid} newUser={IsNew} role={Role} isAdmin={IsAdmin}",
                    uid, isNew, user.Role, user.IsAdmin);

                return Ok(new AuthResponse(uid, email ?? string.Empty, name, picture,
                    user.Role.ToString(), user.IsAdmin));
            }

            return Ok(new AuthResponse(string.Empty, email ?? string.Empty, name, picture,
                role.ToString(), false));
        }
        catch (Microsoft.IdentityModel.Tokens.SecurityTokenException ex)
        {
            logger.LogWarning("Auth FAILED — {Message}", ex.Message);
            return Problem(detail: ex.Message, statusCode: 401, title: "Invalid Firebase ID token");
        }
    }

    [HttpGet("/auth/google")]
    public IActionResult GoogleLogin()
    {
        var state = Guid.NewGuid().ToString("N");
        return Redirect(googleAuthService.BuildAuthorizationUrl(state));
    }

    [HttpGet("/auth/callback")]
    public async Task<IActionResult> GoogleCallback(string? code, string? error, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(error))
            return BadRequest($"Google OAuth error: {error}");

        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("Authorization code is missing.");

        try
        {
            var userInfo = await googleAuthService.ExchangeCodeAsync(code, ct);
            return Ok(new AuthResponse(userInfo.Id, userInfo.Email, userInfo.Name, userInfo.Picture,
                string.Empty, false));
        }
        catch (HttpRequestException ex)
        {
            return Problem(detail: ex.Message, statusCode: 502, title: "Google Auth error");
        }
    }
}
