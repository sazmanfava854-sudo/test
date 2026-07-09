using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Copras;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Orchestrates the COPRAS evaluation: filters the technology dataset to the
/// selected cluster, then runs COPRAS with the adaptive weights. Mirrors
/// <see cref="TopsisService"/> and <see cref="VikorService"/> so all three
/// methods run on identical inputs and can be compared directly.
/// </summary>
public sealed class CoprasService
{
    private readonly ITechnologyRepository _technologyRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ICoprasCalculator _coprasCalculator;

    public CoprasService(
        ITechnologyRepository technologyRepository,
        ISettingsRepository settingsRepository,
        ICoprasCalculator coprasCalculator)
    {
        _technologyRepository = technologyRepository;
        _settingsRepository = settingsRepository;
        _coprasCalculator = coprasCalculator;
    }

    public async Task<CoprasResult> RunAsync(
        IReadOnlyList<string> clusterTechnologyIds,
        IReadOnlyDictionary<string, double> adaptiveWeights,
        int selectedClusterId)
    {
        var allTechnologies = await _technologyRepository.GetAllAsync();
        var settings = await _settingsRepository.GetAsync();

        var clusterTechs = allTechnologies
            .Where(t => clusterTechnologyIds.Contains(t.Id))
            .ToList();

        if (clusterTechs.Count == 0)
            throw new InvalidOperationException("Selected cluster contains no technologies.");

        var result = _coprasCalculator.Calculate(clusterTechs, adaptiveWeights, settings.CriteriaDefinitions);
        result.SelectedClusterId = selectedClusterId;
        return result;
    }
}
