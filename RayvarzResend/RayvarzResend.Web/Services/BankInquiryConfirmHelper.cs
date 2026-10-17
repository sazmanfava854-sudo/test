using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تب «خدمات الکترونیک» — dbo.Income_Fiche.</summary>
public static class BankInquiryConfirmHelper
{
    public const int ConfirmedFicheStatus = 3;
    public const int ConfirmedIncomePaymentType = 4;
    public const string UnpaidFicheMessage = "فیش پرداخت نشده";

    /// <summary>همان فرمول FicheRepository برای Income_Fiche → Base_NosaziCode.</summary>
    public const string IncomeNosaziCodeSql = """
        ISNULL(
          NULLIF(LTRIM(RTRIM(
            CAST(b.District AS varchar) + '-' + CAST(b.Region AS varchar) + '-' +
            CAST(b.Block AS varchar) + '-' + CAST(b.House AS varchar) + '-' +
            CAST(b.Building AS varchar) + '-' + CAST(b.Apartment AS varchar) + '-' +
            ISNULL(NULLIF(CAST(b.Shop AS varchar), ''), '0')
          )), '-'),
          ''
        )
        """;

    public const string IncomeFicheNosaziJoins = """
        JOIN dbo.Income i ON i.NidIncome = f.NidIncome
        LEFT JOIN dbo.Sh_RequestInfo r ON r.NidProc = i.NidProc
        LEFT JOIN dbo.Base_NosaziCode b ON b.NidNosaziCode = r.NidNosaziCode
        """;

    public static string NormalizeSlashDate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        var rayvarz = DateHelper.ToRayvarzDate(input);
        return rayvarz.Length >= 8 ? DateHelper.ToShamsiSlashDate(rayvarz) : "";
    }

    public static bool TryNormalizeSlashDate(string? input, out string slashDate)
    {
        slashDate = NormalizeSlashDate(input);
        return !string.IsNullOrEmpty(slashDate);
    }

    public static bool HasAnySearchFilter(BankInquirySearchRequest req)
    {
        if (!string.IsNullOrWhiteSpace(req.FicheNo)) return true;
        if (!string.IsNullOrWhiteSpace(req.IdentifierValue)) return true;
        return !string.IsNullOrWhiteSpace(req.BillId) && !string.IsNullOrWhiteSpace(req.PaymentId);
    }

    public static string? ValidateSearchRequest(BankInquirySearchRequest req)
    {
        if (!HasAnySearchFilter(req))
            return "حداقل یکی از فیلترها را وارد کنید: شماره فیش، یا شناسه قبض و شناسه پرداخت";

        var hasBill = !string.IsNullOrWhiteSpace(req.BillId);
        var hasPayment = !string.IsNullOrWhiteSpace(req.PaymentId);
        if (hasBill != hasPayment)
            return "هر دو فیلد شناسه قبض و شناسه پرداخت الزامی است";

        return null;
    }

    public static (string WhereSql, List<(string Name, object Value)> Parameters) BuildSearchWhere(
        BankInquirySearchRequest req)
    {
        var clauses = new List<string>();
        var parameters = new List<(string, object)>();

        if (!string.IsNullOrWhiteSpace(req.FicheNo))
        {
            var keys = UnsentBillPayLookupHelper.ExpandFicheNoKeys(req.FicheNo);
            var names = new List<string>(keys.Count);
            for (var i = 0; i < keys.Count; i++)
            {
                var name = $"@ficheNo{i}";
                names.Add(name);
                parameters.Add((name, keys[i]));
            }
            if (names.Count > 0)
                clauses.Add("f.FicheNo IN (" + string.Join(", ", names) + ")");
        }
        else if (!string.IsNullOrWhiteSpace(req.BillId) && !string.IsNullOrWhiteSpace(req.PaymentId))
        {
            var billKeys = UnsentBillPayLookupHelper.ExpandLookupKeys(req.BillId);
            var payKeys = UnsentBillPayLookupHelper.ExpandLookupKeys(req.PaymentId);
            var billNames = new List<string>(billKeys.Count);
            var payNames = new List<string>(payKeys.Count);
            for (var i = 0; i < billKeys.Count; i++)
            {
                var name = $"@billId{i}";
                billNames.Add(name);
                parameters.Add((name, billKeys[i]));
            }
            for (var i = 0; i < payKeys.Count; i++)
            {
                var name = $"@paymentId{i}";
                payNames.Add(name);
                parameters.Add((name, payKeys[i]));
            }
            if (billNames.Count > 0 && payNames.Count > 0)
                clauses.Add($"f.BillID IN ({string.Join(", ", billNames)}) AND f.PaymentID IN ({string.Join(", ", payNames)})");
        }
        else if (!string.IsNullOrWhiteSpace(req.IdentifierValue))
        {
            var identifierFilter = FicheDateChangeHelper.BuildIdentifierFilter(req.IdentifierValue);
            if (identifierFilter != null)
            {
                clauses.Add(identifierFilter.Value.Clause);
                foreach (var (name, value) in identifierFilter.Value.Parameters)
                    parameters.Add((name, value));
            }
        }

        return (string.Join(" AND ", clauses), parameters);
    }

    /// <summary>شناسه قبض/پرداخت epay — فقط رقم، با صفر پیشرو تا ۱۳ رقم.</summary>
    public const int EpayBillPayIdLength = 13;

    public static string NormalizeBillOrPayId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        var digits = NumericHelper.NormalizeDigits(value.Trim());
        var onlyDigits = new string(digits.Where(char.IsDigit).ToArray());
        if (onlyDigits.Length == 0)
            return "";

        return onlyDigits.Length < EpayBillPayIdLength
            ? onlyDigits.PadLeft(EpayBillPayIdLength, '0')
            : onlyDigits;
    }

    public static string? ValidateConfirmRequest(BankInquiryConfirmRequest? req)
    {
        if (req == null)
            return "درخواست نامعتبر است";

        var ficheNos = (req.FicheNos ?? [])
            .Select(s => (s ?? "").Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (ficheNos.Count == 0)
            return "حداقل یک فیش از نتایج انتخاب کنید";

        if (!TryNormalizeSlashDate(req.NewPaymentDate, out _))
            return "تاریخ پرداخت جدید نامعتبر است";

        return null;
    }
}
