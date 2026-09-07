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
}
