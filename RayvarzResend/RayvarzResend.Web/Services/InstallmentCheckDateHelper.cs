namespace RayvarzResend.Web.Services;

public static class InstallmentCheckDateHelper
{
    public static string BuildCommentPrefix(string performedByUser, string newPaymentDate)
    {
        var user = (performedByUser ?? "").Trim();
        var paymentDate = (newPaymentDate ?? "").Trim();
        var actionDate = DateHelper.CurrentShamsiSlashDate();
        var actor = string.IsNullOrEmpty(user) ? "کاربر" : user;
        return $"تغییر تاریخ چک به {paymentDate} توسط {actor} مورخ {actionDate} — ";
    }
}
