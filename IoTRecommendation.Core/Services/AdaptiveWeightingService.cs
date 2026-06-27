using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Core.Models;
using IoTRecommendation.Core.Models.Ahp;
using IoTRecommendation.Core.Models.Questionnaire;

namespace IoTRecommendation.Core.Services;

/// <summary>
/// Orchestrates Phase 3: aggregates questionnaire effects into
/// per-criterion adjustment scores (Si), then applies the adaptive
/// weight formula.
///
/// The formula itself is injected via IAdaptiveWeightFormula,
/// so it can be replaced without touching this service.
/// </summary>
public sealed class AdaptiveWeightingService
{
    private readonly IQuestionRepository _questionRepository;
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAdaptiveWeightFormula _weightFormula;

    public AdaptiveWeightingService(
        IQuestionRepository questionRepository,
        ISettingsRepository settingsRepository,
        IAdaptiveWeightFormula weightFormula)
    {
        _questionRepository = questionRepository;
        _settingsRepository = settingsRepository;
        _weightFormula = weightFormula;
    }

    public async Task<IReadOnlyList<Question>> GetQuestionsAsync()
        => await _questionRepository.GetAllAsync();

    /// <summary>
    /// Computes adaptive weights from base AHP weights and user answers.
    /// </summary>
    /// <param name="baseResult">Phase 2 AHP output.</param>
    /// <param name="answers">User's questionnaire answers (questionId → optionId).</param>
    public async Task<AdaptiveWeightResult> ComputeAsync(
        AhpResult baseResult,
        IReadOnlyList<QuestionnaireAnswer> answers)
    {
        var settings = await _settingsRepository.GetAsync();
        var questions = await _questionRepository.GetAllAsync();
        var topsisKeys = settings.CriteriaDefinitions
            .Where(c => c.UsedInTopsis)
            .Select(c => c.Key)
            .ToList();

        // Initialise counters
        var positives = topsisKeys.ToDictionary(k => k, _ => 0);
        var negatives = topsisKeys.ToDictionary(k => k, _ => 0);

        // Build a lookup: questionId → (optionId → effects)
        var questionLookup = questions.ToDictionary(q => q.Id);

        foreach (var answer in answers)
        {
            if (!questionLookup.TryGetValue(answer.QuestionId, out var question)) continue;
            var option = question.Options.FirstOrDefault(o => o.Id == answer.SelectedOptionId);
            if (option is null) continue;

            foreach (var effect in option.Effects)
            {
                if (!positives.ContainsKey(effect.CriterionKey)) continue;
                if (effect.Delta > 0) positives[effect.CriterionKey] += effect.Delta;
                else if (effect.Delta < 0) negatives[effect.CriterionKey] += Math.Abs(effect.Delta);
            }
        }

        var scores = topsisKeys.ToDictionary(k => k, k => positives[k] - negatives[k]);

        var adaptiveWeights = _weightFormula.Calculate(baseResult.Weights, scores);

        return new AdaptiveWeightResult
        {
            BaseWeights = baseResult.Weights.ToDictionary(k => k.Key, k => k.Value),
            AdjustmentScores = scores,
            PositiveCounts = positives,
            NegativeCounts = negatives,
            AdaptiveWeights = adaptiveWeights
        };
    }
}
