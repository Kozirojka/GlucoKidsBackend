using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using GlucoKids.Application.Interfaces;
using GlucoKids.Infrastructure.Data;
using GlucoKids.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.FormatterName = "simple");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddHttpClient("FatSecretToken");
builder.Services.AddHttpClient("FatSecretApi");
builder.Services.AddHttpClient("GoogleAuth");

builder.Services.AddSingleton<FatSecretTokenService>();
builder.Services.AddSingleton<IFoodService, FatSecretFoodService>();
builder.Services.AddSingleton<IFirebaseTokenService, FirebaseTokenService>();
builder.Services.AddSingleton<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddSingleton<IFirebaseAdminService, FirebaseAdminService>();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Firebase Admin SDK — ініціалізація один раз
var serviceAccountJson = builder.Configuration["Firebase:ServiceAccountJson"];
var serviceAccountPath = builder.Configuration["Firebase:ServiceAccountPath"];

GoogleCredential credential;
if (!string.IsNullOrWhiteSpace(serviceAccountJson))
    credential = GoogleCredential.FromJson(serviceAccountJson);
else if (!string.IsNullOrWhiteSpace(serviceAccountPath) && File.Exists(serviceAccountPath))
    credential = GoogleCredential.FromFile(serviceAccountPath);
else
    throw new InvalidOperationException(
        "Firebase Admin credentials not configured. " +
        "Set Firebase:ServiceAccountJson (env var) or Firebase:ServiceAccountPath (file path).");

FirebaseApp.Create(new AppOptions { Credential = credential });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).ExcludeFromDescription();

app.Run();
