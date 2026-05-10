using System.Net.Http.Headers;
using System.Text.Json;
using GlucoKids.Application.DTOs;
using GlucoKids.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GlucoKids.Infrastructure.Services;

public class GoogleAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : IGoogleAuthService
{
    private readonly string _clientId = configuration["Google:ClientId"]
        ?? throw new InvalidOperationException("Google:ClientId is not configured");

    private readonly string _clientSecret = configuration["Google:ClientSecret"]
        ?? throw new InvalidOperationException("Google:ClientSecret is not configured");

    private readonly string _redirectUri = configuration["Google:RedirectUri"]
        ?? "http://localhost:8080/auth/callback";

    public string BuildAuthorizationUrl(string state)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"]     = _clientId,
            ["redirect_uri"]  = _redirectUri,
            ["response_type"] = "code",
            ["scope"]         = "openid email profile",
            ["state"]         = state,
            ["access_type"]   = "offline",
            ["prompt"]        = "select_account"
        };

        var qs = string.Join("&", query.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        return $"https://accounts.google.com/o/oauth2/v2/auth?{qs}";
    }

    public async Task<GoogleUserInfo> ExchangeCodeAsync(string code, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient("GoogleAuth");

        var tokenBody = new Dictionary<string, string>
        {
            ["code"]          = code,
            ["client_id"]     = _clientId,
            ["client_secret"] = _clientSecret,
            ["redirect_uri"]  = _redirectUri,
            ["grant_type"]    = "authorization_code"
        };

        var tokenResponse = await client.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(tokenBody), ct);

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(ct);
        if (!tokenResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Google token exchange failed ({(int)tokenResponse.StatusCode}): {tokenContent}");

        using var tokenDoc = JsonDocument.Parse(tokenContent);
        var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("Google returned null access_token");

        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var userResponse = await client.SendAsync(userRequest, ct);
        var userContent  = await userResponse.Content.ReadAsStringAsync(ct);
        if (!userResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Google userinfo failed ({(int)userResponse.StatusCode}): {userContent}");

        using var userDoc = JsonDocument.Parse(userContent);
        var root = userDoc.RootElement;

        return new GoogleUserInfo(
            Id:      root.GetProperty("id").GetString()!,
            Email:   root.GetProperty("email").GetString()!,
            Name:    root.TryGetProperty("name",    out var n) ? n.GetString() : null,
            Picture: root.TryGetProperty("picture", out var p) ? p.GetString() : null);
    }
}
