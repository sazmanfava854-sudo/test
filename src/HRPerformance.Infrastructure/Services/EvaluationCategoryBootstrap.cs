using HRPerformance.Domain.Entities;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services;

public static class EvaluationCategoryBootstrap
{
    private static readonly (string Name, string Description, string Color, string Icon, decimal Weight)[] DefaultCategories =
    {
        ("کیفیت کار", "کیفیت انجام وظایف", "#4CAF50", "star", 25),
        ("حضور و غیاب", "حضور به‌موقع و مشارکت", "#2196F3", "event", 25),
        ("انضباط", "رعایت مقررات سازمانی", "#FF9800", "gavel", 20),
        ("کار تیمی", "همکاری با همکاران", "#9C27B0", "groups", 15),
        ("بهره‌وری", "میزان خروجی و اثربخشی", "#F44336", "trending_up", 15)
    };

    public static async Task EnsureForAllOrganizationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("EvaluationCategoryBootstrap");

        List<Guid> orgIds;
        try
        {
            orgIds = await context.Organizations.Select(o => o.Id).ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load organizations for evaluation category bootstrap.");
            return;
        }

        foreach (var orgId in orgIds)
            await EnsureForOrganizationAsync(context, orgId, logger);
    }

    public static async Task EnsureForOrganizationAsync(
        ApplicationDbContext context,
        Guid organizationId,
        CancellationToken ct = default)
    {
        if (organizationId == Guid.Empty)
            return;

        await EnsureForOrganizationAsync(context, organizationId, null, ct);
    }

    private static async Task EnsureForOrganizationAsync(
        ApplicationDbContext context,
        Guid organizationId,
        ILogger? logger,
        CancellationToken ct = default)
    {
        if (await context.EvaluationCategories.AnyAsync(
                c => c.OrganizationId == organizationId && !c.IsDeleted, ct))
            return;

        var sortOrder = 0;
        foreach (var (name, description, color, icon, weight) in DefaultCategories)
        {
            context.EvaluationCategories.Add(new EvaluationCategory
            {
                OrganizationId = organizationId,
                Name = name,
                Description = description,
                Color = color,
                Icon = icon,
                Weight = weight,
                SortOrder = sortOrder++,
                IsActive = true
            });
        }

        await context.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded {Count} evaluation categories for organization {OrgId}",
            DefaultCategories.Length, organizationId);
    }
}
