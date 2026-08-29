using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public sealed class FicheDateChangeService
{
    private readonly IConfiguration _config;
    private readonly string _saraCs;

    public FicheDateChangeService(IConfiguration config)
    {
        _config = config;
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
    }

    public bool IsDryRun =>
        _config.GetValue<bool?>("FicheDateChange:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public async Task<List<string>> ListAccountGroupTitlesAsync(
        string? query = null,
        int limit = 20,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 100);
        var sql = $"""
            SELECT DISTINCT TOP ({limit}) Title
            FROM dbo.CI_IncomeAccountGroup
            WHERE Title IS NOT NULL AND LTRIM(RTRIM(Title)) <> N''
            """;
        if (!string.IsNullOrWhiteSpace(query))
            sql += " AND Title LIKE @pattern";
        sql += " ORDER BY Title";

        var titles = new List<string>();
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        if (!string.IsNullOrWhiteSpace(query))
            cmd.Parameters.AddWithValue("@pattern", $"%{query.Trim()}%");
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var title = reader["Title"]?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(title))
                titles.Add(title);
        }

        return titles;
    }

    public async Task<FicheDateChangeSearchResult> SearchAsync(
        FicheDateChangeSearchRequest req,
        CancellationToken ct = default)
    {
        var result = new FicheDateChangeSearchResult();
        if (!FicheDateChangeHelper.HasAnySearchFilter(req))
        {
            result.Error = "حداقل یک فیلتر (شماره فیش، بازه تاریخ، عنوان مالکیت، یا وضعیت فیش) وارد کنید";
            return result;
        }

        var (whereSql, parameters) = BuildSearchWhere(req);
        var maxResults = req.MaxResults is > 0 and <= 2000 ? req.MaxResults : 500;
        var sql = $"""
            SELECT TOP ({maxResults})
                   f.FicheNo,
                   f.BillId,
                   f.PaymentId,
                   f.ExportPermanentDate,
                   f.ExportTemporaryDate,
                   f.PaymentBreakDate,
                   f.PaymentDate,
                   f.EumFicheStatus,
                   g.Title AS AccountGroupTitle
            FROM dbo.Income_Fiche f
            INNER JOIN dbo.CI_IncomeAccountGroup g ON g.ID = f.CI_IncomeAccountGroup
            WHERE {whereSql}
            ORDER BY f.ExportPermanentDate DESC, f.FicheNo DESC
            """;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var status = ReadInt(reader, "EumFicheStatus");
            result.Items.Add(new FicheDateChangeListItem
            {
                FicheNo = reader["FicheNo"]?.ToString() ?? "",
                BillId = reader["BillId"]?.ToString() ?? "",
                PaymentId = reader["PaymentId"]?.ToString() ?? "",
                ExportPermanentDate = reader["ExportPermanentDate"]?.ToString() ?? "",
                ExportTemporaryDate = reader["ExportTemporaryDate"]?.ToString() ?? "",
                PaymentBreakDate = reader["PaymentBreakDate"]?.ToString() ?? "",
                PaymentDate = reader["PaymentDate"]?.ToString() ?? "",
                EumFicheStatus = status,
                EumFicheStatusLabel = FicheDateChangeHelper.StatusLabel(status),
                AccountGroupTitle = reader["AccountGroupTitle"]?.ToString() ?? ""
            });
        }

        result.Count = result.Items.Count;
        result.Truncated = result.Count >= maxResults;
        return result;
    }

    public async Task<FicheDateChangeUpdateResult> UpdateAsync(
        FicheDateChangeUpdateRequest req,
        CancellationToken ct = default)
    {
        var result = new FicheDateChangeUpdateResult { DryRun = IsDryRun };
        var ficheNos = (req.FicheNos ?? [])
            .Select(s => (s ?? "").Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (ficheNos.Count == 0)
        {
            result.Error = "حداقل یک فیش انتخاب کنید";
            return result;
        }

        if (!FicheDateChangeHelper.HasAnyChange(req))
        {
            result.Error = "حداقل یک فیلد برای تغییر (تاریخ یا وضعیت) مشخص کنید";
            return result;
        }

        var validationError = ValidateChangeRequest(req);
        if (validationError != null)
        {
            result.Error = validationError;
            return result;
        }

        var newPermanent = req.ApplyExportPermanentDate
            ? FicheDateChangeHelper.NormalizeSlashDate(req.NewExportPermanentDate)
            : null;
        var newTemporary = req.ApplyExportTemporaryDate
            ? FicheDateChangeHelper.NormalizeSlashDate(req.NewExportTemporaryDate)
            : null;
        var newBreak = req.ApplyPaymentBreakDate
            ? FicheDateChangeHelper.NormalizeSlashDate(req.NewPaymentBreakDate)
            : null;
        var newStatus = req.ApplyEumFicheStatus ? req.NewEumFicheStatus : null;

        if (result.DryRun)
        {
            result.Total = ficheNos.Count;
            result.WouldUpdate = ficheNos.Count;
            foreach (var ficheNo in ficheNos)
            {
                result.Results.Add(new FicheDateChangeUpdateItemResult
                {
                    FicheNo = ficheNo,
                    Success = true,
                    Found = true,
                    WouldUpdate = 1,
                    Message = "DryRun — UPDATE نمی‌شود (FicheDateChange:DryRun=true)"
                });
            }

            return result;
        }

        var commentPrefix = FicheDateChangeHelper.BuildCommentPrefix(req.PerformedByUser);
        var setClauses = new List<string> { "Comments = @prefix + ISNULL(Comments, N'')" };
        if (req.ApplyExportPermanentDate)
            setClauses.Add("ExportPermanentDate = @epd");
        if (req.ApplyExportTemporaryDate)
            setClauses.Add("ExportTemporaryDate = @etd");
        if (req.ApplyPaymentBreakDate)
            setClauses.Add("PaymentBreakDate = @pbd");
        if (req.ApplyEumFicheStatus)
            setClauses.Add("EumFicheStatus = @status");

        var sql = $"""
            UPDATE dbo.Income_Fiche
            SET {string.Join(", ", setClauses)}
            WHERE FicheNo = @ficheNo
            """;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var ficheNo in ficheNos)
        {
            var item = new FicheDateChangeUpdateItemResult { FicheNo = ficheNo };
            try
            {
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@prefix", commentPrefix);
                cmd.Parameters.AddWithValue("@ficheNo", ficheNo);
                if (req.ApplyExportPermanentDate)
                    cmd.Parameters.AddWithValue("@epd", newPermanent ?? (object)DBNull.Value);
                if (req.ApplyExportTemporaryDate)
                    cmd.Parameters.AddWithValue("@etd", newTemporary ?? (object)DBNull.Value);
                if (req.ApplyPaymentBreakDate)
                    cmd.Parameters.AddWithValue("@pbd", newBreak ?? (object)DBNull.Value);
                if (req.ApplyEumFicheStatus)
                    cmd.Parameters.AddWithValue("@status", newStatus ?? FicheDateChangeHelper.DefaultFicheStatus);

                var affected = await cmd.ExecuteNonQueryAsync(ct);
                item.RowsAffected = affected;
                item.Found = affected > 0;
                item.Success = affected > 0;
                item.Message = affected > 0 ? "به‌روزرسانی شد" : "یافت نشد";
            }
            catch (Exception ex)
            {
                item.Success = false;
                item.Message = ex.Message;
            }

            AppendUpdateItemResult(result, item);
        }

        return result;
    }

    private static string? ValidateChangeRequest(FicheDateChangeUpdateRequest req)
    {
        if (req.ApplyExportPermanentDate
            && !FicheDateChangeHelper.TryNormalizeSlashDate(req.NewExportPermanentDate, out _))
            return "تاریخ صدور دایم نامعتبر است";

        if (req.ApplyExportTemporaryDate
            && !FicheDateChangeHelper.TryNormalizeSlashDate(req.NewExportTemporaryDate, out _))
            return "تاریخ صدور موقت نامعتبر است";

        if (req.ApplyPaymentBreakDate
            && !FicheDateChangeHelper.TryNormalizeSlashDate(req.NewPaymentBreakDate, out _))
            return "تاریخ مهلت پرداخت نامعتبر است";

        if (req.ApplyEumFicheStatus
            && (!req.NewEumFicheStatus.HasValue || !FicheDateChangeHelper.FicheStatusLabels.ContainsKey(req.NewEumFicheStatus.Value)))
            return "وضعیت فیش نامعتبر است";

        return null;
    }

    private static (string WhereSql, List<(string Name, object Value)> Parameters) BuildSearchWhere(
        FicheDateChangeSearchRequest req)
    {
        var clauses = new List<string>();
        var parameters = new List<(string, object)>();

        AddDateRange(clauses, parameters, "f.ExportPermanentDate", req.PermanentFromDate, req.PermanentToDate, "pf", "pt");
        AddDateRange(clauses, parameters, "f.ExportTemporaryDate", req.TemporaryFromDate, req.TemporaryToDate, "tf", "tt");

        if (!string.IsNullOrWhiteSpace(req.AccountGroupTitle))
        {
            clauses.Add("g.Title LIKE @title");
            parameters.Add(("@title", $"%{req.AccountGroupTitle.Trim()}%"));
        }

        var identifierFilter = FicheDateChangeHelper.BuildIdentifierFilter(req.IdentifierValue);
        if (identifierFilter != null)
        {
            clauses.Add(identifierFilter.Value.Clause);
            parameters.Add((identifierFilter.Value.ParamName, identifierFilter.Value.Value));
        }

        if (req.EumFicheStatuses is { Count: > 0 })
        {
            var statusParams = req.EumFicheStatuses
                .Distinct()
                .Select((status, index) =>
                {
                    var name = $"@st{index}";
                    parameters.Add((name, status));
                    return name;
                })
                .ToList();
            if (statusParams.Count > 0)
                clauses.Add($"f.EumFicheStatus IN ({string.Join(", ", statusParams)})");
        }

        return (string.Join(" AND ", clauses), parameters);
    }

    private static void AddDateRange(
        List<string> clauses,
        List<(string, object)> parameters,
        string column,
        string? fromDate,
        string? toDate,
        string fromParamPrefix,
        string toParamPrefix)
    {
        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            var from = FicheDateChangeHelper.NormalizeSlashDate(fromDate);
            if (!string.IsNullOrEmpty(from))
            {
                clauses.Add($"{column} >= @{fromParamPrefix}");
                parameters.Add(($"@{fromParamPrefix}", from));
            }
        }

        if (!string.IsNullOrWhiteSpace(toDate))
        {
            var to = FicheDateChangeHelper.NormalizeSlashDate(toDate);
            if (!string.IsNullOrEmpty(to))
            {
                clauses.Add($"{column} <= @{toParamPrefix}");
                parameters.Add(($"@{toParamPrefix}", to));
            }
        }
    }

    private static void AppendUpdateItemResult(FicheDateChangeUpdateResult result, FicheDateChangeUpdateItemResult item)
    {
        result.Results.Add(item);
        result.Total++;
        if (result.DryRun)
        {
            if (item.WouldUpdate > 0) result.WouldUpdate += item.WouldUpdate;
            if (!item.Found) result.NotFound++;
            else if (!item.Success) result.Failed++;
            return;
        }

        if (item.Success) result.Updated += item.RowsAffected;
        else if (!item.Found) result.NotFound++;
        else result.Failed++;
    }

    private static int ReadInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        if (reader.IsDBNull(ordinal)) return 0;
        return Convert.ToInt32(reader.GetValue(ordinal));
    }
}
