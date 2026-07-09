using IoTRecommendation.Core.Models.Questionnaire;
using IoTRecommendation.Core.Services;
using IoTRecommendation.Core.Interfaces;
using IoTRecommendation.Web.Extensions;
using IoTRecommendation.Web.Session;
using IoTRecommendation.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IoTRecommendation.Web.Controllers;

/// <summary>
/// Manages the four-phase IoT recommendation workflow.
/// Business logic is entirely in the Core services — this controller
/// only maps HTTP requests to service calls and view models.
/// </summary>
public sealed class WorkflowController : Controller
{
    private readonly ClusteringService _clusteringService;
    private readonly AhpService _ahpService;
    private readonly AdaptiveWeightingService _adaptiveWeightingService;
    private readonly TopsisService _topsisService;
    private readonly VikorService _vikorService;
    private readonly RankingComparisonService _rankingComparisonService;
    private readonly ISettingsRepository _settingsRepository;

    public WorkflowController(
        ClusteringService clusteringService,
        AhpService ahpService,
        AdaptiveWeightingService adaptiveWeightingService,
        TopsisService topsisService,
        VikorService vikorService,
        RankingComparisonService rankingComparisonService,
        ISettingsRepository settingsRepository)
    {
        _clusteringService = clusteringService;
        _ahpService = ahpService;
        _adaptiveWeightingService = adaptiveWeightingService;
        _topsisService = topsisService;
        _vikorService = vikorService;
        _rankingComparisonService = rankingComparisonService;
        _settingsRepository = settingsRepository;
    }

    // ──────────────────────────────────────────────────────── Phase 1: Clustering

    [HttpGet]
    public IActionResult Clustering()
    {
        var session = HttpContext.Session.GetWorkflowSession();
        if (session.ClusteringResult is null)
            return View("Clustering", null);

        var settings = _settingsRepository.GetAsync().GetAwaiter().GetResult();
        var vm = new ClusteringViewModel
        {
            Result = session.ClusteringResult,
            ClusteringCriteriaKeys = settings.CriteriaDefinitions
                .Where(c => c.UsedInClustering).Select(c => c.Key).ToList()
        };
        return View("Clustering", vm);
    }

    [HttpPost]
    public async Task<IActionResult> RunClustering()
    {
        var result = await _clusteringService.RunAsync();
        var session = HttpContext.Session.GetWorkflowSession();
        session.ClusteringResult = result;
        session.AhpResult = await _ahpService.RunAsync();
        session.CurrentStep = WorkflowStep.Ahp;
        HttpContext.Session.SetWorkflowSession(session);

        return RedirectToAction(nameof(Ahp));
    }

    // ──────────────────────────────────────────────────────── Phase 2: AHP

    [HttpGet]
    public async Task<IActionResult> Ahp()
    {
        var session = HttpContext.Session.GetWorkflowSession();
        if (session.ClusteringResult is null)
            return RedirectToAction(nameof(Clustering));

        if (session.AhpResult is null)
        {
            session.AhpResult = await _ahpService.RunAsync();
            HttpContext.Session.SetWorkflowSession(session);
        }

        var settings = await _settingsRepository.GetAsync();
        var vm = new AhpViewModel
        {
            Result = session.AhpResult,
            CriteriaDefinitions = settings.CriteriaDefinitions.Where(c => c.UsedInTopsis).ToList()
        };
        return View("Ahp", vm);
    }

    // ──────────────────────────────────────────────────────── Phase 1b: Cluster selection

    [HttpGet]
    public IActionResult SelectCluster()
    {
        var session = HttpContext.Session.GetWorkflowSession();
        if (session.ClusteringResult is null)
            return RedirectToAction(nameof(Clustering));

        var vm = new ClusterSelectionViewModel
        {
            Clusters = session.ClusteringResult.Clusters,
            PreselectedClusterId = session.SelectedClusterId
        };
        return View("SelectCluster", vm);
    }

