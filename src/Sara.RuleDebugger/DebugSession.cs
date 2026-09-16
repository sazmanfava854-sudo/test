namespace Sara.RuleDebugger;

/// <summary>
/// Cooperative debug session: one Roslyn-planned instruction per StepNext, live locals, pinned faults.
/// </summary>
public sealed class DebugSession
{
    private readonly string _script;
    private readonly object? _hostContext;
    private StatementPlan _plan;
    private ScriptExecutor _executor;
    private int _ip;
    private int _currentLine;
    private int _currentColumn;
    private string _currentStatement = "";
    private ExecutionState _state;
    private DebugFault? _fault;
    private IReadOnlyList<DebugVariable> _locals = Array.Empty<DebugVariable>();

    internal DebugSession(string scriptText, object? hostContext)
    {
        Id = Guid.NewGuid();
        _script = scriptText ?? "";
        _hostContext = hostContext;
        _plan = RoslynStatementPlanner.Build(_script);
        _executor = new ScriptExecutor(_hostContext);
        Boot();
    }

    public Guid Id { get; }
    public int CurrentLine => _currentLine;
    public ExecutionState ExecutionState => _state;
    public IReadOnlyList<DebugVariable> Variables => _locals;
    public DebugFault? Fault => _fault;

    public DebugSessionState Snapshot()
    {
        return new DebugSessionState
        {
            SessionId = Id,
            CurrentLine = _currentLine,
            CurrentColumn = _currentColumn,
            ExecutionState = _state,
            CurrentStatement = _currentStatement,
            Locals = _locals,
            CallStack = BuildStack(),
            Fault = _fault
        };
    }

    public DebugStepResult StepNext()
    {
        if (_state is ExecutionState.Completed or ExecutionState.Faulted)
            return new DebugStepResult { State = Snapshot() };

        if (_plan.HasParseError)
        {
            FaultNow(FaultFactory.Parse(_plan.ParseErrorLine, _plan.ParseErrorColumn, FirstLine(_script), _plan.ParseError));
            return new DebugStepResult { State = Snapshot() };
        }

        _state = ExecutionState.Stepping;
        try
        {
            if (!MoveToUserInstruction())
            {
                Complete();
                return new DebugStepResult { State = Snapshot() };
            }

            Instruction inst = _plan.Instructions[_ip];
            _currentLine = inst.Line;
            _currentColumn = inst.Column;
            _currentStatement = inst.Source;
            var before = _executor.SnapshotLocals();

            Run(inst, before);

            if (_state == ExecutionState.Faulted)
                return new DebugStepResult { State = Snapshot() };

            AdvancePast(inst);
            if (!HasRemainingUserInstruction())
            {
                _locals = _executor.SnapshotLocals();
                Complete();
                return new DebugStepResult { State = Snapshot() };
            }

            _locals = _executor.SnapshotLocals();
            _state = ExecutionState.Paused;
            PreviewNext();
            return new DebugStepResult { State = Snapshot() };
        }
        catch (Exception ex)
        {
            var inst = _ip >= 0 && _ip < _plan.Instructions.Count
                ? _plan.Instructions[_ip]
                : new ExecInstruction { Source = _currentStatement, Line = _currentLine, Column = _currentColumn };
            FaultNow(FaultFactory.From(inst, ex, _executor.SnapshotLocals()));
            return new DebugStepResult { State = Snapshot() };
        }
    }

    public DebugSessionState Continue()
    {
        while (_state is ExecutionState.Ready or ExecutionState.Paused or ExecutionState.Stepping)
        {
            var step = StepNext();
            if (step.Completed || step.Faulted) return step.State;
        }
        return Snapshot();
    }

    public void Reset()
    {
        _plan = RoslynStatementPlanner.Build(_script);
        _executor = new ScriptExecutor(_hostContext);
        _fault = null;
        Boot();
    }

    public IReadOnlyList<DebugVariable> GetLocals() => _locals;

    private void Boot()
    {
        _ip = 0;
        _fault = null;
        _locals = Array.Empty<DebugVariable>();
        _currentStatement = "";
        if (_plan.HasParseError)
        {
            FaultNow(FaultFactory.Parse(_plan.ParseErrorLine, _plan.ParseErrorColumn, FirstLine(_script), _plan.ParseError));
            return;
        }
        if (!MoveToUserInstruction())
        {
            _currentLine = 1;
            _currentColumn = 1;
            _state = ExecutionState.Completed;
            return;
        }
        var first = _plan.Instructions[_ip];
        _currentLine = first.Line;
        _currentColumn = first.Column;
        _currentStatement = first.Source;
        _state = ExecutionState.Ready;
    }

    private void Run(Instruction inst, IReadOnlyList<DebugVariable> before)
    {
        try
        {
            if (inst is EvalJumpInstruction cond)
            {
                bool value = _executor.EvalConditionAsync(cond.Source).GetAwaiter().GetResult();
                _ip = value ? cond.ThenIndex : cond.ElseIndex;
                return;
            }
            _executor.ExecAsync(inst.Source).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            FaultNow(FaultFactory.From(inst, ex, before));
        }
    }

    private void AdvancePast(Instruction inst)
    {
        if (inst is EvalJumpInstruction)
            return; // IP already moved to then/else
        _ip++;
    }

    private bool MoveToUserInstruction()
    {
        int guard = 0;
        while (_ip >= 0 && _ip < _plan.Instructions.Count && guard++ < 10000)
        {
            Instruction inst = _plan.Instructions[_ip];
            if (inst is JumpInstruction jmp)
            {
                _ip = jmp.TargetIndex;
                continue;
            }
            if (inst.IsUserVisible)
                return true;
            _ip++;
        }
        return false;
    }

    private bool HasRemainingUserInstruction()
    {
        int saved = _ip;
        bool has = MoveToUserInstruction();
        _ip = saved;
        return has;
    }

    private void PreviewNext()
    {
        int saved = _ip;
        if (MoveToUserInstruction())
        {
            var n = _plan.Instructions[_ip];
            _currentLine = n.Line;
            _currentColumn = n.Column;
            _currentStatement = n.Source;
        }
        _ip = saved;
    }

    private void Complete()
    {
        _state = ExecutionState.Completed;
    }

    private void FaultNow(DebugFault fault)
    {
        _fault = fault;
        _state = ExecutionState.Faulted;
        _currentLine = fault.LineNumber;
        _currentColumn = fault.Column;
        _currentStatement = fault.FailingStatement;
        _locals = fault.VariablesSnapshot;
    }

    private IReadOnlyList<string> BuildStack()
    {
        var frames = new List<string>
        {
            "<rule> line " + _currentLine + (_currentStatement.Length == 0 ? "" : " | " + _currentStatement)
        };
        if (_hostContext != null)
            frames.Add("Host " + _hostContext.GetType().Name);
        return frames;
    }

    private static string FirstLine(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        int i = s.IndexOf('\n');
        return (i < 0 ? s : s.Substring(0, i)).Trim();
    }
}
