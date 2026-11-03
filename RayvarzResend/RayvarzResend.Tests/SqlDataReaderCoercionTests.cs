using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class SqlDataReaderCoercionTests
{
    [Theory]
    [InlineData(1270, 1270)]
    [InlineData("1278", 1278)]
    [InlineData(" 100116 ", 100116)]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("abc", 0)]
    public void ToInt32_handles_legacy_string_columns(object? value, int expected)
    {
        Assert.Equal(expected, SqlDataReaderCoercion.ToInt32(value));
    }

    [Theory]
    [InlineData(100.5, 100.5)]
    [InlineData("22374078", 22374078)]
    public void ToDecimal_handles_string_prices(object? value, decimal expected)
    {
        Assert.Equal(expected, SqlDataReaderCoercion.ToDecimal(value));
    }
}
