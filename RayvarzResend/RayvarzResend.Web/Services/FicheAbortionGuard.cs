using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ابطال فیش در Sara — نوسازی/صنفی: <c>EumDutyFicheStatus=2</c> (ابطال) و در صورت وجود ستون تاریخ ابطال؛
/// درآمد: <c>EumFicheStatus=4</c> (ابطال) و همان ستون (در appsettings).
/// </summary>
public static class FicheAbortionGuard
{
    public const string SingleSendBlockMessage = "فیش ابطال شده است — ارسال مجاز نیست";

    public const int DutyAbortedStatus = 2;
    public const int IncomeAbortedStatus = 4;

    private static FicheSchemaOptions _schema = new();

    public static void Configure(FicheSchemaOptions? options) =>
        _schema = options ?? new FicheSchemaOptions();

    public static string IncomeNotAbortedSql => BuildNotAbortedSql("f", isDuty: false);

    public static string DutyNotAbortedSql => BuildNotAbortedSql("d", isDuty: true);

    public static string IncomeAbortedCaseSql => BuildAbortedCaseSql("f", isDuty: false);

    public static string DutyAbortedCaseSql => BuildAbortedCaseSql("d", isDuty: true);

    public static string IncomeAbortedCaseSqlAliasX => BuildAbortedCaseSql("x", isDuty: false);

    public static string DutyAbortedCaseSqlAliasX => BuildAbortedCaseSql("x", isDuty: true);

    public static string SelectCanceledDateSql(string tableAlias, bool isDuty)
    {
        var column = ResolveCanceledDateColumn(isDuty);
        return string.IsNullOrEmpty(column)
            ? "CAST(NULL AS nvarchar(30)) AS CanceledDate"
            : $"NULLIF(LTRIM(RTRIM({tableAlias}.{column})), '') AS CanceledDate";
    }

    public static bool HasCanceledDate(string? canceledDate) =>
        !string.IsNullOrWhiteSpace(canceledDate);

    public static bool IsFicheAborted(FicheHeaderDto fiche) =>
        fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi
            ? fiche.CurrentStatus == DutyAbortedStatus || HasCanceledDate(fiche.CanceledDate)
            : fiche.CurrentStatus == IncomeAbortedStatus || HasCanceledDate(fiche.CanceledDate);

    public static string? BuildBlockReason(FicheHeaderDto fiche) =>
        IsFicheAborted(fiche) ? SingleSendBlockMessage : null;

    private static string BuildNotAbortedSql(string alias, bool isDuty)
    {
        var statusPart = isDuty
            ? $"{alias}.EumDutyFicheStatus <> {DutyAbortedStatus}"
            : $"{alias}.EumFicheStatus <> {IncomeAbortedStatus}";
        var dateEmpty = BuildCanceledDateEmptySql(alias, isDuty);
        return string.IsNullOrEmpty(dateEmpty) ? statusPart : $"{statusPart} AND {dateEmpty}";
    }

    private static string BuildAbortedCaseSql(string alias, bool isDuty)
    {
        var statusPart = isDuty
            ? $"{alias}.EumDutyFicheStatus = {DutyAbortedStatus}"
            : $"{alias}.EumFicheStatus = {IncomeAbortedStatus}";
        var dateNotEmpty = BuildCanceledDateNotEmptySql(alias, isDuty);
        var when = string.IsNullOrEmpty(dateNotEmpty) ? statusPart : $"{statusPart} OR {dateNotEmpty}";
        return $"CASE WHEN {when} THEN 1 ELSE 0 END";
    }

    private static string? BuildCanceledDateEmptySql(string alias, bool isDuty)
    {
        var column = ResolveCanceledDateColumn(isDuty);
        return string.IsNullOrEmpty(column)
            ? null
            : $"NULLIF(LTRIM(RTRIM({alias}.{column})), '') IS NULL";
    }

    private static string? BuildCanceledDateNotEmptySql(string alias, bool isDuty)
    {
        var column = ResolveCanceledDateColumn(isDuty);
        return string.IsNullOrEmpty(column)
            ? null
            : $"NULLIF(LTRIM(RTRIM({alias}.{column})), '') IS NOT NULL";
    }

    private static string? ResolveCanceledDateColumn(bool isDuty)
    {
        var raw = isDuty ? _schema.DutyFicheCanceledDateColumn : _schema.IncomeFicheCanceledDateColumn;
        return SanitizeSqlColumnName(raw);
    }

    internal static string? SanitizeSqlColumnName(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;
        var t = raw.Trim();
        foreach (var c in t)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return null;
        }

        return t;
    }
}
