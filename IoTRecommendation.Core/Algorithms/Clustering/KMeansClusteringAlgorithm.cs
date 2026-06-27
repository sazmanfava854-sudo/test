using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models.Clustering;

namespace IoTRecommendation.Core.Algorithms.Clustering;

/// <summary>
/// K-Means clustering with K-Means++ initialisation, silhouette-based k selection,
/// and elbow (WCSS knee) detection.
///
/// Custom implementation — no ML.NET or Accord dependency — so the algorithm
/// logic is fully transparent and can be inspected for the thesis.
///
/// Replacement: implement IClusteringAlgorithm and register in DI.
/// </summary>
public sealed class KMeansClusteringAlgorithm : IClusteringAlgorithm
{
    private readonly int _randomSeed;
    private readonly int _maxIterations;
    private readonly int _restarts;

    public KMeansClusteringAlgorithm(int randomSeed = 42, int maxIterations = 300, int restarts = 10)
    {
        _randomSeed = randomSeed;
        _maxIterations = maxIterations;
        _restarts = restarts;
    }

    public ClusteringAlgorithmOutput Run(double[][] featureMatrix, int kMin, int kMax)
    {
        if (featureMatrix.Length < kMax)
            kMax = featureMatrix.Length;

        var allMetrics = new List<KMetrics>();
        int? elbowK = null;

        // Evaluate each candidate k
        KMeansRun? bestRun = null;
        double bestScore = double.NegativeInfinity;

        var runsByK = new Dictionary<int, KMeansRun>();
        for (int k = kMin; k <= kMax; k++)
        {
            var run = BestOfRestarts(featureMatrix, k);
            run.SilhouetteScore = ComputeSilhouetteScore(featureMatrix, run.Labels, k);

            allMetrics.Add(new KMetrics
            {
                K = k,
                Inertia = run.Inertia,
                SilhouetteScore = run.SilhouetteScore,
                MinClusterSize = run.Labels.GroupBy(l => l).Min(g => g.Count())
            });
            runsByK[k] = run;
        }

        elbowK = DetectElbow(allMetrics);

        // Select best k using a composite score
        foreach (var (k, run) in runsByK)
        {
            double score = ScoreK(run, k, elbowK, allMetrics.Max(m => m.SilhouetteScore));
            if (score > bestScore)
            {
                bestScore = score;
                bestRun = run;
            }
        }

        bestRun ??= runsByK[kMin];

        string rationale = BuildRationale(bestRun, elbowK, allMetrics);

        return new ClusteringAlgorithmOutput
        {
            Labels = bestRun.Labels,
            ScaledCentroids = bestRun.Centroids,
            OptimalK = bestRun.K,
            ElbowK = elbowK,
            AllKMetrics = allMetrics,
            SelectionRationale = rationale
        };
    }

    // ──────────────────────────────────────────────────────── K-Means core

    private KMeansRun BestOfRestarts(double[][] data, int k)
    {
        KMeansRun? best = null;
        var rng = new Random(_randomSeed);

        for (int r = 0; r < _restarts; r++)
        {
            var run = RunSingleKMeans(data, k, new Random(rng.Next()));
            if (best is null || run.Inertia < best.Inertia)
                best = run;
        }
        return best!;
    }

    private KMeansRun RunSingleKMeans(double[][] data, int k, Random rng)
    {
        double[][] centroids = KMeansPlusPlusInit(data, k, rng);
        int[] labels = new int[data.Length];

        for (int iter = 0; iter < _maxIterations; iter++)
        {
            bool changed = AssignLabels(data, centroids, labels);
            RecomputeCentroids(data, labels, k, centroids);
            if (!changed) break;
        }

        double inertia = ComputeInertia(data, centroids, labels);
        return new KMeansRun { K = k, Labels = labels, Centroids = centroids, Inertia = inertia };
    }

    private static double[][] KMeansPlusPlusInit(double[][] data, int k, Random rng)
    {
        int n = data.Length, p = data[0].Length;
        double[][] centroids = new double[k][];
        centroids[0] = (double[])data[rng.Next(n)].Clone();

        for (int c = 1; c < k; c++)
        {
            double[] d2 = new double[n];
            double total = 0;
            for (int i = 0; i < n; i++)
            {
                double minDist = double.MaxValue;
                for (int j = 0; j < c; j++)
                    minDist = Math.Min(minDist, SquaredEuclidean(data[i], centroids[j]));
                d2[i] = minDist;
                total += minDist;
            }
            double threshold = rng.NextDouble() * total;
            double cumsum = 0;
            int chosen = n - 1;
            for (int i = 0; i < n; i++)
            {
                cumsum += d2[i];
                if (cumsum >= threshold) { chosen = i; break; }
            }
            centroids[c] = (double[])data[chosen].Clone();
        }
        return centroids;
    }

    private static bool AssignLabels(double[][] data, double[][] centroids, int[] labels)
    {
        bool changed = false;
        for (int i = 0; i < data.Length; i++)
        {
            int nearest = 0;
            double minDist = double.MaxValue;
            for (int c = 0; c < centroids.Length; c++)
            {
                double d = SquaredEuclidean(data[i], centroids[c]);
                if (d < minDist) { minDist = d; nearest = c; }
            }
            if (labels[i] != nearest) changed = true;
            labels[i] = nearest;
        }
        return changed;
    }

