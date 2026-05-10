using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using GlucoKids.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GlucoKids.Infrastructure.Services;

public class FirebaseTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : IFirebaseTokenService
{
    private const string GoogleCertsUrl =
        "https://www.googleapis.com/robot/v1/metadata/x509/securetoken@system.gserviceaccount.com";

    private readonly string _projectId = configuration["Firebase:ProjectId"]
        ?? throw new InvalidOperationException("Firebase:ProjectId is not configured");

    private List<SecurityKey>? _cachedKeys;
    private DateTime _keysExpireAt = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<ClaimsPrincipal> VerifyIdTokenAsync(string idToken, CancellationToken ct = default)
    {
        var keys = await GetSigningKeysAsync(ct);

        var handler = new JwtSecurityTokenHandler();
        var parameters = new TokenValidationParameters
        {
            ValidIssuer              = $"https://securetoken.google.com/{_projectId}",
            ValidAudience            = _projectId,
            IssuerSigningKeys        = keys,
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ClockSkew                = TimeSpan.FromMinutes(5)
        };

        return handler.ValidateToken(idToken, parameters, out _);
    }

    private async Task<List<SecurityKey>> GetSigningKeysAsync(CancellationToken ct)
    {
        if (_cachedKeys is not null && DateTime.UtcNow < _keysExpireAt)
            return _cachedKeys;

        await _lock.WaitAsync(ct);
        try
        {
            if (_cachedKeys is not null && DateTime.UtcNow < _keysExpireAt)
                return _cachedKeys;

            var client = httpClientFactory.CreateClient("GoogleAuth");
            var response = await client.GetAsync(GoogleCertsUrl, ct);
            response.EnsureSuccessStatusCode();

            _keysExpireAt = DateTime.UtcNow.AddHours(1);
            if (response.Headers.CacheControl?.MaxAge is { } maxAge)
                _keysExpireAt = DateTime.UtcNow.Add(maxAge - TimeSpan.FromMinutes(5));

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var keys = new List<SecurityKey>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                    System.Text.Encoding.UTF8.GetBytes(prop.Value.GetString()!));
                keys.Add(new X509SecurityKey(cert));
            }

            _cachedKeys = keys;
            return keys;
        }
        finally
        {
            _lock.Release();
        }
    }
}
