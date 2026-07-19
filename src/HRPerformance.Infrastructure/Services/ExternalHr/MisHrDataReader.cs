using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Infrastructure.Services.ExternalHr;

public class MisHrDataReader
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MisHrDataReader> _logger;

    internal const string SelectColumnsSql = @"SELECT
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
    [FirstTimeType]";

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
        var query = string.IsNullOrWhiteSpace(customQuery) ? MisQueryBuilder.BuildSelectQuery(settings, range) : customQuery;
        var filters = GetFilterSettings(settings);
        var connectionString = settings.MisConnectionString
            ?? throw new InvalidOperationException("اتصال MIS پیکربندی نشده است");

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ShamsiFromKey", range.ShamsiFromKey);
            command.Parameters.AddWithValue("@ShamsiToKey", range.ShamsiToKey);
            AddFilterParameters(command, filters, range);
            command.CommandTimeout = 120;

            _logger.LogInformation(
                "MIS query filters: Range={Range}, ShamsiFromKey={ShamsiFromKey}, ShamsiToKey={ShamsiToKey}, ProvinceFilter={ApplyProvince} ({ProvinceCode}), ShamsiFilter={ApplyShamsi} ({ShamsiPattern}), EmployeeLimit={EmployeeLimit}",
                range.Description,
                range.ShamsiFromKey,
                range.ShamsiToKey,
                filters.ApplyProvinceFilter,
                filters.ProvinceCode,
                filters.ApplyShamsiYearFilter,
                filters.ShamsiYearPrefix,
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

    public async Task<IReadOnlyList<MisHourlyLeaveRecord>> ReadDistinctEmployeesAsync(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange range,
        CancellationToken ct = default)
    {
        var records = new List<MisHourlyLeaveRecord>();
        var query = MisQueryBuilder.BuildDistinctEmployeesQuery(settings, range);
        var filters = GetFilterSettings(settings);
        var connectionString = settings.MisConnectionString
            ?? throw new InvalidOperationException("اتصال MIS پیکربندی نشده است");

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@ShamsiFromKey", range.ShamsiFromKey);
            command.Parameters.AddWithValue("@ShamsiToKey", range.ShamsiToKey);
            AddFilterParameters(command, filters, range);
            command.CommandTimeout = 120;

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var perCod = ReadString(reader, "PerCod");
                if (string.IsNullOrWhiteSpace(perCod))
                    continue;

                records.Add(new MisHourlyLeaveRecord
                {
                    PerCod = perCod,
                    LastName = ReadString(reader, "LastName"),
                    Name = ReadString(reader, "Name"),
                    NationalIDNo = ReadString(reader, "NationalIDNo"),
                    ProvinceCode = ReadString(reader, "ProvinceCode"),
                    StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("StartDate"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read distinct MIS employees");
            throw;
        }

        _logger.LogInformation("MIS distinct employee query returned {EmployeeCount} PerCod", records.Count);
        return records;
    }

    public MisQueryPreview BuildQueryPreview(HrIntegrationRuntimeSettings settings, MisSyncRange range) =>
        MisQueryBuilder.BuildPreview(settings, range);

    public async Task<(bool CanConnect, int RowCount, string? Error)> CountRowsAsync(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange range,
        CancellationToken ct = default)
    {
        var connectionString = settings.MisConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
            return (false, 0, "اتصال MIS در appsettings پیکربندی نشده است");

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(ct);
            var sql = $@"{MisQueryBuilder.BuildFilteredCte(settings, range)}
SELECT COUNT(*)
FROM {MisQueryBuilder.FilteredCteName}";
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ShamsiFromKey", range.ShamsiFromKey);
            command.Parameters.AddWithValue("@ShamsiToKey", range.ShamsiToKey);
            AddFilterParameters(command, GetFilterSettings(settings), range);
            command.CommandTimeout = 30;
            var scalar = await command.ExecuteScalarAsync(ct);
            return (true, scalar == null || scalar == DBNull.Value ? 0 : Convert.ToInt32(scalar), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MIS count query failed");
            return (false, 0, ex.GetBaseException().Message);
        }
    }

    public async Task<MisHrDiagnosticResult> GetDiagnosticAsync(
        HrIntegrationRuntimeSettings settings,
        MisSyncRange range,
        CancellationToken ct = default)
    {
        var filters = GetFilterSettings(settings);
        var result = new MisHrDiagnosticResult
        {
            ShamsiFromKey = range.ShamsiFromKey,
            ShamsiToKey = range.ShamsiToKey,
            ShamsiFromText = range.ShamsiFromText,
            ShamsiToText = range.ShamsiToText,
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
                $"SELECT COUNT(*) FROM [MIS].[dbo].[HZG_View_HourlyLeave] WHERE {MisQueryBuilder.ShamsiDateIntExpr} >= @ShamsiFromKey AND {MisQueryBuilder.ShamsiDateIntExpr} <= @ShamsiToKey",
                ct, ("@ShamsiFromKey", range.ShamsiFromKey), ("@ShamsiToKey", range.ShamsiToKey));

            if (filters.ApplyProvinceFilter)
            {
                result.CountAfterProvince = await CountAsync(connection,
                    $"SELECT COUNT(*) FROM [MIS].[dbo].[HZG_View_HourlyLeave] WHERE {MisQueryBuilder.ShamsiDateIntExpr} >= @ShamsiFromKey AND {MisQueryBuilder.ShamsiDateIntExpr} <= @ShamsiToKey AND CAST([ProvinceCode] AS NVARCHAR(20)) = @ProvinceCode",
                    ct, ("@ShamsiFromKey", range.ShamsiFromKey), ("@ShamsiToKey", range.ShamsiToKey), ("@ProvinceCode", filters.ProvinceCode));
            }

            if (filters.ApplyShamsiYearFilter)
            {
                result.CountAfterShamsiYear = await CountAsync(connection, $@"SELECT COUNT(*) FROM [MIS].[dbo].[HZG_View_HourlyLeave]
WHERE {MisQueryBuilder.ShamsiDateIntExpr} >= @ShamsiFromKey AND {MisQueryBuilder.ShamsiDateIntExpr} <= @ShamsiToKey
  AND CAST([year] AS NVARCHAR(4)) = @ShamsiYearPrefix",
                    ct,
                    ("@ShamsiFromKey", range.ShamsiFromKey),
                    ("@ShamsiToKey", range.ShamsiToKey),
                    ("@ShamsiYearPrefix", filters.ShamsiYearPrefix));
            }

            var activeQuery = MisQueryBuilder.BuildSelectQuery(settings, range)
                .Replace(
                    $"{MisHrDataReader.SelectColumnsSql}\nFROM {MisQueryBuilder.FilteredCteName}",
                    $"SELECT COUNT(*) AS Cnt, COUNT(DISTINCT [PerCod]) AS DistinctPerCod\nFROM {MisQueryBuilder.FilteredCteName}");
            await using var command = new SqlCommand(activeQuery, connection);
            command.Parameters.AddWithValue("@ShamsiFromKey", range.ShamsiFromKey);
            command.Parameters.AddWithValue("@ShamsiToKey", range.ShamsiToKey);
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

    private static void AddFilterParameters(SqlCommand command, MisFilterSettings filters, MisSyncRange? range = null)
    {
        if (range?.ShamsiYear is int shamsiYear && range.ShamsiMonth is int shamsiMonth)
        {
            command.Parameters.Add("@ShamsiYear", SqlDbType.Int).Value = shamsiYear;
            command.Parameters.Add("@ShamsiMonth", SqlDbType.Int).Value = shamsiMonth;
        }

        if (filters.ApplyProvinceFilter)
            command.Parameters.Add("@ProvinceCode", SqlDbType.NVarChar, 20).Value = filters.ProvinceCode;

        if (filters.ApplyShamsiYearFilter)
            command.Parameters.Add("@ShamsiYearPrefix", SqlDbType.NVarChar, 4).Value = filters.ShamsiYearPrefix;

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
        public bool ApplyProvinceFilter { get; init; } = true;
        public bool ApplyShamsiYearFilter { get; init; } = true;
        public int EmployeeLimit { get; init; }
    }
}
