namespace Sara.RuleDebugger;

public sealed class DebugStepResult
{
    public DebugSessionState State { get; init; } = new();
    public bool Completed => State.ExecutionState == ExecutionState.Completed;
    public bool Faulted => State.ExecutionState == ExecutionState.Faulted;
}
