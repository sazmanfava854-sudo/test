using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>ابطال فیش — فقط وقتی <c>CanceledDate</c> در Sara پر باشد (نه صرفاً EumDutyFicheStatus/EumFicheStatus).</summary>
public static class FicheAbortionGuard
{
    public const string SingleSendBlockMessage = "فیش ابطال شده است — ارسال مجاز نیست";

    /// <summary>SQL: alias <c>f</c> برای Income_Fiche.</summary>
    public const string IncomeNotAbortedSql = "NULLIF(LTRIM(RTRIM(f.CanceledDate)), '') IS NULL";

    /// <summary>SQL: alias <c>d</c> برای Duty_Fiche.</summary>
    public const string DutyNotAbortedSql = "NULLIF(LTRIM(RTRIM(d.CanceledDate)), '') IS NULL";

    /// <summary>SQL: alias <c>f</c> برای Income_Fiche.</summary>
    public const string IncomeAbortedCaseSql =
        "CASE WHEN NULLIF(LTRIM(RTRIM(f.CanceledDate)), '') IS NOT NULL THEN 1 ELSE 0 END";

    /// <summary>SQL: alias <c>d</c> برای Duty_Fiche.</summary>
    public const string DutyAbortedCaseSql =
        "CASE WHEN NULLIF(LTRIM(RTRIM(d.CanceledDate)), '') IS NOT NULL THEN 1 ELSE 0 END";

    /// <summary>SQL: alias <c>x</c> برای Income_Fiche در CROSS APPLY.</summary>
    public const string IncomeAbortedCaseSqlAliasX =
        "CASE WHEN NULLIF(LTRIM(RTRIM(x.CanceledDate)), '') IS NOT NULL THEN 1 ELSE 0 END";

    /// <summary>SQL: alias <c>x</c> برای Duty_Fiche در CROSS APPLY.</summary>
    public const string DutyAbortedCaseSqlAliasX =
        "CASE WHEN NULLIF(LTRIM(RTRIM(x.CanceledDate)), '') IS NOT NULL THEN 1 ELSE 0 END";

    public static bool IsAborted(string? canceledDate) =>
        !string.IsNullOrWhiteSpace(canceledDate);

    public static string? BuildBlockReason(FicheHeaderDto fiche) =>
        IsAborted(fiche.CanceledDate) ? SingleSendBlockMessage : null;
}
