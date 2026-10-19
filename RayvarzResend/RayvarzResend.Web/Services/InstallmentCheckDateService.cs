using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>تب تغییر تاریخ چک — SELECT/UPDATE مستقیم dbo.Installment_List با TrackingNo.</summary>
public sealed class InstallmentCheckDateService
{
    private readonly string _saraCs;
    private readonly IConfiguration _config;

    public InstallmentCheckDateService(IConfiguration config)
    {
        _config = config;
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara not set");
    }

    public bool IsDryRun =>
        _config.GetValue<bool?>("InstallmentCheckDate:DryRun")
        ?? _config.GetValue<bool?>("Installment:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public async Task<InstallmentCheckDateSearchResult> SearchAsync(
        InstallmentCheckDateSearchRequest req,
        CancellationToken ct = default)
    {
        var trackingRaw = (req.TrackingNo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trackingRaw))
            return new InstallmentCheckDateSearchResult { Error = "کد پیگیری (TrackingNo) الزامی است" };

        var digits = InstallmentIdentifierDetector.ExtractDigits(trackingRaw);
        if (digits.Length < InstallmentIdentifierDetector.TrackingNoMinDigitLength)
        {
            return new InstallmentCheckDateSearchResult
            {
                Error = "کد پیگیری معتبر نیست — حداقل ۱۰ رقم وارد کنید"
            };
        }

        var lookup = digits;
        var items = new List<InstallmentCheckDateListItem>();

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(InstallmentListDirectQuery.BuildSearchByTrackingNoSql(), conn);
        cmd.Parameters.AddWithValue("@v", lookup);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            items.Add(MapRow(reader));

        return new InstallmentCheckDateSearchResult
        {
            TrackingNo = lookup,
            Items = items,
            Total = items.Count
        };
    }

    public async Task<InstallmentCheckDateUpdateResult> UpdateAsync(
        InstallmentCheckDateUpdateRequest req,
        CancellationToken ct = default)
    {
        if (req.NidInstallmentList <= 0)
            return new InstallmentCheckDateUpdateResult { Error = "ردیف انتخاب نشده است" };

        if (!BankInquiryConfirmHelper.TryNormalizeSlashDate(req.NewPaymentDate, out var slashDate))
            return new InstallmentCheckDateUpdateResult { Error = "تاریخ چک معتبر نیست (مثال: 1405/01/01)" };

        var dryRun = IsDryRun;
        var result = new InstallmentCheckDateUpdateResult
        {
            DryRun = dryRun,
            NidInstallmentList = req.NidInstallmentList,
            NewPaymentDate = slashDate
        };

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);

        var existing = await LoadOneAsync(conn, req.NidInstallmentList, ct);
        if (existing == null)
        {
            result.Error = "ردیف در Installment_List یافت نشد";
            return result;
        }

        result.TrackingNo = existing.TrackingNo;
        result.PaymentDateBefore = existing.PaymentDate;

        if (dryRun)
        {
            result.Success = true;
            result.PaymentDateAfter = slashDate;
            result.Message =
                $"DryRun — PaymentDate از {existing.PaymentDate} به {slashDate} تغییر نمی‌کند (InstallmentCheckDate:DryRun یا Installment:DryRun)";
            return result;
        }

        var prefix = InstallmentCheckDateHelper.BuildCommentPrefix(req.PerformedByUser, slashDate);
        const string sql = """
            UPDATE dbo.Installment_List
            SET PaymentDate = @paymentDate,
                Comments = @prefix + ISNULL(Comments, '')
            WHERE NidInstallmentList = @id
            """;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@paymentDate", slashDate);
        cmd.Parameters.AddWithValue("@prefix", prefix);
        cmd.Parameters.AddWithValue("@id", req.NidInstallmentList);
        var affected = await cmd.ExecuteNonQueryAsync(ct);

        result.Success = affected > 0;
        result.PaymentDateAfter = slashDate;
        result.Message = affected > 0
            ? $"تاریخ چک به {slashDate} به‌روز شد"
            : "UPDATE انجام نشد";
        return result;
    }

    private static async Task<InstallmentCheckDateListItem?> LoadOneAsync(
        SqlConnection conn,
        long nidInstallmentList,
        CancellationToken ct)
    {
        const string sql = """
            SELECT il.NidInstallmentList,
                   il.NoDocument,
                   il.TrackingNo,
                   il.PaymentCost,
                   CAST(il.PaymentDate AS varchar(20)) AS PaymentDate,
                   CAST(il.CI_InstallmentStatus AS varchar(20)) AS CI_InstallmentStatus,
                   il.EndStateDesc,
                   il.EndStateCode,
                   il.Comments
            FROM dbo.Installment_List il
            WHERE il.NidInstallmentList = @id
            """;
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", nidInstallmentList);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;
        return MapRow(reader);
    }

    private static InstallmentCheckDateListItem MapRow(SqlDataReader reader) => new()
    {
        NidInstallmentList = reader["NidInstallmentList"] is DBNull
            ? 0
            : Convert.ToInt64(reader["NidInstallmentList"]),
        NoDocument = reader["NoDocument"]?.ToString() ?? "",
        TrackingNo = reader["TrackingNo"]?.ToString() ?? "",
        PaymentCost = reader["PaymentCost"] is DBNull ? null : Convert.ToDecimal(reader["PaymentCost"]),
        PaymentDate = reader["PaymentDate"]?.ToString() ?? "",
        CI_InstallmentStatus = reader["CI_InstallmentStatus"]?.ToString() ?? "",
        EndStateDesc = reader["EndStateDesc"]?.ToString() ?? "",
        EndStateCode = reader["EndStateCode"]?.ToString() ?? "",
        Comments = reader["Comments"]?.ToString() ?? ""
    };
}
