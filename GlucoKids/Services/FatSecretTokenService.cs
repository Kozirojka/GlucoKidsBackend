using System.Text.Json;

namespace GlucoKids.Services;

public class FatSecretTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    private string? _accessToken;
    private DateTime _expiresAt = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly string _clientId = configuration["FatSecret:ClientId"]
        ?? throw new InvalidOperationException("FatSecret:ClientId is not configured");

    private readonly string _clientSecret = configuration["FatSecret:ClientSecret"]
        ?? throw new InvalidOperationException("FatSecret:ClientSecret is not configured");

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (_accessToken is not null && DateTime.UtcNow < _expiresAt)
            return _accessToken;

        await _lock.WaitAsync(ct);
        try
        {
            // Double-check after acquiring the lock
            if (_accessToken is not null && DateTime.UtcNow < _expiresAt)
                return _accessToken;

            var body = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["scope"] = "basic premier"
            };

            var client = httpClientFactory.CreateClient("FatSecretToken");
            var response = await client.PostAsync(
                "https://oauth.fatsecret.com/connect/token",
                new FormUrlEncodedContent(body),
                ct);

            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"FatSecret token request failed ({(int)response.StatusCode}): {content}");

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            _accessToken = root.GetProperty("access_token").GetString()
                ?? throw new InvalidOperationException("FatSecret returned null access_token");

            var expiresIn = root.GetProperty("expires_in").GetInt32();
            _expiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

            return _accessToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}