    [HttpPost]
    public IActionResult SelectCluster(int selectedClusterId)
    {
        var session = HttpContext.Session.GetWorkflowSession();
        if (session.ClusteringResult is null)
            return RedirectToAction(nameof(Clustering));

        bool valid = session.ClusteringResult.Clusters.Any(c => c.ClusterId == selectedClusterId);
        if (!valid)
        {
            ModelState.AddModelError(string.Empty, "Invalid cluster selection.");
            return View("SelectCluster", new ClusterSelectionViewModel
            {
                Clusters = session.ClusteringResult.Clusters
            });
        }

        session.SelectedClusterId = selectedClusterId;
        session.CurrentStep = WorkflowStep.Questionnaire;
        HttpContext.Session.SetWorkflowSession(session);
        return RedirectToAction(nameof(Questionnaire));
    }

    // ──────────────────────────────────────────────────────── Phase 3: Questionnaire

    [HttpGet]
    public async Task<IActionResult> Questionnaire()
    {
        var session = HttpContext.Session.GetWorkflowSession();
        if (session.SelectedClusterId is null)
            return RedirectToAction(nameof(SelectCluster));

        var questions = await _adaptiveWeightingService.GetQuestionsAsync();
        var vm = new QuestionnaireViewModel { Questions = questions.ToList() };
        return View("Questionnaire", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Questionnaire(Dictionary<string, string> selectedOptions)
    {
        var session = HttpContext.Session.GetWorkflowSession();
        if (session.SelectedClusterId is null)
            return RedirectToAction(nameof(SelectCluster));

        if (session.AhpResult is null)
            session.AhpResult = await _ahpService.RunAsync();

        var questions = await _adaptiveWeightingService.GetQuestionsAsync();

        // Validate all questions answered
        var missingQuestions = questions.Where(q => !selectedOptions.ContainsKey(q.Id)).ToList();
        if (missingQuestions.Any())
        {
            ModelState.AddModelError(string.Empty,
                $"Please answer all questions. Missing: {string.Join(", ", missingQuestions.Select(q => q.Text[..Math.Min(30, q.Text.Length)]))}");
            return View("Questionnaire", new QuestionnaireViewModel
            {
                Questions = questions.ToList(),
                SelectedOptions = selectedOptions
            });
        }

        var answers = selectedOptions
            .Select(kvp => new QuestionnaireAnswer
            {
                QuestionId = kvp.Key,
                SelectedOptionId = kvp.Value
            })
            .ToList();

        var adaptiveResult = await _adaptiveWeightingService.ComputeAsync(session.AhpResult, answers);

        var cluster = session.ClusteringResult!.Clusters.First(c => c.ClusterId == session.SelectedClusterId);

        // TOPSIS and VIKOR run on identical inputs (same cluster + adaptive weights)
        // so their rankings are directly comparable.
        var topsisResult = await _topsisService.RunAsync(
            cluster.TechnologyIds,
            adaptiveResult.AdaptiveWeights,
            session.SelectedClusterId!.Value);
        var vikorResult = await _vikorService.RunAsync(
            cluster.TechnologyIds,
            adaptiveResult.AdaptiveWeights,
            session.SelectedClusterId!.Value);
        var comparison = _rankingComparisonService.Compare(topsisResult, vikorResult);

        session.Answers = answers;
        session.AdaptiveWeightResult = adaptiveResult;
        session.TopsisResult = topsisResult;
        session.VikorResult = vikorResult;
        session.RankingComparison = comparison;
        session.CurrentStep = WorkflowStep.Results;
        HttpContext.Session.SetWorkflowSession(session);

        return RedirectToAction(nameof(Results));
    }

    // ──────────────────────────────────────────────────────── Phase 4: Results

    [HttpGet]
    public async Task<IActionResult> Results()
    {
        var session = HttpContext.Session.GetWorkflowSession();
        if (session.TopsisResult is null || session.VikorResult is null || session.RankingComparison is null)
            return RedirectToAction(nameof(Questionnaire));

        var settings = await _settingsRepository.GetAsync();
        var selectedCluster = session.ClusteringResult!.Clusters
            .First(c => c.ClusterId == session.SelectedClusterId);

        var vm = new ResultsViewModel
        {
            TopsisResult = session.TopsisResult,
            VikorResult = session.VikorResult,
            Comparison = session.RankingComparison,
            AdaptiveWeightResult = session.AdaptiveWeightResult!,
            SelectedCluster = selectedCluster,
            CriteriaDefinitions = settings.CriteriaDefinitions.Where(c => c.UsedInTopsis).ToList()
        };
        return View("Results", vm);
    }

    // ──────────────────────────────────────────────────────── Reset

    [HttpPost]
    public IActionResult Reset()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}
