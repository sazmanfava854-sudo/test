namespace IoTRecommendation.Core.Models.Clustering;

/// <summary>
/// Describes a single cluster produced by the clustering algorithm.
/// </summary>
public sealed class ClusterInfo
{
    public int ClusterId { get; set; }

    /// <summary>Descriptive label inferred from the cluster centroid profile.</summary>
    public string Label { get; set; } = string.Empty;

    public List<string> TechnologyIds { get; set; } = new();
    public List<string> TechnologyNames { get; set; } = new();

    /// <summary>Centroid values in original (non-scaled) space, keyed by CriterionDefinition.Key.</summary>
    public Dictionary<string, double> CentroidValues { get; set; } = new();
}
