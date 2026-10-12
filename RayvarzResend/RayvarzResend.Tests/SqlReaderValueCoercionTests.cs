using RayvarzResend.Web.RuleEngine;
using Xunit;

namespace RayvarzResend.Tests;

public class SqlReaderValueCoercionTests
{
    [Fact]
    public void ToNullableDateTime_ParsesString()
    {
        var dt = SqlReaderValueCoercion.ToNullableDateTime("2024-03-15");
        Assert.NotNull(dt);
        Assert.Equal(2024, dt.Value.Year);
        Assert.Equal(3, dt.Value.Month);
        Assert.Equal(15, dt.Value.Day);
    }

    [Fact]
    public void CombineDateAndTime_MergesParts()
    {
        var date = new DateTime(2024, 3, 15, 10, 0, 0);
        var merged = SqlReaderValueCoercion.CombineDateAndTime(date, new TimeSpan(14, 30, 0));
        Assert.Equal(new DateTime(2024, 3, 15, 14, 30, 0), merged);
    }

    [Fact]
    public void ToNullableTimeOfDay_ParsesString()
    {
        var t = SqlReaderValueCoercion.ToNullableTimeOfDay("14:30:00");
        Assert.Equal(new TimeSpan(14, 30, 0), t);
    }
}
