namespace Sara.RuleDebugger;

public sealed class DebugFault
{
    public int LineNumber { get; init; }
    public int Column { get; init; }
    public string FailingStatement { get; init; } = "";
    public string ExceptionType { get; init; } = "";
    public string ErrorMessage { get; init; } = "";
    public IReadOnlyList<DebugVariable> VariablesSnapshot { get; init; } = Array.Empty<DebugVariable>();
}
