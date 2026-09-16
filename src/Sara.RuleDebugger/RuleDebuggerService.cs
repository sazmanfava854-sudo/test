using System.Collections.Concurrent;

namespace Sara.RuleDebugger;

public sealed class RuleDebuggerService : IRuleDebuggerService
{
    private readonly ConcurrentDictionary<Guid, DebugSession> _sessions = new();

    public DebugSession InitializeSession(string scriptText, object? hostContext = null)
    {
        var session = new DebugSession(scriptText, hostContext);
        _sessions[session.Id] = session;
        return session;
    }

    public DebugStepResult StepNext(Guid sessionId) => Session(sessionId).StepNext();

    public DebugSessionState Continue(Guid sessionId) => Session(sessionId).Continue();

    public void Reset(Guid sessionId) => Session(sessionId).Reset();

    public IReadOnlyList<DebugVariable> GetLocals(Guid sessionId) => Session(sessionId).GetLocals();

    public DebugSessionState GetSessionState(Guid sessionId) => Session(sessionId).Snapshot();

    public bool Drop(Guid sessionId) => _sessions.TryRemove(sessionId, out _);

    private DebugSession Session(Guid sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
            return session;
        throw new KeyNotFoundException("Debug session not found: " + sessionId);
    }
}
