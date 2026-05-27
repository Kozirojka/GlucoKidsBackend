using System.Net.Http.Headers;
using System.Text.Json;
using GlucoKids.Application.DTOs;
using GlucoKids.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GlucoKids.Infrastructure.Services;

public class FatSecretFoodService(
    IHttpClientFactory httpClientFactory,
    FatSecretTokenService tokenService,
    ILogger<FatSecretFoodService> logger) : IFoodService
{
    private const float CarbsPerBreadUnit = 12f;
    private const string ApiUrl = "https://platform.fatsecret.com/rest/foods/search/v5";

    public async Task<FoodSearchResponse> SearchAsync(string query, int page = 0, string? foodType = null, CancellationToken ct = default)
    {
        var json = await FetchRawJsonAsync(query, page, foodType, ct);
        var result = ParseResponse(json);

        logger.LogInformation("FatSecret parsed {Count}/{Total} items:", result.Foods.Count, result.Total);
        for (var i = 0; i < result.Foods.Count; i++)
        {
            var f = result.Foods[i];
            logger.LogInformation("  [{I}] {Name} | brand={Brand} | type={FoodType} | cal={Calories} | carbs={Carbs}g | XE={BreadUnits}",
                i, f.Name, f.Brand ?? "-", f.FoodType ?? "-", f.Calories, f.Carbs, f.BreadUnits);
        }

        return result;
    }

    public async Task<string> GetRawJsonAsync(string query, int page = 0, CancellationToken ct = default)
        => await FetchRawJsonAsync(query, page, null, ct);

    private async Task<string> FetchRawJsonAsync(string query, int page, string? foodType, CancellationToken ct)
    {
        var token = await tokenService.GetAccessTokenAsync(ct);
        var client = httpClientFactory.CreateClient("FatSecretApi");

        var qs = new Dictionary<string, string>
        {
            ["search_expression"] = query,
            ["language"]          = "en",
            ["region"]            = "US",
            ["format"]            = "json",
            ["max_results"]       = "20",
            ["page_number"]       = page.ToString(),
            ["flag_default_serving"] = "true"
        };

        if (foodType is "brand" or "generic")
            qs["food_type"] = foodType;

        var queryString = string.Join("&", qs.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}?{queryString}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"FatSecret search failed ({(int)response.StatusCode}): {error}");
        }

        return await response.Content.ReadAsStringAsync(ct);
    }

    private static FoodSearchResponse ParseResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var items = new List<FoodItem>();
        var total = 0;

        CollectFoodItems(root, items, ref total);

        return new FoodSearchResponse(items, total);
    }

    private static void CollectFoodItems(JsonElement el, List<FoodItem> items, ref int total)
    {
        if (el.ValueKind == JsonValueKind.Object)
        {
            // Extract total_results wherever it appears
            if (total == 0 && el.TryGetProperty("total_results", out var totalEl))
                int.TryParse(totalEl.ValueKind == JsonValueKind.String
                    ? totalEl.GetString()
                    : totalEl.GetRawText(), out total);

            // If this object looks like a food item, parse it directly
            if (el.TryGetProperty("food_name", out _))
            {
                var item = ParseFoodItem(el);
                if (item is not null) items.Add(item);
                return;
            }

            // Otherwise recurse into all properties
            foreach (var prop in el.EnumerateObject())
                CollectFoodItems(prop.Value, items, ref total);
        }
        else if (el.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in el.EnumerateArray())
                CollectFoodItems(child, items, ref total);
        }
    }

    private static FoodItem? ParseFoodItem(JsonElement el)
    {
        var name = el.TryGetProperty("food_name", out var n) ? n.GetString() : null;
        if (name is null) return null;

        var brand    = el.TryGetProperty("brand_name", out var b) ? b.GetString() : null;
        var foodType = el.TryGetProperty("food_type",  out var ft) ? ft.GetString() : null;

        float calories = 0, carbs = 0;
        string? servingDescription = null;

        if (el.TryGetProperty("servings", out var servings) &&
            servings.TryGetProperty("serving", out var servingEl))
        {
            var serving = PickDefaultServing(servingEl);
            calories           = ParseFloat(serving, "calories");
            carbs              = ParseFloat(serving, "carbohydrate");
            servingDescription = serving.TryGetProperty("serving_description", out var sd) ? sd.GetString() : null;
        }
        else if (el.TryGetProperty("food_description", out var descEl))
        {
            var desc = descEl.GetString() ?? string.Empty;
            calories = ExtractFloat(desc, "Calories:", "kcal");
            carbs    = ExtractFloat(desc, "Carbs:", "g");
        }

        return new FoodItem(
            Name:               name,
            Calories:           (int)Math.Round(calories),
            Carbs:              MathF.Round(carbs, 1),
            BreadUnits:         MathF.Round(carbs / CarbsPerBreadUnit, 2),
            Brand:              brand,
            FoodType:           foodType,
            ServingDescription: servingDescription);
    }

    private static JsonElement PickDefaultServing(JsonElement servingEl)
    {
        if (servingEl.ValueKind == JsonValueKind.Object) return servingEl;

        JsonElement? first = null;
        foreach (var s in servingEl.EnumerateArray())
        {
            first ??= s;
            if (s.TryGetProperty("is_default", out var def) && def.GetString() == "1")
                return s;
        }
        return first ?? servingEl[0];
    }

    private static float ParseFloat(JsonElement el, string property)
    {
        if (!el.TryGetProperty(property, out var prop)) return 0f;
        var str = prop.ValueKind == JsonValueKind.String ? prop.GetString() : prop.GetRawText();
        return float.TryParse(str, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0f;
    }

    private static float ExtractFloat(string text, string startMarker, string endMarker)
    {
        var start = text.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return 0f;
        start += startMarker.Length;
        var trimmed = text[start..].TrimStart();
        var end = trimmed.IndexOf(endMarker, StringComparison.OrdinalIgnoreCase);
        if (end < 0) return 0f;
        return float.TryParse(trimmed[..end].Trim(), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0f;
    }
}
