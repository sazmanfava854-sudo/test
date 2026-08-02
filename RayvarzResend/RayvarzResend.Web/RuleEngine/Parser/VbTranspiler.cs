namespace RayvarzResend.Web.RuleEngine.Parser;

/// <summary>XmlBody Body → DslProgram — Run + Nosazi + خانواده iNcOME (فاز ۶).</summary>
public static class VbTranspiler
{
    public static DslProgram Transpile(ClsFunctionDocument document)
    {
        var warnings = new List<string>();
        var extracted = VbFunctionExtractor.Extract(document.BodySource);
        if (extracted.Count == 0)
            warnings.Add("هیچ Public Function در Body یافت نشد.");

        var localNames = extracted.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var dslFunctions = new List<DslFunction>();
        var unsupported = new List<string>();

        foreach (var fn in extracted)
        {
            var isSupported = SupportedDslFunctions.IsSupported(fn.Name, fn.DisplayName);

            if (!isSupported)
            {
                unsupported.Add(fn.Name);
                dslFunctions.Add(new DslFunction
                {
                    Name = fn.Name,
                    DisplayName = fn.DisplayName,
                    IsSupported = false,
                    Body = new[]
                    {
                        new DslUnsupportedStatement(
                            "Function outside supported DSL scope (Run + Nosazi + iNcOME*)",
                            fn.Name)
                    }
                });
                continue;
            }

            // بدنه درآمد در DryRun skip می‌شود؛ parse برای dispatch کافی است (allow unsupported در body)
            var body = VbStatementParser.ParseBlock(fn.Body, localNames, warnings, allowUnsupportedFallback: true);
            dslFunctions.Add(new DslFunction
            {
                Name = fn.Name,
                DisplayName = fn.DisplayName,
                IsSupported = true,
                Body = body
            });
        }

        if (!dslFunctions.Any(f => f.Name.Equals("Run", StringComparison.OrdinalIgnoreCase)))
            warnings.Add("تابع Run در Body یافت نشد.");

        if (!dslFunctions.Any(f => SupportedDslFunctions.IsNosazi(f.Name, f.DisplayName)))
            warnings.Add("تابع Nosazi (نوسازی) در Body یافت نشد.");

        if (!dslFunctions.Any(f => SupportedDslFunctions.IsIncome(f.Name)))
            warnings.Add("تابع iNcOME (درآمد) در Body یافت نشد.");

        return new DslProgram
        {
            ParserVersion = RuleDslParserService.ParserVersion,
            EntryPoint = "Run",
            NidFunction = document.NidFunction,
            FunctionName = document.Name,
            Functions = dslFunctions,
            Warnings = warnings,
            UnsupportedFunctions = unsupported
        };
    }
}
