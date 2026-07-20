using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services;

public static class AttendanceRuleBootstrap
{
    private static readonly (string Name, string Description, RuleOperator Operator, decimal? Min, decimal? Max, decimal Impact, int Priority)[] DefaultDelayRules =
    {
        ("تاخیر تا ۱۰ دقیقه", "در محدوده مجاز — بدون امتیاز منفی", RuleOperator.Between, 0, 10, 0, 1),
        ("تاخیر ۱۱ تا ۳۰ دقیقه", "امتیاز منفی خفیف", RuleOperator.Between, 11, 30, -1, 2),
        ("تاخیر بالای ۳۰ دقیقه", "امتیاز منفی", RuleOperator.GreaterThan, 30, null, -2, 3),
    };

    public static async Task EnsureForAllOrganizationsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AttendanceRuleBootstrap");

        List<Guid> orgIds;
        try
        {
            orgIds = await context.Organizations.Select(o => o.Id).ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not load organizations for delay rule bootstrap.");
            return;
        }

        foreach (var orgId in orgIds)
            await EnsureForOrganizationAsync(context, orgId, logger);
    }

    public static async Task EnsureForOrganizationAsync(
        ApplicationDbContext context,
        Guid organizationId,
        CancellationToken ct = default) =>
        await EnsureForOrganizationAsync(context, organizationId, null, ct);

    private static async Task EnsureForOrganizationAsync(
        ApplicationDbContext context,
        Guid organizationId,
        ILogger? logger,
        CancellationToken ct = default)
    {
        if (organizationId == Guid.Empty)
            return;

        if (await context.EvaluationRules.AnyAsync(
                r => r.OrganizationId == organizationId
                     && r.ConditionType == RuleConditionType.Delay
                     && !r.IsDeleted, ct))
            return;

        foreach (var (name, description, op, min, max, impact, priority) in DefaultDelayRules)
        {
            context.EvaluationRules.Add(new EvaluationRule
            {
                OrganizationId = organizationId,
                Name = name,
                Description = description,
                ConditionType = RuleConditionType.Delay,
                Operator = op,
                MinValue = min,
                MaxValue = max,
                ScoreImpact = impact,
                Priority = priority,
                IsActive = true
            });
        }

        await context.SaveChangesAsync(ct);
        logger?.LogInformation("Seeded delay evaluation rules for organization {OrgId}", organizationId);
    }
}
