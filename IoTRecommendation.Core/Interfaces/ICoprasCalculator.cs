using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Copras;

namespace IoTRecommendation.Core.Interfaces;

/// <summary>
/// Implements the COPRAS MCDM algorithm (COmplex PRoportional ASsessment;
/// Zavadskas &amp; Kaklauskas, 1996; Zavadskas et al., 2008).
/// Implement this interface to replace COPRAS with another proportional-
/// assessment method.
/// </summary>
public interface ICoprasCalculator
{
    /// <summary>
    /// Ranks technologies by relative significance Qᵢ (descending — higher Qᵢ is better).
    /// </summary>
    /// <param name="technologies">Technologies to rank (cluster subset).</param>
    /// <param name="weights">Normalised adaptive weights keyed by CriterionDefinition.Key.</param>
    /// <param name="criteria">Criterion definitions (type: benefit/cost).</param>
    CoprasResult Calculate(
        IReadOnlyList<Technology> technologies,
        IReadOnlyDictionary<string, double> weights,
        IReadOnlyList<CriterionDefinition> criteria);
}
