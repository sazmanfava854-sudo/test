using IoTRecommendation.Core.Models.Clustering;

namespace IoTRecommendation.Web.ViewModels;

public sealed class ClusterSelectionViewModel
{
    public List<ClusterInfo> Clusters { get; set; } = new();
    public int? PreselectedClusterId { get; set; }
}
