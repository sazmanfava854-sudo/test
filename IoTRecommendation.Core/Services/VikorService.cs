using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Vikor;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Orchestrates the VIKOR evaluation: filters the technology dataset to the
/// selected cluster, then runs VIKOR with the adaptive weights. Mirrors
/// <see cref="TopsisService"/> so both methods run on identical inputs and
/// can be compared directly.
/// </summary>
public sealed class VikorService
{
    private readonly ITechnologyRepository _technologyRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IVikorCalculator _vikorCalculator;

    public VikorService(
        ITechnologyRepository technologyRepository,
        ISettingsRepository settingsRepository,
        IVikorCalculator vikorCalculator)
    {
        _technologyRepository = technologyRepository;
        _settingsRepository = settingsRepository;
        _vikorCalculator = vikorCalculator;
    }

    public async Task<VikorResult> RunAsync(
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

        var result = _vikorCalculator.Calculate(
            clusterTechs, adaptiveWeights, settings.CriteriaDefinitions, settings.VikorV);
        result.SelectedClusterId = selectedClusterId;
        return result;
    }
}
