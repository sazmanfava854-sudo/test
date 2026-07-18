using HRPerformance.Domain.Entities;
using HRPerformance.Domain.Enums;
namespace HRPerformance.Domain.Interfaces;
public interface IRankingService
{
    Task CalculateRankingsAsync(Guid organizationId, int year, int? month = null, CancellationToken ct = default);
}
