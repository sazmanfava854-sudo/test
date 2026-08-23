using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// تشخیص NoDocument vs TrackingNo بر اساس طول رقم‌ها.
/// مثال: NoDocument=809552 (۶ رقم) — TrackingNo=0212280614002187 (۱۶ رقم).
/// </summary>
public static class InstallmentIdentifierDetector
{
    /// <summary>TrackingNo معمولاً ۱۰+ رقم؛ NoDocument کوتاه‌تر.</summary>
    public const int TrackingNoMinDigitLength = 10;

    /// <summary>کد پیگیری بانک شهر ۱۶ رقم — اکسل ممکن است صفر اول را حذف کند.</summary>
    public const int TrackingNoStandardLength = 16;

    public static InstallmentLookupKind Detect(string? raw)
    {
        var digits = ExtractDigits(raw);
        if (digits.Length == 0)
            return InstallmentLookupKind.NoDocument;

        return digits.Length >= TrackingNoMinDigitLength
            ? InstallmentLookupKind.TrackingNo
            : InstallmentLookupKind.NoDocument;
    }

    public static string NormalizeLookupValue(string? raw) => (raw ?? "").Trim();

    public static string ExtractDigits(string? raw) =>
        new string((raw ?? "").Where(char.IsDigit).ToArray());

    public static string Describe(InstallmentLookupKind kind) =>
        kind == InstallmentLookupKind.TrackingNo
            ? "کد پیگیری (TrackingNo)"
            : "شماره سند (NoDocument)";

    public static bool WillApplyEndState(InstallmentLookupKind kind, bool applyEndStateRequested) =>
        kind == InstallmentLookupKind.NoDocument || applyEndStateRequested;

    /// <summary>مقایسه کد پیگیری — صفر ابتدای اکسل ممکن است حذف شده باشد (۱۵ رقم vs ۱۶ رقم DB).</summary>
    public static bool TrackingNoDigitsMatch(string? a, string? b)
    {
        var da = ExtractDigits(a);
        var db = ExtractDigits(b);
        if (string.IsNullOrEmpty(da) || string.IsNullOrEmpty(db))
            return false;
        if (da == db)
            return true;

        if (da.Length == TrackingNoStandardLength - 1 && db.Length == TrackingNoStandardLength)
            return "0" + da == db;

        if (db.Length == TrackingNoStandardLength - 1 && da.Length == TrackingNoStandardLength)
            return "0" + db == da;

        return false;
    }

    /// <summary>WHERE برای TrackingNo — تطبیق exact یا با صفر ابتدایی از دست‌رفته در اکسل.</summary>
    public static string BuildTrackingNoWhereClause(string columnSql)
    {
        var trimmed = $"LTRIM(RTRIM(CAST({columnSql} AS varchar(30)))";
        var missingZeroLen = TrackingNoStandardLength - 1;
        return $"({trimmed}) = @v OR (LEN(@v) = {missingZeroLen} AND ({trimmed}) = '0' + @v)";
    }
}
