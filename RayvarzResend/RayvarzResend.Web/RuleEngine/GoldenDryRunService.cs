using System.Text.Json;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Engines;
using RayvarzResend.Web.RuleEngine.Store;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine;

public sealed class GoldenDryRunCaseResult
{
    public int GoldenFicheId { get; init; }
    public string Name { get; init; } = "";
    public string FicheNo { get; init; } = "";
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int RowCount { get; init; }
    public decimal Payable { get; init; }
    public decimal RowSum { get; init; }
    public IReadOnlyList<string> Mismatches { get; init; } = Array.Empty<string>();
}

public sealed class GoldenDryRunSummary
{
    public string EngineName { get; init; } = "LegacyCSharp";
    public int Total { get; init; }
    public int Passed { get; init; }
    public bool AllPassed => Total > 0 && Passed == Total;
    public IReadOnlyList<GoldenDryRunCaseResult> Cases { get; init; } = Array.Empty<GoldenDryRunCaseResult>();
}

/// <summary>
/// فاز ۱: بارگذاری live از Sara + ارزیابی از IFicheRuleEngine + مقایسه با RuleGoldenExpectedRow.
/// </summary>
public sealed class GoldenDryRunService
{
    private readonly RuleEngineStore _store;
    private readonly FicheRepository _fiches;
    private readonly RuleEngineFactory _engineFactory;

    public GoldenDryRunService(RuleEngineStore store, FicheRepository fiches, RuleEngineFactory engineFactory)
    {
        _store = store;
        _fiches = fiches;
        _engineFactory = engineFactory;
    }

    public async Task<GoldenDryRunSummary> RunAllAsync(bool compareExpectedRows = true, CancellationToken ct = default)
    {
        var engine = await _engineFactory.ResolveAsync(ct);
        return await RunAllWithEngineAsync(engine, null, null, compareExpectedRows, allowLegacyFallback: true, ct);
    }

    public async Task<GoldenDryRunSummary> RunAllWithEngineAsync(
        IFicheRuleEngine engine,
        long? candidateId,
        long? snapshotId,
        bool compareExpectedRows = true,
        bool allowLegacyFallback = true,
        CancellationToken ct = default)
    {
        var nidMember = _engineFactory.NidMember;
        var goldens = await _store.GetActiveGoldenFichesAsync(nidMember, ct);
        var results = new List<GoldenDryRunCaseResult>();

        foreach (var g in goldens)
        {
            results.Add(await RunOneAsync(engine, g, candidateId, snapshotId, compareExpectedRows, allowLegacyFallback, ct));
        }

        return new GoldenDryRunSummary
        {
            EngineName = engine.EngineName,
            Total = results.Count,
            Passed = results.Count(r => r.Success),
            Cases = results
        };
    }

    public async Task<GoldenDryRunCaseResult> RunOneAsync(
        RuleGoldenFicheRow golden, bool compareExpectedRows = true, CancellationToken ct = default)
    {
        var engine = await _engineFactory.ResolveAsync(ct);
        return await RunOneAsync(engine, golden, null, null, compareExpectedRows, true, ct);
    }

    private async Task<GoldenDryRunCaseResult> RunOneAsync(
        IFicheRuleEngine engine,
        RuleGoldenFicheRow golden,
        long? candidateId,
        long? snapshotId,
        bool compareExpectedRows,
        bool allowLegacyFallback,
        CancellationToken ct)
    {
        try
        {
            var fiche = await _fiches.LoadAsync(IdentifierType.FicheNo, golden.FicheNo, ct);
            if (fiche == null)
                return Fail(golden, $"فیش {golden.FicheNo} در Sara یافت نشد (live)");

            if (fiche.Category != FicheCategory.DutyNosazi && fiche.Category != FicheCategory.DutySenfi)
                return Fail(golden, $"فیش {golden.FicheNo} از نوع Duty نیست: {fiche.Category}");

            var branch = fiche.ResolvedDistrictBranch ?? 0;
            var fund = fiche.SuggestedFund ?? 0;
            var evaluated = await engine.EvaluateAsync(new FicheRuleContext
            {
                Fiche = fiche,
                Branch = branch,
                Fund = fund,
                AllowLegacyFallback = allowLegacyFallback
            }, buildSoap: false, ct);

            if (!evaluated.Success)
                return Fail(golden, evaluated.ErrorMessage ?? "ارزیابی موتور ناموفق بود");

            var mismatches = new List<string>();

            if (fiche.Rows.Count != golden.ExpectedRowCount)
                mismatches.Add($"تعداد ردیف: expected={golden.ExpectedRowCount} actual={fiche.Rows.Count}");

            if (evaluated.RowSum != fiche.Payable)
                mismatches.Add($"جمع ردیف‌ها ({evaluated.RowSum}) ≠ PayablePrice ({fiche.Payable})");

            if (compareExpectedRows && _store.IsConfigured)
            {
                var expected = await _store.GetExpectedRowsAsync(golden.GoldenFicheId, ct);
                foreach (var exp in expected)
                {
                    var actual = fiche.Rows.FirstOrDefault(r => r.IncmNo == exp.IncmNo);
                    if (actual == null)
                    {
                        mismatches.Add($"IncmNo {exp.IncmNo}: ردیف واقعی وجود ندارد");
                        continue;
                    }

                    if (actual.Val != exp.ExpectedVal)
                        mismatches.Add($"IncmNo {exp.IncmNo}: Val expected={exp.ExpectedVal} actual={actual.Val}");
                }
            }

            var success = mismatches.Count == 0;
            var outputJson = JsonSerializer.Serialize(new
            {
                golden.FicheNo,
                engine = engine.EngineName,
                fiche.Payable,
                rows = fiche.Rows.Select(r => new { r.IncmNo, r.Val, r.IncmRowDsc })
            });

            await _store.InsertDryRunResultAsync(candidateId, snapshotId, golden.GoldenFicheId, engine.EngineName, success,
                success ? null : string.Join("; ", mismatches), outputJson, ct);

            return new GoldenDryRunCaseResult
            {
                GoldenFicheId = golden.GoldenFicheId,
                Name = golden.Name,
                FicheNo = golden.FicheNo,
                Success = success,
                ErrorMessage = success ? null : string.Join("; ", mismatches),
                RowCount = fiche.Rows.Count,
                Payable = fiche.Payable,
                RowSum = evaluated.RowSum,
                Mismatches = mismatches
            };
        }
        catch (Exception ex)
        {
            var engineName = engine.EngineName;
            try
            {
                await _store.InsertDryRunResultAsync(candidateId, snapshotId, golden.GoldenFicheId, engineName, false, ex.Message, null, ct);
            }
            catch
            {
                // ignore secondary DB errors during dry-run logging
            }
            return Fail(golden, ex.Message);
        }
    }

    private static GoldenDryRunCaseResult Fail(RuleGoldenFicheRow golden, string message) =>
        new()
        {
            GoldenFicheId = golden.GoldenFicheId,
            Name = golden.Name,
            FicheNo = golden.FicheNo,
            Success = false,
            ErrorMessage = message,
            Mismatches = new[] { message }
        };
}
