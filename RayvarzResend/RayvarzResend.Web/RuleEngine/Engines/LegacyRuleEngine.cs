using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine.Engines;

/// <summary>
/// مسیر baseline v16 — ردیف‌ها از FicheRepository/DutyNosaziLogic؛ SOAP از SoapBuilder.
/// </summary>
public sealed class LegacyRuleEngine : IFicheRuleEngine
{
    private readonly SoapBuilder _soap;

    public LegacyRuleEngine(SoapBuilder soap) => _soap = soap;

    public string EngineName => "LegacyCSharp";

    public Task<FicheRuleEvaluationResult> EvaluateAsync(
        FicheRuleContext context,
        bool buildSoap = false,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var fiche = context.Fiche;

        if (fiche.Category is not (FicheCategory.DutyNosazi or FicheCategory.DutySenfi or FicheCategory.Income))
        {
            return Task.FromResult(new FicheRuleEvaluationResult
            {
                EngineName = EngineName,
                Success = false,
                ErrorMessage = $"نوع فیش پشتیبانی نشده: {fiche.Category}",
                Fiche = fiche
            });
        }

        if (fiche.Rows.Count == 0)
        {
            return Task.FromResult(new FicheRuleEvaluationResult
            {
                EngineName = EngineName,
                Success = false,
                ErrorMessage = "ردیف IncmNo یافت نشد",
                Fiche = fiche
            });
        }

        var rowSum = fiche.Rows.Sum(r => r.Val);
        if (!TahatorRowBuilder.RowSumMatchesPayable(fiche, rowSum))
        {
            return Task.FromResult(new FicheRuleEvaluationResult
            {
                EngineName = EngineName,
                Success = false,
                ErrorMessage = $"جمع ردیف‌ها ({rowSum}) ≠ PayablePrice ({fiche.Payable})",
                Fiche = fiche,
                RowSum = rowSum
            });
        }

        string? soapXml = null;
        if (buildSoap)
        {
            soapXml = _soap.Build(
                fiche,
                context.Branch,
                context.Fund,
                context.DocDate,
                context.ActDate,
                context.DueDate);
        }

        return Task.FromResult(new FicheRuleEvaluationResult
        {
            EngineName = EngineName,
            Success = true,
            Fiche = fiche,
            SoapXml = soapXml,
            RowSum = rowSum
        });
    }
}
