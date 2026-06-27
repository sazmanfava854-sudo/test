using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Clustering;
using IoTRecommendation.Core.Models.Enums;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Orchestrates Phase 1: feature preparation, scaling, clustering,
/// and centroid back-transformation to original units.
///
/// AHP weights are deliberately NOT used here — clustering is purely data-driven.
/// </summary>
public sealed class ClusteringService
{
    private readonly ITechnologyRepository _technologyRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IClusteringAlgorithm _clusteringAlgorithm;

    public ClusteringService(
        ITechnologyRepository technologyRepository,
        ISettingsRepository settingsRepository,
        IClusteringAlgorithm clusteringAlgorithm)
    {
        _technologyRepository = technologyRepository;
        _settingsRepository = settingsRepository;
        _clusteringAlgorithm = clusteringAlgorithm;
    }

    public async Task<ClusteringResult> RunAsync()
    {
        var technologies = await _technologyRepository.GetAllAsync();
        var settings = await _settingsRepository.GetAsync();

        var clusteringCriteria = settings.CriteriaDefinitions
            .Where(c => c.UsedInClustering)
            .ToList();

        if (clusteringCriteria.Count < 2)
            throw new InvalidOperationException("At least 2 criteria must be marked UsedInClustering.");

        // Build raw feature matrix
        double[][] raw = BuildRawMatrix(technologies, clusteringCriteria);

        // Apply per-criterion transforms (log1p for wide-range columns)
        double[][] transformed = ApplyTransforms(raw, clusteringCriteria);

        // Z-score standardisation (mean=0, std=1)
        var (scaled, means, stds) = Standardize(transformed);

        // Run clustering algorithm (implementation is injected — swappable)
        var output = _clusteringAlgorithm.Run(scaled, settings.KMeansMin, settings.KMeansMax);

        // Back-transform centroids to original units for display
        var clusters = BuildClusters(output, technologies, clusteringCriteria, means, stds);

        return new ClusteringResult
        {
            OptimalK = output.OptimalK,
            Clusters = clusters,
            KMetrics = output.AllKMetrics,
            ElbowK = output.ElbowK,
            SelectionRationale = output.SelectionRationale
        };
    }

    // ──────────────────────────────────────────────────────── Feature matrix

    private static double[][] BuildRawMatrix(
        IReadOnlyList<Technology> technologies,
        List<CriterionDefinition> criteria)
    {
        double[][] matrix = new double[technologies.Count][];
        for (int i = 0; i < technologies.Count; i++)
        {
            matrix[i] = new double[criteria.Count];
            for (int j = 0; j < criteria.Count; j++)
                matrix[i][j] = technologies[i].Criteria.GetValueOrDefault(criteria[j].Key, 0.0);
        }
        return matrix;
    }

    private static double[][] ApplyTransforms(double[][] raw, List<CriterionDefinition> criteria)
    {
        double[][] result = new double[raw.Length][];
        for (int i = 0; i < raw.Length; i++)
        {
            result[i] = new double[raw[i].Length];
            for (int j = 0; j < raw[i].Length; j++)
                result[i][j] = criteria[j].Transform == CriterionTransform.Log1p
                    ? Math.Log(1.0 + Math.Max(0, raw[i][j]))
                    : raw[i][j];
        }
        return result;
    }

    private static (double[][] scaled, double[] means, double[] stds) Standardize(double[][] data)
    {
        int n = data.Length, p = data[0].Length;
        double[] means = new double[p];
        double[] stds = new double[p];

        for (int j = 0; j < p; j++)
        {
            double sum = 0, sum2 = 0;
            for (int i = 0; i < n; i++) { sum += data[i][j]; sum2 += data[i][j] * data[i][j]; }
            means[j] = sum / n;
            double variance = sum2 / n - means[j] * means[j];
            stds[j] = variance > 0 ? Math.Sqrt(variance) : 1.0;
        }

        double[][] scaled = new double[n][];
        for (int i = 0; i < n; i++)
        {
            scaled[i] = new double[p];
            for (int j = 0; j < p; j++)
                scaled[i][j] = (data[i][j] - means[j]) / stds[j];
        }
        return (scaled, means, stds);
    }

    // ──────────────────────────────────────────────────────── Cluster building

    private static List<ClusterInfo> BuildClusters(
        ClusteringAlgorithmOutput output,
        IReadOnlyList<Technology> technologies,
        List<CriterionDefinition> clusteringCriteria,
        double[] means,
        double[] stds)
    {
        var clusters = new List<ClusterInfo>();
        int k = output.OptimalK;

        for (int c = 0; c < k; c++)
        {
            var memberTechs = technologies
                .Where((_, idx) => output.Labels[idx] == c)
                .ToList();

            // Back-transform centroid: scaled → transformed → original
            var centroidOriginal = new Dictionary<string, double>();
            for (int j = 0; j < clusteringCriteria.Count; j++)
            {
                double scaled = output.ScaledCentroids[c][j];
                double transformed = scaled * stds[j] + means[j];
                double original = clusteringCriteria[j].Transform == CriterionTransform.Log1p
                    ? Math.Exp(transformed) - 1.0
                    : transformed;
                centroidOriginal[clusteringCriteria[j].Key] = original;
            }

            clusters.Add(new ClusterInfo
            {
                ClusterId = c,
                Label = InferLabel(centroidOriginal, clusteringCriteria),
                TechnologyIds = memberTechs.Select(t => t.Id).ToList(),
                TechnologyNames = memberTechs.Select(t => t.Name).ToList(),
                CentroidValues = centroidOriginal
            });
        }
        return clusters;
    }

    /// <summary>
    /// Infers a human-readable cluster label from centroid values.
    /// Rules are descriptive — not tied to AHP weights.
    /// </summary>
    private static string InferLabel(
        Dictionary<string, double> centroid,
        List<CriterionDefinition> criteria)
    {
        centroid.TryGetValue("DataRate", out double dr);
        centroid.TryGetValue("TransmissionRange", out double range);
        centroid.TryGetValue("EnergyConsumption", out double energy);
        centroid.TryGetValue("RTTLatency", out double latency);
        centroid.TryGetValue("CellularSupport", out double cellular);

        bool highDataRate = dr >= 100;
        bool longRange = range >= 5000;
        bool lowEnergy = energy < 150;
        bool highLatency = latency > 1000;
        bool isCellular = cellular >= 0.5;

        if (highDataRate && !longRange)
            return "Short-range High-throughput (WLAN)";
        if (isCellular && dr >= 1)
            return "Wide-area Cellular IoT";
        if (longRange && highLatency)
            return "Long-range Low-data-rate (LPWAN)";
        if (lowEnergy && !isCellular)
            return "Low-power Mesh / PAN";
        if (longRange)
            return "Long-range Mixed Profile";
        return "Mixed Profile";
    }
}
