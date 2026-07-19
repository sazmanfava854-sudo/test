using System.Globalization;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public static class MisQueryBuilder
{
    private const string ViewName = "[MIS].[dbo].[HZG_View_HourlyLeave]";

    /// <summary>
    /// ShamsiDate مثل 1404/04/10 یا 1404/4/10 — با PARSENAME ماه/روز بدون صفر هم درست مقایسه می‌شود
    /// </summary>
    internal const string ShamsiDateIntExpr =
        "(CAST(PARSENAME(REPLACE([ShamsiDate], '/', '.'), 3) AS INT) * 10000 + " +
        "CAST(PARSENAME(REPLACE([ShamsiDate], '/', '.'), 2) AS INT) * 100 + " +
        "CAST(PARSENAME(REPLACE([ShamsiDate], '/', '.'), 1) AS INT))";

    public static string BuildSelectQuery(HrIntegrationRuntimeSettings settings, MisSyncRange? range = null)
    {
        var conditions = BuildConditions(settings, range);
        return $@"{MisHrDataReader.SelectColumnsSql}
FROM {ViewName}
WHERE {string.Join("\n  AND ", conditions)}
ORDER BY {ShamsiDateIntExpr} DESC";
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
ORDER BY {ShamsiDateIntExpr} DESC";

        var sqlLiteral = $@"{MisHrDataReader.SelectColumnsSql}
FROM {ViewName}
WHERE {string.Join("\n  AND ", conditions.Select(c => ReplaceParameters(c, parameters)))}
ORDER BY {ShamsiDateIntExpr} DESC";

        return new MisQueryPreview
        {
            SqlWithParameters = sqlWithParams,
            SqlWithLiteralValues = sqlLiteral,
            Parameters = parameters,
            ShamsiFromKey = range.ShamsiFromKey,
            ShamsiToKey = range.ShamsiToKey,
            ShamsiFromText = range.ShamsiFromText,
            ShamsiToText = range.ShamsiToText,
            Note =
                "فیلتر روی ستون ShamsiDate (شمسی، مثل 1404/04/10). " +
                "نیازی به تبدیل میلادی نیست."
        };
    }

    internal static IReadOnlyList<string> BuildConditions(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange? range = null)
    {
        var filters = GetFilterSettings(settings);
        var conditions = new List<string>();

        if (range is { ShamsiFromKey: > 0, ShamsiToKey: > 0 })
        {
            conditions.Add($"{ShamsiDateIntExpr} >= @ShamsiFromKey");
            conditions.Add($"{ShamsiDateIntExpr} <= @ShamsiToKey");
        }

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
            ["@ShamsiFromKey"] = range.ShamsiFromKey,
            ["@ShamsiToKey"] = range.ShamsiToKey
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
    public int ShamsiFromKey { get; init; }
    public int ShamsiToKey { get; init; }
    public string ShamsiFromText { get; init; } = string.Empty;
    public string ShamsiToText { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
}
