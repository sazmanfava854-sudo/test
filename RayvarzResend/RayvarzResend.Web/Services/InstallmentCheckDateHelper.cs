namespace RayvarzResend.Web.Services;

public static class InstallmentCheckDateHelper
{
    public static string BuildCommentPrefix(string performedByUser, string newPaymentDate)
    {
        var user = (performedByUser ?? "").Trim();
        var date = (newPaymentDate ?? "").Trim();
        var baseText = string.IsNullOrEmpty(user)
            ? $"تغییر تاریخ چک به {date}"
            : $"تغییر تاریخ چک به {date} توسط {user}";
        return baseText + " ";
    }
}
