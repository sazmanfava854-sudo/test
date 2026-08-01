using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Engines;

public sealed class FicheRuleContext
{
    public FicheHeaderDto Fiche { get; init; } = new();
    public int Branch { get; init; }
    public int Fund { get; init; }
    public string? DocDate { get; init; }
    public string? ActDate { get; init; }
    public string? DueDate { get; init; }
}

public sealed class FicheRuleEvaluationResult
{
    public string EngineName { get; init; } = "";
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Warning { get; init; }
    public FicheHeaderDto Fiche { get; init; } = new();
    public string? SoapXml { get; init; }
    public decimal RowSum { get; init; }
}
