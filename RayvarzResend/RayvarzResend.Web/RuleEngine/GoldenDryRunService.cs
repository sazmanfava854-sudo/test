using System.Text.Json;
using RayvarzResend.Web.Models;
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
/// فاز ۰: بارگذاری live از Sara + مقایسه ردیف‌های Legacy با RuleGoldenExpectedRow.
/// </summary>
public sealed class GoldenDryRunService
{
    private readonly RuleEngineStore _store;
    private readonly FicheRepository _fiches;
    private readonly IConfiguration _config;

    public GoldenDryRunService(RuleEngineStore store, FicheRepository fiches, IConfiguration config)
    {
        _store = store;
        _fiches = fiches;
        _config = config;
    }

    public async Task<GoldenDryRunSummary> RunAllAsync(bool compareExpectedRows = true, CancellationToken ct = default)
    {
        var nidMember = _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
        var goldens = await _store.GetActiveGoldenFichesAsync(nidMember, ct);
        var results = new List<GoldenDryRunCaseResult>();

        foreach (var g in goldens)
        {
            results.Add(await RunOneAsync(g, compareExpectedRows, ct));
        }

        return new GoldenDryRunSummary
        {
            EngineName = "LegacyCSharp",
            Total = results.Count,
            Passed = results.Count(r => r.Success),
            Cases = results
        };
    }

    public async Task<GoldenDryRunCaseResult> RunOneAsync(
        RuleGoldenFicheRow golden, bool compareExpectedRows = true, CancellationToken ct = default)
    {
        try
        {
            var fiche = await _fiches.LoadAsync(IdentifierType.FicheNo, golden.FicheNo, ct);
            if (fiche == null)
                return Fail(golden, $"فیش {golden.FicheNo} در Sara یافت نشد (live)");

            if (fiche.Category != FicheCategory.DutyNosazi && fiche.Category != FicheCategory.DutySenfi)
                return Fail(golden, $"فیش {golden.FicheNo} از نوع Duty نیست: {fiche.Category}");

            var mismatches = new List<string>();

            if (fiche.Rows.Count != golden.ExpectedRowCount)
                mismatches.Add($"تعداد ردیف: expected={golden.ExpectedRowCount} actual={fiche.Rows.Count}");

            var rowSum = fiche.Rows.Sum(r => r.Val);
            if (rowSum != fiche.Payable)
                mismatches.Add($"جمع ردیف‌ها ({rowSum}) ≠ PayablePrice ({fiche.Payable})");

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
                fiche.Payable,
                rows = fiche.Rows.Select(r => new { r.IncmNo, r.Val, r.IncmRowDsc })
            });

            await _store.InsertDryRunResultAsync(null, null, golden.GoldenFicheId, "LegacyCSharp", success,
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
                RowSum = rowSum,
                Mismatches = mismatches
            };
        }
        catch (Exception ex)
        {
            await _store.InsertDryRunResultAsync(null, null, golden.GoldenFicheId, "LegacyCSharp", false, ex.Message, null, ct);
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
