namespace Sara.RuleDebugger;

public sealed class DebugSessionState
{
    public Guid SessionId { get; init; }
    public int CurrentLine { get; init; }
    public int CurrentColumn { get; init; }
    public ExecutionState ExecutionState { get; init; }
    public string CurrentStatement { get; init; } = "";
    public IReadOnlyList<DebugVariable> Locals { get; init; } = Array.Empty<DebugVariable>();
    public IReadOnlyList<string> CallStack { get; init; } = Array.Empty<string>();
    public DebugFault? Fault { get; init; }
}
