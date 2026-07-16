using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRPerformance.Services.ExternalHr;

public class MisHrDataReader
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MisHrDataReader> _logger;

    private const string DefaultQuery = @"SELECT
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
FROM [MIS].[dbo].[HZG_View_HourlyLeave]
WHERE [StartDate] >= @SyncFrom
  AND CAST([ProvinceCode] AS NVARCHAR(20)) = @ProvinceCode
  AND (
        CAST([ShamsiDate] AS NVARCHAR(30)) LIKE @ShamsiYearPattern
        OR REPLACE(CAST([ShamsiDate] AS NVARCHAR(30)), '/', '') LIKE @ShamsiYearPattern
      )
ORDER BY [StartDate] DESC";

    public MisHrDataReader(IConfiguration configuration, ILogger<MisHrDataReader> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GetConnectionString()
    {
        var section = _configuration.GetSection("HrIntegration");
        var connectionString = section["ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        var server = section["Server"] ?? throw new InvalidOperationException("HrIntegration:Server is not configured");
        var database = section["Database"] ?? "MIS";
        var userId = section["UserId"] ?? throw new InvalidOperationException("HrIntegration:UserId is not configured");
        var password = section["Password"] ?? throw new InvalidOperationException("HrIntegration:Password is not configured");

        return $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true";
    }

    public async Task<IReadOnlyList<MisHourlyLeaveRecord>> ReadHourlyLeavesAsync(DateTime syncFrom, CancellationToken ct = default)
    {
        var records = new List<MisHourlyLeaveRecord>();
        var query = _configuration["HrIntegration:SqlQuery"] ?? DefaultQuery;

        try
        {
            await using var connection = new SqlConnection(GetConnectionString());
            await connection.OpenAsync(ct);
            await using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@SyncFrom", syncFrom);

            var provinceCode = _configuration["HrIntegration:ProvinceCode"] ?? "147";
            var shamsiYearPrefix = _configuration["HrIntegration:ShamsiYearPrefix"] ?? "1405";
            var shamsiYearPattern = shamsiYearPrefix.TrimEnd('%') + "%";

            command.Parameters.Add("@ProvinceCode", SqlDbType.NVarChar, 20).Value = provinceCode;
            command.Parameters.Add("@ShamsiYearPattern", SqlDbType.NVarChar, 20).Value = shamsiYearPattern;
            command.CommandTimeout = 120;

            _logger.LogInformation(
                "MIS query filters: ProvinceCode={ProvinceCode}, ShamsiDate LIKE {ShamsiPattern}, SyncFrom={SyncFrom}",
                provinceCode, shamsiYearPattern, syncFrom);

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                records.Add(new MisHourlyLeaveRecord
                {
                    Id = reader.GetDecimal(reader.GetOrdinal("ID")),
                    Code = reader.IsDBNull(reader.GetOrdinal("Code")) ? null : reader.GetString(reader.GetOrdinal("Code")),
                    PerCod = reader.GetString(reader.GetOrdinal("PerCod")).Trim(),
                    LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? null : reader.GetString(reader.GetOrdinal("LastName")).Trim(),
                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetString(reader.GetOrdinal("Name")).Trim(),
                    NationalIDNo = reader.IsDBNull(reader.GetOrdinal("NationalIDNo")) ? null : reader.GetString(reader.GetOrdinal("NationalIDNo")).Trim(),
                    ProvinceCode = reader.IsDBNull(reader.GetOrdinal("ProvinceCode")) ? null : Convert.ToString(reader.GetValue(reader.GetOrdinal("ProvinceCode")))?.Trim(),
                    ShamsiDate = reader.IsDBNull(reader.GetOrdinal("ShamsiDate")) ? null : Convert.ToInt32(reader.GetValue(reader.GetOrdinal("ShamsiDate"))),
                    StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? null : reader.GetString(reader.GetOrdinal("StartTime")).Trim(),
                    EndTime = reader.IsDBNull(reader.GetOrdinal("EndTime")) ? null : reader.GetString(reader.GetOrdinal("EndTime")).Trim(),
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

        return records;
    }
}
