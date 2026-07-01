using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Enums;
using IoTRecommendation.Core.Models.Vikor;

namespace IoTRecommendation.Core.Algorithms.Vikor;

/// <summary>
/// Classic VIKOR (Opricovic, 1998; Opricovic &amp; Tzeng, 2004; 2007).
///
/// Steps:
///   1. Build decision matrix (m technologies × n criteria).
///   2. Determine best f*_j and worst f-_j per criterion (respecting benefit/cost).
///   3. Compute per-alternative utility measure S_i (weighted sum of normalised
///      distances from the best value) and regret measure R_i (max weighted
///      normalised distance from the best value).
///   4. Compute the VIKOR index Q_i = v·(S_i−S*)/(S⁻−S*) + (1−v)·(R_i−R*)/(R⁻−R*).
///   5. Rank by Q ascending — the smallest Q is the compromise solution.
///   6. Verify the two compromise-solution conditions (C1 acceptable advantage,
///      C2 acceptable stability) and build the compromise set accordingly.
///
/// Replacement: implement IVikorCalculator (e.g. a fuzzy VIKOR variant) and swap in DI.
/// </summary>
public sealed class VikorCalculator : IVikorCalculator
{
    public VikorResult Calculate(
        IReadOnlyList<Technology> technologies,
        IReadOnlyDictionary<string, double> weights,
        IReadOnlyList<CriterionDefinition> criteria,
        double v)
    {
        if (v < 0 || v > 1)
            throw new ArgumentOutOfRangeException(nameof(v), "v must be in the range [0, 1].");

        var keys = criteria.Where(c => c.UsedInTopsis).Select(c => c.Key).ToList();
        int m = technologies.Count;
        int n = keys.Count;

        if (m == 0) throw new InvalidOperationException("No technologies in selected cluster.");
        if (n == 0) throw new InvalidOperationException("No VIKOR criteria defined.");

        // Step 1: Build raw matrix
        double[][] raw = new double[m][];
        for (int i = 0; i < m; i++)
        {
            raw[i] = new double[n];
            for (int j = 0; j < n; j++)
                raw[i][j] = technologies[i].Criteria.GetValueOrDefault(keys[j], 0.0);
        }

        // Step 2: Best/worst per criterion
        double[] best = new double[n];
        double[] worst = new double[n];
        for (int j = 0; j < n; j++)
        {
            var col = raw.Select(row => row[j]).ToArray();
            bool isBenefit = criteria.First(c => c.Key == keys[j]).Type == CriterionType.Benefit;
            best[j] = isBenefit ? col.Max() : col.Min();
            worst[j] = isBenefit ? col.Min() : col.Max();
        }

        // Step 3: Utility measure S_i and regret measure R_i
        double[] w = keys.Select(k => weights.GetValueOrDefault(k, 0.0)).ToArray();
        double[] s = new double[m];
        double[] r = new double[m];
        for (int i = 0; i < m; i++)
        {
            double sSum = 0, rMax = 0;
            for (int j = 0; j < n; j++)
            {
                double range = best[j] - worst[j];
                double normalized = range != 0 ? w[j] * (best[j] - raw[i][j]) / range : 0;
                sSum += normalized;
                rMax = Math.Max(rMax, normalized);
            }
            s[i] = sSum;
            r[i] = rMax;
        }

        // Step 4: VIKOR index Q_i
        double sBest = s.Min(), sWorst = s.Max();
        double rBest = r.Min(), rWorst = r.Max();
        double sRange = sWorst - sBest;
        double rRange = rWorst - rBest;

        var entries = new List<VikorRankEntry>();
        for (int i = 0; i < m; i++)
        {
            double sTerm = sRange != 0 ? (s[i] - sBest) / sRange : 0;
            double rTerm = rRange != 0 ? (r[i] - rBest) / rRange : 0;
            double q = v * sTerm + (1 - v) * rTerm;

            entries.Add(new VikorRankEntry
            {
                TechnologyId = technologies[i].Id,
                TechnologyName = technologies[i].Name,
                S = s[i],
                R = r[i],
                Q = q
            });
        }

        // Step 5: Rank ascending by Q (lower Q = closer to the ideal)
        entries = entries.OrderBy(e => e.Q).ToList();
        for (int i = 0; i < entries.Count; i++)
            entries[i].Rank = i + 1;

        // Step 6: Compromise-solution conditions (Opricovic & Tzeng, 2004; 2007)
        var (acceptableAdvantage, acceptableStability, compromiseSet) =
            EvaluateCompromiseConditions(entries, technologies, s, r);

        return new VikorResult
        {
            Ranking = entries,
            V = v,
            WeightsUsed = weights.ToDictionary(k => k.Key, k => k.Value),
            WinnerName = entries.FirstOrDefault()?.TechnologyName ?? string.Empty,
            AcceptableAdvantage = acceptableAdvantage,
            AcceptableStability = acceptableStability,
            CompromiseSet = compromiseSet
        };
    }

    /// <summary>
    /// Checks the two conditions that determine whether the top-ranked
    /// alternative is a stable compromise solution, and builds the compromise
    /// set accordingly (Opricovic &amp; Tzeng, 2004; 2007).
    /// </summary>
    private static (bool acceptableAdvantage, bool acceptableStability, List<string> compromiseSet)
        EvaluateCompromiseConditions(
            List<VikorRankEntry> rankedByQ,
            IReadOnlyList<Technology> technologies,
            double[] s,
            double[] r)
    {
        int m = rankedByQ.Count;
        if (m < 2)
            return (true, true, rankedByQ.Select(e => e.TechnologyId).ToList());

        var first = rankedByQ[0];

        // C1 — acceptable advantage: the gap between the best and second-best Q
        // must be at least DQ = 1/(m−1).
        double dq = 1.0 / (m - 1);
        double advantageGap = rankedByQ[1].Q - first.Q;
        bool acceptableAdvantage = advantageGap >= dq;

        // C2 — acceptable stability: the top-Q alternative must also be the best
        // (minimum) alternative when ranked by S alone or by R alone.
        int bestSIndex = Array.IndexOf(s, s.Min());
        int bestRIndex = Array.IndexOf(r, r.Min());
        bool acceptableStability =
            technologies[bestSIndex].Id == first.TechnologyId ||
            technologies[bestRIndex].Id == first.TechnologyId;

        List<string> compromiseSet;
        if (acceptableAdvantage && acceptableStability)
        {
            compromiseSet = [first.TechnologyId];
        }
        else if (!acceptableAdvantage)
        {
            // Include every alternative within DQ of the best Q.
            compromiseSet = rankedByQ
                .Where(e => e.Q - first.Q < dq)
                .Select(e => e.TechnologyId)
                .ToList();
        }
        else
        {
            // C2 fails alone: propose the top two alternatives as compromise solutions.
            compromiseSet = rankedByQ.Take(2).Select(e => e.TechnologyId).ToList();
        }

        return (acceptableAdvantage, acceptableStability, compromiseSet);
    }
}
