using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ارسال تهاتر از مسیر جدول واسط Sara — نه SOAP مستقیم Resend.
/// جریان: چک Accounting_DocHeader → نگه‌داشت Income_Fiche → وضعیت 2 (تایید دستی)
/// → انتظار پر شدن واسط → بازگردانی وضعیت 3 → در صورت نبود، Accounting_DocNotSent.
/// </summary>
public sealed class TahatorResendService
{
    private readonly string _saraCs;
    private readonly IConfiguration _config;
    private readonly ILogger<TahatorResendService> _logger;
    private readonly FicheRepository _fiches;

    public TahatorResendService(
        IConfiguration config,
        ILogger<TahatorResendService> logger,
        FicheRepository fiches)
    {
        _config = config;
        _logger = logger;
        _fiches = fiches;
        _saraCs = config.GetConnectionString("Sara")
            ?? throw new InvalidOperationException("ConnectionStrings:Sara تنظیم نشده");
    }

    public int PollIntervalMs => Math.Max(500, _config.GetValue("Tahator:PollIntervalMs", 2000));
    public int PollTimeoutSeconds => Math.Max(5, _config.GetValue("Tahator:PollTimeoutSeconds", 60));

    public bool IsDryRun =>
        _config.GetValue<bool?>("Tahator:DryRun")
        ?? _config.GetValue("Rayvarz:DryRun", true);

    public async Task<TahatorCheckResult> CheckAsync(string ficheNo, CancellationToken ct = default)
    {
        ficheNo = NormalizeFicheNo(ficheNo);
        var inHeader = await ExistsInAccountingDocHeaderAsync(ficheNo, ct);
        var snapshot = await TryLoadSnapshotAsync(ficheNo, ct);
        var notSent = inHeader ? null : await _fiches.GetDocNotSentErrorAsync(ficheNo, ct);

        return new TahatorCheckResult
        {
            FicheNo = ficheNo,
            ExistsInAccountingDocHeader = inHeader,
            ExistsInIncomeFiche = snapshot != null,
            Snapshot = snapshot,
            DocNotSentError = notSent,
            NeedsSend = !inHeader && snapshot != null,
            Message = inHeader
                ? "فیش در جدول واسط Accounting_DocHeader هست — نیاز به ارسال نیست (رایورز از واسط برمی‌دارد)."
                : snapshot == null
                    ? "فیش در Income_Fiche یافت نشد."
                    : string.IsNullOrWhiteSpace(notSent)
                        ? "در واسط نیست — می‌توان فرایند تهاتر (وضعیت ۲→۳) را اجرا کرد."
                        : $"در واسط نیست. آخرین DocNotSent: {notSent}"
        };
    }

