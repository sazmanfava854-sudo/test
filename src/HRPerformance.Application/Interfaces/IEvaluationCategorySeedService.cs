namespace HRPerformance.Application.Interfaces;

public interface IEvaluationCategorySeedService
{
    Task EnsureSeededAsync(Guid organizationId, CancellationToken ct = default);
}
