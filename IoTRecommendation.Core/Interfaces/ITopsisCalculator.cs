using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Topsis;

namespace IoTRecommendation.Core.Interfaces;

/// <summary>
/// Implements the TOPSIS MCDM algorithm.
/// Implement this interface to replace TOPSIS with VIKOR, ELECTRE, etc.
/// </summary>
public interface ITopsisCalculator
{
    /// <summary>
    /// Ranks technologies by closeness coefficient.
    /// </summary>
    /// <param name="technologies">Technologies to rank (cluster subset).</param>
    /// <param name="weights">Normalised adaptive weights keyed by CriterionDefinition.Key.</param>
    /// <param name="criteria">Criterion definitions (type: benefit/cost).</param>
    TopsisResult Calculate(
        IReadOnlyList<Technology> technologies,
        IReadOnlyDictionary<string, double> weights,
        IReadOnlyList<CriterionDefinition> criteria);
}
