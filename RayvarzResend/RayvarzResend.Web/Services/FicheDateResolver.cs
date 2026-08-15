using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تاریخ‌های عملیاتی هر فیش برای DocDate / ActDate / Due — از ستون‌های Sara8M03، نه appsettings.</summary>
public static class FicheDateResolver
{
    /// <summary>
    /// تاریخ مؤثر از PaymentDate / BankPaymentDate (همان منبع فیلتر فیش جمعی):
    /// وضعیت ۱ → PaymentDate؛ غیر آن → BankPaymentDate — با fallback به ستون دیگر.
    /// </summary>
    public static string ResolvePaymentDateByStatus(int ficheStatus, string paymentDate, string bankPaymentDate) =>
        ficheStatus == 1
            ? FirstRayvarzDate(paymentDate, bankPaymentDate)
            : FirstRayvarzDate(bankPaymentDate, paymentDate);

    public static void ApplyFromIncomeColumns(
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

    public static void ApplyFromDutyColumns(
        FicheHeaderDto dto,
        string paymentDate,
        string bankPaymentDate,
        string printDate,
        string exportDate)
    {
        dto.RayvarzDocDate = FirstRayvarzDate(printDate, exportDate, paymentDate, bankPaymentDate);
        dto.RayvarzActDate = FirstRayvarzDate(bankPaymentDate, paymentDate, printDate, exportDate);
        dto.RayvarzDueDate = FirstRayvarzDate(bankPaymentDate, paymentDate, printDate, exportDate);
        dto.RowDate = dto.RayvarzActDate;
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
