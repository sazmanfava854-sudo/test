using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تاریخ‌های عملیاتی هر فیش برای DocDate / ActDate / Due — از ستون‌های Sara8M03، نه appsettings.</summary>
public static class FicheDateResolver
{
    /// <summary>
    /// تاریخ مؤثر از PaymentDate / BankPaymentDate — مطابق VB Member 1388:
    /// وضعیت ۳ → PaymentDate؛ غیر آن → BankPaymentDate — با fallback به ستون دیگر.
    /// </summary>
    public static string ResolvePaymentDateByStatus(int ficheStatus, string paymentDate, string bankPaymentDate) =>
        ficheStatus == 3
            ? FirstRayvarzDate(paymentDate, bankPaymentDate)
            : FirstRayvarzDate(bankPaymentDate, paymentDate);

    /// <summary>DocDate / ActDate / Due از PaymentDate و BankPaymentDate — ارسال تکی درآمد و نوسازی.</summary>
    public static void ApplyFromPaymentColumns(
        FicheHeaderDto dto,
        int ficheStatus,
        string paymentDate,
        string bankPaymentDate)
    {
        dto.RayvarzDocDate = FirstRayvarzDate(paymentDate, bankPaymentDate);
        dto.RayvarzActDate = ResolvePaymentDateByStatus(ficheStatus, paymentDate, bankPaymentDate);
        dto.RayvarzDueDate = FirstRayvarzDate(bankPaymentDate, paymentDate);
        dto.RowDate = dto.RayvarzActDate;
    }

    public static void ApplyFromIncomeColumns(
        FicheHeaderDto dto,
        int ficheStatus,
        string paymentDate,
        string bankPaymentDate,
        bool tahatorFiche = false)
    {
        ApplyFromPaymentColumns(dto, ficheStatus, paymentDate, bankPaymentDate);
        var today = DateHelper.CurrentShamsiRayvarzDate();
        dto.RayvarzActDate = today;
        var paymentRowDate = ResolvePaymentDateByStatus(ficheStatus, paymentDate, bankPaymentDate);
        dto.RowDate = tahatorFiche && paymentRowDate.Length >= 8 ? paymentRowDate : today;
    }

    public static void ApplyFromDutyColumns(
        FicheHeaderDto dto,
        int ficheStatus,
        string paymentDate,
        string bankPaymentDate,
        string printDate,
        string exportDate)
    {
        // DocDate/Due از DB؛ ActDate/RowDate = امروز شمسی (nosazo.vb)
        ApplyFromPaymentColumns(dto, ficheStatus, paymentDate, bankPaymentDate);
        var today = DateHelper.CurrentShamsiRayvarzDate();
        dto.RayvarzActDate = today;
        dto.RowDate = today;
    }

    public static string ResolveForSoap(string? fromRequest, string? fromFiche) =>
        DateHelper.ToRayvarzDate(FirstRayvarzDate(fromRequest, fromFiche));

    public static string FirstRayvarzDate(params string?[] candidates)
    {
        foreach (var c in candidates)
        {
            var d = DateHelper.ToRayvarzDate(c ?? "");
            if (d.Length >= 8)
                return d;
        }

        return "";
    }
}
