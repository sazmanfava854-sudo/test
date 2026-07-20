using HRPerformance.Infrastructure.Services.ExternalHr;
using Xunit;

namespace HRPerformance.Infrastructure.Tests;

public class MisRosterQueryTests
{
    [Fact]
    public void BuildRosterDistinctEmployeesQuery_Uses_ProvinceCode_Only()
    {
        var settings = new HrIntegrationRuntimeSettings
        {
            ProvinceCode = "147",
            ApplyProvinceFilter = true
        };

        var sql = MisQueryBuilder.BuildRosterDistinctEmployeesQuery(settings);

        Assert.Contains("CAST([ProvinceCode] AS NVARCHAR(20)) = @ProvinceCode", sql);
        Assert.DoesNotContain("@ShamsiFromKey", sql);
        Assert.DoesNotContain("@ShamsiToKey", sql);
        Assert.Contains("PARTITION BY [PerCod]", sql);
    }
}
