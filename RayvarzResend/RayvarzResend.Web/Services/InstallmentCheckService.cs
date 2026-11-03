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

        for (var i = 0; i < parsed.Values.Count; i++)
        {
            var rawValue = parsed.Values[i];
            var value = InstallmentIdentifierDetector.NormalizeLookupValue(rawValue);
            var kind = InstallmentIdentifierDetector.Detect(value);
            var willApplyEndState = InstallmentIdentifierDetector.WillApplyEndState(kind, parsed.ApplyEndStateRequested);

            var rows = await LoadRowsForExcelAsync(conn, kind, value, ct);
            AppendPreviewRowsForDbLookup(
                result,
                rows,
                kind,
                value,
                parsed.PerformedByUser,
                willApplyEndState,
                inputRowIndex: i + 1);
        }

        FinalizePreview(result);
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

        var (trackingKeys, selectionError) = InstallmentSendGridHelper.ParseSelectedTrackingNos(req.SelectedTrackingNos);
        if (selectionError != null)
        {
            result.Error = selectionError;
            return result;
        }

        return await UpdateSelectedTrackingAsync(
            parsed,
            trackingKeys,
            result,
            ct);
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

            var rows = await LoadRowsForExcelAsync(conn, kind, lookupValue, ct);
            if (rows.Count == 0)
            {
                item.ValidationMessage = "در دیتابیس یافت نشد";
                result.Items.Add(item);
                continue;
            }

            if (kind == InstallmentLookupKind.TrackingNo)
            {
                if (rows.Count > 1)
                {
                    item.Found = true;
                    item.ValidationMessage = $"بیش از یک ردیف ({rows.Count}) با این کد پیگیری در دیتابیس";
                    result.Items.Add(item);
                    continue;
                }

                AppendExcelMatchedPreviewItem(
                    result,
                    item,
                    excelRow,
                    kind,
                    lookupValue,
                    rows[0],
                    parsed.PerformedByUser,
                    willApplyEndState);
                continue;
            }

            foreach (var dbRow in rows)
            {
                var expanded = new InstallmentCheckPreviewItem
                {
                    RowIndex = result.Items.Count + 1,
                    LookupValue = lookupValue,
                    DetectedLookupKind = kind,
                    ExcelIdentifier = item.ExcelIdentifier,
                    ExcelPaymentCost = item.ExcelPaymentCost,
                    ExcelPaymentDate = item.ExcelPaymentDate,
                    WillApplyEndState = willApplyEndState,
                    DocSibling = rows.Count > 1
                };
                AppendExcelMatchedPreviewItem(
                    result,
                    expanded,
                    excelRow,
                    kind,
                    lookupValue,
                    dbRow,
                    parsed.PerformedByUser,
                    willApplyEndState);
            }
        }

        FinalizePreview(result);
        return result;
    }

    private static void FinalizePreview(InstallmentCheckPreviewResult result)
    {
        for (var i = 0; i < result.Items.Count; i++)
        {
            var item = result.Items[i];
            item.RowIndex = i + 1;
            if (item.Found && InstallmentSendGridHelper.NormalizeTrackingKey(item.TrackingNo).Length == 0)
            {
                item.ValidationMessage = string.IsNullOrWhiteSpace(item.ValidationMessage)
                    ? InstallmentSendGridHelper.MissingTrackingMessage
                    : $"{item.ValidationMessage} — {InstallmentSendGridHelper.MissingTrackingMessage}";
            }
        }

        result.FoundCount = result.Items.Count(i => i.Found);
        result.NotFoundCount = result.Items.Count(i => !i.Found);
        result.MatchedCount = result.Items.Count(i => i.Found && i.DataMatches);
        result.MismatchCount = result.Items.Count(i => i.Found && !i.DataMatches);
    }

    private async Task<InstallmentCheckUpdateResult> UpdateSelectedTrackingAsync(
        ParsedInstallmentRequest parsed,
        List<string> trackingKeys,
        InstallmentCheckUpdateResult result,
        CancellationToken ct)
    {
        var willApplyEndState = parsed.ApplyEndStateRequested;
        var commentPrefix = InstallmentCheckHelper.BuildCommentPrefix(parsed.PerformedByUser);

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var key in trackingKeys)
        {
            var item = new InstallmentCheckUpdateItemResult
            {
                LookupValue = key,
                DetectedLookupKind = InstallmentLookupKind.TrackingNo
            };

            try
            {
                var matches = await LoadRowsForExcelAsync(
                    conn,
                    InstallmentLookupKind.TrackingNo,
                    key,
                    ct);

                if (matches.Count != 1)
                {
                    item.Found = matches.Count > 0;
                    item.Success = false;
                    item.Message = matches.Count == 0
                        ? "کد پیگیری در دیتابیس یافت نشد"
                        : $"کد پیگیری مبهم — {matches.Count} ردیف با این شناسه";
                    AppendUpdateItemResult(result, item);
                    continue;
                }

                if (result.DryRun)
                {
                    item.Found = true;
                    item.Success = true;
                    item.WouldUpdate = 1;
                    item.Message =
                        $"DryRun — ردیف با کد پیگیری {key} UPDATE نمی‌شود (Installment:DryRun=true)";
                    AppendUpdateItemResult(result, item);
                    continue;
                }

                var sql = BuildUpdateSql(InstallmentLookupKind.TrackingNo, willApplyEndState);
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@prefix", commentPrefix);
                cmd.Parameters.AddWithValue("@status", InstallmentCheckHelper.TreasuryStatus);
                cmd.Parameters.AddWithValue("@v", key);
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
                    ? $"{affected} ردیف به‌روز شد (کد پیگیری {key})"
                    : "کد پیگیری در دیتابیس یافت نشد";
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

    private static void AppendPreviewRowsForDbLookup(
        InstallmentCheckPreviewResult result,
        List<InstallmentRowSnapshot> rows,
        InstallmentLookupKind kind,
        string lookupValue,
        string performedByUser,
        bool willApplyEndState,
        int inputRowIndex)
    {
        if (rows.Count == 0)
        {
            result.Items.Add(new InstallmentCheckPreviewItem
            {
                RowIndex = result.Items.Count + 1,
                LookupValue = lookupValue,
                DetectedLookupKind = kind,
                ValidationMessage = "در دیتابیس یافت نشد"
            });
            return;
        }

        if (kind == InstallmentLookupKind.TrackingNo)
        {
            if (rows.Count > 1)
            {
                result.Items.Add(new InstallmentCheckPreviewItem
                {
                    RowIndex = result.Items.Count + 1,
                    LookupValue = lookupValue,
                    DetectedLookupKind = kind,
                    Found = true,
                    ValidationMessage = $"بیش از یک ردیف ({rows.Count}) با این کد پیگیری در دیتابیس"
                });
                return;
            }

            result.Items.Add(MapPreviewItem(
                lookupValue,
                kind,
                rows[0],
                performedByUser,
                willApplyEndState,
                result.Items.Count + 1));
            return;
        }

        foreach (var row in rows)
        {
            var mapped = MapPreviewItem(
                lookupValue,
                kind,
                row,
                performedByUser,
                willApplyEndState,
                result.Items.Count + 1);
            mapped.DocSibling = rows.Count > 1;
            result.Items.Add(mapped);
        }
    }

    private static void AppendExcelMatchedPreviewItem(
        InstallmentCheckPreviewResult result,
        InstallmentCheckPreviewItem item,
        InstallmentExcelRowInput excelRow,
        InstallmentLookupKind kind,
        string lookupValue,
        InstallmentRowSnapshot dbRow,
        string performedByUser,
        bool willApplyEndState)
    {
        if (item.RowIndex <= 0)
            item.RowIndex = result.Items.Count + 1;

        item.LookupValue = kind == InstallmentLookupKind.TrackingNo
            ? InstallmentIdentifierDetector.ExtractDigits(dbRow.TrackingNo)
            : lookupValue;
        item.DetectedLookupKind = kind;
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
        item.ProposedComments = InstallmentCheckHelper.BuildNewComments(performedByUser, dbRow.Comments);
        item.ProposedCI_InstallmentStatus = InstallmentCheckHelper.TreasuryStatus;
        item.ProposedEndStateDesc = willApplyEndState
            ? InstallmentCheckHelper.EndStateDescOdooat
            : dbRow.EndStateDesc;
        item.ProposedEndStateCode = willApplyEndState
            ? InstallmentCheckHelper.EndStateCodeOdooat
            : dbRow.EndStateCode;
        item.WillApplyEndState = willApplyEndState;

        var mismatch = InstallmentExcelMatcher.ValidateAgainstDb(excelRow, dbRow, kind, lookupValue);
        if (mismatch == null)
            item.DataMatches = true;
        else
        {
            item.DataMatches = false;
            item.ValidationMessage = mismatch;
        }

        result.Items.Add(item);
    }

    private static InstallmentCheckPreviewItem MapPreviewItem(
        string lookupValue,
        InstallmentLookupKind kind,
        InstallmentRowSnapshot row,
        string performedByUser,
        bool willApplyEndState,
        int rowIndex = 0)
    {
        var resolvedLookup = kind == InstallmentLookupKind.TrackingNo
            ? InstallmentIdentifierDetector.ExtractDigits(row.TrackingNo)
            : lookupValue;

        return new InstallmentCheckPreviewItem
        {
            RowIndex = rowIndex,
            LookupValue = resolvedLookup,
            DetectedLookupKind = kind,
            Found = true,
            DataMatches = true,
            WillApplyEndState = willApplyEndState,
            NoDocument = row.NoDocument,
            TrackingNo = row.TrackingNo,
            PaymentCost = FormatCost(row.PaymentCost),
            PaymentDate = row.PaymentDate,
            NidWorkItem = row.NidWorkItem,
            NosaziCode = row.NosaziCode,
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
    }

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
        var whereClause = kind == InstallmentLookupKind.NoDocument
            ? "NoDocument = @v"
            : InstallmentIdentifierDetector.BuildTrackingNoWhereClause("TrackingNo");
        var endStateSql = withEndState
            ? ", EndStateDesc = @endDesc, EndStateCode = @endCode"
            : "";
        return $"""
            UPDATE dbo.Installment_List
            SET Comments = @prefix + ISNULL(Comments, ''),
                CI_InstallmentStatus = @status{endStateSql}
            WHERE {whereClause}
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
        cmd.Parameters.AddWithValue("@v", TrackingNoLookupParameter(kind, value));
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(MapExcelInstallmentRow(reader));
        }
        return rows;
    }

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

    private static string FormatCost(decimal? cost) =>
        cost?.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) ?? "";

    private static string TrackingNoLookupParameter(InstallmentLookupKind kind, string value) =>
        kind == InstallmentLookupKind.TrackingNo
            ? InstallmentIdentifierDetector.ExtractDigits(value)
            : value;
}

internal sealed record ParsedInstallmentRequest(
    List<string> Values,
    string PerformedByUser,
    bool ApplyEndStateRequested,
    bool IsExcelMode,
    List<InstallmentExcelRowInput> ExcelRows);
