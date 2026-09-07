using System.Text.Json;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class BankInquiryRequestBuilderTests
{
    [Fact]
    public void SerializeEnvelope_wraps_fields_in_request_object()
    {
        var json = BankInquiryRequestBuilder.SerializeEnvelope(
            "FinancialAssistant",
            "secret",
            "1169619842101",
            "0000000010158",
            18);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("request", out var request));
        Assert.Equal("FinancialAssistant", request.GetProperty("userName").GetString());
        Assert.Equal("secret", request.GetProperty("password").GetString());
        Assert.Equal("1169619842101", request.GetProperty("billId").GetString());
        Assert.Equal("0000000010158", request.GetProperty("payId").GetString());
        Assert.Equal(18, request.GetProperty("bankCode").GetInt32());
    }

    [Fact]
    public void Parse_unwraps_response_object()
    {
        var raw = """{"response":{"success":true,"paymentDate":"1404/02/01"}}""";
        var result = BankInquiryResponseParser.Parse(raw, 200);

        Assert.True(result.IsPaid);
        Assert.Equal("1404/02/01", result.PaymentDate);
    }
}
