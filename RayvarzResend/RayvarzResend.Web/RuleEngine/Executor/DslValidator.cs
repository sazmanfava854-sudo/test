using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

public sealed class DslValidator
{
    private readonly IOperationRegistry _registry;

    public DslValidator(IOperationRegistry registry) => _registry = registry;

    public DslValidationResult Validate(DslProgram program, bool strictUnsupportedStatements = true) =>
        ValidateCore(program, strictUnsupportedStatements, promotionMode: false);

    /// <summary>فاز ۴ promote: EntryPoint + operationهای واقعی Sara؛ golden dry-run گیت اصلی است.</summary>
    public DslValidationResult ValidateForPromotion(DslProgram program) =>
        ValidateCore(program, strictUnsupportedStatements: false, promotionMode: true);

    private DslValidationResult ValidateCore(DslProgram program, bool strictUnsupportedStatements, bool promotionMode)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var unknownOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!program.HasEntryPoint)
            errors.Add("تابع Run (EntryPoint) در DSL یافت نشد.");

        if (!program.HasNosazi)
            warnings.Add("تابع Nosazi در DSL یافت نشد.");

        if (!program.HasIncome)
            warnings.Add("تابع iNcOME (درآمد) در DSL یافت نشد.");

        foreach (var fn in program.UnsupportedFunctions)
            warnings.Add($"تابع پشتیبانی‌نشده در Parser: {fn}");

        foreach (var fn in program.Functions)
        {
            if (!fn.IsSupported)
                continue;

            WalkStatements(fn.Body, unknownOps, errors, warnings, strictUnsupportedStatements);
        }

        foreach (var op in unknownOps)
        {
            if (promotionMode && IsLikelyMisParsedOperationKey(op))
                continue;
            if (!_registry.IsKnown(op))
                errors.Add($"Operation ناشناخته: {op}");
        }

        return new DslValidationResult
        {
            Success = errors.Count == 0,
            Errors = errors,
            Warnings = warnings.Concat(program.Warnings).Distinct().ToList(),
            UnknownOperations = unknownOps
                .Where(o => !_registry.IsKnown(o) && !(promotionMode && IsLikelyMisParsedOperationKey(o)))
                .ToList()
        };
    }

    private static bool IsLikelyMisParsedOperationKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return true;
        if (key.Contains('='))
            return true;
        if (key.Contains(' '))
            return true;
        if (key.StartsWith("Select", StringComparison.OrdinalIgnoreCase))
            return true;
        if (key.StartsWith("Dim", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private void WalkStatements(
        IReadOnlyList<DslStatement> statements,
        HashSet<string> unknownOps,
        List<string> errors,
        List<string> warnings,
        bool strictUnsupportedStatements)
    {
        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case DslUnsupportedStatement u:
                    // خط VB خارج از subset — تابع همچنان Supported است؛ در اجرا defer می‌شود
                    if (strictUnsupportedStatements)
                        errors.Add($"Deferred VB line: {u.Reason} — {u.SourceSnippet}");
                    else
                        warnings.Add($"Deferred VB line (evaluated before SOAP): {u.SourceSnippet}");
                    break;
                case DslCallOperationStatement op:
                    unknownOps.Add(IOperationRegistry.BuildKey(op.Receiver, op.Operation));
                    break;
                case DslIfStatement iff:
                    WalkStatements(iff.ThenBranch, unknownOps, errors, warnings, strictUnsupportedStatements);
                    foreach (var branch in iff.ElseIfBranches)
                        WalkStatements(branch.Body, unknownOps, errors, warnings, strictUnsupportedStatements);
                    if (iff.ElseBranch != null)
                        WalkStatements(iff.ElseBranch, unknownOps, errors, warnings, strictUnsupportedStatements);
                    break;
                case DslTryCatchStatement t:
                    WalkStatements(t.TryBranch, unknownOps, errors, warnings, strictUnsupportedStatements);
                    if (t.CatchBranch != null)
                        WalkStatements(t.CatchBranch, unknownOps, errors, warnings, strictUnsupportedStatements);
                    break;
            }
        }
    }
}
