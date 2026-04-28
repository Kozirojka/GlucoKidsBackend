using GlucoKids.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient("FatSecretToken");
builder.Services.AddHttpClient("FatSecretApi");

builder.Services.AddSingleton<FatSecretTokenService>();
builder.Services.AddSingleton<FatSecretFoodService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.MapGet("/api/food/search", async (
    string q,
    FatSecretFoodService foodService,
    CancellationToken ct,
    int page = 0,
    string? foodType = null) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest("Query parameter 'q' is required.");

    if (page < 0)
        return Results.BadRequest("Page must be >= 0.");

    try
    {
        var result = await foodService.SearchAsync(q, page, foodType, ct);
        return Results.Ok(result);
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway,
            title: "FatSecret API error");
    }
})
.WithName("SearchFood")
.WithOpenApi()
.WithSummary("Search food by name (v5). Optional foodType: 'brand' | 'generic'");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("Health")
   .ExcludeFromDescription();

app.Run();
