using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Clustering;
using IoTRecommendation.Core.Models.Topsis;

namespace IoTRecommendation.Web.ViewModels;

public sealed class ResultsViewModel
{
    public TopsisResult TopsisResult { get; set; } = null!;
    public AdaptiveWeightResult AdaptiveWeightResult { get; set; } = null!;
    public ClusterInfo SelectedCluster { get; set; } = null!;
    public List<CriterionDefinition> CriteriaDefinitions { get; set; } = new();
}
