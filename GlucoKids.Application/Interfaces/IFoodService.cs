using GlucoKids.Application.DTOs;

namespace GlucoKids.Application.Interfaces;

public interface IFoodService
{
    Task<FoodSearchResponse> SearchAsync(string query, int page = 0, string? foodType = null, CancellationToken ct = default);
    Task<string> GetRawJsonAsync(string query, int page = 0, CancellationToken ct = default);
}
