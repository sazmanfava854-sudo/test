using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class TahatorHelpersTests
{
    [Fact]
    public void CurrentShamsiSlashDate_has_yyyy_MM_dd_shape()
    {
        var s = DateHelper.CurrentShamsiSlashDate();
        Assert.Matches(@"^\d{4}/\d{2}/\d{2}$", s);
    }

    [Theory]
    [InlineData("14050323", "1405/03/23")]
    [InlineData("1405/03/23", "1405/03/23")]
    [InlineData("", "")]
    public void ToShamsiSlashDate_normalizes(string input, string expected)
    {
        Assert.Equal(expected, DateHelper.ToShamsiSlashDate(input));
    }
}
