using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Topsis;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Orchestrates Phase 4: filters the technology dataset to the selected
/// cluster, then runs TOPSIS with the adaptive weights.
/// </summary>
public sealed class TopsisService
{
    private readonly ITechnologyRepository _technologyRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly ITopsisCalculator _topsisCalculator;

    public TopsisService(
        ITechnologyRepository technologyRepository,
        ISettingsRepository settingsRepository,
        ITopsisCalculator topsisCalculator)
    {
        _technologyRepository = technologyRepository;
        _settingsRepository = settingsRepository;
        _topsisCalculator = topsisCalculator;
    }

    public async Task<TopsisResult> RunAsync(
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

        var result = _topsisCalculator.Calculate(clusterTechs, adaptiveWeights, settings.CriteriaDefinitions);
        result.SelectedClusterId = selectedClusterId;
        return result;
    }
}
