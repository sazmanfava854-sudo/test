using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class ShahkarService
{
    private readonly ShahkarOptions _options;
    private readonly IConfiguration _config;
    private readonly ILogger<ShahkarService> _logger;

    public ShahkarService(IOptions<ShahkarOptions> options, IConfiguration config, ILogger<ShahkarService> logger)
    {
        _options = options.Value;
        _config = config;
        _logger = logger;
    }

    public bool IsDryRun =>
        _config.GetValue<bool?>("Shahkar:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ResolveConnectionString());

    public async Task<ShahkarSearchResult> SearchAsync(ShahkarSearchRequest req, CancellationToken ct = default)
    {
        var result = new ShahkarSearchResult();
        var validationError = ShahkarSearchHelper.ValidateSearchRequest(req);
        if (validationError != null)
        {
            result.Error = validationError;
            return result;
        }

        var cs = ResolveConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
        {
            result.Error = "ConnectionStrings:Security (یا نام تنظیم Shahkar:ConnectionStringName) پیکربندی نشده است";
            return result;
        }

        var (whereSql, parameters) = ShahkarSearchHelper.BuildSearchWhere(req);
        var table = ShahkarSearchHelper.QualifyTable(_options);
        var top = _options.MaxSearchRows > 0 ? _options.MaxSearchRows : 200;

        var sql = $"""
            SELECT TOP ({top})
                UserName,
                NationalCode,
                CAST(ShahkarOk AS nvarchar(20)) AS ShahkarOk
            FROM {table}
            WHERE {whereSql}
            ORDER BY UserName
            """;

        try
        {
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            foreach (var (key, value) in parameters)
                cmd.Parameters.AddWithValue("@" + key, value);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Items.Add(new ShahkarUserRowDto
                {
                    UserName = reader.GetString(0).Trim(),
                    NationalCode = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                    ShahkarOk = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim()
                });
            }

            result.Total = result.Items.Count;
            result.Success = true;
            return result;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Shahkar search failed");
            result.Error = ex.Message;
            return result;
        }
    }

    public async Task<ShahkarConfirmResult> ConfirmAsync(ShahkarConfirmRequest req, CancellationToken ct = default)
    {
        var result = new ShahkarConfirmResult { DryRun = IsDryRun };
        var names = (req?.UserNames ?? [])
            .Select(x => (x ?? "").Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (names.Count == 0)
        {
            result.Error = "حداقل یک کاربر انتخاب کنید";
            return result;
        }

        var cs = ResolveConnectionString();
        if (string.IsNullOrWhiteSpace(cs))
        {
            result.Error = "ConnectionStrings:Security پیکربندی نشده است";
            return result;
        }

        var table = ShahkarSearchHelper.QualifyTable(_options);
        var okValue = string.IsNullOrWhiteSpace(_options.ShahkarOkValue) ? "1" : _options.ShahkarOkValue.Trim();

        try
        {
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync(ct);

            foreach (var userName in names)
            {
                var row = new ShahkarConfirmRowResult { UserName = userName };
                if (IsDryRun)
                {
                    row.Success = true;
                    row.Message = "DryRun — به‌روزرسانی انجام نشد";
                    result.Results.Add(row);
                    continue;
                }

                var sql = $"""
                    UPDATE {table}
                    SET ShahkarOk = @ok
                    WHERE UserName = @userName
                    """;
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ok", okValue);
                cmd.Parameters.AddWithValue("@userName", userName);
                var affected = await cmd.ExecuteNonQueryAsync(ct);
                if (affected == 0)
                {
                    row.Success = false;
                    row.Message = "کاربر یافت نشد یا تغییری اعمال نشد";
                }
                else
                {
                    row.Success = true;
                    row.Message = "ShahkarOk ثبت شد";
                    result.Updated++;
                }

                result.Results.Add(row);
            }

            result.Success = result.Results.All(r => r.Success);
            result.Message = IsDryRun
                ? $"DryRun — {names.Count} کاربر شبیه‌سازی شد"
                : $"{result.Updated} از {names.Count} کاربر به‌روز شد";
            return result;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Shahkar confirm failed");
            result.Error = ex.Message;
            return result;
        }
    }

    private string? ResolveConnectionString()
    {
        var name = string.IsNullOrWhiteSpace(_options.ConnectionStringName)
            ? "Security"
            : _options.ConnectionStringName.Trim();
        return _config.GetConnectionString(name);
    }
}
