using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Vikor;

namespace IoTRecommendation.Core.Interfaces;

/// <summary>
/// Implements the VIKOR MCDM algorithm (VIseKriterijumska Optimizacija i
/// Kompromisno Resenje — Multi-criteria Optimization and Compromise Solution).
/// Implement this interface to replace VIKOR with another compromise-ranking method.
/// </summary>
public interface IVikorCalculator
{
    /// <summary>
    /// Ranks technologies by the VIKOR index Q (ascending — lower Q is better).
    /// </summary>
    /// <param name="technologies">Technologies to rank (cluster subset).</param>
    /// <param name="weights">Normalised adaptive weights keyed by CriterionDefinition.Key.</param>
    /// <param name="criteria">Criterion definitions (type: benefit/cost).</param>
    /// <param name="v">Weight of the "majority of criteria" strategy, in [0, 1]. 0.5 is the classical default.</param>
    VikorResult Calculate(
        IReadOnlyList<Technology> technologies,
        IReadOnlyDictionary<string, double> weights,
        IReadOnlyList<CriterionDefinition> criteria,
        double v);
}
