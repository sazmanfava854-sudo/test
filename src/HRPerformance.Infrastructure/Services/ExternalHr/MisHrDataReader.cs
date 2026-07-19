using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public class MisHrDataReader
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MisHrDataReader> _logger;

    private const string SelectColumns = @"SELECT
    CAST(ID AS NUMERIC(20,0)) AS ID,
    CAST([Code] AS NVARCHAR(50)) AS [Code],
    [PerCod],
    [LastName],
    [Name],
    [NationalIDNo],
    [ProvinceCode],
    CAST(REPLACE([ShamsiDate], '/', '') AS INT) AS [ShamsiDate],
    CAST([StartTime] AS NVARCHAR(8)) AS [StartTime],
    CAST([EndTime] AS NVARCHAR(8)) AS [EndTime],
    [LeaveDurationMinutes],
    [StartDate],
    [EndDate],
    CAST(CAST([year] AS NVARCHAR(4)) AS INT) AS [year],
    CAST([Month] AS INT) AS [Month],
    [FirstTimeType]
FROM [MIS].[dbo].[HZG_View_HourlyLeave]";

    public MisHrDataReader(IConfiguration configuration, ILogger<MisHrDataReader> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MisHourlyLeaveRecord>> ReadHourlyLeavesAsync(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange range,
        CancellationToken ct = default)
    {
        var records = new List<MisHourlyLeaveRecord>();
        var customQuery = _configuration["HrIntegration:SqlQuery"];
        var query = string.IsNullOrWhiteSpace(customQuery) ? BuildQuery(settings, range) : customQuery;
        var filters = GetFilterSettings(settings);
        var connectionString = settings.MisConnectionString
            ?? throw new InvalidOperationException("اتصال MIS پیکربندی نشده است");

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SyncFrom", range.SyncFrom);
            command.Parameters.AddWithValue("@SyncTo", range.SyncToExclusive);
            AddFilterParameters(command, filters, range);
            command.CommandTimeout = 120;

            _logger.LogInformation(
                "MIS query filters: Range={Range}, SyncFrom={SyncFrom}, SyncTo={SyncTo}, ShamsiMonth={ShamsiYear}/{ShamsiMonth}, ProvinceFilter={ApplyProvince} ({ProvinceCode}), ShamsiFilter={ApplyShamsi} ({ShamsiPattern}), EmployeeLimit={EmployeeLimit}",
                range.Description,
                range.SyncFrom,
                range.SyncToExclusive,
                range.ShamsiYear,
                range.ShamsiMonth,
                filters.ApplyProvinceFilter,
                filters.ProvinceCode,
                filters.ApplyShamsiYearFilter,
                filters.ShamsiYearPattern,
                filters.EmployeeLimit);

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var perCod = ReadString(reader, "PerCod");
                if (string.IsNullOrWhiteSpace(perCod))
                    continue;

                records.Add(new MisHourlyLeaveRecord
                {
                    Id = reader.GetDecimal(reader.GetOrdinal("ID")),
                    Code = ReadString(reader, "Code"),
                    PerCod = perCod,
                    LastName = ReadString(reader, "LastName"),
                    Name = ReadString(reader, "Name"),
                    NationalIDNo = ReadString(reader, "NationalIDNo"),
                    ProvinceCode = ReadString(reader, "ProvinceCode"),
                    ShamsiDate = reader.IsDBNull(reader.GetOrdinal("ShamsiDate")) ? null : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("ShamsiDate"))),
                    StartTime = ReadString(reader, "StartTime"),
                    EndTime = ReadString(reader, "EndTime"),
                    LeaveDurationMinutes = reader.IsDBNull(reader.GetOrdinal("LeaveDurationMinutes")) ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("LeaveDurationMinutes"))),
                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                    Year = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("year"))),
                    Month = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("Month"))),
                    FirstTimeType = reader.IsDBNull(reader.GetOrdinal("FirstTimeType")) ? 0 : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("FirstTimeType")))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read MIS hourly leave view");
            throw;
        }

        _logger.LogInformation("MIS query returned {RecordCount} records ({DistinctEmployees} distinct PerCod)",
            records.Count,
            records.Select(r => r.PerCod).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        return records;
    }

    public async Task<MisHrDiagnosticResult> GetDiagnosticAsync(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange range,
        CancellationToken ct = default)
    {
        var filters = GetFilterSettings(settings);
        var result = new MisHrDiagnosticResult
        {
            SyncFrom = range.SyncFrom,
            SyncTo = range.SyncToExclusive,
            ShamsiYear = range.ShamsiYear,
            ShamsiMonth = range.ShamsiMonth,
            ProvinceCode = filters.ProvinceCode,
            ShamsiYearPrefix = filters.ShamsiYearPrefix,
            ApplyProvinceFilter = filters.ApplyProvinceFilter,
            ApplyShamsiYearFilter = filters.ApplyShamsiYearFilter,
            EmployeeLimit = filters.EmployeeLimit
        };

        var connectionString = settings.MisConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            result.CanConnect = false;
            result.ErrorMessage = "اتصال MIS در appsettings پیکربندی نشده است";
            return result;
        }

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);
            result.CanConnect = true;

            result.TotalInView = await CountAsync(connection, "SELECT COUNT(*) FROM [MIS].[dbo].[HZG_View_HourlyLeave]", ct);
            result.CountAfterSyncFrom = await CountAsync(connection,
                "SELECT COUNT(*) FROM [MIS].[dbo].[HZG_View_HourlyLeave] WHERE [StartDate] >= @SyncFrom AND [StartDate] < @SyncTo",
                ct, ("@SyncFrom", range.SyncFrom), ("@SyncTo", range.SyncToExclusive));

            if (filters.ApplyProvinceFilter)
            {
                result.CountAfterProvince = await CountAsync(connection,
                    "SELECT COUNT(*) FROM [MIS].[dbo].[HZG_View_HourlyLeave] WHERE [StartDate] >= @SyncFrom AND [StartDate] < @SyncTo AND CAST([ProvinceCode] AS NVARCHAR(20)) = @ProvinceCode",
                    ct, ("@SyncFrom", range.SyncFrom), ("@SyncTo", range.SyncToExclusive), ("@ProvinceCode", filters.ProvinceCode));
            }

            if (filters.ApplyShamsiYearFilter)
            {
                result.CountAfterShamsiYear = await CountAsync(connection, $@"SELECT COUNT(*) FROM [MIS].[dbo].[HZG_View_HourlyLeave]
WHERE [StartDate] >= @SyncFrom AND [StartDate] < @SyncTo
  AND (
        CAST([ShamsiDate] AS NVARCHAR(30)) LIKE @ShamsiYearPattern
        OR REPLACE(CAST([ShamsiDate] AS NVARCHAR(30)), '/', '') LIKE @ShamsiYearPattern
        OR CAST([year] AS NVARCHAR(4)) = @ShamsiYearPrefix
      )",
                    ct,
                    ("@SyncFrom", range.SyncFrom),
                    ("@SyncTo", range.SyncToExclusive),
                    ("@ShamsiYearPattern", filters.ShamsiYearPattern),
                    ("@ShamsiYearPrefix", filters.ShamsiYearPrefix));
            }

            var activeQuery = BuildQuery(settings, range).Replace(SelectColumns, "SELECT COUNT(*) AS Cnt, COUNT(DISTINCT [PerCod]) AS DistinctPerCod");
            await using var command = new SqlCommand(activeQuery, connection);
            command.Parameters.AddWithValue("@SyncFrom", range.SyncFrom);
            command.Parameters.AddWithValue("@SyncTo", range.SyncToExclusive);
            AddFilterParameters(command, filters, range);
            command.CommandTimeout = 120;

            await using var reader = await command.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                result.CountWithActiveFilters = reader.GetInt32(0);
                result.DistinctEmployeesWithActiveFilters = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
            }
        }
        catch (Exception ex)
        {
            result.CanConnect = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "MIS diagnostic query failed");
        }

        return result;
    }

    private static string BuildQuery(HrIntegrationRuntimeSettings settings, MisSyncRange? range = null)
    {
        var filters = GetFilterSettings(settings);
        var conditions = new List<string>
        {
            "[StartDate] >= @SyncFrom",
            "[StartDate] < @SyncTo"
        };

        if (range?.ShamsiFromYm is int fromYm && range.ShamsiToYm is int toYm)
        {
            conditions.Add("(CAST([year] AS INT) * 100 + CAST([Month] AS INT)) >= @ShamsiFromYm");
            conditions.Add("(CAST([year] AS INT) * 100 + CAST([Month] AS INT)) <= @ShamsiToYm");
        }

        if (range?.ShamsiYear is int shamsiYear && range.ShamsiMonth is int shamsiMonth)
        {
            conditions.Add("CAST([year] AS INT) = @ShamsiYear");
            conditions.Add("CAST([Month] AS INT) = @ShamsiMonth");
        }

        if (filters.ApplyProvinceFilter)
            conditions.Add("CAST([ProvinceCode] AS NVARCHAR(20)) = @ProvinceCode");

        if (filters.ApplyShamsiYearFilter)
        {
            conditions.Add(@"(
        CAST([ShamsiDate] AS NVARCHAR(30)) LIKE @ShamsiYearPattern
        OR REPLACE(CAST([ShamsiDate] AS NVARCHAR(30)), '/', '') LIKE @ShamsiYearPattern
        OR CAST([year] AS NVARCHAR(4)) = @ShamsiYearPrefix
      )");
        }

        if (filters.EmployeeLimit > 0)
        {
            var employeeSelectionConditions = string.Join("\n          AND ", conditions);
            conditions.Add($@"[PerCod] IN (
        SELECT TOP (@EmployeeLimit) [PerCod]
        FROM [MIS].[dbo].[HZG_View_HourlyLeave]
        WHERE {employeeSelectionConditions}
          AND [PerCod] IS NOT NULL
        GROUP BY [PerCod]
        ORDER BY [PerCod]
      )");
        }

        return $"{SelectColumns}\nWHERE {string.Join("\n  AND ", conditions)}\nORDER BY [StartDate] DESC";
    }

    private static MisFilterSettings GetFilterSettings(HrIntegrationRuntimeSettings settings)
    {
        var shamsiYearPrefix = settings.ShamsiYearPrefix;
        return new MisFilterSettings
        {
            ProvinceCode = settings.ProvinceCode,
            ShamsiYearPrefix = shamsiYearPrefix,
            ShamsiYearPattern = shamsiYearPrefix.TrimEnd('%') + "%",
            ApplyProvinceFilter = settings.ApplyProvinceFilter,
            ApplyShamsiYearFilter = settings.ApplyShamsiYearFilter,
            EmployeeLimit = settings.EmployeeLimit
        };
    }

    private static void AddFilterParameters(SqlCommand command, MisFilterSettings filters, MisSyncRange? range = null)
    {
        if (range?.ShamsiFromYm is int fromYm && range.ShamsiToYm is int toYm)
        {
            command.Parameters.Add("@ShamsiFromYm", SqlDbType.Int).Value = fromYm;
            command.Parameters.Add("@ShamsiToYm", SqlDbType.Int).Value = toYm;
        }

        if (range?.ShamsiYear is int shamsiYear && range.ShamsiMonth is int shamsiMonth)
        {
            command.Parameters.Add("@ShamsiYear", SqlDbType.Int).Value = shamsiYear;
            command.Parameters.Add("@ShamsiMonth", SqlDbType.Int).Value = shamsiMonth;
        }

        if (filters.ApplyProvinceFilter)
            command.Parameters.Add("@ProvinceCode", SqlDbType.NVarChar, 20).Value = filters.ProvinceCode;

        if (filters.ApplyShamsiYearFilter)
        {
            command.Parameters.Add("@ShamsiYearPattern", SqlDbType.NVarChar, 20).Value = filters.ShamsiYearPattern;
            command.Parameters.Add("@ShamsiYearPrefix", SqlDbType.NVarChar, 4).Value = filters.ShamsiYearPrefix;
        }

        if (filters.EmployeeLimit > 0)
            command.Parameters.Add("@EmployeeLimit", SqlDbType.Int).Value = filters.EmployeeLimit;
    }

    private static async Task<int> CountAsync(SqlConnection connection, string sql, CancellationToken ct, params (string Name, object Value)[] parameters)
    {
        await using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = 120;
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value);

        var result = await command.ExecuteScalarAsync(ct);
        return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static string? ReadString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return null;
        return Convert.ToString(reader.GetValue(ordinal))?.Trim();
    }

    private sealed class MisFilterSettings
    {
        public string ProvinceCode { get; init; } = "147";
        public string ShamsiYearPrefix { get; init; } = "1404";
        public string ShamsiYearPattern { get; init; } = "1404%";
        public bool ApplyProvinceFilter { get; init; } = true;
        public bool ApplyShamsiYearFilter { get; init; } = true;
        public int EmployeeLimit { get; init; }
    }
}
