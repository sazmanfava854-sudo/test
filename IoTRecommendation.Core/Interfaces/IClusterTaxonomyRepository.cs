using IoTRecommendation.Core.Models.Clustering;

namespace IoTRecommendation.Core.Interfaces;

public interface IClusterTaxonomyRepository
{
    Task<IReadOnlyList<ClusterTaxonomyEntry>> GetAsync();
}
