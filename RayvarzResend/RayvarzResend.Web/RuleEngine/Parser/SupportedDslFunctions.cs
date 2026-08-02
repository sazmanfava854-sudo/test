namespace RayvarzResend.Web.RuleEngine.Parser;

/// <summary>
/// توابع پشتیبانی‌شده در Parser/Executor.
/// فاز ۲–۴: Run + Nosazi؛ فاز ۶: خانواده iNcOME (درآمد).
/// </summary>
public static class SupportedDslFunctions
{
    public static readonly HashSet<string> Core = new(StringComparer.OrdinalIgnoreCase)
    {
        "Run", "Nosazi", "نوسازی"
    };

    /// <summary>توابع درآمدی که در Run از Member 1388 dispatch می‌شوند.</summary>
    public static readonly HashSet<string> Income = new(StringComparer.OrdinalIgnoreCase)
    {
        "iNcOME",
        "iNcOMEOragh",
        "iNcOMESeprdeh",
        "iNcOMEEshghal",
        "iNcOMESepordeh",
        "Income"
    };

    public static bool IsNosazi(string name, string? displayName = null) =>
        name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase)
        || string.Equals(displayName, "نوسازی", StringComparison.Ordinal);

    public static bool IsIncome(string name) =>
        Income.Contains(name)
        || name.StartsWith("iNcOME", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("Income", StringComparison.OrdinalIgnoreCase);

    public static bool IsSupported(string name, string? displayName = null) =>
        Core.Contains(name)
        || string.Equals(displayName, "نوسازی", StringComparison.Ordinal)
        || IsIncome(name);

    public static bool IsDryRunBodySkip(string name, string? displayName = null) =>
        IsNosazi(name, displayName) || IsIncome(name);
}
