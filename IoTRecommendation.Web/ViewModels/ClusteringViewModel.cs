using IoTRecommendation.Core.Models.Clustering;

namespace IoTRecommendation.Web.ViewModels;

public sealed class ClusteringViewModel
{
    public ClusteringResult Result { get; set; } = null!;
    public List<string> ClusteringCriteriaKeys { get; set; } = new();
}
