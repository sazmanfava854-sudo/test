using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تب «خدمات الکترونیک» — dbo.Income_Fiche.</summary>
public static class BankInquiryConfirmHelper
{
    public const int ConfirmedFicheStatus = 3;
    public const int ConfirmedIncomePaymentType = 4;

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

        if (!string.IsNullOrWhiteSpace(req.BillId) && !string.IsNullOrWhiteSpace(req.PaymentId))
        {
            clauses.Add("f.BillID = @billId AND f.PaymentID = @paymentId");
            parameters.Add(("@billId", req.BillId.Trim()));
            parameters.Add(("@paymentId", req.PaymentId.Trim()));
        }
        else if (!string.IsNullOrWhiteSpace(req.FicheNo))
        {
            clauses.Add("f.FicheNo = @ficheNo");
            parameters.Add(("@ficheNo", req.FicheNo.Trim()));
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
