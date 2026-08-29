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

    public static (string WhereClause, List<(string Name, object Value)> Parameters)? BuildWhereClause(
        string? ficheNo,
        string? billId,
        string? paymentId,
        string? identifierValue)
    {
        var normalizedFicheNo = (ficheNo ?? "").Trim();
        var normalizedBillId = (billId ?? "").Trim();
        var normalizedPaymentId = (paymentId ?? "").Trim();
        var normalizedIdentifier = (identifierValue ?? "").Trim();

        if (!string.IsNullOrEmpty(normalizedFicheNo))
        {
            return ("FicheNo = @ficheNo", [("@ficheNo", normalizedFicheNo)]);
        }

        if (!string.IsNullOrEmpty(normalizedBillId) && !string.IsNullOrEmpty(normalizedPaymentId))
        {
            return (
                "BillID = @billId AND PaymentID = @paymentId",
                [("@billId", normalizedBillId), ("@paymentId", normalizedPaymentId)]);
        }

        if (!string.IsNullOrEmpty(normalizedIdentifier))
        {
            var identifierFilter = FicheDateChangeHelper.BuildIdentifierFilter(normalizedIdentifier);
            if (identifierFilter == null)
                return null;

            return (identifierFilter.Value.Clause, [(identifierFilter.Value.ParamName, identifierFilter.Value.Value)]);
        }

        return null;
    }

    public static string? ValidateRequest(BankInquiryConfirmRequest req)
    {
        if (!TryNormalizeSlashDate(req.PaymentDate, out _))
            return "تاریخ پرداخت نامعتبر است";

        var hasFicheNo = !string.IsNullOrWhiteSpace(req.FicheNo);
        var hasBillPayment = !string.IsNullOrWhiteSpace(req.BillId) && !string.IsNullOrWhiteSpace(req.PaymentId);
        var hasIdentifier = !string.IsNullOrWhiteSpace(req.IdentifierValue);

        if (!hasFicheNo && !hasBillPayment && !hasIdentifier)
            return "شماره فیش یا شناسه قبض و شناسه پرداخت را وارد کنید";

        if (hasBillPayment && (string.IsNullOrWhiteSpace(req.BillId) || string.IsNullOrWhiteSpace(req.PaymentId)))
            return "هر دو فیلد شناسه قبض و شناسه پرداخت الزامی است";

        if (BuildWhereClause(req.FicheNo, req.BillId, req.PaymentId, req.IdentifierValue) == null)
            return "شناسه فیش نامعتبر است";

        return null;
    }
}
