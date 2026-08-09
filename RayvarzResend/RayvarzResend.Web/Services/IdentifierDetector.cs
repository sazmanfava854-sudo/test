using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// تشخیص خودکار: شماره فیش (FicheNo) یا BillID+PaymentID ادغام‌شده.
/// </summary>
public static class IdentifierDetector
{
    /// <summary>حداقل طول رشتهٔ عددی برای BillID+PaymentID (معمولاً ۲۶ رقم).</summary>
    private const int BillPaymentMinDigits = 20;

    public static IdentifierType Detect(string? raw)
    {
        var value = (raw ?? "").Trim();
        if (value.Length == 0)
            return IdentifierType.FicheNo;

        // FicheNo: اغلب اسلش دارد — 101104/9881711
        if (value.Contains('/'))
            return IdentifierType.FicheNo;

        var digitsOnly = value.All(char.IsDigit);
        if (digitsOnly && value.Length >= BillPaymentMinDigits)
            return IdentifierType.BillPaymentKey;

        return IdentifierType.FicheNo;
    }

    public static string Describe(IdentifierType type) =>
        type == IdentifierType.BillPaymentKey
            ? "BillID + PaymentID"
            : "شماره فیش (FicheNo)";
}
