using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی ارسال و وضعیت UI — مسیر تهاتر جدا از ارسال عادی.</summary>
public static class FicheSendService
{
    public static string? ValidateSendable(FicheHeaderDto fiche)
    {
        if (fiche.ExistsInRayvarz)
            return "فیش در رایورز موجود است — ارسال نشد";

        if (TahatorRowBuilder.IsTahatorFiche(fiche))
            return "فیش تهاتر — از مسیر تهاتر ارسال کنید";

        if (fiche.Payable <= 0)
            return "مبلغ قابل پرداخت صفر است";

        if (fiche.Rows.Count == 0)
            return "ردیف IncmNo یافت نشد";

        if (!TahatorRowBuilder.IsTahatorFiche(fiche)
            && !FicheBranchResolver.TryResolve(fiche, out _, out _, out var branchError))
            return branchError;

        return null;
    }

    /// <summary>وضعیت ارسال را روی DTO فیش می‌گذارد (بارگذاری تکی / نمایش UI).</summary>
    public static void ApplySendStatus(FicheHeaderDto fiche)
    {
        if (TahatorRowBuilder.IsTahatorFiche(fiche))
        {
            if (fiche.ExistsInRayvarz)
            {
                fiche.CanSend = false;
                fiche.BlockReason = "فیش در رایورز موجود است — ارسال نشد";
                fiche.StatusMessage = "تکراری — در رایورز موجود است";
                return;
            }

            fiche.CanSend = true;
            fiche.BlockReason = null;
            fiche.StatusMessage = "تهاتر — آماده ارسال از مسیر تهاتر";
            return;
        }

        var blockReason = ValidateSendable(fiche);
        if (blockReason != null)
        {
            fiche.CanSend = false;
            fiche.BlockReason = blockReason;
            fiche.StatusMessage = blockReason;
            return;
        }

        fiche.CanSend = true;
        fiche.BlockReason = null;
        fiche.StatusMessage = "آماده ارسال";
    }
}
