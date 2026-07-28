namespace RayvarzResend.Web.RuleEngine;

/// <summary>خروجی پارس <c>ClsFunction</c> از ستون XmlBody جدول Member.</summary>
public sealed class ClsFunctionDocument
{
    public int NidClass { get; init; }
    public int NidFunction { get; init; }
    public string Name { get; init; } = "";
    public string DisplayText { get; init; } = "";
    public string BodySource { get; init; } = "";
    public bool IsActive { get; init; }
    public int Version { get; init; }
    public int FormulaVersion { get; init; }

    /// <summary>نام توابع VB داخل Body (برای تطبیق با nosazo / iNcOME).</summary>
    public IReadOnlyList<string> FunctionNames { get; init; } = Array.Empty<string>();

    public bool ContainsFunction(string displayOrName) =>
        FunctionNames.Any(f => f.Contains(displayOrName, StringComparison.OrdinalIgnoreCase))
        || BodySource.Contains($"Function {displayOrName}", StringComparison.OrdinalIgnoreCase)
        || BodySource.Contains($"DisplayName(\"{displayOrName}\")", StringComparison.OrdinalIgnoreCase);
}
