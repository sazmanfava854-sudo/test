using IoTRecommendation.Core.Models.Enums;

namespace IoTRecommendation.Core.Models;

/// <summary>
/// Describes a single decision criterion. Loaded from Settings.json so that
/// the set of criteria can be modified without recompiling.
/// </summary>
public sealed class CriterionDefinition
{
    /// <summary>Stable key used in code and JSON files (e.g. "TransmissionRange").</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Human-readable name shown in the UI.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Unit of measurement shown in the UI.</summary>
    public string Unit { get; set; } = string.Empty;

    public CriterionType Type { get; set; }

    /// <summary>Whether this criterion participates in K-Means clustering.</summary>
    public bool UsedInClustering { get; set; }

    /// <summary>Whether this criterion participates in AHP/TOPSIS.</summary>
    public bool UsedInTopsis { get; set; }

    /// <summary>Transform applied before clustering feature scaling.</summary>
    public CriterionTransform Transform { get; set; } = CriterionTransform.None;
}
