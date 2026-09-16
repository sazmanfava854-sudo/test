namespace Sara.RuleDebugger;

public interface IRuleDebuggerService
{
    DebugSession InitializeSession(string scriptText, object? hostContext = null);
    DebugStepResult StepNext(Guid sessionId);
    DebugSessionState Continue(Guid sessionId);
    void Reset(Guid sessionId);
    IReadOnlyList<DebugVariable> GetLocals(Guid sessionId);
    DebugSessionState GetSessionState(Guid sessionId);
}
