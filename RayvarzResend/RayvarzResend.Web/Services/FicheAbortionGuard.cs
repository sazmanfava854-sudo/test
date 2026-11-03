using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ابطال فیش در Sara — نوسازی/صنفی: <c>EumDutyFicheStatus=2</c> (ابطال) یا <c>CanceledDate</c>؛
/// درآمد شهرسازی: <c>EumFicheStatus=4</c> (ابطال) یا <c>CanceledDate</c>.
/// </summary>
public static class FicheAbortionGuard
{
    public const string SingleSendBlockMessage = "فیش ابطال شده است — ارسال مجاز نیست";

    /// <summary>نوسازی/صنفی — Duty_Fiche.EumDutyFicheStatus (۰ صدور، ۱ تائید، ۲ ابطال، ۳ صدور موقت، ۴ تائید بانک).</summary>
    public const int DutyAbortedStatus = 2;

    /// <summary>درآمد شهرسازی — Income_Fiche.EumFicheStatus (۴ = ابطال).</summary>
    public const int IncomeAbortedStatus = 4;

    private const string CanceledDateNotEmptySql = "NULLIF(LTRIM(RTRIM({0}.CanceledDate)), '') IS NOT NULL";
    private const string CanceledDateEmptySql = "NULLIF(LTRIM(RTRIM({0}.CanceledDate)), '') IS NULL";

    /// <summary>SQL: alias <c>f</c> برای Income_Fiche.</summary>
    public static string IncomeNotAbortedSql =>
        $"f.EumFicheStatus <> {IncomeAbortedStatus} AND {string.Format(CanceledDateEmptySql, "f")}";

    /// <summary>SQL: alias <c>d</c> برای Duty_Fiche.</summary>
    public static string DutyNotAbortedSql =>
        $"d.EumDutyFicheStatus <> {DutyAbortedStatus} AND {string.Format(CanceledDateEmptySql, "d")}";

    /// <summary>SQL: alias <c>f</c> برای Income_Fiche.</summary>
    public static string IncomeAbortedCaseSql =>
        $"CASE WHEN f.EumFicheStatus = {IncomeAbortedStatus} OR {string.Format(CanceledDateNotEmptySql, "f")} THEN 1 ELSE 0 END";

    /// <summary>SQL: alias <c>d</c> برای Duty_Fiche.</summary>
    public static string DutyAbortedCaseSql =>
        $"CASE WHEN d.EumDutyFicheStatus = {DutyAbortedStatus} OR {string.Format(CanceledDateNotEmptySql, "d")} THEN 1 ELSE 0 END";

    /// <summary>SQL: alias <c>x</c> برای Income_Fiche در CROSS APPLY.</summary>
    public static string IncomeAbortedCaseSqlAliasX =>
        $"CASE WHEN x.EumFicheStatus = {IncomeAbortedStatus} OR {string.Format(CanceledDateNotEmptySql, "x")} THEN 1 ELSE 0 END";

    /// <summary>SQL: alias <c>x</c> برای Duty_Fiche در CROSS APPLY.</summary>
    public static string DutyAbortedCaseSqlAliasX =>
        $"CASE WHEN x.EumDutyFicheStatus = {DutyAbortedStatus} OR {string.Format(CanceledDateNotEmptySql, "x")} THEN 1 ELSE 0 END";

    public static bool HasCanceledDate(string? canceledDate) =>
        !string.IsNullOrWhiteSpace(canceledDate);

    public static bool IsFicheAborted(FicheHeaderDto fiche) =>
        fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi
            ? fiche.CurrentStatus == DutyAbortedStatus || HasCanceledDate(fiche.CanceledDate)
            : fiche.CurrentStatus == IncomeAbortedStatus || HasCanceledDate(fiche.CanceledDate);

    public static string? BuildBlockReason(FicheHeaderDto fiche) =>
        IsFicheAborted(fiche) ? SingleSendBlockMessage : null;
}
