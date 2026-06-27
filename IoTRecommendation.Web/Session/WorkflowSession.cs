using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Ahp;
using IoTRecommendation.Core.Models.Clustering;
using IoTRecommendation.Core.Models.Questionnaire;
using IoTRecommendation.Core.Models.Topsis;

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
    public AdaptiveWeightResult? AdaptiveWeightResult { get; set; }
    public TopsisResult? TopsisResult { get; set; }

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
