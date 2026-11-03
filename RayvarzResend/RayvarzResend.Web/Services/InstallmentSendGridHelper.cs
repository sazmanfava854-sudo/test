using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی گرید نهایی تب «تغییر وضعیت چک به خزانه» — مشترک بین UI و API.</summary>
public static class InstallmentSendGridHelper
{
    public const string DuplicateTrackingGridError =
        "کد پیگیری تکراری در گرید — هر کد پیگیری فقط یک‌بار مجاز است";

    public const string DuplicateTrackingSelectionError =
        "کد پیگیری تکراری در انتخاب‌ها — ارسال مجاز نیست";

    public const string NoSelectionError = "حداقل یک فیش از گرید انتخاب کنید";

    public sealed record DocSendWarning(string NoDocument, int TotalInGrid, int SelectedCount);

    public static string NormalizeTrackingKey(string? trackingNo)
    {
        var digits = InstallmentIdentifierDetector.ExtractDigits(trackingNo);
        if (digits.Length == 0)
            return "";

        if (digits.Length == InstallmentIdentifierDetector.TrackingNoStandardLength - 1)
            return "0" + digits;

        return digits;
    }

    public static string NormalizeNoDocument(string? noDocument)
    {
        var t = (noDocument ?? "").Trim();
        return string.IsNullOrEmpty(t) ? "—" : t;
    }

    public static bool IsRowEligibleForSend(InstallmentCheckPreviewItem row) =>
        row.Found && row.DataMatches && NormalizeTrackingKey(row.TrackingNo).Length > 0;

    /// <summary>اولین کد پیگیری تکراری در کل گرید (یا null).</summary>
    public static string? FindDuplicateTrackingInGrid(IEnumerable<InstallmentCheckPreviewItem> gridItems)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in gridItems)
        {
            var key = NormalizeTrackingKey(row.TrackingNo);
            if (key.Length == 0)
                continue;
            if (!seen.Add(key))
                return key;
        }

        return null;
    }

    public static (List<string> Keys, string? Error) ParseSelectedTrackingNos(IEnumerable<string>? raw)
    {
        if (raw == null)
            return ([], NoSelectionError);

        var keys = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in raw)
        {
            var key = NormalizeTrackingKey(item);
            if (key.Length == 0)
                continue;
            if (!seen.Add(key))
                return ([], DuplicateTrackingSelectionError);
            keys.Add(key);
        }

        if (keys.Count == 0)
            return ([], NoSelectionError);

        return (keys, null);
    }

    public static IReadOnlyList<DocSendWarning> BuildDocNumberWarnings(
        IEnumerable<InstallmentCheckPreviewItem> gridItems,
        IReadOnlySet<string> selectedTrackingKeys)
    {
        var warnings = new List<DocSendWarning>();
        var groups = gridItems
            .Where(i => i.Found && !string.IsNullOrWhiteSpace(i.NoDocument))
            .GroupBy(i => NormalizeNoDocument(i.NoDocument), StringComparer.Ordinal);

        foreach (var group in groups)
        {
            var members = group.ToList();
            if (members.Count <= 1)
                continue;

            var selected = members.Count(m =>
                selectedTrackingKeys.Contains(NormalizeTrackingKey(m.TrackingNo)));

            if (selected > 1)
            {
                warnings.Add(new DocSendWarning(
                    group.Key,
                    members.Count,
                    selected));
            }
        }

        return warnings;
    }

    public static string FormatDocWarningLine(DocSendWarning warning) =>
        $"برای شماره سند {warning.NoDocument} تعداد {warning.TotalInGrid} فیش وجود دارد که {warning.SelectedCount} فیش از آن‌ها انتخاب شده است.";

    public static string FormatDocWarningConfirmBody(IReadOnlyList<DocSendWarning> warnings)
    {
        if (warnings.Count == 0)
            return "";

        var lines = warnings.Select(FormatDocWarningLine).ToList();
        lines.Add("معمولاً فقط یک فیش باید ارسال شود. آیا از ارسال فیش‌های انتخاب‌شده اطمینان دارید؟");
        return string.Join("\n\n", lines);
    }
}
