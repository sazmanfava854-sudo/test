using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class InstallmentExcelMatcherTests
{
    [Theory]
    [InlineData("1405/09/25", "14050925", true)]
    [InlineData("1405-09-25", "1405/09/25", true)]
    [InlineData("1405/09/25", "1405/10/01", false)]
    public void DatesMatch_normalizes_shamsi_formats(string a, string b, bool expected)
    {
        Assert.Equal(expected, InstallmentExcelMatcher.DatesMatch(a, b));
    }

    [Theory]
    [InlineData("20335141229.000", 20335141229.000, true)]
    [InlineData("813516015", 813516015.000, true)]
    [InlineData("100", 200, false)]
    public void CostsMatch_compares_decimal_values(string excel, decimal db, bool expected)
    {
        Assert.Equal(expected, InstallmentExcelMatcher.CostsMatch(excel, db));
    }

    [Fact]
    public void ResolveLookup_prefers_NoDocument_when_both_present()
    {
        var row = new InstallmentExcelRowInput
        {
            NoDocument = "310037",
            TrackingNo = "0502090614002610"
        };

        var (kind, value, error) = InstallmentExcelMatcher.ResolveLookup(row);

        Assert.Null(error);
        Assert.Equal(InstallmentLookupKind.NoDocument, kind);
        Assert.Equal("310037", value);
    }

    [Fact]
    public void ResolveLookup_uses_TrackingNo_when_NoDocument_empty()
    {
        var row = new InstallmentExcelRowInput
        {
            TrackingNo = "0502090614002610"
        };

        var (kind, value, error) = InstallmentExcelMatcher.ResolveLookup(row);

        Assert.Null(error);
        Assert.Equal(InstallmentLookupKind.TrackingNo, kind);
        Assert.Equal("0502090614002610", value);
    }

    [Fact]
    public void ValidateAgainstDb_returns_null_when_all_fields_match()
    {
        var excel = new InstallmentExcelRowInput
        {
            NoDocument = "809552",
            PaymentCost = "20335141229",
            PaymentDate = "1405/09/25"
        };
        var db = new InstallmentRowSnapshot
        {
            NoDocument = "809552",
            PaymentCost = 20335141229m,
            PaymentDate = "1405/09/25"
        };

        Assert.Null(InstallmentExcelMatcher.ValidateAgainstDb(excel, db));
    }

    [Fact]
    public void ValidateAgainstDb_detects_payment_cost_mismatch()
    {
        var excel = new InstallmentExcelRowInput
        {
            NoDocument = "809552",
            PaymentCost = "1",
            PaymentDate = "1405/09/25"
        };
        var db = new InstallmentRowSnapshot
        {
            NoDocument = "809552",
            PaymentCost = 20335141229m,
            PaymentDate = "1405/09/25"
        };

        var message = InstallmentExcelMatcher.ValidateAgainstDb(excel, db);
        Assert.Contains("PaymentCost", message);
    }

    [Fact]
    public void TrackingMatches_allows_empty_excel_tracking()
    {
        Assert.True(InstallmentExcelMatcher.TrackingMatches("", "0502090614002610"));
        Assert.True(InstallmentExcelMatcher.TrackingMatches(null, null));
    }
}
