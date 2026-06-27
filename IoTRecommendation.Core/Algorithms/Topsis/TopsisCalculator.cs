using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Enums;
using IoTRecommendation.Core.Models.Topsis;

namespace IoTRecommendation.Core.Algorithms.Topsis;

/// <summary>
/// Classic TOPSIS (Technique for Order of Preference by Similarity to Ideal Solution).
///
/// Steps:
///   1. Build decision matrix (m technologies × n criteria).
///   2. Normalise using vector normalisation.
///   3. Apply weights.
///   4. Determine ideal best (A+) and ideal worst (A−).
///   5. Compute Euclidean distances D+ and D−.
///   6. Compute closeness coefficient C* = D− / (D+ + D−).
///   7. Rank by C* descending.
///
/// Replacement: implement ITopsisCalculator (e.g. VIKOR) and swap in DI.
/// </summary>
public sealed class TopsisCalculator : ITopsisCalculator
{
    public TopsisResult Calculate(
        IReadOnlyList<Technology> technologies,
        IReadOnlyDictionary<string, double> weights,
        IReadOnlyList<CriterionDefinition> criteria)
    {
        var topsisKeys = criteria.Where(c => c.UsedInTopsis).Select(c => c.Key).ToList();
        int m = technologies.Count;
        int n = topsisKeys.Count;

        if (m == 0) throw new InvalidOperationException("No technologies in selected cluster.");
        if (n == 0) throw new InvalidOperationException("No TOPSIS criteria defined.");

        // Step 1: Build raw matrix
        double[][] raw = new double[m][];
        for (int i = 0; i < m; i++)
        {
            raw[i] = new double[n];
            for (int j = 0; j < n; j++)
                raw[i][j] = technologies[i].Criteria.GetValueOrDefault(topsisKeys[j], 0.0);
        }

        // Step 2: Vector normalisation
        double[] colNorms = new double[n];
        for (int j = 0; j < n; j++)
            for (int i = 0; i < m; i++)
                colNorms[j] += raw[i][j] * raw[i][j];
        for (int j = 0; j < n; j++)
            colNorms[j] = Math.Sqrt(colNorms[j]);

        double[][] norm = new double[m][];
        for (int i = 0; i < m; i++)
        {
            norm[i] = new double[n];
            for (int j = 0; j < n; j++)
                norm[i][j] = colNorms[j] > 0 ? raw[i][j] / colNorms[j] : 0;
        }

        // Step 3: Weighted matrix
        double[] w = topsisKeys.Select(k => weights.GetValueOrDefault(k, 0.0)).ToArray();
        double[][] weighted = new double[m][];
        for (int i = 0; i < m; i++)
        {
            weighted[i] = new double[n];
            for (int j = 0; j < n; j++)
                weighted[i][j] = norm[i][j] * w[j];
        }

        // Step 4: Ideal solutions
        double[] best = new double[n];
        double[] worst = new double[n];
        for (int j = 0; j < n; j++)
        {
            var col = weighted.Select(row => row[j]).ToArray();
            bool isBenefit = criteria.First(c => c.Key == topsisKeys[j]).Type == CriterionType.Benefit;
            best[j] = isBenefit ? col.Max() : col.Min();
            worst[j] = isBenefit ? col.Min() : col.Max();
        }

        // Steps 5–6: Distances and closeness
        var ranking = new List<TopsisRankEntry>();
        for (int i = 0; i < m; i++)
        {
            double dBest = 0, dWorst = 0;
            for (int j = 0; j < n; j++)
            {
                dBest += Math.Pow(weighted[i][j] - best[j], 2);
                dWorst += Math.Pow(weighted[i][j] - worst[j], 2);
            }
            dBest = Math.Sqrt(dBest);
            dWorst = Math.Sqrt(dWorst);
            double denom = dBest + dWorst;
            double closeness = denom > 0 ? dWorst / denom : 0;

            ranking.Add(new TopsisRankEntry
            {
                TechnologyId = technologies[i].Id,
                TechnologyName = technologies[i].Name,
                DistanceToBestIdeal = dBest,
                DistanceToWorstIdeal = dWorst,
                ClosenessCoefficient = closeness
            });
        }

        // Step 7: Rank
        ranking = ranking.OrderByDescending(r => r.ClosenessCoefficient).ToList();
        for (int i = 0; i < ranking.Count; i++)
            ranking[i].Rank = i + 1;

        return new TopsisResult
        {
            Ranking = ranking,
            WeightsUsed = weights.ToDictionary(k => k.Key, k => k.Value),
            WinnerName = ranking.FirstOrDefault()?.TechnologyName ?? string.Empty
        };
    }
}
