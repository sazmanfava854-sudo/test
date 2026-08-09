using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

/// <summary>
/// اجرای DSL: اول قوانین بر اساس نوع فیش (Run + Call chain)، بعد Build*Rows، بعد آماده‌سازی برای SOAP.
/// هیچ تابعی Unsupported نیست؛ خطوط غیرقابل‌parse فقط soft-skip می‌شوند.
/// </summary>
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

            trace.Add($"PreSOAP: نوع فیش={context.Fiche.Category}, DocTyp={context.Fiche.DocTyp}");
            foreach (var fn in program.Functions)
            {
                var role = SupportedDslFunctions.GetRole(fn.Name, fn.DisplayName);
                var applies = SupportedDslFunctions.AppliesToFiche(fn.Name, fn.DisplayName, context.Fiche);
                trace.Add($"  fn {fn.Name} role={role} applies={applies}");
            }

            // ۱) اجرای قوانین (Run و Call chain وابسته به نوع فیش)
            ExecuteFunction(entry, program, context, trace);

            // ۲) ساخت ردیف از فیش live پس از اعمال قوانین
            FinalizeRowsByCategory(context, trace);

            // ۳) تأیید نقش‌های اجباری قبل از SOAP
            var preSoapErrors = ValidateRequiredRolesBeforeSoap(context, trace);

            var rows = context.Rows.Count > 0 ? context.Rows : context.Fiche.Rows;
            var sum = rows.Sum(r => r.Val);

            if (preSoapErrors.Count > 0)
            {
                return new DslExecutionResult
                {
                    Success = false,
                    ErrorMessage = string.Join("; ", preSoapErrors),
                    DispatchedFunction = context.DispatchedFunction,
                    Rows = rows,
                    RowSum = sum,
                    Trace = trace,
                    AppliedFunctions = context.InvokedFunctions.ToList(),
                    SkippedNotApplicable = context.SkippedNotApplicable.ToList(),
                    PreSoapRuleErrors = preSoapErrors,
                    FunctionsWithEffect = context.FunctionsWithEffect.ToList()
                };
            }

            return new DslExecutionResult
            {
                Success = true,
                DispatchedFunction = context.DispatchedFunction,
                Rows = rows,
                RowSum = sum,
                Trace = trace,
                AppliedFunctions = context.InvokedFunctions.ToList(),
                SkippedNotApplicable = context.SkippedNotApplicable.ToList(),
                PreSoapRuleErrors = Array.Empty<string>(),
                FunctionsWithEffect = context.FunctionsWithEffect.ToList()
            };
        }
        catch (Exception ex)
        {
            trace.Add($"ERROR: {ex.Message}");
            return Fail(ex.Message, trace, context);
        }
    }

    private static List<string> ValidateRequiredRolesBeforeSoap(DslExecutionContext context, List<string> trace)
    {
        var errors = new List<string>();
        var required = SupportedDslFunctions.RequiredRolesBeforeSoap(context.Fiche);
        foreach (var role in required)
        {
            var hit = context.InvokedFunctions.Any(name =>
                SupportedDslFunctions.GetRole(name) == role);
            if (!hit)
            {
                // fallback در FinalizeRows ممکن است نقش را پر کرده باشد
                if (role == DslFunctionRole.Duty
                    && context.DispatchedFunction.Equals("Nosazi", StringComparison.OrdinalIgnoreCase))
                    hit = true;
                if (role == DslFunctionRole.Income
                    && (SupportedDslFunctions.IsIncome(context.DispatchedFunction)
                        || SupportedDslFunctions.IsIncomeCheck(context.DispatchedFunction)))
                    hit = true;
                if (role == DslFunctionRole.Tahator
                    && SupportedDslFunctions.IsTahator(context.DispatchedFunction))
                    hit = true;
                if (role == DslFunctionRole.IncomeCheck
                    && context.InvokedFunctions.Any(SupportedDslFunctions.IsIncomeCheck))
                    hit = true;
            }

            if (!hit)
            {
                var msg = $"قبل از SOAP: نقش اجباری {role} برای فیش {context.Fiche.Category}/DocTyp={context.Fiche.DocTyp} اعمال نشد.";
                errors.Add(msg);
                trace.Add("ERROR: " + msg);
            }
            else
            {
                trace.Add($"PreSOAP OK: نقش {role} اعمال شد");
            }
        }

        return errors;
    }

    private void FinalizeRowsByCategory(DslExecutionContext context, List<string> trace)
    {
        if (context.Fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi)
        {
            if (!context.InvokedFunctions.Any(f => SupportedDslFunctions.IsNosazi(f, null)))
            {
                context.DispatchedFunction = "Nosazi";
                context.InvokedFunctions.Add("Nosazi");
                trace.Add("→ Nosazi() [اعمال قانون Duty قبل از SOAP]");
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
            var hasIncomePath = context.InvokedFunctions.Any(SupportedDslFunctions.IsIncome)
                || context.InvokedFunctions.Any(SupportedDslFunctions.IsTahator)
                || context.InvokedFunctions.Any(SupportedDslFunctions.IsIncomeCheck);

            if (!hasIncomePath)
            {
                context.DispatchedFunction = "iNcOME";
                context.InvokedFunctions.Add("iNcOME");
                trace.Add("→ iNcOME() [اعمال قانون Income قبل از SOAP]");
            }
            else
            {
                // آخرین Call ممکن است Tahator باشد — برای Build ردیف، تابع درآمدی اول را بگیر
                context.DispatchedFunction = context.InvokedFunctions.FirstOrDefault(SupportedDslFunctions.IsIncome)
                    ?? context.InvokedFunctions.FirstOrDefault(SupportedDslFunctions.IsTahator)
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
        var applies = SupportedDslFunctions.AppliesToFiche(function.Name, function.DisplayName, context.Fiche);
        var role = SupportedDslFunctions.GetRole(function.Name, function.DisplayName);

        if (!SupportedDslFunctions.IsEntryPoint(function.Name) && !applies)
        {
            context.SkippedNotApplicable.Add(function.Name);
            trace.Add($"→ {function.Name}() [غیرمرتبط با نوع فیش {context.Fiche.Category} — نقش={role}]");
            return;
        }

        trace.Add($"→ {function.Name}() [اعمال قانون role={role}]");
        context.DispatchedFunction = function.Name;
        if (!SupportedDslFunctions.IsEntryPoint(function.Name))
            context.InvokedFunctions.Add(function.Name);

        if (SupportedDslFunctions.IsNosazi(function.Name, function.DisplayName))
            context.DispatchedFunction = "Nosazi";

        if (context.Member1388FullExecution && Member1388Catalog.IsCatalogFunction(function.Name))
        {
            var m1388 = Member1388FunctionExecutor.Execute(function.Name, context, _registry);
            foreach (var line in m1388.Trace)
                trace.Add(line);
            if (m1388.WasHandled)
            {
                if (m1388.HadEffect)
                    context.FunctionsWithEffect.Add(function.Name);
                if (m1388.SkipFunctionBody)
                {
                    context.SkipCurrentFunctionBody = false;
                    return;
                }
            }
        }

        context.SkipCurrentFunctionBody = false;
        // بدنه AST اجرا می‌شود — نه skip کل تابع به‌عنوان Unsupported
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
                    RefParameterCollector.TrackAssignment(context, assign.Target, assign.Expression);
                    trace.Add($"  {assign.Target} = …");
                    break;

                case DslCallOperationStatement op:
                {
                    var key = IOperationRegistry.BuildKey(op.Receiver, op.Operation);
                    if (!_registry.IsKnown(key))
                    {
                        // قبل از SOAP: operation ناشناخته مانع کل قانون نمی‌شود
                        context.DeferredRuleLines.Add(key);
                        trace.Add($"  defer op (unknown): {key}");
                        break;
                    }

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
                    // خط VB خارج از subset — قانون تابع همچنان Supported است؛ فقط این خط به تعویق
                    context.DeferredRuleLines.Add(u.SourceSnippet);
                    trace.Add($"  defer line: {Truncate(u.SourceSnippet, 80)}");
                    break;
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

        if (normalized.Contains("AccountingDocumentingCause", StringComparison.OrdinalIgnoreCase)
            && normalized.Contains("Confirm", StringComparison.OrdinalIgnoreCase))
        {
            var cause = context.Fiche.AccountingDocumentingCause ?? Member1388AccountingCause.Confirm;
            return cause == Member1388AccountingCause.Confirm;
        }

        if (normalized.Contains("AccountingDocumentingCause", StringComparison.OrdinalIgnoreCase)
            && (normalized.Contains("=7", StringComparison.Ordinal)
                || normalized.EndsWith("=7", StringComparison.Ordinal)))
        {
            var cause = context.Fiche.AccountingDocumentingCause ?? Member1388AccountingCause.Confirm;
            return cause == Member1388AccountingCause.InstallmentCheck;
        }

        if (normalized.Contains("ObjOnPrice", StringComparison.OrdinalIgnoreCase)
            && normalized.Contains("Income", StringComparison.OrdinalIgnoreCase))
        {
            return context.Fiche.Category == FicheCategory.Income;
        }

        if (normalized.Contains("ExistRayvarz=False", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("ExistRayvarz=0", StringComparison.OrdinalIgnoreCase))
        {
            if (context.Variables.TryGetValue("ExistRayvarz", out var ev) && ev is bool existVal)
                return !existVal;
            return !context.Fiche.ExistsInRayvarz;
        }

        if (normalized.Contains("ExistRayvarz=True", StringComparison.OrdinalIgnoreCase))
        {
            if (context.Variables.TryGetValue("ExistRayvarz", out var ev2) && ev2 is bool existVal2)
                return existVal2;
            return context.Fiche.ExistsInRayvarz;
        }

        if (normalized.Contains("Payable>0", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("PayablePrice>0", StringComparison.OrdinalIgnoreCase))
            return context.Fiche.Payable > 0;

        if (normalized.Contains("CI_IncomeAccountGroup=157", StringComparison.OrdinalIgnoreCase))
            return context.Fiche.IncomeAccountGroup == 157;
        if (normalized.Contains("CI_IncomeAccountGroup=158", StringComparison.OrdinalIgnoreCase))
            return context.Fiche.IncomeAccountGroup == 158;
        if (normalized.Contains("CI_IncomeAccountGroup=163", StringComparison.OrdinalIgnoreCase))
            return context.Fiche.IncomeAccountGroup == 163;
        if (normalized.Contains("CI_IncomeAccountGroup=164", StringComparison.OrdinalIgnoreCase))
            return context.Fiche.IncomeAccountGroup == 164;

        if (bool.TryParse(condition, out var b))
            return b;

        return false;
    }

    private static DslExecutionResult Fail(string message, List<string> trace, DslExecutionContext? context = null) =>
        new()
        {
            Success = false,
            ErrorMessage = message,
            Trace = trace,
            AppliedFunctions = context?.InvokedFunctions.ToList() ?? [],
            FunctionsWithEffect = context?.FunctionsWithEffect.ToList() ?? []
        };
}
