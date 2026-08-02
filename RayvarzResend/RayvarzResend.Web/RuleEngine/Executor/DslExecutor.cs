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

            // پس از همان Call chain فایل اصلی: ردیف از فیش live بر اساس Category
            FinalizeRowsByCategory(context, trace);

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

    private void FinalizeRowsByCategory(DslExecutionContext context, List<string> trace)
    {
        if (context.Fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi)
        {
            if (!context.InvokedFunctions.Any(f =>
                    SupportedDslFunctions.IsNosazi(f, null)
                    || f.Equals("Nosazi", StringComparison.OrdinalIgnoreCase)))
            {
                // اگر شرط Run به‌هر دلیل Nosazi را صدا نزد، باز هم Duty باید ردیف داشته باشد
                context.DispatchedFunction = "Nosazi";
                context.InvokedFunctions.Add("Nosazi");
                trace.Add("→ Nosazi() [category fallback for Duty]");
            }
            else
            {
                context.DispatchedFunction = "Nosazi";
            }

            _registry.Invoke("Nosazi.BuildDutyRows", context, Array.Empty<string>());
            _registry.Invoke("Validate.RowSumEqualsPayable", context, Array.Empty<string>());
            return;
        }

        if (context.Fiche.Category == FicheCategory.Income)
        {
            if (!context.InvokedFunctions.Any(SupportedDslFunctions.IsIncome))
            {
                context.DispatchedFunction = "iNcOME";
                context.InvokedFunctions.Add("iNcOME");
                trace.Add("→ iNcOME() [category fallback for Income]");
            }
            else if (string.IsNullOrWhiteSpace(context.DispatchedFunction)
                     || !SupportedDslFunctions.IsIncome(context.DispatchedFunction))
            {
                // آخرین Call ممکن است BazAfarine/Tahator باشد — برای Build از iNcOME استفاده کن
                context.DispatchedFunction = context.InvokedFunctions.FirstOrDefault(SupportedDslFunctions.IsIncome)
                    ?? "iNcOME";
            }

            _registry.Invoke("Income.BuildIncomeRows", context, Array.Empty<string>());
            _registry.Invoke("Validate.RowSumEqualsPayable", context, Array.Empty<string>());
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
        if (!SupportedDslFunctions.IsEntryPoint(function.Name))
            context.InvokedFunctions.Add(function.Name);

        // DryRun: بدنه همه به‌جز Run skip — ردیف‌ها از Fiche live
        if (context.DryRun && SupportedDslFunctions.IsDryRunBodySkip(function.Name, function.DisplayName))
        {
            if (SupportedDslFunctions.IsNosazi(function.Name, function.DisplayName))
                context.DispatchedFunction = "Nosazi";
            trace.Add($"→ {function.Name}() [DryRun: body skipped — same Call order as XmlBody]");
            return;
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
                    // خطوط VB خارج از subset — در DryRun رد؛ ردیف از Fiche live.
                    if (context.DryRun)
                    {
                        trace.Add($"  skip unsupported: {Truncate(u.SourceSnippet, 80)}");
                        break;
                    }
                    throw new InvalidOperationException($"{u.Reason}: {u.SourceSnippet}");
            }
        }
    }

    private static string Truncate(string? s, int max) =>
        string.IsNullOrEmpty(s) ? "" : s.Length <= max ? s : s[..max] + "…";

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
        if (TryParseInlineCall(expression, out _))
        {
            if (context.Variables.TryGetValue(expression, out var cached))
                return cached;
        }

        return expression;
    }

    /// <summary>
    /// شرط‌های Run فایل اصلی Member 1388 + الگوی ساده fixture (Duty/Income list Count).
    /// </summary>
    internal static bool EvaluateCondition(string condition, DslExecutionContext context)
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

        // AccountingDocumentingCause = Confirm → مسیر عادی ارسال/resend
        if (normalized.Contains("AccountingDocumentingCause", StringComparison.OrdinalIgnoreCase)
            && normalized.Contains("Confirm", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // AccountingDocumentingCause = 7 → iNcOMECheck (مسیر خاص؛ در resend عادی نه)
        if (normalized.Contains("AccountingDocumentingCause", StringComparison.OrdinalIgnoreCase)
            && (normalized.Contains("=7", StringComparison.Ordinal)
                || normalized.EndsWith("=7", StringComparison.Ordinal)))
        {
            return false;
        }

        // ObjOnPrice = Income → فیش درآمدی
        if (normalized.Contains("ObjOnPrice", StringComparison.OrdinalIgnoreCase)
            && normalized.Contains("Income", StringComparison.OrdinalIgnoreCase))
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
