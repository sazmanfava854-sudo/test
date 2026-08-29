using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تب «تایید استعلام بانک» — dbo.Income_Fiche.</summary>
public static class BankInquiryConfirmHelper
{
    public const int ConfirmedFicheStatus = 3;
    public const int ConfirmedIncomePaymentType = 4;

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
        if (!string.IsNullOrWhiteSpace(req.PaymentDate))
            return TryNormalizeSlashDate(req.PaymentDate, out _);
        if (!string.IsNullOrWhiteSpace(req.FicheNo)) return true;
        if (!string.IsNullOrWhiteSpace(req.IdentifierValue)) return true;
        return !string.IsNullOrWhiteSpace(req.BillId) && !string.IsNullOrWhiteSpace(req.PaymentId);
    }

    public static string? ValidateSearchRequest(BankInquirySearchRequest req)
    {
        if (!HasAnySearchFilter(req))
            return "حداقل یکی از فیلترها را وارد کنید: تاریخ پرداخت، شماره فیش، یا شناسه قبض و شناسه پرداخت";

        if (!string.IsNullOrWhiteSpace(req.PaymentDate)
            && !TryNormalizeSlashDate(req.PaymentDate, out _))
            return "تاریخ پرداخت نامعتبر است";

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

        if (!string.IsNullOrWhiteSpace(req.PaymentDate)
            && TryNormalizeSlashDate(req.PaymentDate, out var paymentDate))
        {
            clauses.Add("f.PaymentDate = @paymentDate");
            parameters.Add(("@paymentDate", paymentDate));
        }

        if (!string.IsNullOrWhiteSpace(req.FicheNo))
        {
            clauses.Add("f.FicheNo = @ficheNo");
            parameters.Add(("@ficheNo", req.FicheNo.Trim()));
        }
        else if (!string.IsNullOrWhiteSpace(req.BillId) && !string.IsNullOrWhiteSpace(req.PaymentId))
        {
            clauses.Add("f.BillID = @billId AND f.PaymentID = @paymentId");
            parameters.Add(("@billId", req.BillId.Trim()));
            parameters.Add(("@paymentId", req.PaymentId.Trim()));
        }
        else if (!string.IsNullOrWhiteSpace(req.IdentifierValue))
        {
            var identifierFilter = FicheDateChangeHelper.BuildIdentifierFilter(req.IdentifierValue);
            if (identifierFilter != null)
            {
                clauses.Add(identifierFilter.Value.Clause);
                parameters.Add((identifierFilter.Value.ParamName, identifierFilter.Value.Value));
            }
        }

        return (string.Join(" AND ", clauses), parameters);
    }

    public static string? ValidateConfirmRequest(BankInquiryConfirmRequest req)
    {
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
