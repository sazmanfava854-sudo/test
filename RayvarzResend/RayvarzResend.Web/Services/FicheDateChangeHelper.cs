using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تب «تغییر تاریخ فیش» — dbo.Income_Fiche (بدون رایورز).</summary>
public static class FicheDateChangeHelper
{
    public static readonly IReadOnlyDictionary<int, string> FicheStatusLabels = new Dictionary<int, string>
    {
        [0] = "صدورموقت",
        [1] = "صدوردایم",
        [2] = "چاپ",
        [3] = "تایید دستی/لحظه‌ای بانک",
        [4] = "ابطال",
        [5] = "تایید بانک"
    };

    public const int DefaultFicheStatus = 1;

    public static string StatusLabel(int status) =>
        FicheStatusLabels.TryGetValue(status, out var label) ? label : status.ToString();

    public static string NormalizeSlashDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        var rayvarz = DateHelper.ToRayvarzDate(input);
        return rayvarz.Length >= 8 ? DateHelper.ToShamsiSlashDate(rayvarz) : "";
    }

    public static bool TryNormalizeSlashDate(string? input, out string slashDate)
    {
        slashDate = NormalizeSlashDate(input);
        return !string.IsNullOrEmpty(slashDate);
    }

    public static string BuildCommentPrefix(string performedByUser)
    {
        var today = DateHelper.CurrentShamsiSlashDate();
        var user = (performedByUser ?? "").Trim();
        var actor = string.IsNullOrEmpty(user) ? "کاربر" : user;
        return $"تاریخ مهلت پرداخت و تاریخ صدور و وضعیت فیش توسط {actor} در مورخ {today} — ";
    }

    public static string BuildNewComments(string performedByUser, string? existingComments)
    {
        var prefix = BuildCommentPrefix(performedByUser);
        if (string.IsNullOrWhiteSpace(existingComments))
            return prefix.TrimEnd();
        return prefix + existingComments.Trim();
    }

    public static string BuildChangeSummary(
        bool applyExportPermanentDate,
        string? exportPermanentDate,
        bool applyExportTemporaryDate,
        string? exportTemporaryDate,
        bool applyPaymentBreakDate,
        string? paymentBreakDate,
        bool applyEumFicheStatus,
        int? eumFicheStatus)
    {
        var parts = new List<string>();
        if (applyExportPermanentDate && !string.IsNullOrWhiteSpace(exportPermanentDate))
            parts.Add($"صدور دایم:{exportPermanentDate}");
        if (applyExportTemporaryDate && !string.IsNullOrWhiteSpace(exportTemporaryDate))
            parts.Add($"صدور موقت:{exportTemporaryDate}");
        if (applyPaymentBreakDate && !string.IsNullOrWhiteSpace(paymentBreakDate))
            parts.Add($"مهلت:{paymentBreakDate}");
        if (applyEumFicheStatus && eumFicheStatus.HasValue)
            parts.Add($"وضعیت:{eumFicheStatus.Value}({StatusLabel(eumFicheStatus.Value)})");
        return parts.Count == 0 ? "" : string.Join(" | ", parts);
    }

    public static bool HasAnySearchFilter(FicheDateChangeSearchRequest req) =>
        !string.IsNullOrWhiteSpace(req.PermanentFromDate)
        || !string.IsNullOrWhiteSpace(req.PermanentToDate)
        || !string.IsNullOrWhiteSpace(req.TemporaryFromDate)
        || !string.IsNullOrWhiteSpace(req.TemporaryToDate)
        || !string.IsNullOrWhiteSpace(req.AccountGroupTitle)
        || !string.IsNullOrWhiteSpace(req.IdentifierValue)
        || req.EumFicheStatuses is { Count: > 0 };

    public static (string Clause, string ParamName, string Value)? BuildIdentifierFilter(string? raw)
    {
        var value = (raw ?? "").Trim();
        if (value.Length == 0) return null;

        var type = IdentifierDetector.Detect(value);
        return type == IdentifierType.BillPaymentKey
            ? ("f.BillID + f.PaymentID = @idVal", "@idVal", value)
            : ("f.FicheNo = @idVal", "@idVal", value);
    }

    public static bool HasAnyChange(FicheDateChangeUpdateRequest req) =>
        (req.ApplyExportPermanentDate && !string.IsNullOrWhiteSpace(req.NewExportPermanentDate))
        || (req.ApplyExportTemporaryDate && !string.IsNullOrWhiteSpace(req.NewExportTemporaryDate))
        || (req.ApplyPaymentBreakDate && !string.IsNullOrWhiteSpace(req.NewPaymentBreakDate))
        || (req.ApplyEumFicheStatus && req.NewEumFicheStatus.HasValue);
}
