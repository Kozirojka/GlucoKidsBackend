using GlucoKids.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GlucoKids.Controllers;

[ApiController]
[Route("api/food")]
public class FoodController(IFoodService foodService, ILogger<FoodController> logger) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        CancellationToken ct,
        [FromQuery] int page = 0,
        [FromQuery] string? foodType = null)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query parameter 'q' is required.");

        if (page < 0)
            return BadRequest("Page must be >= 0.");

        try
        {
            var result = await foodService.SearchAsync(q, page, foodType, ct);
            logger.LogInformation("Food search q={Query} page={Page} → {Count} results", q, page, result.Foods.Count);
            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            return Problem(detail: ex.Message, statusCode: 502, title: "FatSecret API error");
        }
    }
}
