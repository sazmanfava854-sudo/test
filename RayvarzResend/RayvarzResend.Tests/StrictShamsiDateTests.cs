using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class StrictShamsiDateTests
{
    [Theory]
    [InlineData("1405/08/07", "1405/08/07")]
    [InlineData(" 1405/08/07 ", "1405/08/07")]
    [InlineData("۱۴۰۵/۰۸/۰۷", "1405/08/07")]
    [InlineData("1405/06/31", "1405/06/31")]
    [InlineData("1405/12/30", "1405/12/30")]
    public void Accepts_yyyy_slash_mm_slash_dd(string input, string expected)
    {
        Assert.True(DateHelper.TryParseStrictShamsiSlash(input, out var slash));
        Assert.Equal(expected, slash);
    }

    [Theory]
    [InlineData("051205/20601482")]
    [InlineData("14050807")]
    [InlineData("1405-08-07")]
    [InlineData("1405/8/7")]
    [InlineData("1405/08/07/01")]
    [InlineData("1405/13/01")]
    [InlineData("1405/07/31")]
    [InlineData("1405/00/10")]
    [InlineData("0512/05/20")]
    [InlineData("abcd/ef/gh")]
    [InlineData("")]
    [InlineData(null)]
    public void Rejects_anything_else(string? input)
    {
        Assert.False(DateHelper.TryParseStrictShamsiSlash(input, out var slash));
        Assert.Equal("", slash);
    }

    [Fact]
    public void ValidateOptionalUserDate_names_field_and_value()
    {
        Assert.Null(DateHelper.ValidateOptionalUserDate(null, "از تاریخ"));
        Assert.Null(DateHelper.ValidateOptionalUserDate("  ", "از تاریخ"));
        Assert.Null(DateHelper.ValidateOptionalUserDate("1405/08/07", "از تاریخ"));

        var error = DateHelper.ValidateOptionalUserDate("051205/20601482", "از تاریخ");
        Assert.NotNull(error);
        Assert.Contains("از تاریخ", error);
        Assert.Contains("051205/20601482", error);
        Assert.Contains("1405/08/07", error);
    }

    [Fact]
    public void Persian_digit_dates_reach_sql_with_latin_digits()
    {
        Assert.Equal("1405/08/07", DateHelper.NormalizeOptionalUserDate("۱۴۰۵/۰۸/۰۷"));
        Assert.Null(DateHelper.NormalizeOptionalUserDate(null));
        Assert.Equal("1405/08/07", DateHelper.ToShamsiSlashDate("۱۴۰۵/۰۸/۰۷"));
    }

    [Fact]
    public void Bank_inquiry_and_check_date_helpers_use_strict_format()
    {
        Assert.True(BankInquiryConfirmHelper.TryNormalizeSlashDate("1405/08/07", out var ok));
        Assert.Equal("1405/08/07", ok);
        Assert.False(BankInquiryConfirmHelper.TryNormalizeSlashDate("051205/20601482", out _));
        Assert.False(BankInquiryConfirmHelper.TryNormalizeSlashDate("14050807", out _));
    }
}