    public async Task<TahatorSendResult> SendAsync(string ficheNo, CancellationToken ct = default)
    {
        ficheNo = NormalizeFicheNo(ficheNo);
        var steps = new List<string>();
        var dryRun = IsDryRun;

        var inHeaderBefore = await ExistsInAccountingDocHeaderAsync(ficheNo, ct);
        steps.Add(inHeaderBefore
            ? "1) Accounting_DocHeader: موجود — ارسال لازم نیست"
            : "1) Accounting_DocHeader: موجود نیست");

        if (inHeaderBefore)
        {
            return new TahatorSendResult
            {
                Success = true,
                Skipped = true,
                FicheNo = ficheNo,
                DryRun = dryRun,
                ExistsInAccountingDocHeaderBefore = true,
                ExistsInAccountingDocHeaderAfter = true,
                Steps = steps,
                Message = "فیش از قبل در جدول واسط است — هیچ UPDATE انجام نشد."
            };
        }

        var snapshot = await TryLoadSnapshotAsync(ficheNo, ct);
        if (snapshot == null)
        {
            return new TahatorSendResult
            {
                Success = false,
                FicheNo = ficheNo,
                DryRun = dryRun,
                Steps = steps,
                Message = "فیش در Income_Fiche یافت نشد."
            };
        }

        steps.Add(
            $"2) SELECT نگه‌داشت: Status={snapshot.EumFicheStatus}, Export={snapshot.ExportPermanentDate}, " +
            $"Break={snapshot.PaymentBreakDate}, Pay={snapshot.PaymentDate}, Confirm={snapshot.UserConfirmDate}");

        var today = DateHelper.CurrentShamsiSlashDate();
        if (dryRun)
        {
            steps.Add($"3) DryRun — UPDATE وضعیت ۲ شبیه‌سازی شد (Export/Break={today}, PaymentDate خالی)");
            steps.Add("4) DryRun — انتظار واسط رد شد");
            steps.Add("5) DryRun — UPDATE بازگردانی وضعیت ۳ شبیه‌سازی شد");
            var notSentDry = await _fiches.GetDocNotSentErrorAsync(ficheNo, ct);
            return new TahatorSendResult
            {
                Success = true,
                FicheNo = ficheNo,
                DryRun = true,
                ExistsInAccountingDocHeaderBefore = false,
                ExistsInAccountingDocHeaderAfter = false,
                Snapshot = snapshot,
                TriggerDate = today,
                DocNotSentError = notSentDry,
                Steps = steps,
                Message = "DryRun: هیچ UPDATE واقعی روی Income_Fiche انجام نشد. برای ارسال واقعی Tahator:DryRun/Rayvarz:DryRun=false"
            };
        }

        await ApplyTriggerStatus2Async(ficheNo, today, ct);
        steps.Add($"3) UPDATE وضعیت ۲ انجام شد (ExportPermanentDate/PaymentBreakDate={today}, PaymentDate='')");

        var appeared = await WaitForAccountingDocHeaderAsync(ficheNo, steps, ct);
        steps.Add(appeared
            ? "4) پس از وضعیت ۲: فیش در Accounting_DocHeader ظاهر شد"
            : $"4) پس از {PollTimeoutSeconds}s هنوز در Accounting_DocHeader نیست");

        await RestoreSnapshotStatus3Async(snapshot, ct);
        steps.Add("5) UPDATE بازگردانی وضعیت ۳ با اطلاعات SELECT اولیه انجام شد");

        var inHeaderAfter = await ExistsInAccountingDocHeaderAsync(ficheNo, ct);
        string? notSent = null;
        if (!inHeaderAfter)
        {
            notSent = await _fiches.GetDocNotSentErrorAsync(ficheNo, ct);
            steps.Add(string.IsNullOrWhiteSpace(notSent)
                ? "6) Accounting_DocNotSent: رکوردی یافت نشد"
                : $"6) Accounting_DocNotSent: {notSent}");
        }
        else
        {
            steps.Add("6) جدول واسط پر است — ارسال توسط مسیر Sara/رایورز انجام می‌شود");
        }

        var success = inHeaderAfter;
        return new TahatorSendResult
        {
            Success = success,
            FicheNo = ficheNo,
            DryRun = false,
            ExistsInAccountingDocHeaderBefore = false,
            ExistsInAccountingDocHeaderAfter = inHeaderAfter,
            Snapshot = snapshot,
            TriggerDate = today,
            DocNotSentError = notSent,
            Steps = steps,
            Message = success
                ? "تهاتر: فیش در جدول واسط ثبت شد."
                : string.IsNullOrWhiteSpace(notSent)
                    ? "تهاتر: پس از وضعیت ۲ فیش در واسط دیده نشد (علت در DocNotSent نبود)."
                    : $"تهاتر ناموفق — علت عدم ارسال: {notSent}"
        };
    }

    public async Task<bool> ExistsInAccountingDocHeaderAsync(string ficheNo, CancellationToken ct = default)
    {
        const string sql = @"SELECT TOP 1 1 FROM dbo.Accounting_DocHeader WHERE FicheNo = @f";
        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
        var result = await cmd.ExecuteScalarAsync(ct);
        return result != null;
    }

    public async Task<IncomeFicheTahatorSnapshot?> TryLoadSnapshotAsync(string ficheNo, CancellationToken ct = default)
    {
        const string sql = @"
SELECT FicheNo,
       EumFicheStatus,
       ExportPermanentDate,
       PaymentBreakDate,
       PaymentDate,
       UserConfirmDate,
       UsernameUserConfirm,
       NidUserUserConfirm
FROM dbo.Income_Fiche
WHERE FicheNo = @f";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return null;

        return new IncomeFicheTahatorSnapshot
        {
            FicheNo = reader.GetString(reader.GetOrdinal("FicheNo")),
            EumFicheStatus = ReadInt(reader, "EumFicheStatus"),
            ExportPermanentDate = ReadDateString(reader, "ExportPermanentDate"),
            PaymentBreakDate = ReadDateString(reader, "PaymentBreakDate"),
            PaymentDate = ReadDateString(reader, "PaymentDate"),
            UserConfirmDate = ReadDateString(reader, "UserConfirmDate"),
            UsernameUserConfirm = ReadNullableString(reader, "UsernameUserConfirm"),
            NidUserUserConfirm = ReadNullableGuid(reader, "NidUserUserConfirm")
        };
    }

