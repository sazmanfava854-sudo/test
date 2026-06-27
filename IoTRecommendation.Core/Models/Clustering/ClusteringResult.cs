namespace IoTRecommendation.Core.Models.Clustering;

/// <summary>
/// Full output of Phase 1 clustering.
/// </summary>
public sealed class ClusteringResult
{
    public int OptimalK { get; set; }
    public List<ClusterInfo> Clusters { get; set; } = new();

    /// <summary>Inertia (WCSS) per k value — used to plot the elbow curve.</summary>
    public List<KMetrics> KMetrics { get; set; } = new();

    public int? ElbowK { get; set; }
    public string SelectionRationale { get; set; } = string.Empty;
}

public sealed class KMetrics
{
    public int K { get; set; }
    public double Inertia { get; set; }
    public double SilhouetteScore { get; set; }
    public int MinClusterSize { get; set; }
}
