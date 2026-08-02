namespace RayvarzResend.Web.RuleEngine.Parser;

/// <summary>XmlBody Body → DslProgram — همه توابع Member (Public/Private) با IsSupported=true.</summary>
public static class VbTranspiler
{
    public static DslProgram Transpile(ClsFunctionDocument document)
    {
        var warnings = new List<string>();
        var extracted = VbFunctionExtractor.Extract(document.BodySource);
        if (extracted.Count == 0)
            warnings.Add("هیچ Function در Body یافت نشد.");

        var localNames = extracted.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var dslFunctions = new List<DslFunction>();

        foreach (var fn in extracted)
        {
            // همه توابع Supported — بدنه با allowUnsupportedFallback parse می‌شود
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
            UnsupportedFunctions = Array.Empty<string>()
        };
    }
}
