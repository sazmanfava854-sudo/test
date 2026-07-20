using HRPerformance.Application.Interfaces;
using HRPerformance.Infrastructure.Data;
using HRPerformance.Infrastructure.Services;

namespace HRPerformance.Infrastructure.Services;

public class EvaluationCategorySeedService : IEvaluationCategorySeedService
{
    private readonly ApplicationDbContext _context;

    public EvaluationCategorySeedService(ApplicationDbContext context) => _context = context;

    public Task EnsureSeededAsync(Guid organizationId, CancellationToken ct = default) =>
        EvaluationCategoryBootstrap.EnsureForOrganizationAsync(_context, organizationId, ct);
}
