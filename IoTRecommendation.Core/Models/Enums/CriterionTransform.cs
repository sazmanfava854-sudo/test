namespace IoTRecommendation.Core.Models.Enums;

/// <summary>
/// Pre-processing transform applied to a criterion before clustering.
/// Log1p compresses wide-range values (e.g. data rate spanning 0.0001..23059 Mbps).
/// </summary>
public enum CriterionTransform
{
    None,
    Log1p
}
