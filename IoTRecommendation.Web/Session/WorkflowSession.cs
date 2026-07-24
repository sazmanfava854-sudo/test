using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Ahp;
using IoTRecommendation.Core.Models.Clustering;
using IoTRecommendation.Core.Models.Comparison;
using IoTRecommendation.Core.Models.Copras;
using IoTRecommendation.Core.Models.Questionnaire;
using IoTRecommendation.Core.Services;
using IoTRecommendation.Core.Models.Topsis;
using IoTRecommendation.Core.Models.Vikor;

namespace IoTRecommendation.Web.Session;

/// <summary>
/// Holds all inter-step state for one user workflow session.
/// Serialized to/from HTTP session as JSON.
/// </summary>
public sealed class WorkflowSession
{
    public ClusteringResult? ClusteringResult { get; set; }
    public AhpResult? AhpResult { get; set; }
    public int? SelectedClusterId { get; set; }
    public List<QuestionnaireAnswer> Answers { get; set; } = new();
    public EligibilityResult? EligibilityResult { get; set; }
    public AdaptiveWeightResult? AdaptiveWeightResult { get; set; }
    public TopsisResult? TopsisResult { get; set; }
    public VikorResult? VikorResult { get; set; }
    public CoprasResult? CoprasResult { get; set; }
    public RankingComparisonResult? RankingComparison { get; set; }
    public MultiMethodComparisonResult? MultiMethodComparison { get; set; }

    public WorkflowStep CurrentStep { get; set; } = WorkflowStep.Clustering;
}

public enum WorkflowStep
{
    Clustering = 1,
    Ahp = 2,
    ClusterSelection = 3,
    Questionnaire = 4,
    Results = 5
}
