namespace IoTRecommendation.Infrastructure.Configuration;

/// <summary>
/// Paths to the data directory. Bound from appsettings.json under "DataPaths".
/// </summary>
public sealed class DataPathOptions
{
    public const string SectionName = "DataPaths";

    public string DataDirectory { get; set; } = "Data";
    public string TechnologiesFile { get; set; } = "Technologies.json";
    public string QuestionsFile { get; set; } = "Questions.json";
    public string SettingsFile { get; set; } = "Settings.json";
    public string ExpertsDirectory { get; set; } = "Experts";
    public string ClusterTaxonomyFile { get; set; } = "ClusterTaxonomy.json";
}
