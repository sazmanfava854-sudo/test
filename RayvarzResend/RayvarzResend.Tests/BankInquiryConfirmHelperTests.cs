using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class BankInquiryConfirmHelperTests
{
    [Fact]
    public void ValidateSearchRequest_requires_at_least_one_filter()
    {
        Assert.Equal(
            "حداقل یکی از فیلترها را وارد کنید: شماره فیش، یا شناسه قبض و شناسه پرداخت",
            BankInquiryConfirmHelper.ValidateSearchRequest(new BankInquirySearchRequest()));
    }

    [Fact]
    public void ValidateSearchRequest_accepts_fiche_no()
    {
        Assert.Null(BankInquiryConfirmHelper.ValidateSearchRequest(new BankInquirySearchRequest
        {
            FicheNo = "101104/9881711"
        }));
    }

    [Fact]
    public void ValidateSearchRequest_accepts_bill_and_payment_ids()
    {
        Assert.Null(BankInquiryConfirmHelper.ValidateSearchRequest(new BankInquirySearchRequest
        {
            BillId = "1234567890",
            PaymentId = "9876543210"
        }));
    }

    [Fact]
    public void BuildSearchWhere_uses_fiche_no()
    {
        var (where, parameters) = BankInquiryConfirmHelper.BuildSearchWhere(new BankInquirySearchRequest
        {
            FicheNo = "101104/9881711"
        });

        Assert.Contains("FicheNo", where);
        Assert.DoesNotContain("PaymentDate", where);
        Assert.Single(parameters);
    }

    [Fact]
    public void IncomeNosaziCodeSql_uses_base_nosazi_join_fields()
    {
        Assert.Contains("b.District", BankInquiryConfirmHelper.IncomeNosaziCodeSql);
        Assert.Contains("b.Shop", BankInquiryConfirmHelper.IncomeNosaziCodeSql);
        Assert.Contains("Base_NosaziCode", BankInquiryConfirmHelper.IncomeFicheNosaziJoins);
        Assert.Contains("Sh_RequestInfo", BankInquiryConfirmHelper.IncomeFicheNosaziJoins);
    }

    [Fact]
    public void ValidateConfirmRequest_requires_selection_and_new_date()
    {
        Assert.Equal("حداقل یک فیش از نتایج انتخاب کنید",
            BankInquiryConfirmHelper.ValidateConfirmRequest(new BankInquiryConfirmRequest()));
        Assert.Equal("تاریخ پرداخت جدید نامعتبر است",
            BankInquiryConfirmHelper.ValidateConfirmRequest(new BankInquiryConfirmRequest
            {
                FicheNos = ["101104/9881711"]
            }));
    }

    [Fact]
    public void ValidateConfirmRequest_accepts_valid_payload()
    {
        Assert.Null(BankInquiryConfirmHelper.ValidateConfirmRequest(new BankInquiryConfirmRequest
        {
            FicheNos = ["101104/9881711"],
            NewPaymentDate = "1404/02/01"
        }));
    }

    [Fact]
    public void Confirmed_constants_match_business_rule()
    {
        Assert.Equal(3, BankInquiryConfirmHelper.ConfirmedFicheStatus);
        Assert.Equal(4, BankInquiryConfirmHelper.ConfirmedIncomePaymentType);
    }
}
