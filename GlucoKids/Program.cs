using GlucoKids.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient factory — two named clients for clarity
builder.Services.AddHttpClient("FatSecretToken");
builder.Services.AddHttpClient("FatSecretApi");

// FatSecret services (singleton so token cache lives for the lifetime of the app)
builder.Services.AddSingleton<FatSecretTokenService>();
builder.Services.AddSingleton<FatSecretFoodService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ── Food search proxy ──────────────────────────────────────────────────────────
app.MapGet("/api/food/search", async (
    string q,
    int page,
    FatSecretFoodService foodService,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(q))
        return Results.BadRequest("Query parameter 'q' is required.");

    try
    {
        var result = await foodService.SearchAsync(q, page, ct);
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
.WithSummary("Search food by name and return nutritional info including bread units");

app.Run();
