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

    public const string IncomeDutyConflictReason =
        "در درآمد و نوسازی/صنفی همزمان وجود دارد؛ به گرید اضافه نشد";

    public static string PairKey(NormalizedBillPayPair pair) => pair.BillId + "|" + pair.PaymentId;

    public static string PairKey(string billId, string paymentId) =>
        BankInquiryConfirmHelper.NormalizeBillOrPayId(billId) + "|" +
        BankInquiryConfirmHelper.NormalizeBillOrPayId(paymentId);

    public static List<NormalizedBillPayPair> NormalizePairs(IEnumerable<UnsentBillPayPair>? pairs)
    {
        var result = new List<NormalizedBillPayPair>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pair in pairs ?? Enumerable.Empty<UnsentBillPayPair>())
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
        foreach (var pair in pairs ?? Enumerable.Empty<UnsentBillPayPair>())
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

    /// <summary>ادغام نتایج جستجوی Income سپس Duty با تشخیص تعارض همزمان در هر دو جدول.</summary>
    public static UnsentBillPayLookupResult BuildMixedLookupResult(
        IReadOnlyList<NormalizedBillPayPair> requested,
        IReadOnlyList<UnsentFicheListItem> incomeItems,
        IReadOnlyList<UnsentFicheListItem> dutyMatchesForIncomePairs,
        IReadOnlyList<UnsentFicheListItem> dutyItemsForRemaining)
    {
        var result = new UnsentBillPayLookupResult
        {
            MixedLookup = true,
            Requested = requested.Count
        };

        var incomeByKey = new Dictionary<string, UnsentFicheListItem>(StringComparer.Ordinal);
        foreach (var item in incomeItems)
        {
            var key = MatchKey(item.BillId, item.PaymentId);
            if (key.Length == 0 || incomeByKey.ContainsKey(key))
                continue;
            incomeByKey[key] = item;
        }

        var conflictKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var dutyHit in dutyMatchesForIncomePairs)
        {
            var key = MatchKey(dutyHit.BillId, dutyHit.PaymentId);
            if (key.Length > 0 && incomeByKey.ContainsKey(key))
                conflictKeys.Add(key);
        }

        var acceptedKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (key, item) in incomeByKey)
        {
            if (conflictKeys.Contains(key))
            {
                result.Conflicts.Add(new UnsentBillPayConflict
                {
                    BillId = item.BillId,
                    PaymentId = item.PaymentId,
                    Reason = IncomeDutyConflictReason
                });
                continue;
            }

            item.SourceKind = UnsentFicheKind.Income;
            result.Items.Add(item);
            acceptedKeys.Add(key);
        }

        var dutyByKey = new Dictionary<string, UnsentFicheListItem>(StringComparer.Ordinal);
        foreach (var item in dutyItemsForRemaining)
        {
            var key = MatchKey(item.BillId, item.PaymentId);
            if (key.Length == 0 || dutyByKey.ContainsKey(key))
                continue;
            item.SourceKind = UnsentFicheKind.Duty;
            dutyByKey[key] = item;
        }

        foreach (var (key, item) in dutyByKey)
        {
            result.Items.Add(item);
            acceptedKeys.Add(key);
        }

        foreach (var pair in requested)
        {
            var key = PairKey(pair);
            if (conflictKeys.Contains(key) || acceptedKeys.Contains(key))
                continue;
            result.Misses.Add(new UnsentBillPayMiss
            {
                BillId = pair.BillId,
                PaymentId = pair.PaymentId,
                Reason = "در دیتابیس یافت نشد"
            });
        }

        result.Found = result.Items.Count;
        result.NotFound = result.Misses.Count;
        result.ConflictCount = result.Conflicts.Count;
        return result;
    }
}
