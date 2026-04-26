using System.Net.Http.Headers;
using System.Text.Json;
using GlucoKids.Models;

namespace GlucoKids.Services;

public class FatSecretFoodService(IHttpClientFactory httpClientFactory, FatSecretTokenService tokenService)
{
    private const float CarbsPerBreadUnit = 12f;
    private const string ApiUrl = "https://platform.fatsecret.com/rest/server.api";

    public async Task<FoodSearchResponse> SearchAsync(string query, int page = 0, CancellationToken ct = default)
    {
        var token = await tokenService.GetAccessTokenAsync(ct);
        var client = httpClientFactory.CreateClient("FatSecretApi");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var body = new Dictionary<string, string>
        {
            ["method"] = "foods.search.v3",
            ["search_expression"] = query,
            ["language"] = "uk",
            ["region"] = "UA",
            ["format"] = "json",
            ["max_results"] = "20",
            ["page_number"] = page.ToString()
        };

        var response = await client.PostAsync(ApiUrl, new FormUrlEncodedContent(body), ct);

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

        // FatSecret wraps everything under "foods"
        if (!root.TryGetProperty("foods", out var foodsNode))
            return new FoodSearchResponse([], 0);

        int total = 0;
        if (foodsNode.TryGetProperty("total_results", out var totalEl))
            int.TryParse(totalEl.GetString(), out total);

        var items = new List<FoodItem>();

        if (!foodsNode.TryGetProperty("food", out var foodArray))
            return new FoodSearchResponse(items, total);

        // "food" can be a single object (1 result) or an array (multiple results)
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

        float calories = 0, carbs = 0;

        // Structured nutrients are inside servings.serving (first serving = per 100g)
        if (el.TryGetProperty("servings", out var servings) &&
            servings.TryGetProperty("serving", out var servingEl))
        {
            // "serving" can be array or single object
            var serving = servingEl.ValueKind == JsonValueKind.Array
                ? servingEl[0]
                : servingEl;

            calories = ParseDecimal(serving, "calories");
            carbs = ParseDecimal(serving, "carbohydrate");
        }
        else if (el.TryGetProperty("food_description", out var descEl))
        {
            // Fallback: parse legacy description string
            // "Per 100g - Calories: 264kcal | Fat: 1.00g | Carbs: 53.00g | Protein: 9.00g"
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
            Brand: brand);
    }

    private static float ParseDecimal(JsonElement el, string property)
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
