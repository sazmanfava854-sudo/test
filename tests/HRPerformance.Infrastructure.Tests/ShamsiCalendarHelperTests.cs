using HRPerformance.Domain.Models;
using HRPerformance.Infrastructure.Services.ExternalHr;
using Xunit;

namespace HRPerformance.Infrastructure.Tests;

public class ShamsiCalendarHelperTests
{
    [Fact]
    public void ToDateKey_1404_04_10_Returns_14040410()
    {
        Assert.Equal(14040410, ShamsiCalendarHelper.ToDateKey(1404, 4, 10));
    }

    [Fact]
    public void ToGregorianDateOnly_1404_04_10_Returns_2025_07_01()
    {
        var date = ShamsiCalendarHelper.ToGregorianDateOnly(1404, 4, 10);
        Assert.Equal(new DateTime(2025, 7, 1), date.Date);
    }

    [Fact]
    public void MisSyncRequestMapper_Builds_Shami_Keys()
    {
        var request = new MisSyncDateRangeRequest(1404, 4, 10, 1404, 4, 11);
        var range = MisSyncRequestMapper.ToSyncRange(request);

        Assert.Equal(14040410, range.ShamsiFromKey);
        Assert.Equal(14040411, range.ShamsiToKey);
        Assert.Equal("1404/04/10", range.ShamsiFromText);
    }

    [Fact]
    public void MisQueryBuilder_Preview_Filters_ShamsiDate_Column()
    {
        var request = new MisSyncDateRangeRequest(1404, 4, 10, 1404, 4, 11);
        var range = MisSyncRequestMapper.ToSyncRange(request);
        var settings = new HrIntegrationRuntimeSettings
        {
            ProvinceCode = "147",
            ApplyProvinceFilter = true,
            ApplyShamsiYearFilter = false
        };

        var preview = MisQueryBuilder.BuildPreview(settings, range);

        Assert.Contains("[ShamsiDate]", preview.SqlWithLiteralValues);
        Assert.Contains("14040410", preview.SqlWithLiteralValues);
        Assert.Contains("14040411", preview.SqlWithLiteralValues);
        Assert.DoesNotContain("[StartDate] >=", preview.SqlWithLiteralValues);
    }
}
