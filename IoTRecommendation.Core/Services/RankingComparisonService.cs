using IoTRecommendation.Core.Models.Comparison;
using IoTRecommendation.Core.Models.Topsis;
using IoTRecommendation.Core.Models.Vikor;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Compares the rankings produced by TOPSIS and VIKOR for the same technology
/// set, weights, and cluster. Stateless — pure post-processing of two results
/// already produced by the pipeline, so it has no repository dependencies.
/// </summary>
public sealed class RankingComparisonService
{
    public RankingComparisonResult Compare(TopsisResult topsisResult, VikorResult vikorResult)
    {
        var topsisByTech = topsisResult.Ranking.ToDictionary(e => e.TechnologyId);
        var vikorByTech = vikorResult.Ranking.ToDictionary(e => e.TechnologyId);

        var allIds = topsisByTech.Keys.Union(vikorByTech.Keys).ToList();
        if (allIds.Count == 0)
            throw new InvalidOperationException("Cannot compare empty rankings.");

        var entries = new List<RankingComparisonEntry>();
        foreach (var id in allIds)
        {
            topsisByTech.TryGetValue(id, out var t);
            vikorByTech.TryGetValue(id, out var v);

            entries.Add(new RankingComparisonEntry
            {
                TechnologyId = id,
                TechnologyName = t?.TechnologyName ?? v?.TechnologyName ?? id,
                TopsisRank = t?.Rank ?? 0,
                TopsisCloseness = t?.ClosenessCoefficient ?? 0,
                VikorRank = v?.Rank ?? 0,
                VikorQ = v?.Q ?? 0
            });
        }

        entries = entries.OrderBy(e => e.TopsisRank).ToList();

        return new RankingComparisonResult
        {
            Entries = entries,
            TopsisWinner = topsisResult.WinnerName,
            VikorWinner = vikorResult.WinnerName,
            WinnersAgree = string.Equals(topsisResult.WinnerName, vikorResult.WinnerName, StringComparison.Ordinal),
            SpearmanRankCorrelation = ComputeSpearmanRankCorrelation(entries)
        };
    }

    /// <summary>
    /// ρ = 1 − 6·Σd² / (n·(n²−1)), where d is the per-item rank difference.
    /// Assumes no rank ties, which holds here because both algorithms produce
    /// strict total orders over the same technology set.
    /// </summary>
    private static double ComputeSpearmanRankCorrelation(List<RankingComparisonEntry> entries)
    {
        int n = entries.Count;
        if (n < 2) return 1.0;

        double sumSquaredDiff = entries.Sum(e => (double)e.RankDifference * e.RankDifference);
        return 1.0 - (6.0 * sumSquaredDiff) / (n * ((double)n * n - 1));
    }
}
