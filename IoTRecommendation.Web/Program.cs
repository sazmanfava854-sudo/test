using IoTRecommendation.Core.Algorithms.Ahp;
using IoTRecommendation.Core.Algorithms.AdaptiveWeighting;
using IoTRecommendation.Core.Algorithms.Clustering;
using IoTRecommendation.Core.Algorithms.Copras;
using IoTRecommendation.Core.Algorithms.Topsis;
using IoTRecommendation.Core.Algorithms.Vikor;
using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Services;
using IoTRecommendation.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────────────────
// Services
// ──────────────────────────────────────────────────────────────────────────

builder.Services.AddControllersWithViews();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(60);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
});

// Infrastructure (repositories + config)
builder.Services.AddInfrastructure(builder.Configuration);

// ──────────────────────────────────────────────────────────────────────────
// Algorithm registrations (swap any of these to change the algorithm)
// ──────────────────────────────────────────────────────────────────────────

// K-Means (custom) — swap for any IClusteringAlgorithm implementation
builder.Services.AddSingleton<IClusteringAlgorithm>(sp =>
{
    var settingsRepo = sp.GetRequiredService<ISettingsRepository>();
    var settings = settingsRepo.GetAsync().GetAwaiter().GetResult();
    return new KMeansClusteringAlgorithm(
        settings.KMeansRandomSeed,
        settings.KMeansMaxIterations,
        settings.KMeansRestarts);
});

// AHP (power-iteration eigenvector method)
builder.Services.AddSingleton<IAhpCalculator, AhpCalculator>();

// Adaptive weight formula — swap for IAdaptiveWeightFormula to change formula
builder.Services.AddSingleton<IAdaptiveWeightFormula>(sp =>
{
    var settingsRepo = sp.GetRequiredService<ISettingsRepository>();
    var settings = settingsRepo.GetAsync().GetAwaiter().GetResult();
    return new ExponentialAdaptiveWeightFormula(settings.AdaptiveWeightAlpha);
});

// TOPSIS calculator
builder.Services.AddSingleton<ITopsisCalculator, TopsisCalculator>();

// VIKOR calculator — runs alongside TOPSIS on identical inputs for comparison
builder.Services.AddSingleton<IVikorCalculator, VikorCalculator>();

// COPRAS calculator — runs alongside TOPSIS/VIKOR on identical inputs for comparison
builder.Services.AddSingleton<ICoprasCalculator, CoprasCalculator>();

// ──────────────────────────────────────────────────────────────────────────
// Domain services
// ──────────────────────────────────────────────────────────────────────────

builder.Services.AddScoped<ClusteringService>();
builder.Services.AddScoped<AhpService>();
builder.Services.AddScoped<AdaptiveWeightingService>();
builder.Services.AddScoped<QuestionnaireEligibilityService>();
builder.Services.AddScoped<TopsisService>();
builder.Services.AddScoped<VikorService>();
builder.Services.AddScoped<CoprasService>();
builder.Services.AddScoped<RankingComparisonService>();
builder.Services.AddScoped<MultiMethodComparisonService>();

// ──────────────────────────────────────────────────────────────────────────
// App
// ──────────────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
