namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>
/// اعتبارسنجی candidate قبل از پذیرش: XmlBody باید با ClsFunctionParser قابل parse باشد.
/// dry-run golden (RuleDryRunResult) در فاز بعد اضافه می‌شود؛ تا آن زمان ردیف ساختگی ثبت نمی‌شود.
/// </summary>
public sealed class RuleCandidateValidator
{
    public RuleValidationOutcome Validate(RuleCandidateRow candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate.XmlBody))
            return RuleValidationOutcome.Fail("XmlBody خالی است");

        try
        {
            var doc = ClsFunctionParser.Parse(candidate.XmlBody);
            return RuleValidationOutcome.Pass(doc.FunctionNames.Count);
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.Xml.XmlException)
        {
            return RuleValidationOutcome.Fail("Parse failed: " + ex.Message);
        }
    }
}

public sealed record RuleValidationOutcome(bool Success, string? ErrorMessage, int FunctionCount)
{
    public static RuleValidationOutcome Pass(int functionCount) => new(true, null, functionCount);
    public static RuleValidationOutcome Fail(string error) => new(false, error, 0);
}
