using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.RuleEngine.Parser;

/// <summary>نقش تابع در Member 1388 — کلی یا وابسته به نوع فیش.</summary>
public enum DslFunctionRole
{
    EntryPoint,
    /// <summary>برای همه نوع فیش / helperهای مشترک (ChangeDate, FnSMS, …).</summary>
    Global,
    Duty,
    Income,
    Tahator,
    IncomeCheck
}

/// <summary>
/// همه توابع Supported هستند. قبل از SOAP، قوانین بر اساس نوع فیش اعمال می‌شوند.
/// بدنه به‌خاطر Unsupported یکجا skip نمی‌شود — فقط خطوط غیرقابل‌parse نرم رد می‌شوند.
/// </summary>
public static class SupportedDslFunctions
{
    public static bool IsEntryPoint(string name) =>
        name.Equals("Run", StringComparison.OrdinalIgnoreCase);

    public static bool IsNosazi(string name, string? displayName = null) =>
        name.Equals("Nosazi", StringComparison.OrdinalIgnoreCase)
        || string.Equals(displayName, "نوسازی", StringComparison.Ordinal);

    public static bool IsIncome(string name) =>
        !IsIncomeCheck(name)
        && (name.Equals("iNcOME", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("iNcOME", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Income", StringComparison.OrdinalIgnoreCase)
            || name.Equals("BedeHi", StringComparison.OrdinalIgnoreCase)
            || name.Equals("BazAfarine", StringComparison.OrdinalIgnoreCase)
            || name.Equals("BazAfarineOld", StringComparison.OrdinalIgnoreCase));

    public static bool IsTahator(string name) =>
        name.Equals("Tahator", StringComparison.OrdinalIgnoreCase)
        || name.Equals("Tahator1", StringComparison.OrdinalIgnoreCase);

    public static bool IsIncomeCheck(string name) =>
        name.Equals("iNcOMECheck", StringComparison.OrdinalIgnoreCase);

    public static bool IsGlobalHelper(string name) =>
        name.Equals("ChangeDate", StringComparison.OrdinalIgnoreCase)
        || name.Equals("GetDiffDate", StringComparison.OrdinalIgnoreCase)
        || name.Equals("GetSara8Workflow", StringComparison.OrdinalIgnoreCase)
        || name.Equals("FnSMS", StringComparison.OrdinalIgnoreCase)
        || name.Equals("AddDateForHolidays", StringComparison.OrdinalIgnoreCase)
        || name.Equals("Logfile", StringComparison.OrdinalIgnoreCase);

    public static DslFunctionRole GetRole(string name, string? displayName = null)
    {
        if (IsEntryPoint(name)) return DslFunctionRole.EntryPoint;
        if (IsNosazi(name, displayName)) return DslFunctionRole.Duty;
        if (IsTahator(name)) return DslFunctionRole.Tahator;
        if (IsIncomeCheck(name)) return DslFunctionRole.IncomeCheck;
        if (IsIncome(name)) return DslFunctionRole.Income;
        if (IsGlobalHelper(name)) return DslFunctionRole.Global;
        // پیش‌فرض: کلی (توابع ناشناختهٔ جدید هم Supported و قابل اعمال در Call)
        return DslFunctionRole.Global;
    }

    /// <summary>آیا این تابع برای این فیش باید قوانینش بررسی/اعمال شود؟</summary>
    public static bool AppliesToFiche(string name, string? displayName, FicheHeaderDto fiche)
    {
        var role = GetRole(name, displayName);
        return role switch
        {
            DslFunctionRole.EntryPoint => true,
            DslFunctionRole.Global => true,
            DslFunctionRole.Duty =>
                fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi,
            DslFunctionRole.Income =>
                fiche.Category == FicheCategory.Income,
            DslFunctionRole.Tahator =>
                fiche.Category == FicheCategory.Income, // Run درآمد: Tahator1(۱۵۷) و Tahator(۱۵۸)
            DslFunctionRole.IncomeCheck =>
                fiche.Category == FicheCategory.Income,
            _ => true
        };
    }

    /// <summary>همه توابع استخراج‌شده Supported هستند — هیچ Unsupported function نداریم.</summary>
    public static bool IsSupported(string name, string? displayName = null) => true;

    /// <summary>نقش‌های اجباری که قبل از SOAP برای این فیش باید اعمال شده باشند.</summary>
    public static IReadOnlyList<DslFunctionRole> RequiredRolesBeforeSoap(FicheHeaderDto fiche)
    {
        if (fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi)
            return new[] { DslFunctionRole.Duty };

        if (fiche.Category == FicheCategory.Income)
        {
            if (fiche.AccountingDocumentingCause == 7)
                return new[] { DslFunctionRole.IncomeCheck };

            if (fiche.DocTyp is 14 or 15 or 17 or 18
                || fiche.IncomeAccountGroup is 157 or 158)
                return new[] { DslFunctionRole.Income, DslFunctionRole.Tahator };
            return new[] { DslFunctionRole.Income };
        }

        return Array.Empty<DslFunctionRole>();
    }
}
