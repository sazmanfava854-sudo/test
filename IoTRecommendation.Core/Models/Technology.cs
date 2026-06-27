namespace IoTRecommendation.Core.Models;

/// <summary>
/// Represents one IoT communication technology and its criterion values.
/// Loaded from Technologies.json — no values are hardcoded.
/// </summary>
public sealed class Technology
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Dictionary keyed by CriterionDefinition.Key (e.g. "TransmissionRange").
    /// Values are raw numeric measurements.
    /// </summary>
    public Dictionary<string, double> Criteria { get; set; } = new();
}
