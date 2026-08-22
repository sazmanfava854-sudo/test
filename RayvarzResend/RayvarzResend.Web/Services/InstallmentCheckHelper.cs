namespace RayvarzResend.Web.Services;

/// <summary>منطق تب «تغییر وضعیت چک به خزانه» — بدون ارتباط با رایورز.</summary>
public static class InstallmentCheckHelper
{
    public const string TreasuryStatus = "28";
    public const string EndStateDescOdooat = "عودت";
    public const string EndStateCodeOdooat = "17";

    public static string BuildCommentPrefix(string performedByUser)
    {
        var user = (performedByUser ?? "").Trim();
        var baseText = string.IsNullOrEmpty(user)
            ? "تغییر وضعیت چک به خزانه"
            : $"تغییر وضعیت چک به خزانه توسط {user}";
        return baseText + " ";
    }

    /// <summary>پیشوند + کامنت قبلی (همان منطق UPDATE در Sara).</summary>
    public static string BuildNewComments(string performedByUser, string? existingComments)
    {
        var prefix = BuildCommentPrefix(performedByUser);
        if (string.IsNullOrWhiteSpace(existingComments))
            return prefix;
        return prefix + existingComments.Trim();
    }

    public static List<string> ParseIdentifierList(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return new List<string>();

        return raw
            .Split(new[] { ',', '\n', '\r', ';', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
