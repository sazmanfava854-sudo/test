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

    [Theory]
    [InlineData("809552", InstallmentLookupKind.NoDocument, "809552")]
    [InlineData("0502090614002610", InstallmentLookupKind.TrackingNo, "0502090614002610")]
    public void ResolveLookup_detects_kind_by_digit_length(string identifier, InstallmentLookupKind kind, string value)
    {
        var row = new InstallmentExcelRowInput { Identifier = identifier };
        var (resolvedKind, resolvedValue, error) = InstallmentExcelMatcher.ResolveLookup(row);

        Assert.Null(error);
        Assert.Equal(kind, resolvedKind);
        Assert.Equal(value, resolvedValue);
    }

    [Fact]
    public void ResolveLookup_requires_identifier()
    {
        var row = new InstallmentExcelRowInput();
        var (_, value, error) = InstallmentExcelMatcher.ResolveLookup(row);

        Assert.Contains("شناسه", error);
        Assert.Equal("", value);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("بله", true)]
    [InlineData("عودت", true)]
    [InlineData("0", false)]
    [InlineData("خیر", false)]
    [InlineData("", null)]
    public void TryParseOdooatFlag_parses_excel_values(string raw, bool? expected)
    {
        Assert.Equal(expected, InstallmentExcelMatcher.TryParseOdooatFlag(raw));
    }

    [Fact]
    public void ResolveWillApplyEndState_no_document_always_true()
    {
        var row = new InstallmentExcelRowInput { Odooat = "0" };
        Assert.True(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.NoDocument, false, row));
    }

    [Fact]
    public void ResolveWillApplyEndState_tracking_uses_excel_column_over_checkbox()
    {
        var rowYes = new InstallmentExcelRowInput { Odooat = "1" };
        var rowNo = new InstallmentExcelRowInput { Odooat = "0" };
        var rowEmpty = new InstallmentExcelRowInput();

        Assert.True(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.TrackingNo, false, rowYes));
        Assert.False(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.TrackingNo, true, rowNo));
        Assert.True(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.TrackingNo, true, rowEmpty));
        Assert.False(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.TrackingNo, false, rowEmpty));
    }

    [Fact]
    public void ValidateAgainstDb_returns_null_when_all_fields_match_NoDocument()
    {
        var excel = new InstallmentExcelRowInput
        {
            Identifier = "809552",
            PaymentCost = "20335141229",
            PaymentDate = "1405/09/25"
        };
        var db = new InstallmentRowSnapshot
        {
            NoDocument = "809552",
            PaymentCost = 20335141229m,
            PaymentDate = "1405/09/25"
        };

        Assert.Null(InstallmentExcelMatcher.ValidateAgainstDb(
            excel, db, InstallmentLookupKind.NoDocument, "809552"));
    }

    [Fact]
    public void ValidateAgainstDb_TrackingNo_skips_cost_and_date()
    {
        var excel = new InstallmentExcelRowInput
        {
            Identifier = "0502090614002610",
            PaymentCost = "1",
            PaymentDate = "1400/01/01"
        };
        var db = new InstallmentRowSnapshot
        {
            TrackingNo = "0502090614002610",
            PaymentCost = 813516015m,
            PaymentDate = "1405/05/05"
        };

        Assert.Null(InstallmentExcelMatcher.ValidateAgainstDb(
            excel, db, InstallmentLookupKind.TrackingNo, "0502090614002610"));
    }

    [Fact]
    public void ValidateAgainstDb_detects_payment_cost_mismatch_for_NoDocument()
    {
        var excel = new InstallmentExcelRowInput
        {
            Identifier = "809552",
            PaymentCost = "1",
            PaymentDate = "1405/09/25"
        };
        var db = new InstallmentRowSnapshot
        {
            NoDocument = "809552",
            PaymentCost = 20335141229m,
            PaymentDate = "1405/09/25"
        };

        var message = InstallmentExcelMatcher.ValidateAgainstDb(
            excel, db, InstallmentLookupKind.NoDocument, "809552");
        Assert.Contains("PaymentCost", message);
    }

    [Fact]
    public void RequiredColumnNames_has_three_columns()
    {
        Assert.Equal(3, InstallmentExcelMatcher.RequiredColumnNames.Length);
        Assert.Equal("Identifier", InstallmentExcelMatcher.RequiredColumnNames[0]);
    }
}
