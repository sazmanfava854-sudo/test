namespace RayvarzResend.Web.RuleEngine.Engines;

/// <summary>
/// موتور قوانین فیش — فاز ۱: Legacy (DutyNosaziLogic + SoapBuilder).
/// فیش باید از <see cref="Services.FicheRepository"/> live بارگذاری شده باشد.
/// </summary>
public interface IFicheRuleEngine
{
    string EngineName { get; }

    Task<FicheRuleEvaluationResult> EvaluateAsync(
        FicheRuleContext context,
        bool buildSoap = false,
        CancellationToken ct = default);
}
