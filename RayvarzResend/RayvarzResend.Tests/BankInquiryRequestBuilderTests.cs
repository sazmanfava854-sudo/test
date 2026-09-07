using System.Text.Json;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class BankInquiryRequestBuilderTests
{
    [Fact]
    public void SerializeEnvelope_uses_flat_body_with_bank_code()
    {
        var json = BankInquiryRequestBuilder.SerializeEnvelope(
            "FinancialAssistant",
            "secret",
            "1169619842101",
            "0000000010158",
            18);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.False(root.TryGetProperty("request", out _));
        Assert.Equal("FinancialAssistant", root.GetProperty("userName").GetString());
        Assert.Equal("secret", root.GetProperty("password").GetString());
        Assert.Equal("1169619842101", root.GetProperty("billId").GetString());
        Assert.Equal("0000000010158", root.GetProperty("payId").GetString());
        Assert.Equal(18, root.GetProperty("bankCode").GetInt32());
    }

    [Fact]
    public void BuildBillPayEnvelope_uses_flat_body_without_request_wrapper()
    {
        var envelope = BankInquiryRequestBuilder.BuildBillPayEnvelope(
            "FinancialAssistant",
            "secret",
            "9000152552362",
            "4172333232581");

        var json = JsonSerializer.Serialize(envelope);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.False(root.TryGetProperty("request", out _));
        Assert.Equal("FinancialAssistant", root.GetProperty("userName").GetString());
        Assert.Equal("secret", root.GetProperty("password").GetString());
        Assert.Equal("9000152552362", root.GetProperty("billId").GetString());
        Assert.Equal("4172333232581", root.GetProperty("payId").GetString());
        Assert.False(root.TryGetProperty("bankCode", out _));
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
