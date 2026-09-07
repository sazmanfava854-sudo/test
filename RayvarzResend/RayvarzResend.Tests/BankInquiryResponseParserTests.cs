using System.Text.Json;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class BankInquiryResponseParserTests
{
    [Fact]
    public void Parse_success_true_means_paid()
    {
        var raw = """{"success":true,"message":"پرداخت موفق","paymentDate":"1404/02/01"}""";
        var result = BankInquiryResponseParser.Parse(raw, 200);

        Assert.True(result.IsPaid);
        Assert.Equal("1404/02/01", result.PaymentDate);
    }

    [Fact]
    public void Parse_success_false_means_unpaid()
    {
        var raw = """{"success":false,"message":"فیش پرداخت نشده"}""";
        var result = BankInquiryResponseParser.Parse(raw, 200);

        Assert.False(result.IsPaid);
        Assert.Equal("فیش پرداخت نشده", result.Message);
    }

    [Fact]
    public void Parse_result_code_zero_means_paid()
    {
        var raw = """{"result":0,"description":"OK","data":{"paymentDate":"1405/01/15"}}""";
        var result = BankInquiryResponseParser.Parse(raw, 200);

        Assert.True(result.IsPaid);
        Assert.Equal("1405/01/15", result.PaymentDate);
    }

    [Fact]
    public void Parse_unpaid_message_defaults_to_not_paid()
    {
        var raw = """{"message":"فیش در بانک یافت نشد"}""";
        var result = BankInquiryResponseParser.Parse(raw, 200);

        Assert.False(result.IsPaid);
        Assert.Equal("فیش در بانک یافت نشد", result.Message);
    }

    [Fact]
    public void Parse_http_error_is_service_error()
    {
        var result = BankInquiryResponseParser.Parse("""{"message":"خطا"}""", 500);

        Assert.False(result.IsPaid);
        Assert.True(result.ServiceError);
    }

    [Fact]
    public void Parse_invalid_json_is_service_error()
    {
        var result = BankInquiryResponseParser.Parse("not-json", 200);

        Assert.False(result.IsPaid);
        Assert.True(result.ServiceError);
    }

    [Fact]
    public void Unpaid_fiche_message_constant_matches_requirement()
    {
        Assert.Equal("فیش پرداخت نشده", BankInquiryConfirmHelper.UnpaidFicheMessage);
    }

    [Fact]
    public void Parse_http_error_502_includes_gateway_hint()
    {
        var result = BankInquiryResponseParser.ParseHttpError(502, "<html>Bad Gateway</html>");

        Assert.False(result.IsPaid);
        Assert.True(result.ServiceError);
        Assert.Contains("502", result.Message);
        Assert.Contains("Bad Gateway", result.Message);
    }

    [Fact]
    public void BuildUserErrorMessage_ssl_includes_network_hint()
    {
        var message = BankInquiryApiClient.BuildUserErrorMessage(
            new HttpRequestException("The SSL connection could not be established"));

        Assert.Contains("SSL", message);
        Assert.Contains("UseSystemProxy", message);
    }

    [Fact]
    public void Parse_fiche_lookup_portal_style_paid_response()
    {
        var raw = """
            {
              "intResult": 1,
              "strResult": "قبض با شناسه قبض و شناسه پرداخت وارد شده به شرح زیر یافت شد",
              "isPay": 1,
              "fichesId": 7021215,
              "registerDate": "1405/04/24",
              "forgeDate": "1405/04/23",
              "payAmount": 41723332
            }
            """;
        var step = BankInquiryResponseParser.ParseFicheLookupStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.Paid, step.Kind);
        Assert.Equal("1405/04/24", step.PaymentDate);
    }

    [Fact]
    public void Parse_fiche_lookup_record_with_dates_but_missing_isPay_is_paid()
    {
        var raw = """{"intResult":1,"strResult":"OK","fichesId":7021215,"registerDate":"1405/04/24"}""";
        var step = BankInquiryResponseParser.ParseFicheLookupStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.Paid, step.Kind);
    }

    [Fact]
    public void Parse_fiche_lookup_no_data_message_is_record_not_found()
    {
        var raw = """{"intResult":1,"strResult":"عدم وجود اطلاعات بر اساس پارامترهای ورودی","isPay":0,"fichesId":0}""";
        var step = BankInquiryResponseParser.ParseFicheLookupStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.RecordNotFound, step.Kind);
    }

    [Fact]
    public void Parse_online_bank_service_failure_is_service_error()
    {
        var raw = """{"intResualt":1,"strResualt":"خطا در ارتباط با سرویس بانک شهر: contract mismatch","pay":0}""";
        var step = BankInquiryResponseParser.ParseOnlineBankStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.ServiceError, step.Kind);
        Assert.Contains("سرویس بانک", step.Message);
    }

    [Fact]
    public void Parse_fiche_lookup_isPay_1_means_paid()
    {
        var raw = """{"intResult":0,"strResult":"OK","isPay":1,"fichesId":12345,"registerDate":"1405/06/10"}""";
        var step = BankInquiryResponseParser.ParseFicheLookupStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.Paid, step.Kind);
        Assert.Equal("1405/06/10", step.PaymentDate);
    }

    [Fact]
    public void Parse_fiche_lookup_isPay_0_with_fiche_means_not_paid()
    {
        var raw = """{"intResult":0,"strResult":"پرداخت نشده","isPay":0,"fichesId":12345}""";
        var step = BankInquiryResponseParser.ParseFicheLookupStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.NotPaid, step.Kind);
    }

    [Fact]
    public void Parse_fiche_lookup_not_found_goes_to_record_not_found()
    {
        var raw = """{"intResult":1,"strResult":"فیش یافت نشد","isPay":0,"fichesId":0}""";
        var step = BankInquiryResponseParser.ParseFicheLookupStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.RecordNotFound, step.Kind);
    }

    [Fact]
    public void Parse_online_bank_pay_1_means_paid()
    {
        var raw = """{"intResualt":0,"strResualt":"OK","pay":1,"payDate":"1405/06/11"}""";
        var step = BankInquiryResponseParser.ParseOnlineBankStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.Paid, step.Kind);
        Assert.Equal("1405/06/11", step.PaymentDate);
    }

    [Fact]
    public void Parse_online_bank_pay_0_means_not_paid()
    {
        var raw = """{"intResualt":0,"strResualt":"پرداخت نشده","pay":0}""";
        var step = BankInquiryResponseParser.ParseOnlineBankStep(raw, 200);

        Assert.Equal(BankInquiryStepKind.NotPaid, step.Kind);
    }

    [Fact]
    public void Parse_online_bank_http_503_is_service_error()
    {
        var step = BankInquiryResponseParser.ParseOnlineBankStep("""{"message":"down"}""", 503);

        Assert.Equal(BankInquiryStepKind.ServiceError, step.Kind);
        Assert.Contains("503", step.Message);
        Assert.Contains(BankInquiryResponseParser.OnlineBankSourceLabel, step.Message);
    }
}
