using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class DateHelperTests
{
    [Theory]
    [InlineData("۱۴۰۴/۰۵/۰۵", "14040505")]
    [InlineData("۱۴۰۴۰۵۰۵", "14040505")]
    public void ToRayvarzDate_parses_persian_digits(string input, string expected)
    {
        Assert.Equal(expected, DateHelper.ToRayvarzDate(input));
    }

    [Theory]
    [InlineData("1404/05/05", "14040505")]
    [InlineData("1404/5/5", "14040505")]
    [InlineData("1404-05-05", "14040505")]
    [InlineData("14040505", "14040505")]
    [InlineData("14050323", "14050323")]
    [InlineData("1396/08/26", "13960826")]
    public void ToRayvarzDate_parses_valid_shamsi_formats(string input, string expected)
    {
        Assert.Equal(expected, DateHelper.ToRayvarzDate(input));
        Assert.Equal(int.Parse(expected[..4]), DateHelper.ExtractShamsiYear(input));
    }

    [Theory]
    [InlineData("4050505", "14050505")]
    public void ToRayvarzDate_repairs_missing_leading_one_on_7_digit_year(string input, string expected)
    {
        Assert.Equal(expected, DateHelper.ToRayvarzDate(input));
    }

    [Theory]
    [InlineData("04050505")]
    [InlineData("5050505")]
    [InlineData("123")]
    [InlineData("99999999")]
    public void ToRayvarzDate_rejects_invalid_or_ambiguous_short_dates(string input)
    {
        Assert.Equal("", DateHelper.ToRayvarzDate(input));
        Assert.Equal(0, DateHelper.ExtractShamsiYear(input));
    }

    [Fact]
    public void ToRayvarzDate_does_not_pad_left_to_wrong_century()
    {
        // رفتار قدیمی PadLeft(8): 4050505 → 04050505 (سال ۴۰۵ — غلط)
        Assert.NotEqual("04050505", DateHelper.ToRayvarzDate("4050505"));
        Assert.Equal("14050505", DateHelper.ToRayvarzDate("4050505"));
    }

    [Fact]
    public void FromDatabaseDateValue_keeps_shamsi_datetime()
    {
        var dt = new DateTime(1404, 5, 5);
        Assert.Equal("14040505", DateHelper.FromDatabaseDateValue(dt));
    }

    [Theory]
    [InlineData("1404/05/05", 1404, 5, 5)]
    [InlineData("14040505", 1404, 5, 5)]
    public void ToSqlDateTimeFromRayvarz_parses_shamsi_components(string input, int y, int m, int d)
    {
        var dt = DateHelper.ToSqlDateTimeFromRayvarz(input);
        Assert.NotNull(dt);
        Assert.Equal(y, dt!.Value.Year);
        Assert.Equal(m, dt.Value.Month);
        Assert.Equal(d, dt.Value.Day);
        Assert.Equal(dt.Value.AddDays(1), DateHelper.ToSqlDateTimeEndExclusiveFromRayvarz(input));
    }
}
