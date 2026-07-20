using HRPerformance.Infrastructure.Services.ExternalHr;
using Xunit;

namespace HRPerformance.Infrastructure.Tests;

public class MisFirstHourLeaveHelperTests
{
    [Fact]
    public void ComputeDelayMinutes_0700_to_0731_returns_32()
    {
        var start = new TimeSpan(7, 0, 0);
        var delay = MisFirstHourLeaveHelper.ComputeDelayMinutes(start, 32, 0);
        Assert.Equal(32, delay);
    }

    [Fact]
    public void ComputeDelayMinutes_outside_window_returns_0()
    {
        var start = new TimeSpan(9, 0, 0);
        var delay = MisFirstHourLeaveHelper.ComputeDelayMinutes(start, 30, 0);
        Assert.Equal(0, delay);
    }

    [Fact]
    public void FirstTimeType_1_counts_as_first_hour_leave()
    {
        var delay = MisFirstHourLeaveHelper.ComputeDelayMinutes(null, 15, 1);
        Assert.Equal(15, delay);
    }

    [Fact]
    public void BuildLeaveTypeLabel_shows_delay_text()
    {
        var label = MisFirstHourLeaveHelper.BuildLeaveTypeLabel(1, 32);
        Assert.Contains("مرخصی اول وقت", label);
        Assert.Contains("32", label);
    }
}
