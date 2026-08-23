using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class InstallmentCheckService
{
    private readonly IConfiguration _config;
    private readonly string _saraCs;

    public InstallmentCheckService(IConfiguration config)
    {
        _config = config;
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
    }

    public bool IsDryRun =>
        _config.GetValue<bool?>("Installment:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public async Task<InstallmentCheckPreviewResult> PreviewAsync(
        InstallmentCheckRequest req,
        CancellationToken ct = default)
    {
        var parsed = ParseRequest(req);
        var result = new InstallmentCheckPreviewResult
        {
            ExcelMode = parsed.IsExcelMode,
            ApplyEndState = parsed.ApplyEndStateRequested
        };

        if (parsed.IsExcelMode)
            return await PreviewExcelAsync(req, parsed, result, ct);

        if (parsed.Values.Count == 0)
        {
            result.Error = "حداقل یک شماره سند یا کد پیگیری وارد کنید";
            return result;
        }

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        var foundKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawValue in parsed.Values)
        {
            var value = InstallmentIdentifierDetector.NormalizeLookupValue(rawValue);
            var kind = InstallmentIdentifierDetector.Detect(value);
            var willApplyEndState = InstallmentIdentifierDetector.WillApplyEndState(kind, parsed.ApplyEndStateRequested);

            var rows = await LoadRowsAsync(conn, kind, value, ct);
            if (rows.Count == 0)
            {
                result.Items.Add(new InstallmentCheckPreviewItem
                {
                    LookupValue = value,
                    DetectedLookupKind = kind,
                    Found = false
                });
                continue;
            }

            foundKeys.Add(value);
            foreach (var row in rows)
            {
                result.Items.Add(MapPreviewItem(value, kind, row, parsed.PerformedByUser, willApplyEndState));
            }
        }

        result.FoundCount = result.Items.Count(i => i.Found);
        result.NotFoundCount = parsed.Values.Count - foundKeys.Count;
        result.MatchedCount = result.Items.Count(i => i.Found);
        return result;
    }

    public async Task<InstallmentCheckUpdateResult> UpdateAsync(
        InstallmentCheckRequest req,
        CancellationToken ct = default)
    {
        var parsed = ParseRequest(req);
        var dryRun = IsDryRun;
        var result = new InstallmentCheckUpdateResult
        {
            ExcelMode = parsed.IsExcelMode,
            ApplyEndState = parsed.ApplyEndStateRequested,
            DryRun = dryRun
        };

        if (parsed.IsExcelMode)
            return await UpdateExcelAsync(req, parsed, result, ct);

        if (parsed.Values.Count == 0)
        {
            result.Error = "حداقل یک شماره سند یا کد پیگیری وارد کنید";
            return result;
        }

        if (dryRun)
            return await SimulateUpdateAsync(req, parsed, result, ct);

        var commentPrefix = InstallmentCheckHelper.BuildCommentPrefix(parsed.PerformedByUser);

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var rawValue in parsed.Values)
        {
            var value = InstallmentIdentifierDetector.NormalizeLookupValue(rawValue);
            var kind = InstallmentIdentifierDetector.Detect(value);
            var willApplyEndState = InstallmentIdentifierDetector.WillApplyEndState(kind, parsed.ApplyEndStateRequested);
            var item = new InstallmentCheckUpdateItemResult
            {
                LookupValue = value,
                DetectedLookupKind = kind
            };

            try
            {
                var sql = BuildUpdateSql(kind, willApplyEndState);

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@prefix", commentPrefix);
                cmd.Parameters.AddWithValue("@status", InstallmentCheckHelper.TreasuryStatus);
                cmd.Parameters.AddWithValue("@v", value);
                if (willApplyEndState)
                {
                    cmd.Parameters.AddWithValue("@endDesc", InstallmentCheckHelper.EndStateDescOdooat);
                    cmd.Parameters.AddWithValue("@endCode", InstallmentCheckHelper.EndStateCodeOdooat);
                }

                var affected = await cmd.ExecuteNonQueryAsync(ct);
                item.RowsAffected = affected;
                item.Found = affected > 0;
                item.Success = affected > 0;
                item.Message = affected > 0
                    ? $"{affected} ردیف به‌روز شد ({InstallmentIdentifierDetector.Describe(kind)})"
                    : $"ردیفی با {InstallmentIdentifierDetector.Describe(kind)} یافت نشد";
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

    private async Task<InstallmentCheckPreviewResult> PreviewExcelAsync(
        InstallmentCheckRequest req,
        ParsedInstallmentRequest parsed,
        InstallmentCheckPreviewResult result,
        CancellationToken ct)
    {
        if (parsed.ExcelRows.Count == 0)
        {
            result.Error = "فایل اکسل خالی است یا ردیفی خوانده نشد";
            return result;
        }

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        for (var i = 0; i < parsed.ExcelRows.Count; i++)
        {
            var excelRow = parsed.ExcelRows[i];
            var (kind, lookupValue, lookupError) = InstallmentExcelMatcher.ResolveLookup(excelRow);
            var willApplyEndState = InstallmentExcelMatcher.ResolveWillApplyEndState(
                kind, parsed.ApplyEndStateRequested, excelRow, excelMode: true);

            var item = new InstallmentCheckPreviewItem
            {
                RowIndex = i + 1,
                LookupValue = lookupValue,
                DetectedLookupKind = kind,
                ExcelIdentifier = InstallmentExcelMatcher.NormalizeCell(excelRow.Identifier),
                ExcelPaymentCost = InstallmentExcelMatcher.NormalizeCell(excelRow.PaymentCost),
                ExcelPaymentDate = InstallmentExcelMatcher.NormalizeCell(excelRow.PaymentDate),
                ExcelOdooat = kind == InstallmentLookupKind.TrackingNo
                    ? InstallmentExcelMatcher.NormalizeCell(excelRow.Odooat)
                    : null,
                WillApplyEndState = willApplyEndState
            };

            if (!string.IsNullOrEmpty(lookupError))
            {
                item.ValidationMessage = lookupError;
                result.Items.Add(item);
                continue;
            }

            var usedCostDateFallback = false;
            var rows = await LoadRowsForExcelAsync(conn, kind, lookupValue, ct);
            if (rows.Count == 0 && InstallmentExcelMatcher.CanUseCostDateFallback(excelRow, kind))
            {
                rows = await LoadRowsForExcelByCostDateAsync(conn, excelRow, ct);
                usedCostDateFallback = rows.Count > 0;
            }

            if (rows.Count == 0)
            {
                item.ValidationMessage = InstallmentExcelMatcher.LooksLikeScientificNotation(excelRow.Identifier)
                    ? "کد پیگیری در اکسل خراب است (5.02E+14) — مبلغ/تاریخ هم منطبق نبود"
                    : "در دیتابیس یافت نشد";
                result.Items.Add(item);
                continue;
            }

            if (rows.Count > 1)
            {
                item.Found = true;
                item.ValidationMessage = usedCostDateFallback
                    ? $"بیش از یک ردیف با همین مبلغ/تاریخ ({rows.Count})"
                    : $"بیش از یک ردیف ({rows.Count}) در دیتابیس یافت شد";
                result.Items.Add(item);
                continue;
            }

            var dbRow = rows[0];
            if (usedCostDateFallback)
            {
                kind = InstallmentLookupKind.TrackingNo;
                lookupValue = InstallmentExcelMatcher.NormalizeDigits(dbRow.TrackingNo);
                item.DetectedLookupKind = kind;
                item.LookupValue = lookupValue;
                item.MatchedByCostDate = true;
                willApplyEndState = InstallmentExcelMatcher.ResolveWillApplyEndState(
                    kind, parsed.ApplyEndStateRequested, excelRow, excelMode: true);
                item.WillApplyEndState = willApplyEndState;
            }

            item.Found = true;
            item.NoDocument = dbRow.NoDocument;
            item.TrackingNo = dbRow.TrackingNo;
            item.PaymentCost = FormatCost(dbRow.PaymentCost);
            item.PaymentDate = dbRow.PaymentDate;
            item.NidWorkItem = dbRow.NidWorkItem;
            item.NosaziCode = dbRow.NosaziCode;
            item.CI_InstallmentStatus = dbRow.CI_InstallmentStatus;
            item.EndStateDesc = dbRow.EndStateDesc;
            item.EndStateCode = dbRow.EndStateCode;
            item.Comments = dbRow.Comments;
            item.ProposedComments = InstallmentCheckHelper.BuildNewComments(parsed.PerformedByUser, dbRow.Comments);
            item.ProposedCI_InstallmentStatus = InstallmentCheckHelper.TreasuryStatus;
            item.ProposedEndStateDesc = willApplyEndState
                ? InstallmentCheckHelper.EndStateDescOdooat
                : dbRow.EndStateDesc;
            item.ProposedEndStateCode = willApplyEndState
                ? InstallmentCheckHelper.EndStateCodeOdooat
                : dbRow.EndStateCode;

            var mismatch = InstallmentExcelMatcher.ValidateAgainstDb(excelRow, dbRow, kind, lookupValue);
            if (mismatch == null)
            {
                item.DataMatches = true;
                if (item.MatchedByCostDate)
                    item.ValidationMessage = "تطابق با مبلغ و تاریخ (شناسه اکسل خراب بود)";
            }
            else
            {
                item.DataMatches = false;
                item.ValidationMessage = mismatch;
            }

            result.Items.Add(item);
        }

        result.FoundCount = result.Items.Count(i => i.Found);
        result.NotFoundCount = result.Items.Count(i => !i.Found);
        result.MatchedCount = result.Items.Count(i => i.Found && i.DataMatches);
        result.MismatchCount = result.Items.Count(i => i.Found && !i.DataMatches);
        return result;
    }

    private async Task<InstallmentCheckUpdateResult> UpdateExcelAsync(
        InstallmentCheckRequest req,
        ParsedInstallmentRequest parsed,
        InstallmentCheckUpdateResult result,
        CancellationToken ct)
    {
        var preview = await PreviewExcelAsync(req, parsed, new InstallmentCheckPreviewResult
        {
            ExcelMode = true,
            ApplyEndState = parsed.ApplyEndStateRequested
        }, ct);

        if (!string.IsNullOrWhiteSpace(preview.Error))
        {
            result.Error = preview.Error;
            return result;
        }

        var eligible = preview.Items
            .Where(i => i.Found && i.DataMatches)
            .ToList();

        if (eligible.Count == 0)
        {
            result.Error = "هیچ ردیف معتبری برای اعمال یافت نشد — ابتدا پیش‌نمایش را بررسی کنید";
            return result;
        }

        if (result.DryRun)
        {
            foreach (var row in eligible)
            {
                var item = new InstallmentCheckUpdateItemResult
                {
                    LookupValue = row.LookupValue,
                    DetectedLookupKind = row.DetectedLookupKind,
                    Found = true,
                    Success = true,
                    WouldUpdate = 1,
                    Message = $"DryRun — ردیف {row.RowIndex} UPDATE نمی‌شود (Installment:DryRun=true)"
                };
                AppendUpdateItemResult(result, item);
            }

            foreach (var row in preview.Items.Where(i => !i.Found))
            {
                var item = new InstallmentCheckUpdateItemResult
                {
                    LookupValue = row.LookupValue,
                    DetectedLookupKind = row.DetectedLookupKind,
                    Found = false,
                    Success = false,
                    Message = $"ردیف {row.RowIndex}: {row.ValidationMessage ?? "یافت نشد"}"
                };
                AppendUpdateItemResult(result, item);
            }

            foreach (var row in preview.Items.Where(i => i.Found && !i.DataMatches))
            {
                result.SkippedMismatch++;
                var item = new InstallmentCheckUpdateItemResult
                {
                    LookupValue = row.LookupValue,
                    DetectedLookupKind = row.DetectedLookupKind,
                    Found = true,
                    Success = false,
                    Message = $"ردیف {row.RowIndex}: {row.ValidationMessage ?? "عدم تطابق با دیتابیس"}"
                };
                AppendUpdateItemResult(result, item);
            }

            return result;
        }

        var commentPrefix = InstallmentCheckHelper.BuildCommentPrefix(parsed.PerformedByUser);

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var row in eligible)
        {
            var willApplyEndState = row.WillApplyEndState;
            var item = new InstallmentCheckUpdateItemResult
            {
                LookupValue = row.LookupValue,
                DetectedLookupKind = row.DetectedLookupKind
            };

            try
            {
                var sql = BuildUpdateSql(row.DetectedLookupKind, willApplyEndState);
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@prefix", commentPrefix);
                cmd.Parameters.AddWithValue("@status", InstallmentCheckHelper.TreasuryStatus);
                cmd.Parameters.AddWithValue("@v", row.LookupValue);
                if (willApplyEndState)
                {
                    cmd.Parameters.AddWithValue("@endDesc", InstallmentCheckHelper.EndStateDescOdooat);
                    cmd.Parameters.AddWithValue("@endCode", InstallmentCheckHelper.EndStateCodeOdooat);
                }

                var affected = await cmd.ExecuteNonQueryAsync(ct);
                item.RowsAffected = affected;
                item.Found = affected > 0;
                item.Success = affected > 0;
                item.Message = affected > 0
                    ? $"ردیف {row.RowIndex}: {affected} ردیف به‌روز شد"
                    : $"ردیف {row.RowIndex}: یافت نشد";
            }
            catch (Exception ex)
            {
                item.Success = false;
                item.Message = $"ردیف {row.RowIndex}: {ex.Message}";
            }

            AppendUpdateItemResult(result, item);
        }

        foreach (var row in preview.Items.Where(i => !i.Found || !i.DataMatches))
        {
            if (row.Found && !row.DataMatches)
                result.SkippedMismatch++;

            var item = new InstallmentCheckUpdateItemResult
            {
                LookupValue = row.LookupValue,
                DetectedLookupKind = row.DetectedLookupKind,
                Found = row.Found,
                Success = false,
                Message = $"ردیف {row.RowIndex}: {row.ValidationMessage ?? (row.Found ? "عدم تطابق" : "یافت نشد")}"
            };
            AppendUpdateItemResult(result, item);
        }

        return result;
    }

    private async Task<InstallmentCheckUpdateResult> SimulateUpdateAsync(
        InstallmentCheckRequest req,
        ParsedInstallmentRequest parsed,
        InstallmentCheckUpdateResult result,
        CancellationToken ct)
    {
        var preview = await PreviewAsync(req, ct);
        if (!string.IsNullOrWhiteSpace(preview.Error))
        {
            result.Error = preview.Error;
            return result;
        }

        foreach (var rawValue in parsed.Values)
        {
            var value = InstallmentIdentifierDetector.NormalizeLookupValue(rawValue);
            var kind = InstallmentIdentifierDetector.Detect(value);
            var matches = preview.Items
                .Where(i => i.Found && string.Equals(i.LookupValue, value, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var item = new InstallmentCheckUpdateItemResult
            {
                LookupValue = value,
                DetectedLookupKind = kind
            };

            if (matches.Count == 0)
            {
                item.Found = false;
                item.Success = false;
                item.Message = $"DryRun — ردیفی یافت نشد ({InstallmentIdentifierDetector.Describe(kind)})";
            }
            else
            {
                item.Found = true;
                item.Success = true;
                item.WouldUpdate = matches.Count;
                item.Message =
                    $"DryRun — {matches.Count} ردیف UPDATE نمی‌شود (Installment:DryRun=true)";
            }

            AppendUpdateItemResult(result, item);
        }

        return result;
    }

    private static InstallmentCheckPreviewItem MapPreviewItem(
        string lookupValue,
        InstallmentLookupKind kind,
        InstallmentRowSnapshot row,
        string performedByUser,
        bool willApplyEndState) => new()
    {
        LookupValue = lookupValue,
        DetectedLookupKind = kind,
        Found = true,
        DataMatches = true,
        NoDocument = row.NoDocument,
        TrackingNo = row.TrackingNo,
        PaymentCost = FormatCost(row.PaymentCost),
        PaymentDate = row.PaymentDate,
        CI_InstallmentStatus = row.CI_InstallmentStatus,
        EndStateDesc = row.EndStateDesc,
        EndStateCode = row.EndStateCode,
        Comments = row.Comments,
        ProposedComments = InstallmentCheckHelper.BuildNewComments(performedByUser, row.Comments),
        ProposedCI_InstallmentStatus = InstallmentCheckHelper.TreasuryStatus,
        ProposedEndStateDesc = willApplyEndState
            ? InstallmentCheckHelper.EndStateDescOdooat
            : row.EndStateDesc,
        ProposedEndStateCode = willApplyEndState
            ? InstallmentCheckHelper.EndStateCodeOdooat
            : row.EndStateCode
    };

    private static ParsedInstallmentRequest ParseRequest(InstallmentCheckRequest req)
    {
        var excelRows = (req.ExcelRows ?? new List<InstallmentExcelRowInput>())
            .Where(r => !IsExcelRowEmpty(r))
            .ToList();

        return new ParsedInstallmentRequest(
            InstallmentCheckHelper.ParseIdentifierList(req.ValuesText),
            (req.PerformedByUser ?? "").Trim(),
            req.ApplyEndState,
            excelRows.Count > 0,
            excelRows);
    }

    private static bool IsExcelRowEmpty(InstallmentExcelRowInput row) =>
        string.IsNullOrWhiteSpace(row.Identifier)
        && string.IsNullOrWhiteSpace(row.PaymentCost)
        && string.IsNullOrWhiteSpace(row.PaymentDate)
        && string.IsNullOrWhiteSpace(row.Odooat);

    private static void AppendUpdateItemResult(InstallmentCheckUpdateResult result, InstallmentCheckUpdateItemResult item)
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

    private static string BuildUpdateSql(InstallmentLookupKind kind, bool withEndState)
    {
        var whereColumn = kind == InstallmentLookupKind.NoDocument ? "NoDocument" : "TrackingNo";
        var endStateSql = withEndState
            ? ", EndStateDesc = @endDesc, EndStateCode = @endCode"
            : "";
        return $"""
            UPDATE dbo.Installment_List
            SET Comments = @prefix + ISNULL(Comments, ''),
                CI_InstallmentStatus = @status{endStateSql}
            WHERE {whereColumn} = @v
            """;
    }

    private static async Task<List<InstallmentRowSnapshot>> LoadRowsForExcelAsync(
        SqlConnection conn,
        InstallmentLookupKind kind,
        string value,
        CancellationToken ct)
    {
        var column = kind == InstallmentLookupKind.NoDocument ? "NoDocument" : "TrackingNo";
        var sql = InstallmentListQuery.BuildExcelLookupSql(column);

        var rows = new List<InstallmentRowSnapshot>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@v", value);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(MapExcelInstallmentRow(reader));
        }
        return rows;
    }

    private static async Task<List<InstallmentRowSnapshot>> LoadRowsForExcelByCostDateAsync(
        SqlConnection conn,
        InstallmentExcelRowInput excelRow,
        CancellationToken ct)
    {
        if (!InstallmentExcelMatcher.TryParseCost(excelRow.PaymentCost, out var cost))
            return [];

        var paymentDateDigits = InstallmentExcelMatcher.NormalizeDateDigits(excelRow.PaymentDate);
        if (paymentDateDigits.Length < 8)
            return [];

        var sql = InstallmentListQuery.BuildExcelLookupByCostDateSql();
        var rows = new List<InstallmentRowSnapshot>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cost", cost);
        cmd.Parameters.AddWithValue("@paymentDateDigits", paymentDateDigits);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(MapExcelInstallmentRow(reader));
        }
        return rows;
    }

    private static InstallmentRowSnapshot MapSimpleInstallmentRow(SqlDataReader reader) => new()
    {
        NoDocument = reader["NoDocument"]?.ToString() ?? "",
        TrackingNo = reader["TrackingNo"]?.ToString() ?? "",
        PaymentCost = reader["PaymentCost"] is DBNull ? null : Convert.ToDecimal(reader["PaymentCost"]),
        PaymentDate = reader["PaymentDate"]?.ToString() ?? "",
        CI_InstallmentStatus = reader["CI_InstallmentStatus"]?.ToString() ?? "",
        EndStateDesc = reader["EndStateDesc"]?.ToString() ?? "",
        EndStateCode = reader["EndStateCode"]?.ToString() ?? "",
        Comments = reader["Comments"]?.ToString() ?? ""
    };

    private static InstallmentRowSnapshot MapExcelInstallmentRow(SqlDataReader reader) => new()
    {
        NoDocument = reader["NoDocument"]?.ToString() ?? "",
        TrackingNo = reader["trackingno"]?.ToString() ?? reader["TrackingNo"]?.ToString() ?? "",
        PaymentCost = reader["PaymentCost"] is DBNull ? null : Convert.ToDecimal(reader["PaymentCost"]),
        PaymentDate = reader["PaymentDate"]?.ToString() ?? "",
        NidWorkItem = reader["nidworkitem"]?.ToString() ?? reader["NidWorkItem"]?.ToString() ?? "",
        NosaziCode = reader["NosaziCode"]?.ToString() ?? "",
        CI_InstallmentStatus = reader["CI_InstallmentStatus"]?.ToString() ?? "",
        EndStateDesc = reader["EndStateDesc"]?.ToString() ?? "",
        EndStateCode = reader["EndStateCode"]?.ToString() ?? "",
        Comments = reader["Comments"]?.ToString() ?? ""
    };

    private static async Task<List<InstallmentRowSnapshot>> LoadRowsAsync(
        SqlConnection conn,
        InstallmentLookupKind kind,
        string value,
        CancellationToken ct)
    {
        var column = kind == InstallmentLookupKind.NoDocument ? "NoDocument" : "TrackingNo";
        var sql = $@"
SELECT NoDocument, TrackingNo,
       PaymentCost,
       CAST(PaymentDate AS varchar(20)) AS PaymentDate,
       CAST(CI_InstallmentStatus AS varchar(20)) AS CI_InstallmentStatus,
       EndStateDesc, EndStateCode, Comments
FROM dbo.Installment_List
WHERE {column} = @v";

        var rows = new List<InstallmentRowSnapshot>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@v", value);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(MapSimpleInstallmentRow(reader));
        }
        return rows;
    }

    private static string FormatCost(decimal? cost) =>
        cost?.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) ?? "";
}

internal sealed record ParsedInstallmentRequest(
    List<string> Values,
    string PerformedByUser,
    bool ApplyEndStateRequested,
    bool IsExcelMode,
    List<InstallmentExcelRowInput> ExcelRows);
