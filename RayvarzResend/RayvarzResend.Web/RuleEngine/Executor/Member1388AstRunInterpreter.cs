using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>اجرای Run از AST — ترتیب Call و شرط‌ها از XmlBody/VB، نه Member1388Catalog ثابت.</summary>
public static class Member1388AstRunInterpreter
{
    public static bool TryExecute(
        DslProgram program,
        DslExecutionContext context,
        IOperationRegistry registry,
        IList<string> trace,
        out bool astDriven)
    {
        astDriven = false;
        var runFn = program.Functions.FirstOrDefault(f =>
            f.Name.Equals("Run", StringComparison.OrdinalIgnoreCase));
        if (runFn is null)
        {
            context.CompatibilityWarnings.Add("Run: تابع Run در AST یافت نشد — fallback به catalog");
            return false;
        }

        astDriven = true;
        trace.Add("Run: AST-driven dispatch");
        ExecuteStatements(runFn.Body, program, context, registry, trace);
        return true;
    }

    private static void ExecuteStatements(
        IReadOnlyList<DslStatement> statements,
        DslProgram program,
        DslExecutionContext context,
        IOperationRegistry registry,
        IList<string> trace)
    {
        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case DslAssignStatement assign:
                    context.Variables[assign.Target] = assign.Expression;
                    if (assign.Target.Contains("Ref", StringComparison.OrdinalIgnoreCase))
                        RefParameterCollector.TrackAssignment(context, assign.Target, assign.Expression);
                    break;

                case DslCallFunctionStatement fn:
                    Member1388RunDispatcher.DispatchChild(fn.FunctionName, context, registry, trace);
                    break;

                case DslIfStatement iff:
                    if (DslConditionEvaluator.Evaluate(iff.Condition, context))
                    {
                        ExecuteStatements(iff.ThenBranch, program, context, registry, trace);
                    }
                    else
                    {
                        var matched = false;
                        foreach (var branch in iff.ElseIfBranches)
                        {
                            if (!DslConditionEvaluator.Evaluate(branch.Condition, context))
                                continue;
                            ExecuteStatements(branch.Body, program, context, registry, trace);
                            matched = true;
                            break;
                        }

                        if (!matched && iff.ElseBranch != null)
                            ExecuteStatements(iff.ElseBranch, program, context, registry, trace);
                    }
                    break;

                case DslTryCatchStatement t:
                    ExecuteStatements(t.TryBranch, program, context, registry, trace);
                    break;

                case DslUnsupportedStatement u:
                    context.DeferredRuleLines.Add(u.SourceSnippet);
                    context.CompatibilityWarnings.Add($"Run defer: {Truncate(u.SourceSnippet, 60)}");
                    trace.Add($"  Run defer: {Truncate(u.SourceSnippet, 60)}");
                    break;

                case DslCallOperationStatement op:
                {
                    var key = IOperationRegistry.BuildKey(op.Receiver, op.Operation);
                    if (!_IsBenignRunOperation(key))
                    {
                        context.CompatibilityWarnings.Add($"Run: operation غیرپشتیبانی {key}");
                        trace.Add($"  Run unsupported op: {key}");
                        break;
                    }

                    if (registry.IsKnown(key))
                        registry.Invoke(key, context, op.Arguments);
                    break;
                }
            }
        }
    }

    private static bool _IsBenignRunOperation(string key) =>
        key.Contains("ListRefP", StringComparison.OrdinalIgnoreCase)
        || key.Contains("AddError", StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string? s, int max) =>
        string.IsNullOrEmpty(s) ? "" : s.Length <= max ? s : s[..max] + "…";
}
