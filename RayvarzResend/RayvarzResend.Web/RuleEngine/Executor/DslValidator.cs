using RayvarzResend.Web.RuleEngine.Parser;

namespace RayvarzResend.Web.RuleEngine.Executor;

public sealed class DslValidator
{
    private readonly IOperationRegistry _registry;

    public DslValidator(IOperationRegistry registry) => _registry = registry;

    public DslValidationResult Validate(DslProgram program)
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        var unknownOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!program.HasEntryPoint)
            errors.Add("تابع Run (EntryPoint) در DSL یافت نشد.");

        if (!program.HasNosazi)
            warnings.Add("تابع Nosazi در DSL یافت نشد.");

        foreach (var fn in program.UnsupportedFunctions)
            warnings.Add($"تابع پشتیبانی‌نشده در Parser: {fn}");

        foreach (var fn in program.Functions)
        {
            if (!fn.IsSupported)
                continue;

            WalkStatements(fn.Body, unknownOps, errors);
        }

        foreach (var op in unknownOps)
        {
            if (!_registry.IsKnown(op))
                errors.Add($"Operation ناشناخته: {op}");
        }

        return new DslValidationResult
        {
            Success = errors.Count == 0,
            Errors = errors,
            Warnings = warnings.Concat(program.Warnings).Distinct().ToList(),
            UnknownOperations = unknownOps.Where(o => !_registry.IsKnown(o)).ToList()
        };
    }

    private void WalkStatements(
        IReadOnlyList<DslStatement> statements,
        HashSet<string> unknownOps,
        List<string> errors)
    {
        foreach (var stmt in statements)
        {
            switch (stmt)
            {
                case DslUnsupportedStatement u:
                    errors.Add($"Unsupported: {u.Reason} — {u.SourceSnippet}");
                    break;
                case DslCallOperationStatement op:
                    unknownOps.Add(IOperationRegistry.BuildKey(op.Receiver, op.Operation));
                    break;
                case DslIfStatement iff:
                    WalkStatements(iff.ThenBranch, unknownOps, errors);
                    foreach (var branch in iff.ElseIfBranches)
                        WalkStatements(branch.Body, unknownOps, errors);
                    if (iff.ElseBranch != null)
                        WalkStatements(iff.ElseBranch, unknownOps, errors);
                    break;
                case DslTryCatchStatement t:
                    WalkStatements(t.TryBranch, unknownOps, errors);
                    if (t.CatchBranch != null)
                        WalkStatements(t.CatchBranch, unknownOps, errors);
                    break;
            }
        }
    }
}
