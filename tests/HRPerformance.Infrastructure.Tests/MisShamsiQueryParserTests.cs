using HRPerformance.Infrastructure.Services.ExternalHr;
using Xunit;

namespace HRPerformance.Infrastructure.Tests;

public class MisShamsiQueryParserTests
{
    [Fact]
    public void TryParseRange_CompactDates_Works()
    {
        var ok = MisShamsiQueryParser.TryParseRange(
            "1404/04/10", "1404/04/11",
            null, null, null,
            null, null, null,
            out var request, out var error);

        Assert.True(ok);
        Assert.Empty(error);
        Assert.Equal(1404, request.ShamsiFromYear);
        Assert.Equal(11, request.ShamsiToDay);
    }

    [Fact]
    public void TryParseRange_InvalidDay_Returns_Persian_Error()
    {
        var ok = MisShamsiQueryParser.TryParseRange(
            null, null,
            "1404", "4", "10",
            "1404", "4", "11 باید greg",
            out _, out var error);

        Assert.False(ok);
        Assert.Contains("shamsiToDay", error);
        Assert.Contains("11 باید greg", error);
    }
}
