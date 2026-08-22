using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class InstallmentCheckService
{
    private readonly string _saraCs;

    public InstallmentCheckService(IConfiguration config)
    {
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
    }

    public async Task<InstallmentCheckPreviewResult> PreviewAsync(
        InstallmentCheckRequest req,
        CancellationToken ct = default)
    {
        var lookup = NormalizeRequest(req);
        var result = new InstallmentCheckPreviewResult
        {
            LookupKind = lookup.LookupKind,
            ApplyEndState = lookup.WillApplyEndState
        };

        if (lookup.Values.Count == 0)
        {
            result.Error = "حداقل یک شماره سند یا کد پیگیری وارد کنید";
            return result;
        }

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        var foundKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in lookup.Values)
        {
            var rows = await LoadRowsAsync(conn, lookup.LookupKind, value, ct);
            if (rows.Count == 0)
            {
                result.Items.Add(new InstallmentCheckPreviewItem
                {
                    LookupValue = value,
                    Found = false
                });
                continue;
            }

            foundKeys.Add(value);
            foreach (var row in rows)
            {
                result.Items.Add(new InstallmentCheckPreviewItem
                {
                    LookupValue = value,
                    Found = true,
                    NoDocument = row.NoDocument,
                    TrackingNo = row.TrackingNo,
                    CI_InstallmentStatus = row.CI_InstallmentStatus,
                    EndStateDesc = row.EndStateDesc,
                    EndStateCode = row.EndStateCode,
                    Comments = row.Comments,
                    ProposedComments = InstallmentCheckHelper.BuildNewComments(
                        lookup.PerformedByUser, row.Comments),
                    ProposedCI_InstallmentStatus = InstallmentCheckHelper.TreasuryStatus,
                    ProposedEndStateDesc = lookup.WillApplyEndState
                        ? InstallmentCheckHelper.EndStateDescOdooat
                        : row.EndStateDesc,
                    ProposedEndStateCode = lookup.WillApplyEndState
                        ? InstallmentCheckHelper.EndStateCodeOdooat
                        : row.EndStateCode
                });
            }
        }

        result.FoundCount = result.Items.Count(i => i.Found);
        result.NotFoundCount = lookup.Values.Count - foundKeys.Count;
        return result;
    }

    public async Task<InstallmentCheckUpdateResult> UpdateAsync(
        InstallmentCheckRequest req,
        CancellationToken ct = default)
    {
        var lookup = NormalizeRequest(req);
        var result = new InstallmentCheckUpdateResult
        {
            LookupKind = lookup.LookupKind,
            ApplyEndState = lookup.WillApplyEndState
        };

        if (lookup.Values.Count == 0)
        {
            result.Error = "حداقل یک شماره سند یا کد پیگیری وارد کنید";
            return result;
        }

        var commentPrefix = InstallmentCheckHelper.BuildCommentPrefix(lookup.PerformedByUser);

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        foreach (var value in lookup.Values)
        {
            var item = new InstallmentCheckUpdateItemResult { LookupValue = value };

            try
            {
                var withEndState = lookup.WillApplyEndState;
                var sql = lookup.LookupKind == InstallmentLookupKind.NoDocument
                    ? BuildUpdateSql(lookup.LookupKind, withEndState: true)
                    : BuildUpdateSql(lookup.LookupKind, withEndState);

                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@prefix", commentPrefix);
                cmd.Parameters.AddWithValue("@status", InstallmentCheckHelper.TreasuryStatus);
                cmd.Parameters.AddWithValue("@v", value);
                if (withEndState)
                {
                    cmd.Parameters.AddWithValue("@endDesc", InstallmentCheckHelper.EndStateDescOdooat);
                    cmd.Parameters.AddWithValue("@endCode", InstallmentCheckHelper.EndStateCodeOdooat);
                }

                var affected = await cmd.ExecuteNonQueryAsync(ct);
                item.RowsAffected = affected;
                item.Found = affected > 0;
                item.Success = affected > 0;
                item.Message = affected > 0
                    ? $"{affected} ردیف به‌روز شد"
                    : "ردیفی با این شناسه یافت نشد";
            }
            catch (Exception ex)
            {
                item.Success = false;
                item.Message = ex.Message;
            }

            result.Results.Add(item);
            result.Total++;
            if (item.Success) result.Updated += item.RowsAffected;
            else if (!item.Found) result.NotFound++;
            else result.Failed++;
        }

        return result;
    }

    private static InstallmentLookupContext NormalizeRequest(InstallmentCheckRequest req)
    {
        var lookupKind = req.LookupKind;
        var values = InstallmentCheckHelper.ParseIdentifierList(req.ValuesText);
        var applyEndState = lookupKind == InstallmentLookupKind.NoDocument || req.ApplyEndState;
        return new InstallmentLookupContext(
            lookupKind,
            values,
            (req.PerformedByUser ?? "").Trim(),
            applyEndState);
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

    private static async Task<List<InstallmentRow>> LoadRowsAsync(
        SqlConnection conn,
        InstallmentLookupKind kind,
        string value,
        CancellationToken ct)
    {
        var column = kind == InstallmentLookupKind.NoDocument ? "NoDocument" : "TrackingNo";
        var sql = $@"
SELECT NoDocument, TrackingNo,
       CAST(CI_InstallmentStatus AS varchar(20)) AS CI_InstallmentStatus,
       EndStateDesc, EndStateCode, Comments
FROM dbo.Installment_List
WHERE {column} = @v";

        var rows = new List<InstallmentRow>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@v", value);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new InstallmentRow
            {
                NoDocument = reader["NoDocument"]?.ToString() ?? "",
                TrackingNo = reader["TrackingNo"]?.ToString() ?? "",
                CI_InstallmentStatus = reader["CI_InstallmentStatus"]?.ToString() ?? "",
                EndStateDesc = reader["EndStateDesc"]?.ToString() ?? "",
                EndStateCode = reader["EndStateCode"]?.ToString() ?? "",
                Comments = reader["Comments"]?.ToString() ?? ""
            });
        }
        return rows;
    }

    private sealed class InstallmentRow
    {
        public string NoDocument { get; set; } = "";
        public string TrackingNo { get; set; } = "";
        public string CI_InstallmentStatus { get; set; } = "";
        public string EndStateDesc { get; set; } = "";
        public string EndStateCode { get; set; } = "";
        public string Comments { get; set; } = "";
    }

    private sealed record InstallmentLookupContext(
        InstallmentLookupKind LookupKind,
        List<string> Values,
        string PerformedByUser,
        bool WillApplyEndState);
}
