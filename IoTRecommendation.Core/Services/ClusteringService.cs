using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Clustering;
using IoTRecommendation.Core.Models.Enums;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Orchestrates Phase 1: feature preparation, K-Means evaluation metrics,
/// and domain-recommended cluster assignments from ClusterTaxonomy.json.
///
/// AHP weights are deliberately NOT used here.
/// </summary>
public sealed class ClusteringService
{
    private readonly ITechnologyRepository _technologyRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IClusterTaxonomyRepository _clusterTaxonomyRepository;
    private readonly IClusteringAlgorithm _clusteringAlgorithm;

    public ClusteringService(
        ITechnologyRepository technologyRepository,
        ISettingsRepository settingsRepository,
        IClusterTaxonomyRepository clusterTaxonomyRepository,
        IClusteringAlgorithm clusteringAlgorithm)
    {
        _technologyRepository = technologyRepository;
        _settingsRepository = settingsRepository;
        _clusterTaxonomyRepository = clusterTaxonomyRepository;
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

        // Run K-Means for k-evaluation metrics (elbow / silhouette table in UI)
        var output = _clusteringAlgorithm.Run(scaled, settings.KMeansMin, settings.KMeansMax);

        var taxonomy = await _clusterTaxonomyRepository.GetAsync();
        var clusters = BuildClustersFromTaxonomy(taxonomy, technologies, clusteringCriteria);

        return new ClusteringResult
        {
            OptimalK = clusters.Count,
            Clusters = clusters,
            KMetrics = output.AllKMetrics,
            ElbowK = output.ElbowK,
            SelectionRationale =
                $"k={clusters.Count} recommended from domain taxonomy (ClusterTaxonomy.json). " +
                $"K-Means reference: {output.SelectionRationale}"
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

    private static List<ClusterInfo> BuildClustersFromTaxonomy(
        IReadOnlyList<ClusterTaxonomyEntry> taxonomy,
        IReadOnlyList<Technology> technologies,
        List<CriterionDefinition> clusteringCriteria)
    {
        var techById = technologies.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        var assigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var clusters = new List<ClusterInfo>();

        foreach (var entry in taxonomy.OrderBy(e => e.ClusterId))
        {
            var memberTechs = new List<Technology>();
            foreach (var techId in entry.TechnologyIds)
            {
                if (!techById.TryGetValue(techId, out var tech))
                    throw new InvalidOperationException(
                        $"Cluster taxonomy references unknown technology '{techId}'.");

                if (!assigned.Add(techId))
                    throw new InvalidOperationException(
                        $"Technology '{techId}' appears in more than one taxonomy cluster.");

                memberTechs.Add(tech);
            }

            clusters.Add(new ClusterInfo
            {
                ClusterId = entry.ClusterId,
                Label = entry.Label,
                TechnologyIds = memberTechs.Select(t => t.Id).ToList(),
                TechnologyNames = memberTechs.Select(t => t.Name).ToList(),
                CentroidValues = ComputeMeanCentroid(memberTechs, clusteringCriteria)
            });
        }

        var missing = technologies
            .Where(t => !assigned.Contains(t.Id))
            .Select(t => t.Id)
            .ToList();

        if (missing.Count > 0)
            throw new InvalidOperationException(
                $"Cluster taxonomy does not assign all technologies. Missing: {string.Join(", ", missing)}");

        return clusters;
    }

    private static Dictionary<string, double> ComputeMeanCentroid(
        IReadOnlyList<Technology> members,
        List<CriterionDefinition> clusteringCriteria)
    {
        if (members.Count == 0)
            return new Dictionary<string, double>();

        var centroid = new Dictionary<string, double>();
        foreach (var criterion in clusteringCriteria)
        {
            double mean = members.Average(t => t.Criteria.GetValueOrDefault(criterion.Key, 0.0));
            centroid[criterion.Key] = mean;
        }
        return centroid;
    }
}
