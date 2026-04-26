using System.Text.Json;

namespace GlucoKids.Services;

public class FatSecretTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    private string? _accessToken;
    private DateTime _expiresAt = DateTime.MinValue;

    private readonly string _clientId = configuration["FatSecret:ClientId"]
        ?? throw new InvalidOperationException("FatSecret:ClientId is not configured");

    private readonly string _clientSecret = configuration["FatSecret:ClientSecret"]
        ?? throw new InvalidOperationException("FatSecret:ClientSecret is not configured");

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_accessToken is not null && DateTime.UtcNow < _expiresAt)
            return _accessToken;

        var client = httpClientFactory.CreateClient("FatSecretToken");

        var body = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["scope"] = "basic localization"
        };

        var response = await client.PostAsync(
            "https://oauth.fatsecret.com/connect/token",
            new FormUrlEncodedContent(body),
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"FatSecret token request failed ({(int)response.StatusCode}): {error}");
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = doc.RootElement;

        _accessToken = root.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("FatSecret returned null access_token");

        var expiresIn = root.GetProperty("expires_in").GetInt32();
        // Refresh 60 seconds before actual expiry to avoid race conditions
        _expiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        return _accessToken;
    }
}
