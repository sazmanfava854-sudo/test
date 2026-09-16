using Sara.RuleDebugger;
using Xunit;

namespace Sara.RuleDebugger.Tests;

public class DebuggerEngineTests
{
    private readonly IRuleDebuggerService _svc = new RuleDebuggerService();

    [Fact]
    public void StepNext_exposes_locals_after_each_statement()
    {
        var session = _svc.InitializeSession("""
            int a = 10;
            int b = 20;
            int c = a + b;
            """);
        Assert.Equal(ExecutionState.Ready, session.ExecutionState);
        Assert.Equal(1, session.CurrentLine);

        var s1 = _svc.StepNext(session.Id);
        Assert.Equal(ExecutionState.Paused, s1.State.ExecutionState);
        Assert.Equal("10", Local(s1, "a"));

        var s2 = _svc.StepNext(session.Id);
        Assert.Equal("20", Local(s2, "b"));

        var s3 = _svc.StepNext(session.Id);
        Assert.True(s3.Completed);
        Assert.Equal("30", Local(s3, "c"));
        Assert.Equal(typeof(int).FullName, s3.State.Locals.First(v => v.Name == "c").TypeName);
    }

    [Fact]
    public void DivideByZero_is_pinned_to_the_failing_line_with_snapshot()
    {
        var session = _svc.InitializeSession("""
            int a = 10;
            int b = 0;
            int c = a / b;
            """);
        _svc.StepNext(session.Id);
        _svc.StepNext(session.Id);
        var faulted = _svc.StepNext(session.Id);

        Assert.True(faulted.Faulted);
        Assert.NotNull(faulted.State.Fault);
        Assert.Equal(3, faulted.State.Fault!.LineNumber);
        Assert.Contains("a / b", faulted.State.Fault.FailingStatement);
        Assert.Equal(typeof(DivideByZeroException).FullName, faulted.State.Fault.ExceptionType);
        Assert.Contains("Division by zero", faulted.State.Fault.ErrorMessage);
        Assert.Equal("10", LocalName(faulted.State.Fault.VariablesSnapshot, "a"));
        Assert.Equal("0", LocalName(faulted.State.Fault.VariablesSnapshot, "b"));
        Assert.DoesNotContain(faulted.State.Fault.VariablesSnapshot, v => v.Name == "c");
    }

    [Fact]
    public void NullReference_is_pinned_and_does_not_crash_the_host()
    {
        var session = _svc.InitializeSession("""
            string s = null;
            int n = s.Length;
            """);
        _svc.StepNext(session.Id);
        var faulted = _svc.StepNext(session.Id);
        Assert.True(faulted.Faulted);
        Assert.Equal(2, faulted.State.Fault!.LineNumber);
        Assert.Contains("s.Length", faulted.State.Fault.FailingStatement);
        Assert.Equal(typeof(NullReferenceException).FullName, faulted.State.Fault.ExceptionType);
        Assert.Contains("Null reference", faulted.State.Fault.ErrorMessage);
    }

    [Fact]
    public void Missing_host_property_is_a_structured_fault()
    {
        var session = _svc.InitializeSession("int v = Host.DoesNotExist;", new { ActiveNidZabeteh = "" });
        var faulted = _svc.StepNext(session.Id);
        Assert.True(faulted.Faulted);
        Assert.Equal(1, faulted.State.Fault!.LineNumber);
        Assert.Contains("DoesNotExist", faulted.State.Fault.FailingStatement);
        Assert.Contains("Missing context", faulted.State.Fault.ErrorMessage);
    }

    [Fact]
    public void Host_context_properties_are_readable_as_globals()
    {
        var session = _svc.InitializeSession(
            "int work = Host.WorkItem;",
            new { WorkItem = 300002275 });
        var done = _svc.Continue(session.Id);
        Assert.Equal(ExecutionState.Completed, done.ExecutionState);
        Assert.Equal("300002275", LocalName(done.Locals, "work"));
        Assert.Contains(done.Locals, v => v.Name == "Host.WorkItem" && v.Scope == "Global");
    }

    [Fact]
    public void If_false_branch_is_skipped()
    {
        var session = _svc.InitializeSession("""
            int x = 1;
            int y = 0;
            if (x > 5) y = 9;
            """);
        var done = _svc.Continue(session.Id);
        Assert.Equal(ExecutionState.Completed, done.ExecutionState);
        Assert.Equal("0", LocalName(done.Locals, "y"));
    }

    [Fact]
    public void If_true_branch_assigns()
    {
        var session = _svc.InitializeSession("""
            int x = 8;
            int y = 0;
            if (x > 5) y = 9;
            """);
        var done = _svc.Continue(session.Id);
        Assert.Equal("9", LocalName(done.Locals, "y"));
    }

    [Fact]
    public void While_loop_steps_until_condition_fails()
    {
        var session = _svc.InitializeSession("""
            int i = 0;
            int s = 0;
            while (i < 3)
            {
                s = s + i;
                i = i + 1;
            }
            """);
        var done = _svc.Continue(session.Id);
        Assert.Equal(ExecutionState.Completed, done.ExecutionState);
        Assert.Equal("3", LocalName(done.Locals, "s"));
        Assert.Equal("3", LocalName(done.Locals, "i"));
    }

    [Fact]
    public void Reset_replays_from_the_first_line()
    {
        var session = _svc.InitializeSession("""
            int a = 1;
            a = a + 1;
            """);
        _svc.Continue(session.Id);
        Assert.Equal("2", LocalName(_svc.GetLocals(session.Id), "a"));
        _svc.Reset(session.Id);
        var state = _svc.GetSessionState(session.Id);
        Assert.Equal(ExecutionState.Ready, state.ExecutionState);
        Assert.Equal(1, state.CurrentLine);
        Assert.DoesNotContain(state.Locals, v => v.Name == "a" && v.Scope == "Local");
        var again = _svc.Continue(session.Id);
        Assert.Equal("2", LocalName(again.Locals, "a"));
    }

    [Fact]
    public void Parse_error_faults_without_throwing()
    {
        var session = _svc.InitializeSession("int a = ;");
        var state = _svc.GetSessionState(session.Id);
        Assert.Equal(ExecutionState.Faulted, state.ExecutionState);
        Assert.NotNull(state.Fault);
        Assert.Contains("did not parse", state.Fault!.ErrorMessage);
    }

    [Fact]
    public void Continue_after_fault_stays_faulted()
    {
        var session = _svc.InitializeSession("""
            int b = 0;
            int c = 1 / b;
            """);
        var faulted = _svc.Continue(session.Id);
        Assert.True(faulted.ExecutionState == ExecutionState.Faulted);
        var again = _svc.StepNext(session.Id);
        Assert.True(again.Faulted);
        Assert.Equal(2, again.State.Fault!.LineNumber);
    }

    private static string Local(DebugStepResult step, string name) => LocalName(step.State.Locals, name);

    private static string LocalName(IReadOnlyList<DebugVariable> locals, string name)
    {
        var hit = locals.FirstOrDefault(v => v.Name == name);
        Assert.NotNull(hit);
        return hit!.ValueString;
    }
}
