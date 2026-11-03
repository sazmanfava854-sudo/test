using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی گرید نهایی تب «تغییر وضعیت چک به خزانه» — مشترک بین UI و API.</summary>
public static class InstallmentSendGridHelper
{
    public const string DuplicateTrackingGridError =
        "ردیف تکراری در گرید — هر کد پیگیری/ردیف فقط یک‌بار مجاز است";

    public const string DuplicateTrackingSelectionError =
        "انتخاب تکراری — ارسال مجاز نیست";

    public const string NoSelectionError = "حداقل یک فیش از گرید انتخاب کنید";

    public const string MissingTrackingMessage = "بدون کد پیگیری — قابل انتخاب/ارسال نیست";

    public const string InsListKeyPrefix = "ins:";
    public const string NidInstallmentKeyPrefix = "nid:";

    public const string DocSiblingHint =
        "فیش دیگر همان شماره سند — مبلغ/تاریخ اکسل مربوط به ردیف دیگر است";

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

    public static string BuildRowSelectionKey(InstallmentCheckPreviewItem row)
    {
        var tracking = NormalizeTrackingKey(row.TrackingNo);
        if (tracking.Length > 0)
            return tracking;
        if (row.NidInsList != Guid.Empty)
            return InsListKeyPrefix + row.NidInsList.ToString("D");
        if (row.NidInstallment != Guid.Empty)
            return NidInstallmentKeyPrefix + row.NidInstallment.ToString("D");
        return "";
    }

    public static bool IsRowEligibleForSend(InstallmentCheckPreviewItem row) =>
        row.Found
        && BuildRowSelectionKey(row).Length > 0
        && (row.DataMatches || row.DocSibling);

    public static string? FindDuplicateTrackingInGrid(IEnumerable<InstallmentCheckPreviewItem> gridItems)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in gridItems)
        {
            var key = BuildRowSelectionKey(row);
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
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in raw ?? [])
        {
            var token = (item ?? "").Trim();
            if (token.Length == 0)
                continue;

            string key;
            if (token.StartsWith(InsListKeyPrefix, StringComparison.OrdinalIgnoreCase)
                || token.StartsWith(NidInstallmentKeyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                key = token;
            }
            else
            {
                key = NormalizeTrackingKey(token);
                if (key.Length == 0)
                    continue;
            }

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
        IReadOnlySet<string> selectedRowKeys)
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
                selectedRowKeys.Contains(BuildRowSelectionKey(m)));

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
