using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Copras;
using IoTRecommendation.Core.Models.Enums;

namespace IoTRecommendation.Core.Algorithms.Copras;

/// <summary>
/// Classic COPRAS — COmplex PRoportional ASsessment (Zavadskas &amp; Kaklauskas,
/// 1996; Zavadskas et al., 2008).
///
/// Steps:
///   1. Build decision matrix (m technologies × n criteria).
///   2. Normalise each column by its sum: r_ij = x_ij / Σ_k x_kj.
///   3. Apply weights: d_ij = r_ij · w_j.
///   4. Split criteria into benefit and cost groups; sum the weighted normalised
///      values per alternative into S+ᵢ (benefit) and S−ᵢ (cost).
///   5. Compute the relative significance:
///      Qᵢ = S+ᵢ + (S−min · Σ_k S−k) / (S−ᵢ · Σ_k (S−min / S−k)).
///   6. Compute the utility degree Nᵢ = (Qᵢ / Qmax) · 100%.
///   7. Rank by Qᵢ descending — the largest Qᵢ is the best alternative.
///
/// Unlike TOPSIS (vector normalisation, Euclidean distance to ideal points) and
/// VIKOR (linear min-max normalisation, S/R compromise measures), COPRAS uses
/// sum normalisation and a proportional significance index — a third,
/// independent formulation useful for cross-checking robustness.
///
/// Replacement: implement ICoprasCalculator and swap in DI.
/// </summary>
public sealed class CoprasCalculator : ICoprasCalculator
{
    /// <summary>
    /// Floor applied to a degenerate zero S− (e.g. no cost criteria defined, or
    /// all cost values happen to be zero for an alternative) to avoid division
    /// by zero in the Qᵢ correction term.
    /// </summary>
    private const double SMinusFloor = 1e-9;

    public CoprasResult Calculate(
        IReadOnlyList<Technology> technologies,
        IReadOnlyDictionary<string, double> weights,
        IReadOnlyList<CriterionDefinition> criteria)
    {
        var keys = criteria.Where(c => c.UsedInTopsis).Select(c => c.Key).ToList();
        int m = technologies.Count;
        int n = keys.Count;

        if (m == 0) throw new InvalidOperationException("No technologies in selected cluster.");
        if (n == 0) throw new InvalidOperationException("No COPRAS criteria defined.");

        // Step 1: Build raw matrix
        double[][] raw = new double[m][];
        for (int i = 0; i < m; i++)
        {
            raw[i] = new double[n];
            for (int j = 0; j < n; j++)
                raw[i][j] = technologies[i].Criteria.GetValueOrDefault(keys[j], 0.0);
        }

        // Step 2: Sum normalisation per column
        double[] colSums = new double[n];
        for (int j = 0; j < n; j++)
            for (int i = 0; i < m; i++)
                colSums[j] += raw[i][j];

        double[][] norm = new double[m][];
        for (int i = 0; i < m; i++)
        {
            norm[i] = new double[n];
            for (int j = 0; j < n; j++)
                norm[i][j] = colSums[j] != 0 ? raw[i][j] / colSums[j] : 0;
        }

        // Step 3: Weighted normalised values
        double[] w = keys.Select(k => weights.GetValueOrDefault(k, 0.0)).ToArray();
        double[][] weighted = new double[m][];
        for (int i = 0; i < m; i++)
        {
            weighted[i] = new double[n];
            for (int j = 0; j < n; j++)
                weighted[i][j] = norm[i][j] * w[j];
        }

        // Step 4: S+ (benefit) and S- (cost) sums per alternative
        bool[] isBenefit = keys
            .Select(k => criteria.First(c => c.Key == k).Type == CriterionType.Benefit)
            .ToArray();

        double[] sPlus = new double[m];
        double[] sMinus = new double[m];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (isBenefit[j]) sPlus[i] += weighted[i][j];
                else sMinus[i] += weighted[i][j];
            }
            if (sMinus[i] <= 0) sMinus[i] = SMinusFloor;
        }

        // Step 5: Relative significance Qᵢ
        double sMinusMin = sMinus.Min();
        double sMinusTotal = sMinus.Sum();
        double reciprocalSum = sMinus.Sum(s => sMinusMin / s);

        var entries = new List<CoprasRankEntry>();
        for (int i = 0; i < m; i++)
        {
            double correction = reciprocalSum != 0
                ? (sMinusMin * sMinusTotal) / (sMinus[i] * reciprocalSum)
                : 0;
            double q = sPlus[i] + correction;

            entries.Add(new CoprasRankEntry
            {
                TechnologyId = technologies[i].Id,
                TechnologyName = technologies[i].Name,
                SPlus = sPlus[i],
                SMinus = sMinus[i],
                Q = q
            });
        }

        // Step 6: Utility degree Nᵢ (%)
        double qMax = entries.Count > 0 ? entries.Max(e => e.Q) : 0;
        foreach (var entry in entries)
            entry.N = qMax != 0 ? entry.Q / qMax * 100.0 : 0;

        // Step 7: Rank descending by Qᵢ — higher Qᵢ is better
        entries = entries.OrderByDescending(e => e.Q).ToList();
        for (int i = 0; i < entries.Count; i++)
            entries[i].Rank = i + 1;

        return new CoprasResult
        {
            Ranking = entries,
            WeightsUsed = weights.ToDictionary(k => k.Key, k => k.Value),
            WinnerName = entries.FirstOrDefault()?.TechnologyName ?? string.Empty
        };
    }
}