    private static void RecomputeCentroids(double[][] data, int[] labels, int k, double[][] centroids)
    {
        int p = data[0].Length;
        double[][] sums = Enumerable.Range(0, k).Select(_ => new double[p]).ToArray();
        int[] counts = new int[k];

        for (int i = 0; i < data.Length; i++)
        {
            counts[labels[i]]++;
            for (int j = 0; j < p; j++)
                sums[labels[i]][j] += data[i][j];
        }
        for (int c = 0; c < k; c++)
            if (counts[c] > 0)
                for (int j = 0; j < p; j++)
                    centroids[c][j] = sums[c][j] / counts[c];
    }

    // ──────────────────────────────────────────────────────── Metrics

    private static double ComputeInertia(double[][] data, double[][] centroids, int[] labels)
    {
        double sum = 0;
        for (int i = 0; i < data.Length; i++)
            sum += SquaredEuclidean(data[i], centroids[labels[i]]);
        return sum;
    }

    /// <summary>
    /// Silhouette score: average of s(i) = (b(i) − a(i)) / max(a(i), b(i)).
    /// O(n²) — acceptable for n ≤ ~100 technologies.
    /// </summary>
    private static double ComputeSilhouetteScore(double[][] data, int[] labels, int k)
    {
        if (k <= 1 || data.Length <= k) return 0;

        int n = data.Length;
        double totalSil = 0;

        for (int i = 0; i < n; i++)
        {
            int myCluster = labels[i];
            // Intra-cluster mean distance (a)
            double aSum = 0; int aCount = 0;
            for (int j = 0; j < n; j++)
                if (j != i && labels[j] == myCluster)
                { aSum += Math.Sqrt(SquaredEuclidean(data[i], data[j])); aCount++; }
            double a = aCount > 0 ? aSum / aCount : 0;

            // Nearest-cluster mean distance (b)
            double b = double.MaxValue;
            for (int c = 0; c < k; c++)
            {
                if (c == myCluster) continue;
                double cSum = 0; int cCount = 0;
                for (int j = 0; j < n; j++)
                    if (labels[j] == c)
                    { cSum += Math.Sqrt(SquaredEuclidean(data[i], data[j])); cCount++; }
                if (cCount > 0) b = Math.Min(b, cSum / cCount);
            }
            if (b == double.MaxValue) b = 0;

            double denom = Math.Max(a, b);
            totalSil += denom > 0 ? (b - a) / denom : 0;
        }
        return totalSil / n;
    }

    // ──────────────────────────────────────────────────────── K selection

    /// <summary>
    /// Finds the "knee" of the WCSS curve using the maximum perpendicular
    /// distance from the line connecting the first and last k values.
    /// </summary>
    private static int? DetectElbow(List<KMetrics> metrics)
    {
        if (metrics.Count < 3) return null;

        double x1 = metrics[0].K, y1 = metrics[0].Inertia;
        double x2 = metrics[^1].K, y2 = metrics[^1].Inertia;
        double dx = x2 - x1, dy = y2 - y1;
        double len = Math.Sqrt(dx * dx + dy * dy);
        if (len == 0) return null;

        double maxDist = 0;
        int? elbowK = null;
        foreach (var m in metrics)
        {
            double dist = Math.Abs(dy * m.K - dx * m.Inertia + x2 * y1 - y2 * x1) / len;
            if (dist > maxDist) { maxDist = dist; elbowK = m.K; }
        }
        return elbowK;
    }

    private static double ScoreK(KMeansRun run, int k, int? elbowK, double maxSil)
    {
        double silScore = maxSil > 0 ? run.SilhouetteScore / maxSil : run.SilhouetteScore;
        double interpretabilityBonus = k == 4 ? 0.04 : 0.0;
        double elbowBonus = elbowK.HasValue && k == elbowK.Value ? 0.03 : 0.0;
        double overclusterPenalty = Math.Max(0, k - 4) * 0.015;
        bool hasSingleton = run.Labels.GroupBy(l => l).Any(g => g.Count() == 1);
        double singletonPenalty = hasSingleton ? 0.4 : 0.0;
        return silScore + interpretabilityBonus + elbowBonus - overclusterPenalty - singletonPenalty;
    }

    private static string BuildRationale(KMeansRun run, int? elbowK, List<KMetrics> allMetrics)
    {
        var m = allMetrics.First(x => x.K == run.K);
        string elbowStr = elbowK.HasValue ? $"Elbow at k={elbowK}" : "Elbow undetermined";
        return $"k={run.K} selected | Silhouette={m.SilhouetteScore:F4} | Inertia={m.Inertia:F2} | {elbowStr}";
    }

    private static double SquaredEuclidean(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++) { double d = a[i] - b[i]; sum += d * d; }
        return sum;
    }

    private sealed class KMeansRun
    {
        public int K { get; set; }
        public int[] Labels { get; set; } = [];
        public double[][] Centroids { get; set; } = [];
        public double Inertia { get; set; }
        public double SilhouetteScore { get; set; }
    }
}
