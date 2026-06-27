using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Ahp;

namespace IoTRecommendation.Core.Algorithms.Ahp;

/// <summary>
/// AHP calculator supporting multiple experts.
///
/// Algorithm:
///   1. Validate each expert matrix.
///   2. Aggregate via element-wise geometric mean (Saaty, 1980).
///   3. Compute priority vector via power iteration (more numerically
///      stable than direct eigenvalue decomposition for thesis use).
///   4. Compute λmax, CI, RI, CR.
/// </summary>
public sealed class AhpCalculator : IAhpCalculator
{
    // Saaty's RI table for n = 1..10 (index = n-1)
    private static readonly double[] RiTable = { 0.0, 0.0, 0.58, 0.90, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49 };
    private const double ConsistencyThreshold = 0.10;

    public AhpResult Calculate(IReadOnlyList<Expert> experts, IReadOnlyList<CriterionDefinition> criteria)
    {
        if (experts.Count == 0)
            throw new ArgumentException("At least one expert is required.", nameof(experts));

        var topsisKeys = criteria.Where(c => c.UsedInTopsis).Select(c => c.Key).ToList();
        int n = topsisKeys.Count;

        ValidateExpertMatrices(experts, n);

        // Compute individual CR values for transparency
        var expertConsistencies = experts.Select(e =>
        {
            var (w, lm) = ComputePriorityVector(e.Matrix);
            double cr = ComputeCR(lm, n);
            return new ExpertConsistency
            {
                ExpertId = e.ExpertId,
                ExpertName = e.ExpertName,
                ConsistencyRatio = cr,
                IsConsistent = cr <= ConsistencyThreshold
            };
        }).ToList();

        double[][] aggregated = AggregateByGeometricMean(experts.Select(e => e.Matrix).ToList(), n);

        return BuildResult(aggregated, topsisKeys, expertConsistencies);
    }

    public AhpResult CalculateFromMatrix(double[][] matrix, IReadOnlyList<CriterionDefinition> criteria)
    {
        var topsisKeys = criteria.Where(c => c.UsedInTopsis).Select(c => c.Key).ToList();
        return BuildResult(matrix, topsisKeys, new List<ExpertConsistency>());
    }

    // ──────────────────────────────────────────────────────── Aggregation

    private static double[][] AggregateByGeometricMean(IList<double[][]> matrices, int n)
    {
        int count = matrices.Count;
        double[][] agg = Alloc(n);

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                if (i == j) { agg[i][j] = 1.0; continue; }
                double product = 1.0;
                foreach (var m in matrices) product *= m[i][j];
                agg[i][j] = Math.Pow(product, 1.0 / count);
            }
        return agg;
    }

    // ──────────────────────────────────────────────────────── Priority vector

    /// <summary>
    /// Power iteration until convergence — O(n²·iter) and numerically robust.
    /// </summary>
    private static (double[] weights, double lambdaMax) ComputePriorityVector(double[][] matrix)
    {
        int n = matrix.Length;
        double[] w = Enumerable.Repeat(1.0 / n, n).ToArray();

        for (int iter = 0; iter < 1000; iter++)
        {
            double[] wNew = new double[n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    wNew[i] += matrix[i][j] * w[j];

            double sum = wNew.Sum();
            if (sum <= 0 || !double.IsFinite(sum)) break;
            for (int i = 0; i < n; i++) wNew[i] /= sum;

            double maxDiff = w.Zip(wNew).Max(p => Math.Abs(p.First - p.Second));
            Array.Copy(wNew, w, n);
            if (maxDiff < 1e-12) break;
        }

        // λmax = mean of (Aw)_i / w_i
        double lambdaMax = 0;
        for (int i = 0; i < n; i++)
        {
            double aw = 0;
            for (int j = 0; j < n; j++) aw += matrix[i][j] * w[j];
            if (w[i] > 1e-15) lambdaMax += aw / w[i];
        }
        lambdaMax /= n;
        return (w, lambdaMax);
    }

    // ──────────────────────────────────────────────────────── Consistency

    private static double ComputeCR(double lambdaMax, int n)
    {
        if (n <= 2) return 0;
        double ci = (lambdaMax - n) / (n - 1);
        double ri = n <= RiTable.Length ? RiTable[n - 1] : 1.49;
        double cr = ri > 0 ? ci / ri : 0;
        return double.IsFinite(cr) && cr >= 0 ? cr : 0;
    }

    // ──────────────────────────────────────────────────────── Result building

    private static AhpResult BuildResult(
        double[][] aggregated,
        List<string> criterionKeys,
        List<ExpertConsistency> expertConsistencies)
    {
        int n = aggregated.Length;
        var (weights, lambdaMax) = ComputePriorityVector(aggregated);
        double ci = n > 2 ? (lambdaMax - n) / (n - 1) : 0;
        double ri = n <= RiTable.Length ? RiTable[n - 1] : 1.49;
        double cr = ri > 0 ? ci / ri : 0;
        if (!double.IsFinite(cr) || cr < 0) cr = 0;

        var weightDict = new Dictionary<string, double>();
        for (int i = 0; i < Math.Min(weights.Length, criterionKeys.Count); i++)
            weightDict[criterionKeys[i]] = weights[i];

        return new AhpResult
        {
            Weights = weightDict,
            LambdaMax = lambdaMax,
            ConsistencyIndex = ci,
            RandomIndex = ri,
            ConsistencyRatio = cr,
            IsConsistent = cr <= ConsistencyThreshold,
            ExpertConsistencies = expertConsistencies,
            AggregatedMatrix = aggregated
        };
    }

    // ──────────────────────────────────────────────────────── Validation

    private static void ValidateExpertMatrices(IReadOnlyList<Expert> experts, int expectedN)
    {
        foreach (var expert in experts)
        {
            if (expert.Matrix.Length != expectedN)
                throw new InvalidOperationException(
                    $"Expert '{expert.ExpertId}' matrix has {expert.Matrix.Length} rows; expected {expectedN}.");
            foreach (var row in expert.Matrix)
                if (row.Length != expectedN)
                    throw new InvalidOperationException(
                        $"Expert '{expert.ExpertId}' has a non-square matrix row.");
        }
    }

    private static double[][] Alloc(int n)
    {
        var m = new double[n][];
        for (int i = 0; i < n; i++) m[i] = new double[n];
        return m;
    }
}
