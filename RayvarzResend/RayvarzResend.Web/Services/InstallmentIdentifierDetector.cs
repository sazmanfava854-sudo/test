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
}
