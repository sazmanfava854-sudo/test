using System.Text.Json;
using System.Text.Json.Serialization;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.RuleEngine.Parser;
using RayvarzResend.Web.RuleEngine.Store;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Engines;

/// <summary>
/// اجرای AST از RuleDslSnapshot: قوانین (کلی + وابسته به نوع فیش) قبل از SOAP.
/// DryRun فقط ClsAccounting.Save را skip می‌کند؛ بدنه توابع به‌خاطر Unsupported یکجا skip نمی‌شود.
/// </summary>
public sealed class DynamicRuleEngine : IFicheRuleEngine
{
    private readonly IConfiguration _config;
    private readonly RuleEngineStore _store;
    private readonly RuleDslParserService _parser;
    private readonly DslValidator _validator;
    private readonly DslExecutor _executor;
    private readonly SoapBuilder _soap;
    private readonly LegacyRuleEngine _legacy;

    public DynamicRuleEngine(
        IConfiguration config,
        RuleEngineStore store,
        RuleDslParserService parser,
        DslValidator validator,
        DslExecutor executor,
        SoapBuilder soap,
        LegacyRuleEngine legacy)
    {
        _config = config;
        _store = store;
        _parser = parser;
        _validator = validator;
        _executor = executor;
        _soap = soap;
        _legacy = legacy;
    }

    public string EngineName => "Dynamic";

    private int NidMember => _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
    private bool FallbackToLegacy => _config.GetValue("RuleEngine:DynamicFallbackToLegacy", true);

    public async Task<FicheRuleEvaluationResult> EvaluateAsync(
        FicheRuleContext context,
        bool buildSoap = false,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var program = await LoadProgramAsync(ct);
            if (program == null)
                return await FailOrFallbackAsync(context, buildSoap, "DSL snapshot یافت نشد — POST /api/rule/dsl/parse را اجرا کنید.", ct);

            // قوانین قبل از SOAP: Run + Call chain بر اساس نوع فیش؛ خطوط VB خارج از subset defer می‌شوند.
            // DryRun=true فقط Save حسابداری را skip می‌کند (نه کل بدنه تابع).
            var validation = _validator.ValidateForPromotion(program);
            if (!validation.Success)
                return await FailOrFallbackAsync(context, buildSoap, string.Join("; ", validation.Errors.Take(5)), ct);
            var execContext = new DslExecutionContext
            {
                Fiche = context.Fiche,
                Branch = context.Branch,
                Fund = context.Fund,
                DocDate = context.DocDate,
                ActDate = context.ActDate,
                DueDate = context.DueDate,
                DryRun = true,
                BuildSoap = buildSoap,
                AllowLegacyFallback = context.AllowLegacyFallback
            };

            var executed = _executor.Execute(program, execContext);
            if (!executed.Success)
                return await FailOrFallbackAsync(context, buildSoap, executed.ErrorMessage ?? "اجرای DSL ناموفق", ct);

            var fiche = context.Fiche;
            if (executed.Rows.Count > 0)
                fiche.Rows = executed.Rows.ToList();

            if (fiche.Rows.Count == 0)
                return await FailOrFallbackAsync(context, buildSoap, "ردیف IncmNo یافت نشد", ct);

            var rowSum = executed.RowSum;
            if (!TahatorRowBuilder.RowSumMatchesPayable(fiche, rowSum))
                return await FailOrFallbackAsync(context, buildSoap,
                    $"جمع ردیف‌ها ({rowSum}) ≠ PayablePrice ({fiche.Payable})", ct);

            // فقط پس از موفقیت PreSOAP / نقش‌های اجباری
            string? soapXml = null;
            if (buildSoap)
            {
                soapXml = _soap.Build(
                    fiche, context.Branch, context.Fund,
                    context.DocDate, context.ActDate, context.DueDate);
            }

            var warning = validation.Warnings.Count > 0
                ? string.Join(" | ", validation.Warnings.Take(3))
                : null;

            return new FicheRuleEvaluationResult
            {
                EngineName = EngineName,
                Success = true,
                Fiche = fiche,
                SoapXml = soapXml,
                RowSum = rowSum,
                Warning = warning
            };
        }
        catch (Exception ex)
        {
            return await FailOrFallbackAsync(context, buildSoap, ex.Message, ct);
        }
    }

    private async Task<DslProgram?> LoadProgramAsync(CancellationToken ct)
    {
        if (_store.IsConfigured && await _store.IsSchemaReadyAsync(ct))
        {
            var snapshot = await _store.GetActiveSnapshotAsync(NidMember, ct)
                ?? await _store.GetLatestSnapshotAsync(NidMember, ct);
            if (!string.IsNullOrWhiteSpace(snapshot?.DslJson))
                return RuleDslParserService.DeserializeProgram(snapshot.DslJson);
        }

        var parsed = await _parser.ParseActiveMemberAsync(ct: ct);
        return parsed.Parse?.Program;
    }

    private bool ShouldFallback(FicheRuleContext context) =>
        context.AllowLegacyFallback && FallbackToLegacy;

    private async Task<FicheRuleEvaluationResult> FailOrFallbackAsync(
        FicheRuleContext context,
        bool buildSoap,
        string error,
        CancellationToken ct)
    {
        if (!ShouldFallback(context))
        {
            return new FicheRuleEvaluationResult
            {
                EngineName = EngineName,
                Success = false,
                ErrorMessage = error,
                Fiche = context.Fiche
            };
        }

        var fallback = await _legacy.EvaluateAsync(context, buildSoap, ct);
        return new FicheRuleEvaluationResult
        {
            EngineName = fallback.EngineName,
            Success = fallback.Success,
            ErrorMessage = fallback.ErrorMessage,
            Fiche = fallback.Fiche,
            SoapXml = fallback.SoapXml,
            RowSum = fallback.RowSum,
            Warning = CombineWarnings(error, fallback.Warning)
        };
    }

    private static string? CombineWarnings(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a)) return b;
        if (string.IsNullOrWhiteSpace(b)) return a;
        return $"Dynamic: {a} | {b}";
    }
}
