using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class BankInquiryConfirmHelperTests
{
    [Fact]
    public void ValidateRequest_requires_payment_date_and_identifier()
    {
        Assert.Equal("تاریخ پرداخت نامعتبر است", BankInquiryConfirmHelper.ValidateRequest(new BankInquiryConfirmRequest()));
        Assert.Equal("شماره فیش یا شناسه قبض و شناسه پرداخت را وارد کنید",
            BankInquiryConfirmHelper.ValidateRequest(new BankInquiryConfirmRequest
            {
                PaymentDate = "1404/01/11"
            }));
    }

    [Fact]
    public void ValidateRequest_accepts_fiche_no()
    {
        Assert.Null(BankInquiryConfirmHelper.ValidateRequest(new BankInquiryConfirmRequest
        {
            PaymentDate = "1404/01/11",
            FicheNo = "101104/9881711"
        }));
    }

    [Fact]
    public void ValidateRequest_accepts_bill_and_payment_ids()
    {
        Assert.Null(BankInquiryConfirmHelper.ValidateRequest(new BankInquiryConfirmRequest
        {
            PaymentDate = "14040111",
            BillId = "1234567890",
            PaymentId = "9876543210"
        }));
    }

    [Fact]
    public void BuildWhereClause_uses_fiche_no_or_bill_payment()
    {
        var fiche = BankInquiryConfirmHelper.BuildWhereClause("101104/9881711", null, null, null);
        Assert.NotNull(fiche);
        Assert.Contains("FicheNo", fiche.Value.WhereClause);

        var bill = BankInquiryConfirmHelper.BuildWhereClause(null, "111", "222", null);
        Assert.NotNull(bill);
        Assert.Contains("BillID", bill.Value.WhereClause);
        Assert.Contains("PaymentID", bill.Value.WhereClause);
    }

    [Fact]
    public void Confirmed_constants_match_business_rule()
    {
        Assert.Equal(3, BankInquiryConfirmHelper.ConfirmedFicheStatus);
        Assert.Equal(4, BankInquiryConfirmHelper.ConfirmedIncomePaymentType);
    }
}
