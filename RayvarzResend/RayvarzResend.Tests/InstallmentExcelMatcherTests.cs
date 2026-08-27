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
    public void ResolveWillApplyEndState_uses_global_checkbox_only()
    {
        var rowWithOdooat = new InstallmentExcelRowInput { Odooat = "1" };
        var rowNoOdooat = new InstallmentExcelRowInput { Odooat = "0" };

        Assert.True(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.NoDocument, true, rowNoOdooat, excelMode: true));
        Assert.False(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.NoDocument, false, rowWithOdooat, excelMode: true));

        Assert.True(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.TrackingNo, true, rowNoOdooat, excelMode: false));
        Assert.False(InstallmentExcelMatcher.ResolveWillApplyEndState(
            InstallmentLookupKind.TrackingNo, false, rowWithOdooat, excelMode: false));
    }

    [Fact]
    public void DescribeOdooatPlan_reflects_checkbox_only()
    {
        Assert.Equal("بله", InstallmentExcelMatcher.DescribeOdooatPlan(InstallmentLookupKind.NoDocument, true));
        Assert.Equal("خیر", InstallmentExcelMatcher.DescribeOdooatPlan(InstallmentLookupKind.TrackingNo, false));
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
    public void ValidateAgainstDb_TrackingNo_accepts_missing_leading_zero_in_excel()
    {
        var excel = new InstallmentExcelRowInput
        {
            Identifier = "502090614002610",
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
            excel, db, InstallmentLookupKind.TrackingNo, "502090614002610"));
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

    [Theory]
    [InlineData("5.02091E+14", true)]
    [InlineData("0502090614002610", false)]
    public void LooksLikeScientificNotation_detects_excel_damage(string raw, bool expected)
    {
        Assert.Equal(expected, InstallmentExcelMatcher.LooksLikeScientificNotation(raw));
    }

    [Fact]
    public void RequiredColumnNames_has_three_persian_columns()
    {
        Assert.Equal(3, InstallmentExcelMatcher.RequiredColumnNames.Length);
        Assert.Equal("شناسه", InstallmentExcelMatcher.RequiredColumnNames[0]);
        Assert.Equal("مبلغ", InstallmentExcelMatcher.RequiredColumnNames[1]);
        Assert.Equal("تاریخ پرداخت", InstallmentExcelMatcher.RequiredColumnNames[2]);
    }

    [Fact]
    public void OptionalColumnNames_is_empty()
    {
        Assert.Empty(InstallmentExcelMatcher.OptionalColumnNames);
    }

    [Theory]
    [InlineData("1405/05/05", "14050505")]
    [InlineData("1405-5-5", "14050505")]
    public void NormalizeDateDigits_strips_separators(string raw, string expected)
    {
        Assert.Equal(expected, InstallmentExcelMatcher.NormalizeDateDigits(raw));
    }

    [Fact]
    public void ResolveLookup_rejects_scientific_identifier()
    {
        var row = new InstallmentExcelRowInput { Identifier = "5.02091E+14" };
        var (_, value, error) = InstallmentExcelMatcher.ResolveLookup(row);

        Assert.Contains("5.02E+14", error);
        Assert.Equal("", value);
    }
}
