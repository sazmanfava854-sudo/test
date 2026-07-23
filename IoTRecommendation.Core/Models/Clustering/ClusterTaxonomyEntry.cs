namespace IoTRecommendation.Core.Models.Clustering;

/// <summary>
/// Domain-expert suggested cluster: a label and the technology IDs that belong to it.
/// Loaded from Data/ClusterTaxonomy.json.
/// </summary>
public sealed class ClusterTaxonomyEntry
{
    public int ClusterId { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<string> TechnologyIds { get; set; } = new();
}
