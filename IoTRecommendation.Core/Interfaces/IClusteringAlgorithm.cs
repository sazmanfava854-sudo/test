using IoTRecommendation.Core.Models.Clustering;

namespace IoTRecommendation.Core.Interfaces;

/// <summary>
/// Abstraction for the clustering algorithm used in Phase 1.
/// Implement this interface to swap K-Means for another algorithm
/// (e.g. DBSCAN, hierarchical) without touching any service.
/// </summary>
public interface IClusteringAlgorithm
{
    /// <summary>
    /// Clusters the rows of <paramref name="featureMatrix"/> (already scaled).
    /// </summary>
    /// <param name="featureMatrix">m × p matrix: m technologies, p clustering features.</param>
    /// <param name="kMin">Minimum number of clusters to evaluate.</param>
    /// <param name="kMax">Maximum number of clusters to evaluate.</param>
    /// <returns>Assignment labels (one per row) and supporting metrics.</returns>
    ClusteringAlgorithmOutput Run(double[][] featureMatrix, int kMin, int kMax);
}

/// <summary>
/// Raw output from the clustering algorithm, before cluster labels are resolved.
/// </summary>
public sealed class ClusteringAlgorithmOutput
{
    /// <summary>Cluster assignment for each technology row.</summary>
    public int[] Labels { get; set; } = [];

    /// <summary>Centroids in the scaled feature space (one per cluster).</summary>
    public double[][] ScaledCentroids { get; set; } = [];

    public int OptimalK { get; set; }
    public int? ElbowK { get; set; }
    public List<KMetrics> AllKMetrics { get; set; } = new();
    public string SelectionRationale { get; set; } = string.Empty;
}
