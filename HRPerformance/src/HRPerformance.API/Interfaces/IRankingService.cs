using HRPerformance.Entities;
using HRPerformance.Enums;
namespace HRPerformance.Interfaces;
public interface IRankingService
{
    Task CalculateRankingsAsync(Guid organizationId, int year, int? month = null, CancellationToken ct = default);
}
