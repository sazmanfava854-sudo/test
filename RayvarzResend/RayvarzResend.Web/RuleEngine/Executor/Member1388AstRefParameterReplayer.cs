using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>اجرای بخش RefParameter از بدنه AST تابع — hybrid Member 1388.</summary>
public static class Member1388AstRefParameterReplayer
{
    public static void Replay(
        IReadOnlyList<DslStatement> body,
        DslExecutionContext context,
        IOperationRegistry registry,
        IList<string> trace)
    {
        ReplayStatements(body, context, registry, trace);
        var refs = RefParameterCollector.GetOrCreateList(context);
        var result = RefParameterRegistry.ApplyAll(context.Fiche, refs, context.CompatibilityWarnings);
        foreach (var name in result.UnknownNames)
            trace.Add($"  ref warning: unknown {name}");
        if (result.AppliedCount > 0)
            trace.Add($"  ref applied: {result.AppliedCount}");
    }

    private static void ReplayStatements(
        IReadOnlyList<DslStatement> statements,
        DslExecutionContext context,
        IOperationRegistry registry,
        IList<string> trace)
    {
        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case DslAssignStatement assign:
                    if (!IsRefRelated(assign.Target))
                        break;
                    context.Variables[assign.Target] = assign.Expression;
                    RefParameterCollector.TrackAssignment(context, assign.Target, assign.Expression);
                    trace.Add($"  ref assign {assign.Target}");
                    break;

                case DslCallOperationStatement op:
                {
                    var key = IOperationRegistry.BuildKey(op.Receiver, op.Operation);
                    if (!IsRefOperation(key))
                        break;
                    if (registry.IsKnown(key))
                        registry.Invoke(key, context, op.Arguments);
                    else
                        RefParameterCollector.AddPending(context, op.Arguments);
                    trace.Add($"  ref op {key}");
                    break;
                }

                case DslIfStatement iff:
                    if (DslConditionEvaluator.Evaluate(iff.Condition, context))
                        ReplayStatements(iff.ThenBranch, context, registry, trace);
                    else
                    {
                        var matched = false;
                        foreach (var branch in iff.ElseIfBranches)
                        {
                            if (!DslConditionEvaluator.Evaluate(branch.Condition, context))
                                continue;
                            ReplayStatements(branch.Body, context, registry, trace);
                            matched = true;
                            break;
                        }

                        if (!matched && iff.ElseBranch != null)
                            ReplayStatements(iff.ElseBranch, context, registry, trace);
                    }
                    break;

                case DslTryCatchStatement t:
                    ReplayStatements(t.TryBranch, context, registry, trace);
                    break;
            }
        }
    }

    private static bool IsRefRelated(string target) =>
        target.Contains("Ref", StringComparison.OrdinalIgnoreCase)
        || target.Contains("ListRefP", StringComparison.OrdinalIgnoreCase);

    private static bool IsRefOperation(string key) =>
        key.Contains("ListRefP", StringComparison.OrdinalIgnoreCase)
        || key.Contains("RefP", StringComparison.OrdinalIgnoreCase);
}
