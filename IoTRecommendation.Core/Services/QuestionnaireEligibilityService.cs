using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Questionnaire;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Applies hard eligibility rules from questionnaire answers before MCDM
/// (TOPSIS / VIKOR / COPRAS). Adaptive weights only shift criterion importance;
/// they do not remove unsuitable technologies.
/// </summary>
public sealed class QuestionnaireEligibilityService
{
    private readonly ITechnologyRepository _technologyRepository;

    public QuestionnaireEligibilityService(ITechnologyRepository technologyRepository)
    {
        _technologyRepository = technologyRepository;
    }

    /// <summary>
    /// Returns technology IDs from the cluster that remain eligible after questionnaire rules.
    /// </summary>
    public async Task<EligibilityResult> FilterClusterAsync(
        IReadOnlyList<string> clusterTechnologyIds,
        IReadOnlyList<QuestionnaireAnswer> answers)
    {
        var all = await _technologyRepository.GetAllAsync();
        var byId = all.ToDictionary(t => t.Id);
        var eligible = clusterTechnologyIds.Where(byId.ContainsKey).ToList();
        var excluded = new List<ExcludedTechnology>();

        bool blockCellular = answers.Any(a =>
            a.QuestionId == "Q06" &&
            string.Equals(a.SelectedOptionId, "Q06_C", StringComparison.Ordinal));

        if (blockCellular)
        {
            var remaining = new List<string>();
            foreach (string id in eligible)
            {
                double cellular = byId[id].Criteria.GetValueOrDefault("CellularSupport", 0);
                if (cellular >= 0.5)
                {
                    excluded.Add(new ExcludedTechnology
                    {
                        TechnologyId = id,
                        TechnologyName = byId[id].Name,
                        Reason = "Q06: cellular coverage poor or not allowed — cellular technologies excluded."
                    });
                }
                else
                    remaining.Add(id);
            }
            eligible = remaining;
        }

        if (eligible.Count == 0)
        {
            throw new InvalidOperationException(
                "No technologies remain eligible after questionnaire constraints. " +
                "Relax site constraints (e.g. cellular availability) or choose another cluster.");
        }

        return new EligibilityResult
        {
            EligibleTechnologyIds = eligible,
            Excluded = excluded
        };
    }
}

public sealed class EligibilityResult
{
    public List<string> EligibleTechnologyIds { get; set; } = new();
    public List<ExcludedTechnology> Excluded { get; set; } = new();
}

public sealed class ExcludedTechnology
{
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
