using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

public sealed class DslExecutor
{
    private readonly IOperationRegistry _registry;

    public DslExecutor(IOperationRegistry registry) => _registry = registry;

    public DslExecutionResult Execute(DslProgram program, DslExecutionContext context)
    {
        var trace = new List<string>();
        try
        {
            var entry = program.Functions.FirstOrDefault(f =>
                f.Name.Equals(program.EntryPoint, StringComparison.OrdinalIgnoreCase));

            if (entry == null)
                return Fail("تابع Run یافت نشد.", trace);

            ExecuteFunction(entry, program, context, trace);

            if (context.Rows.Count == 0 && context.Fiche.Rows.Count > 0)
            {
                context.Rows.AddRange(context.Fiche.Rows);
            }

            if (context.Fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi
                && string.Equals(context.DispatchedFunction, "Nosazi", StringComparison.OrdinalIgnoreCase))
            {
                _registry.Invoke("Nosazi.BuildDutyRows", context, Array.Empty<string>());
                _registry.Invoke("Validate.RowSumEqualsPayable", context, Array.Empty<string>());
            }

            var rows = context.Rows.Count > 0 ? context.Rows : context.Fiche.Rows;
            var sum = rows.Sum(r => r.Val);

            return new DslExecutionResult
            {
                Success = true,
                DispatchedFunction = context.DispatchedFunction,
                Rows = rows,
                RowSum = sum,
                Trace = trace
            };
        }
        catch (Exception ex)
        {
            trace.Add($"ERROR: {ex.Message}");
            return Fail(ex.Message, trace);
        }
    }

    private void ExecuteFunction(
        DslFunction function,
        DslProgram program,
        DslExecutionContext context,
        List<string> trace)
    {
        trace.Add($"→ {function.Name}()");
        context.DispatchedFunction = function.Name;

        if (!function.IsSupported && !function.Name.Equals("Run", StringComparison.OrdinalIgnoreCase)
            && !function.Name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"تابع {function.Name} در فاز ۳ پشتیبانی نمی‌شود.");
        }

        ExecuteStatements(function.Body, program, context, trace);
    }

    private void ExecuteStatements(
        IReadOnlyList<DslStatement> statements,
        DslProgram program,
        DslExecutionContext context,
        List<string> trace)
    {
        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case DslAssignStatement assign:
                    context.Variables[assign.Target] = EvaluateExpression(assign.Expression, context);
                    trace.Add($"  {assign.Target} = …");
                    break;

                case DslCallOperationStatement op:
                {
                    var key = IOperationRegistry.BuildKey(op.Receiver, op.Operation);
                    _registry.Invoke(key, context, op.Arguments);
                    trace.Add($"  call {key}");
                    break;
                }

                case DslCallFunctionStatement fn:
                {
                    var target = program.Functions.FirstOrDefault(f =>
                        f.Name.Equals(fn.FunctionName, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(f.DisplayName, fn.FunctionName, StringComparison.Ordinal));
                    if (target == null)
                        throw new InvalidOperationException($"تابع {fn.FunctionName} یافت نشد.");
                    ExecuteFunction(target, program, context, trace);
                    break;
                }

                case DslIfStatement iff:
                    if (EvaluateCondition(iff.Condition, context))
                    {
                        ExecuteStatements(iff.ThenBranch, program, context, trace);
                    }
                    else
                    {
                        var matched = false;
                        foreach (var branch in iff.ElseIfBranches)
                        {
                            if (!EvaluateCondition(branch.Condition, context))
                                continue;
                            ExecuteStatements(branch.Body, program, context, trace);
                            matched = true;
                            break;
                        }

                        if (!matched && iff.ElseBranch != null)
                            ExecuteStatements(iff.ElseBranch, program, context, trace);
                    }
                    break;

                case DslTryCatchStatement t:
                    try
                    {
                        ExecuteStatements(t.TryBranch, program, context, trace);
                    }
                    catch (Exception ex)
                    {
                        if (t.CatchBranch == null)
                            throw;
                        if (!string.IsNullOrWhiteSpace(t.CatchVariable))
                            context.Variables[t.CatchVariable] = ex.Message;
                        ExecuteStatements(t.CatchBranch, program, context, trace);
                    }
                    break;

                case DslReturnStatement ret:
                    context.LastReturnValue = ret.Expression;
                    trace.Add($"  return {ret.Expression ?? "null"}");
                    if (!string.IsNullOrWhiteSpace(ret.Expression)
                        && ret.Expression.Contains('(')
                        && TryParseInlineCall(ret.Expression, out var fnName))
                    {
                        var target = program.Functions.FirstOrDefault(f =>
                            f.Name.Equals(fnName, StringComparison.OrdinalIgnoreCase));
                        if (target != null)
                            ExecuteFunction(target, program, context, trace);
                    }
                    return;

                case DslUnsupportedStatement u:
                    throw new InvalidOperationException($"{u.Reason}: {u.SourceSnippet}");
            }
        }
    }

    private static bool TryParseInlineCall(string expression, out string functionName)
    {
        functionName = "";
        var open = expression.IndexOf('(');
        if (open <= 0)
            return false;
        functionName = expression[..open].Trim();
        return !string.IsNullOrWhiteSpace(functionName);
    }

    private static object? EvaluateExpression(string expression, DslExecutionContext context)
    {
        if (TryParseInlineCall(expression, out var fnName))
        {
            var key = IOperationRegistry.BuildKey(null, fnName);
            if (context.Variables.TryGetValue(expression, out var cached))
                return cached;
        }

        return expression;
    }

    private static bool EvaluateCondition(string condition, DslExecutionContext context)
    {
        var normalized = condition.Replace(" ", "", StringComparison.Ordinal);
        if (normalized.Contains("DutyFicheResultList.Count>", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("DutyFicheResultList.Count>=", StringComparison.OrdinalIgnoreCase))
        {
            return context.Fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi;
        }

        if (normalized.Contains("IncomeFicheResultList.Count>", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("IncomeFicheResultList.Count>=", StringComparison.OrdinalIgnoreCase))
        {
            return context.Fiche.Category == FicheCategory.Income;
        }

        if (bool.TryParse(condition, out var b))
            return b;

        return false;
    }

    private static DslExecutionResult Fail(string message, List<string> trace) =>
        new() { Success = false, ErrorMessage = message, Trace = trace };
}
