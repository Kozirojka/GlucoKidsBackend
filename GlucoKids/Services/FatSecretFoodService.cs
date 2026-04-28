using System.Net.Http.Headers;
using System.Text.Json;
using GlucoKids.Models;

namespace GlucoKids.Services;

public class FatSecretFoodService(IHttpClientFactory httpClientFactory, FatSecretTokenService tokenService)
{
    private const float CarbsPerBreadUnit = 12f;
    private const string ApiUrl = "https://platform.fatsecret.com/rest/server.api";

    public async Task<FoodSearchResponse> SearchAsync(string query, int page = 0, string? foodType = null, CancellationToken ct = default)
    {
        var token = await tokenService.GetAccessTokenAsync(ct);
        var client = httpClientFactory.CreateClient("FatSecretApi");

        var body = new Dictionary<string, string>
        {
            ["method"] = "foods.search.v5",
            ["search_expression"] = query,
            ["language"] = "en",
            ["region"] = "US",
            ["format"] = "json",
            ["max_results"] = "20",
            ["page_number"] = page.ToString(),
            ["include_food_attributes"] = "true"
        };

        if (foodType is "brand" or "generic")
            body["food_type"] = foodType;

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
        {
            Content = new FormUrlEncodedContent(body)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"FatSecret search failed ({(int)response.StatusCode}): {error}");
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        return ParseResponse(json);
    }

    private static FoodSearchResponse ParseResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("foods", out var foodsNode))
            return new FoodSearchResponse([], 0);

        int total = 0;
        if (foodsNode.TryGetProperty("total_results", out var totalEl))
            int.TryParse(totalEl.GetString(), out total);

        var items = new List<FoodItem>();

        if (!foodsNode.TryGetProperty("food", out var foodArray))
            return new FoodSearchResponse(items, total);

        if (foodArray.ValueKind == JsonValueKind.Object)
        {
            var item = ParseFoodItem(foodArray);
            if (item is not null) items.Add(item);
        }
        else if (foodArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in foodArray.EnumerateArray())
            {
                var item = ParseFoodItem(el);
                if (item is not null) items.Add(item);
            }
        }

        return new FoodSearchResponse(items, total);
    }

    private static FoodItem? ParseFoodItem(JsonElement el)
    {
        var name = el.TryGetProperty("food_name", out var n) ? n.GetString() : null;
        if (name is null) return null;

        var brand = el.TryGetProperty("brand_name", out var b) ? b.GetString() : null;

        var foodType = el.TryGetProperty("food_type", out var ft) ? ft.GetString() : null;

        float calories = 0, carbs = 0;

        if (el.TryGetProperty("servings", out var servings) &&
            servings.TryGetProperty("serving", out var servingEl))
        {
            var serving = servingEl.ValueKind == JsonValueKind.Array
                ? servingEl[0]
                : servingEl;

            calories = ParseFloat(serving, "calories");
            carbs = ParseFloat(serving, "carbohydrate");
        }
        else if (el.TryGetProperty("food_description", out var descEl))
        {
            var desc = descEl.GetString() ?? string.Empty;
            calories = ExtractFloat(desc, "Calories:", "kcal");
            carbs = ExtractFloat(desc, "Carbs:", "g");
        }

        var breadUnits = carbs / CarbsPerBreadUnit;

        return new FoodItem(
            Name: name,
            Calories: (int)Math.Round(calories),
            Carbs: MathF.Round(carbs, 1),
            BreadUnits: MathF.Round(breadUnits, 2),
            Brand: brand,
            FoodType: foodType);
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
