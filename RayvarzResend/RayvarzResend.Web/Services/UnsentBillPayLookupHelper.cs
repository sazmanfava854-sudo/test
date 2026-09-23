using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public readonly record struct NormalizedBillPayPair(
    string BillId,
    string PaymentId,
    string BillIdTrim,
    string PaymentIdTrim);

/// <summary>نرمال‌سازی شناسه قبض/پرداخت فایل اکسل برای جستجوی فیش‌های ارسال‌نشده.</summary>
public static class UnsentBillPayLookupHelper
{
    public const int MaxPairs = 400;

    public static List<NormalizedBillPayPair> NormalizePairs(IEnumerable<UnsentBillPayPair>? pairs)
    {
        var result = new List<NormalizedBillPayPair>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in pairs ?? [])
        {
            var bill = BankInquiryConfirmHelper.NormalizeBillOrPayId(pair.BillId);
            var pay = BankInquiryConfirmHelper.NormalizeBillOrPayId(pair.PaymentId);
            if (bill.Length == 0 || pay.Length == 0)
                continue;

            var key = bill + "|" + pay;
            if (!seen.Add(key))
                continue;

            result.Add(new NormalizedBillPayPair(bill, pay, TrimLeadingZeros(bill), TrimLeadingZeros(pay)));
        }

        return result;
    }

    public static string MatchKey(string? billId, string? paymentId)
    {
        var bill = BankInquiryConfirmHelper.NormalizeBillOrPayId(billId);
        var pay = BankInquiryConfirmHelper.NormalizeBillOrPayId(paymentId);
        return bill.Length == 0 || pay.Length == 0 ? "" : bill + "|" + pay;
    }

    public static string TrimLeadingZeros(string digits)
    {
        if (string.IsNullOrEmpty(digits))
            return "0";

        var trimmed = digits.TrimStart('0');
        return trimmed.Length == 0 ? "0" : trimmed;
    }

    public static Dictionary<string, (string RawBill, string RawPay)> IndexRawPairs(
        IEnumerable<UnsentBillPayPair>? pairs)
    {
        var map = new Dictionary<string, (string RawBill, string RawPay)>(StringComparer.Ordinal);
        foreach (var pair in pairs ?? [])
        {
            var bill = BankInquiryConfirmHelper.NormalizeBillOrPayId(pair.BillId);
            var pay = BankInquiryConfirmHelper.NormalizeBillOrPayId(pair.PaymentId);
            if (bill.Length == 0 || pay.Length == 0)
                continue;

            var key = bill + "|" + pay;
            if (!map.ContainsKey(key))
                map[key] = ((pair.BillId ?? "").Trim(), (pair.PaymentId ?? "").Trim());
        }

        return map;
    }

    public static string DescribeMiss(
        BillPayMissDiagnostic? diagnostic,
        string? displayBillId = null,
        string? displayPaymentId = null)
    {
        var idSuffix = FormatMissIdSuffix(displayBillId, displayPaymentId);
        if (diagnostic == null || !diagnostic.Found)
            return $"قابل ارسال نیست — فیشی با این شناسه‌ها پیدا نشد{idSuffix}";

        if (diagnostic.SwappedColumns)
            return $"قابل ارسال نیست — شناسه قبض و پرداخت در اکسل جابه‌جا شده{idSuffix}";

        if (diagnostic.AlreadySent)
            return $"فیش {diagnostic.FicheNo}: قبلاً ارسال شده{idSuffix}";

        if (diagnostic.Cancelled)
            return $"فیش {diagnostic.FicheNo}: لغو شده{idSuffix}";

        return $"قابل ارسال نیست{idSuffix}";
    }

    private static string FormatMissIdSuffix(string? displayBillId, string? displayPaymentId)
    {
        var bill = (displayBillId ?? "").Trim();
        var pay = (displayPaymentId ?? "").Trim();
        if (bill.Length == 0 && pay.Length == 0)
            return "";

        return $" — شناسه قبض: {bill}، شناسه پرداخت: {pay}";
    }

    public static string? ValidateRequest(UnsentBillPayLookupRequest? req)
    {
        if (req == null)
            return "درخواست خالی است";

        var pairs = NormalizePairs(req.Pairs);
        if (pairs.Count == 0)
            return "فایل اکسل باید حداقل یک ردیف با شناسه قبض و شناسه پرداخت معتبر داشته باشد";

        if (pairs.Count > MaxPairs)
            return $"حداکثر {MaxPairs} ردیف در هر بارگذاری مجاز است";

        return null;
    }
}
