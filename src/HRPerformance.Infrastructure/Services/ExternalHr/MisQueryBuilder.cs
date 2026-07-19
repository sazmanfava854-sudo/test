using System.Globalization;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public static class MisQueryBuilder
{
    private const string ViewName = "[MIS].[dbo].[HZG_View_HourlyLeave]";

    public static string BuildSelectQuery(HrIntegrationRuntimeSettings settings, MisSyncRange? range = null)
    {
        var conditions = BuildConditions(settings, range);
        return $@"{MisHrDataReader.SelectColumnsSql}
FROM {ViewName}
WHERE {string.Join("\n  AND ", conditions)}
ORDER BY [StartDate] DESC";
    }

    public static MisQueryPreview BuildPreview(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange range)
    {
        var conditions = BuildConditions(settings, range);
        var parameters = BuildParameterValues(settings, range);

        var sqlWithParams = $@"{MisHrDataReader.SelectColumnsSql}
FROM {ViewName}
WHERE {string.Join("\n  AND ", conditions)}
ORDER BY [StartDate] DESC";

        var sqlLiteral = $@"{MisHrDataReader.SelectColumnsSql}
FROM {ViewName}
WHERE {string.Join("\n  AND ", conditions.Select(c => ReplaceParameters(c, parameters)))}
ORDER BY [StartDate] DESC";

        return new MisQueryPreview
        {
            SqlWithParameters = sqlWithParams,
            SqlWithLiteralValues = sqlLiteral,
            Parameters = parameters,
            GregorianFrom = range.SyncFrom,
            GregorianToInclusive = range.SyncToExclusive.AddDays(-1),
            Note =
                "StartDate/EndDate در MIS از نوع datetime هستند. " +
                "بازه میلادی با >= ابتدای روز و < ابتدای روز بعد جستجو می‌شود تا ساعت هم پوشش داده شود."
        };
    }

    internal static IReadOnlyList<string> BuildConditions(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange? range = null)
    {
        var filters = GetFilterSettings(settings);
        var conditions = new List<string>
        {
            "[StartDate] >= @SyncFrom",
            "[StartDate] < @SyncTo"
        };

        if (range?.ShamsiYear is int shamsiYear && range.ShamsiMonth is int shamsiMonth)
        {
            conditions.Add("CAST([year] AS INT) = @ShamsiYear");
            conditions.Add("CAST([Month] AS INT) = @ShamsiMonth");
        }

        if (filters.ApplyProvinceFilter)
            conditions.Add("CAST([ProvinceCode] AS NVARCHAR(20)) = @ProvinceCode");

        if (filters.ApplyShamsiYearFilter)
            conditions.Add("CAST([year] AS NVARCHAR(4)) = @ShamsiYearPrefix");

        if (filters.EmployeeLimit > 0)
        {
            var employeeSelectionConditions = string.Join("\n          AND ", conditions);
            conditions.Add($@"[PerCod] IN (
        SELECT TOP (@EmployeeLimit) [PerCod]
        FROM {ViewName}
        WHERE {employeeSelectionConditions}
          AND [PerCod] IS NOT NULL
        GROUP BY [PerCod]
        ORDER BY [PerCod]
      )");
        }

        return conditions;
    }

    internal static Dictionary<string, object> BuildParameterValues(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange range)
    {
        var filters = GetFilterSettings(settings);
        var parameters = new Dictionary<string, object>
        {
            ["@SyncFrom"] = range.SyncFrom,
            ["@SyncTo"] = range.SyncToExclusive
        };

        if (range.ShamsiYear is int shamsiYear && range.ShamsiMonth is int shamsiMonth)
        {
            parameters["@ShamsiYear"] = shamsiYear;
            parameters["@ShamsiMonth"] = shamsiMonth;
        }

        if (filters.ApplyProvinceFilter)
            parameters["@ProvinceCode"] = filters.ProvinceCode;

        if (filters.ApplyShamsiYearFilter)
            parameters["@ShamsiYearPrefix"] = filters.ShamsiYearPrefix;

        if (filters.EmployeeLimit > 0)
            parameters["@EmployeeLimit"] = filters.EmployeeLimit;

        return parameters;
    }

    private static string ReplaceParameters(string condition, Dictionary<string, object> parameters)
    {
        var result = condition;
        foreach (var (name, value) in parameters)
            result = result.Replace(name, FormatSqlLiteral(value));

        return result;
    }

    private static string FormatSqlLiteral(object value) => value switch
    {
        DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
        string s => $"'{s.Replace("'", "''")}'",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "NULL"
    };

    private static MisFilterSettings GetFilterSettings(HrIntegrationRuntimeSettings settings)
    {
        var shamsiYearPrefix = settings.ShamsiYearPrefix;
        return new MisFilterSettings
        {
            ProvinceCode = settings.ProvinceCode,
            ShamsiYearPrefix = shamsiYearPrefix,
            ApplyProvinceFilter = settings.ApplyProvinceFilter,
            ApplyShamsiYearFilter = settings.ApplyShamsiYearFilter,
            EmployeeLimit = settings.EmployeeLimit
        };
    }

    private sealed class MisFilterSettings
    {
        public string ProvinceCode { get; init; } = "147";
        public string ShamsiYearPrefix { get; init; } = "1404";
        public bool ApplyProvinceFilter { get; init; } = true;
        public bool ApplyShamsiYearFilter { get; init; } = true;
        public int EmployeeLimit { get; init; }
    }
}

public class MisQueryPreview
{
    public string SqlWithParameters { get; init; } = string.Empty;
    public string SqlWithLiteralValues { get; init; } = string.Empty;
    public Dictionary<string, object> Parameters { get; init; } = new();
    public DateTime GregorianFrom { get; init; }
    public DateTime GregorianToInclusive { get; init; }
    public string Note { get; init; } = string.Empty;
}
