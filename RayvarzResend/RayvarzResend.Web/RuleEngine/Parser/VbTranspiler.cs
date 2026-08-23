namespace RayvarzResend.Web.RuleEngine.Parser;

/// <summary>فاز ۲: XmlBody Body → DslProgram — Run dispatch + Nosazi subset.</summary>
public static class VbTranspiler
{
    private static readonly HashSet<string> Phase2Functions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Run", "Nosazi"
    };

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
            var isSupported = Phase2Functions.Contains(fn.Name)
                || string.Equals(fn.DisplayName, "نوسازی", StringComparison.Ordinal);

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
                            "Function outside Phase 2 scope (Run + Nosazi only)",
                            fn.Name)
                    }
                });
                continue;
            }

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

        if (!dslFunctions.Any(f => f.Name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(f.DisplayName, "نوسازی", StringComparison.Ordinal)))
            warnings.Add("تابع Nosazi (نوسازی) در Body یافت نشد.");

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