    private async Task ApplyTriggerStatus2Async(string ficheNo, string todaySlash, CancellationToken ct)
    {
        const string sql = @"
UPDATE dbo.Income_Fiche
SET EumFicheStatus = 2,
    ExportPermanentDate = @today,
    PaymentBreakDate = @today,
    PaymentDate = ''
WHERE FicheNo = @f";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
        cmd.Parameters.AddWithValue("@today", todaySlash);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        if (n == 0)
            throw new InvalidOperationException($"UPDATE وضعیت ۲ برای فیش {ficheNo} هیچ ردیفی را تغییر نداد.");
        _logger.LogInformation("Tahator trigger status=2 FicheNo={FicheNo} Today={Today}", ficheNo, todaySlash);
    }

    private async Task RestoreSnapshotStatus3Async(IncomeFicheTahatorSnapshot snap, CancellationToken ct)
    {
        const string sql = @"
UPDATE dbo.Income_Fiche
SET EumFicheStatus = 3,
    ExportPermanentDate = @export,
    PaymentBreakDate = @brk,
    PaymentDate = @pay,
    UserConfirmDate = @ucDate,
    UsernameUserConfirm = @ucName,
    NidUserUserConfirm = @ucNid
WHERE FicheNo = @f";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", snap.FicheNo);
        cmd.Parameters.AddWithValue("@export", (object?)NullIfEmpty(snap.ExportPermanentDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@brk", (object?)NullIfEmpty(snap.PaymentBreakDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@pay", (object?)NullIfEmpty(snap.PaymentDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ucDate", (object?)NullIfEmpty(snap.UserConfirmDate) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ucName", (object?)NullIfEmpty(snap.UsernameUserConfirm) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ucNid", (object?)snap.NidUserUserConfirm ?? DBNull.Value);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        if (n == 0)
            throw new InvalidOperationException($"بازگردانی وضعیت ۳ برای فیش {snap.FicheNo} انجام نشد.");
        _logger.LogInformation("Tahator restore status=3 FicheNo={FicheNo}", snap.FicheNo);
    }

    private async Task<bool> WaitForAccountingDocHeaderAsync(string ficheNo, List<string> steps, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow.AddSeconds(PollTimeoutSeconds);
        var attempt = 0;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            if (await ExistsInAccountingDocHeaderAsync(ficheNo, ct))
                return true;

            attempt++;
            if (attempt == 1 || attempt % 5 == 0)
                steps.Add($"   … انتظار واسط (تلاش {attempt})");

            await Task.Delay(PollIntervalMs, ct);
        }

        return await ExistsInAccountingDocHeaderAsync(ficheNo, ct);
    }

    private static string NormalizeFicheNo(string ficheNo)
    {
        var f = (ficheNo ?? "").Trim();
        if (string.IsNullOrWhiteSpace(f))
            throw new ArgumentException("شماره فیش تهاتر الزامی است.");
        return f;
    }

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s;

    private static int ReadInt(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return 0;
        return Convert.ToInt32(reader.GetValue(ord));
    }

    private static string? ReadNullableString(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return null;
        return reader.GetValue(ord)?.ToString();
    }

    private static Guid? ReadNullableGuid(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return null;
        var v = reader.GetValue(ord);
        if (v is Guid g) return g;
        return Guid.TryParse(v?.ToString(), out var parsed) ? parsed : null;
    }

    private static string? ReadDateString(SqlDataReader reader, string column)
    {
        var ord = reader.GetOrdinal(column);
        if (reader.IsDBNull(ord)) return null;
        var value = reader.GetValue(ord);
        if (value is DateTime dt)
        {
            if (dt.Year is >= 1300 and <= 1500)
                return dt.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
            return DateHelper.ToShamsiSlashDate(DateHelper.FromDatabaseDateValue(dt));
        }

        var s = value.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) return null;
        return s.Contains('/') ? s : DateHelper.ToShamsiSlashDate(s);
    }
}
