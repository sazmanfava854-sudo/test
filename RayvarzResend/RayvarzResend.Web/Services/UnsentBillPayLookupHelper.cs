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

    public static string DescribeMiss(BillPayMissDiagnostic? diagnostic)
    {
        if (diagnostic == null || !diagnostic.Found)
            return "فیش ارسال‌نشده با این شناسه قبض و شناسه پرداخت یافت نشد";

        if (diagnostic.SwappedColumns)
            return "ستون‌های اکسل جابه‌جا است؛ مقدار «شناسه قبض» و «شناسه پرداخت» را در فایل با دیتابیس یکسان کنید";

        if (diagnostic.AlreadySent)
            return $"فیش {diagnostic.FicheNo} قبلاً در رایورز ثبت شده و در لیست ارسال‌نشده نیست";

        if (diagnostic.Cancelled)
            return $"فیش {diagnostic.FicheNo} لغو شده است";

        return "فیش ارسال‌نشده با این شناسه قبض و شناسه پرداخت یافت نشد";
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
