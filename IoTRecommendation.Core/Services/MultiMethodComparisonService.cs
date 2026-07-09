using IoTRecommendation.Core.Models.Comparison;
using IoTRecommendation.Core.Models.Copras;
using IoTRecommendation.Core.Models.Topsis;
using IoTRecommendation.Core.Models.Vikor;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Compares the rankings produced by TOPSIS, VIKOR, and COPRAS for the same
/// technology set, weights, and cluster, and computes all three pairwise
/// Spearman rank-correlation coefficients. Stateless — pure post-processing
/// of three results already produced by the pipeline, so it has no
/// repository dependencies. Complements (does not replace)
/// <see cref="RankingComparisonService"/>, which remains the dedicated
/// two-method TOPSIS/VIKOR comparison.
/// </summary>
public sealed class MultiMethodComparisonService
{
    public MultiMethodComparisonResult Compare(
        TopsisResult topsisResult,
        VikorResult vikorResult,
        CoprasResult coprasResult)
    {
        var topsisByTech = topsisResult.Ranking.ToDictionary(e => e.TechnologyId);
        var vikorByTech = vikorResult.Ranking.ToDictionary(e => e.TechnologyId);
        var coprasByTech = coprasResult.Ranking.ToDictionary(e => e.TechnologyId);

        var allIds = topsisByTech.Keys
            .Union(vikorByTech.Keys)
            .Union(coprasByTech.Keys)
            .ToList();
        if (allIds.Count == 0)
            throw new InvalidOperationException("Cannot compare empty rankings.");

        var entries = new List<MultiMethodComparisonEntry>();
        foreach (var id in allIds)
        {
            topsisByTech.TryGetValue(id, out var t);
            vikorByTech.TryGetValue(id, out var v);
            coprasByTech.TryGetValue(id, out var c);

            entries.Add(new MultiMethodComparisonEntry
            {
                TechnologyId = id,
                TechnologyName = t?.TechnologyName ?? v?.TechnologyName ?? c?.TechnologyName ?? id,
                TopsisRank = t?.Rank ?? 0,
                TopsisScore = t?.ClosenessCoefficient ?? 0,
                VikorRank = v?.Rank ?? 0,
                VikorScore = v?.Q ?? 0,
                CoprasRank = c?.Rank ?? 0,
                CoprasScore = c?.Q ?? 0
            });
        }

        entries = entries.OrderBy(e => e.TopsisRank).ToList();

        return new MultiMethodComparisonResult
        {
            Entries = entries,
            TopsisWinner = topsisResult.WinnerName,
            VikorWinner = vikorResult.WinnerName,
            CoprasWinner = coprasResult.WinnerName,
            AllWinnersAgree =
                string.Equals(topsisResult.WinnerName, vikorResult.WinnerName, StringComparison.Ordinal) &&
                string.Equals(vikorResult.WinnerName, coprasResult.WinnerName, StringComparison.Ordinal),
            SpearmanTopsisVikor = ComputeSpearman(entries, e => e.TopsisRank, e => e.VikorRank),
            SpearmanTopsisCopras = ComputeSpearman(entries, e => e.TopsisRank, e => e.CoprasRank),
            SpearmanVikorCopras = ComputeSpearman(entries, e => e.VikorRank, e => e.CoprasRank)
        };
    }

    /// <summary>
    /// ρ = 1 − 6·Σd² / (n·(n²−1)), where d is the per-item difference between
    /// the two supplied rank selectors. Assumes no rank ties, which holds here
    /// because each algorithm produces a strict total order over the same
    /// technology set.
    /// </summary>
    private static double ComputeSpearman(
        List<MultiMethodComparisonEntry> entries,
        Func<MultiMethodComparisonEntry, int> rankA,
        Func<MultiMethodComparisonEntry, int> rankB)
    {
        int n = entries.Count;
        if (n < 2) return 1.0;

        double sumSquaredDiff = entries.Sum(e => Math.Pow(rankA(e) - rankB(e), 2));
        return 1.0 - (6.0 * sumSquaredDiff) / (n * ((double)n * n - 1));
    }
}
