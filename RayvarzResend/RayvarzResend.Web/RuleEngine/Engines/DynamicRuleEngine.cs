namespace RayvarzResend.Web.RuleEngine.Engines;

/// <summary>
/// فاز ۳ — اجرای AST از XmlBody. فعلاً stub؛ Promote تا فاز ۴ فعال نمی‌شود.
/// </summary>
public sealed class DynamicRuleEngine : IFicheRuleEngine
{
    public string EngineName => "Dynamic";

    public Task<FicheRuleEvaluationResult> EvaluateAsync(
        FicheRuleContext context,
        bool buildSoap = false,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(new FicheRuleEvaluationResult
        {
            EngineName = EngineName,
            Success = false,
            ErrorMessage = "DynamicRuleEngine هنوز پیاده‌سازی نشده (فاز ۳). ActiveEngine را Legacy نگه دارید.",
            Fiche = context.Fiche
        });
    }
}
