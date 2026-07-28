using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تاریخ‌های عملیاتی هر فیش برای DocDate / ActDate / Due — از ستون‌های Sara8M03، نه appsettings.</summary>
internal static class FicheDateResolver
{
    public static void ApplyFromIncomeColumns(
        FicheHeaderDto dto,
        string paymentDate,
        string bankPaymentDate)
    {
        dto.RayvarzDocDate = FirstRayvarzDate(paymentDate, bankPaymentDate);
        dto.RayvarzActDate = FirstRayvarzDate(bankPaymentDate, paymentDate);
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
