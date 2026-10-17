using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public readonly record struct NormalizedBillPayPair(
    string BillId,
    string PaymentId,
    string BillIdTrim,
    string PaymentIdTrim,
    string FicheNo);

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
        foreach (var pair in pairs ?? [])
        {
            var bill = BankInquiryConfirmHelper.NormalizeBillOrPayId(pair.BillId);
            var pay = BankInquiryConfirmHelper.NormalizeBillOrPayId(pair.PaymentId);
            if (bill.Length == 0 || pay.Length == 0)
                continue;

            var key = bill + "|" + pay;
            if (!seen.Add(key))
                continue;

            var ficheNo = IdentifierText.Normalize(pair.FicheNo);
            result.Add(new NormalizedBillPayPair(bill, pay, TrimLeadingZeros(bill), TrimLeadingZeros(pay), ficheNo));
        }

        return result;
    }

    public static string MatchKey(string? billId, string? paymentId)
    {
        var bill = BankInquiryConfirmHelper.NormalizeBillOrPayId(billId);
        var pay = BankInquiryConfirmHelper.NormalizeBillOrPayId(paymentId);
        return bill.Length == 0 || pay.Length == 0 ? "" : bill + "|" + pay;
    }

    /// <summary>
    /// شکل‌های ذخیره‌شدهٔ ممکن یک شناسه (۱۳ رقم، بدون یک صفر، بدون همهٔ صفرها)
    /// تا مقایسهٔ مستقیم ستون ایندکس‌شده ممکن شود.
    /// </summary>
    public static IReadOnlyList<string> ExpandLookupKeys(string? value)
    {
        var keys = new List<string>();
        void Add(string? candidate)
        {
            var text = (candidate ?? "").Trim();
            if (text.Length == 0 || keys.Exists(existing => existing == text))
                return;
            keys.Add(text);
        }

        Add(value);
        var normalized = BankInquiryConfirmHelper.NormalizeBillOrPayId(value);
        Add(normalized);
        var current = normalized;
        while (current.Length > 1 && current[0] == '0' && keys.Count < 8)
        {
            current = current[1..];
            Add(current);
        }

        return keys;
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

        if (diagnostic.AlreadySent)
            return $"فیش {diagnostic.FicheNo}: قبلاً ارسال شده{idSuffix}";

        if (diagnostic.Cancelled)
            return $"فیش {diagnostic.FicheNo}: لغو شده{idSuffix}";

        if (diagnostic.SwappedColumns && !IsSameBillAndPayment(displayBillId, displayPaymentId))
            return $"قابل ارسال نیست — شناسه قبض و پرداخت در اکسل جابه‌جا شده{idSuffix}";

        return $"قابل ارسال نیست{idSuffix}";
    }

    /// <summary>
    /// فیش تهاتری که در رایورز با مبلغ منفی ثبت شده (مبلغ/۱۵۷) نباید در گرید ارسال اکسل بماند؛
    /// فیش جفت (درآمدی/۱۵۸) نشان داده می‌شود.
    /// </summary>
    public static bool ShouldReplaceWithTahatorPartner(
        bool isTahator,
        decimal? rayvarzAmount,
        string? matchedFicheNo,
        string? partnerFicheNo)
    {
        if (!isTahator || rayvarzAmount is not < 0)
            return false;

        var partner = (partnerFicheNo ?? "").Trim();
        var matched = (matchedFicheNo ?? "").Trim();
        return partner.Length > 0 && !string.Equals(partner, matched, StringComparison.Ordinal);
    }

    public static bool IsSameBillAndPayment(string? billId, string? paymentId)
    {
        var bill = BankInquiryConfirmHelper.NormalizeBillOrPayId(billId);
        var pay = BankInquiryConfirmHelper.NormalizeBillOrPayId(paymentId);
        return bill.Length > 0 && bill == pay;
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
