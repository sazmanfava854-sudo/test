using HRPerformance.Domain.Models;
using HRPerformance.Infrastructure.Services.ExternalHr;
using Xunit;

namespace HRPerformance.Infrastructure.Tests;

public class ShamsiCalendarHelperTests
{
    [Fact]
    public void ToGregorianDate_1404_04_10_Returns_2025_07_01()
    {
        var date = ShamsiCalendarHelper.ToGregorianDate(1404, 4, 10);
        Assert.Equal(new DateTime(2025, 7, 1), date.Date);
    }

    [Fact]
    public void ToGregorianDate_1404_07_12_Returns_2025_10_04()
    {
        var date = ShamsiCalendarHelper.ToGregorianDate(1404, 7, 12);
        Assert.Equal(new DateTime(2025, 10, 4), date.Date);
    }

    [Fact]
    public void MisSyncRequestMapper_Builds_Gregorian_Range_For_Sample_Input()
    {
        var request = new MisSyncDateRangeRequest(1404, 4, 10, 1404, 4, 11);
        var range = MisSyncRequestMapper.ToSyncRange(request);

        Assert.Equal(new DateTime(2025, 7, 1), range.SyncFrom);
        Assert.Equal(new DateTime(2025, 7, 3), range.SyncToExclusive);
    }

    [Fact]
    public void MisQueryBuilder_Preview_Uses_Gregorian_Dates_In_Sql()
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

        Assert.Contains("2025-07-01", preview.SqlWithLiteralValues);
        Assert.Contains("2025-07-03", preview.SqlWithLiteralValues);
        Assert.DoesNotContain("1404-04-10", preview.SqlWithLiteralValues);
    }
}
