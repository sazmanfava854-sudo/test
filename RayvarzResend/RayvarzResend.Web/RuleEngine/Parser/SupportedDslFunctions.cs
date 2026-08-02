namespace RayvarzResend.Web.RuleEngine.Parser;

/// <summary>
/// توابع Member 1388 — همه در DSL Supported هستند.
/// اجرا: Run کامل (همان Call chain فایل اصلی)؛ بدنه بقیه در DryRun skip → Build*Rows از فیش live.
/// </summary>
public static class SupportedDslFunctions
{
    public static bool IsEntryPoint(string name) =>
        name.Equals("Run", StringComparison.OrdinalIgnoreCase);

    public static bool IsNosazi(string name, string? displayName = null) =>
        name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase)
        || string.Equals(displayName, "نوسازی", StringComparison.Ordinal);

    public static bool IsIncome(string name) =>
        name.Equals("iNcOME", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("iNcOME", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("Income", StringComparison.OrdinalIgnoreCase);

    /// <summary>همه توابع استخراج‌شده از XmlBody پشتیبانی می‌شوند.</summary>
    public static bool IsSupported(string name, string? displayName = null) => true;

    /// <summary>
    /// بدنه همه توابع به‌جز Run در DryRun اجرا نمی‌شود (وابستگی Info8/Biz).
    /// Run باید همان Call chain فایل اصلی را طی کند.
    /// </summary>
    public static bool IsDryRunBodySkip(string name, string? displayName = null) =>
        !IsEntryPoint(name);
}
