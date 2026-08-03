using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ارسال تهاتر: چک Accounting_DocHeader + نگه‌داشت Income_Fiche + وضعیت ۲
/// + ساخت/ارسال SOAP (DocTyp 14/15 مثل تابع Tahator1 در Member 1388)
/// + بازگردانی وضعیت ۳ + بررسی واسط / DocNotSent / incmdocsys.
/// </summary>
public sealed class TahatorResendService
{
    private readonly string _saraCs;
    private readonly IConfiguration _config;
    private readonly ILogger<TahatorResendService> _logger;
    private readonly FicheRepository _fiches;
    private readonly RayvarzPayloadBuilder _payload;
    private readonly RayvarzClient _client;
    private readonly TahatorSnapshotStore _snapshots;

    public TahatorResendService(
        IConfiguration config,
        ILogger<TahatorResendService> logger,
        FicheRepository fiches,
        RayvarzPayloadBuilder payload,
        RayvarzClient client,
        TahatorSnapshotStore snapshots)
    {
        _config = config;
        _logger = logger;
        _fiches = fiches;
        _payload = payload;
        _client = client;
        _snapshots = snapshots;
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
        var (resolvedNo, fiche) = await ResolveIncomeFicheAsync(ficheNo, ct);
        ficheNo = resolvedNo;
        var inHeader = await ExistsInAccountingDocHeaderAnyAsync(ficheNo, ct);
        var snapshot = await TryLoadSnapshotAnyAsync(ficheNo, ct);
        if (fiche != null)
            ApplyTahatorDocTyp(fiche);

        var notSent = inHeader ? null : await _fiches.GetDocNotSentErrorAsync(ficheNo, ct);
        bool inRayvarz = false;
        try
        {
            if (fiche != null)
                inRayvarz = await ExistsInRayvarzAnyAsync(ficheNo, DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate), ct);
        }
        catch (SqlException)
        {
            // optional
        }

        IncomeFicheTahatorSnapshot? pendingStored = null;
        try
        {
            if (_snapshots.IsConfigured)
                pendingStored = await GetPendingSnapshotAnyAsync(ficheNo, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "خواندن snapshot Pending تهاتر ناموفق");
        }

        var msg = inHeader
            ? "فیش در جدول واسط Accounting_DocHeader هست — نیاز به ارسال نیست."
            : inRayvarz
                ? "فیش در رایورز (incmdocsys) هست — ارسال لازم نیست."
                : snapshot == null || fiche == null
                    ? "فیش درآمدی در Income_Fiche یافت نشد."
                    : string.IsNullOrWhiteSpace(notSent)
                        ? $"آماده ارسال SOAP تهاتر (DocTyp={fiche.DocTyp})."
                        : $"آماده ارسال SOAP. آخرین DocNotSent: {notSent}";
        if (pendingStored != null)
            msg += $" | Snapshot Pending ذخیره‌شده: Id={pendingStored.SnapshotId} — در صورت نیاز POST /api/tahator/restore";

        return new TahatorCheckResult
        {
            FicheNo = ficheNo,
            ExistsInAccountingDocHeader = inHeader,
            ExistsInIncomeFiche = snapshot != null,
            ExistsInRayvarz = inRayvarz,
            Snapshot = snapshot,
            PendingStoredSnapshot = pendingStored,
            Fiche = fiche,
            DocNotSentError = notSent,
            NeedsSend = !inHeader && !inRayvarz && snapshot != null && fiche != null,
            Message = msg
        };
    }

    public async Task<TahatorSendResult> SendAsync(TahatorFicheRequest req, CancellationToken ct = default)
    {
        var steps = new List<string>();
        var dryRun = IsDryRun;
        var statusChanged = false;
        IncomeFicheTahatorSnapshot? snapshot = null;

        var (ficheNo, fiche) = await ResolveIncomeFicheAsync(req.FicheNo, ct);
        steps.Add($"0) FicheNo ورودی={req.FicheNo?.Trim()} → Sara={ficheNo}");

        try
        {
            var inHeaderBefore = await ExistsInAccountingDocHeaderAnyAsync(ficheNo, ct);
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
                    Message = "فیش از قبل در جدول واسط است — SOAP ارسال نشد."
                };
            }

            if (fiche == null || fiche.Category != FicheCategory.Income)
            {
                return Fail(ficheNo, dryRun, steps,
                    $"فیش درآمدی برای تهاتر در Sara یافت نشد (ورودی={req.FicheNo?.Trim()}).");
            }

            // گروه 157 از Load قبلاً ردیف تهاتر + Centers دارد؛ در غیر این صورت اینجا بساز
            if (!TahatorRowBuilder.IsTahatorFiche(fiche)
                || fiche.Rows.Count != 1
                || fiche.Rows[0].IncmNo is not (TahatorRowBuilder.IncmNoBank4 or TahatorRowBuilder.IncmNoOther))
                TahatorRowBuilder.ApplyTahatorRows(fiche);
            else
                ApplyTahatorDocTyp(fiche);

            // FicheNo را از رکورد Sara نگه دار (مثلاً 040933/318150) — اسلش را حذف نکن
            ficheNo = fiche.FicheNo.Trim();

            steps.Add(
                $"2) فیش تهاتر — DocTyp={fiche.DocTyp} ({fiche.DocTypDsc}), IncmNo={fiche.Rows[0].IncmNo}, " +
                $"Val={fiche.Rows[0].Val}, Center={fiche.Center}, Center1={fiche.Rows[0].Center1}, Center3={fiche.Rows[0].Center3}");

            if (fiche.Rows.Count == 0 || fiche.Payable <= 0)
                return Fail(ficheNo, dryRun, steps, "فیش تهاتر ردیف/مبلغ معتبر ندارد.");

            bool inRayvarz;
            try
            {
                inRayvarz = await ExistsInRayvarzAnyAsync(
                    ficheNo, DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate), ct);
            }
            catch (SqlException ex)
            {
                inRayvarz = false;
                steps.Add($"⚠ چک incmdocsys ناموفق: {ex.Message}");
            }

            if (inRayvarz && !req.Force)
            {
                return new TahatorSendResult
                {
                    Success = true,
                    Skipped = true,
                    FicheNo = ficheNo,
                    DryRun = dryRun,
                    ExistsInRayvarz = true,
                    Steps = steps,
                    Message = "فیش در رایورز موجود است — SOAP ارسال نشد. برای تست وضعیت ۲: force=true بفرستید."
                };
            }

            if (inRayvarz && req.Force)
                steps.Add("2b) Force=true — وجود در رایورز نادیده گرفته شد (تست وضعیت ۲ / ارسال مجدد)");

            snapshot = await TryLoadSnapshotAnyAsync(ficheNo, ct);
            if (snapshot == null)
                return Fail(ficheNo, dryRun, steps, "Snapshot از Income_Fiche خوانده نشد.");

            steps.Add(
                $"3) SELECT نگه‌داشت اصلی: Status={snapshot.EumFicheStatus}, " +
                $"Export={snapshot.ExportPermanentDate}, Break={snapshot.PaymentBreakDate}, " +
                $"Pay={snapshot.PaymentDate}, UserConfirm={snapshot.UserConfirmDate}, " +
                $"User={snapshot.UsernameUserConfirm}");

            var today = DateHelper.CurrentShamsiSlashDate();
            var branch = ResolveBranch(fiche, req.Branch);
            var fund = req.Fund > 0
                ? req.Fund
                : fiche.SuggestedFund is > 0
                    ? fiche.SuggestedFund.Value
                    : TahatorRowBuilder.ResolveTahatorFund(fiche.ResolvedDistrictBranch ?? 0);
            var docDate = FirstDate(req.DocDate, fiche.RayvarzDocDate);
            var actDate = FirstDate(req.ActDate, fiche.RayvarzActDate);
            var dueDate = FirstDate(req.DueDate, fiche.RayvarzDueDate);

            // مرحله وضعیت ۲ نیاز به نوشتن روی Sara دارد — DryRun مانع می‌شود
            if (dryRun && req.HoldAfterStatus2)
            {
                return Fail(ficheNo, dryRun, steps,
                    "HoldAfterStatus2 با DryRun ممکن نیست — Rayvarz:DryRun=false بگذارید تا UPDATE تاریخ روز روی Income_Fiche زده شود.");
            }

            long? snapshotId = null;
            if (!dryRun)
            {
                if (!_snapshots.IsConfigured)
                    return Fail(ficheNo, dryRun, steps,
                        "ConnectionStrings:RayvarzRuleEngine برای ذخیره snapshot تهاتر تنظیم نشده.");

                snapshotId = await _snapshots.InsertPendingAsync(
                    snapshot, today, "قبل از UPDATE وضعیت ۲ — ارسال تهاتر", ct);
                snapshot.SnapshotId = snapshotId.Value;
                snapshot.PersistStatus = TahatorSnapshotStore.StatusPending;
                steps.Add($"3b) Snapshot در RayvarzRuleEngine ذخیره شد — SnapshotId={snapshotId}");

                await ApplyTriggerStatus2Async(ficheNo, today, ct);
                statusChanged = true;
                steps.Add(
                    $"4) UPDATE وضعیت ۲ روی Sara: EumFicheStatus=2, " +
                    $"ExportPermanentDate={today}, PaymentBreakDate={today}, PaymentDate='' " +
                    "(UserConfirm* دست نخورده). الان SELECT بزنید تا تاریخ روز را ببینید.");
            }
            else
            {
                steps.Add(
                    $"4) DryRun=true — UPDATE وضعیت ۲ زده نشد (تاریخ روز اعمال نمی‌شود). " +
                    $"برای تست مرحله ۳: DryRun=false و در صورت نیاز holdAfterStatus2=true");
            }

            // فقط نگه‌داشت + وضعیت ۲ — توقف برای مشاهده تاریخ روز در Sara
            if (!dryRun && req.HoldAfterStatus2 && snapshotId is > 0)
            {
                return new TahatorSendResult
                {
                    Success = true,
                    FicheNo = ficheNo,
                    DryRun = false,
                    ExistsInRayvarz = inRayvarz,
                    Snapshot = snapshot,
                    SnapshotId = snapshotId,
                    TriggerDate = today,
                    DocTyp = fiche.DocTyp,
                    Branch = branch,
                    Fund = fund,
                    Steps = steps,
                    Message =
                        $"وضعیت ۲ اعمال شد (تاریخ={today}). " +
                        "در Sara همان SELECT را بزنید؛ Export/Break باید تاریخ روز باشد. " +
                        "سپس POST /api/tahator/restore برای بازگردانی مقادیر اصلی."
                };
            }

            steps.Add($"5) ساخت SOAP تهاتر via DSL/PayloadBuilder (Branch={branch}, Fund={fund}, Engine=Active)");
            var built = await _payload.BuildAsync(fiche, branch, fund, docDate, actDate, dueDate, ct);
            steps.Add($"   engineName={built.EngineName}, payloadMode={built.Mode}, xmlBytes={built.Xml.Length}");
            if (!string.IsNullOrWhiteSpace(built.Warning))
                steps.Add($"   warning: {built.Warning}");

            var soapResult = await _client.SendAsync(built.Xml, dryRun, ct);
            steps.Add(dryRun
                ? "6) DryRun — SOAP به رایورز POST نشد (فقط XML ساخته شد)"
                : soapResult.Success
                    ? $"6) SOAP ارسال شد — {soapResult.Message}"
                    : $"6) SOAP ناموفق — {soapResult.Message}");

            if (statusChanged && snapshotId is > 0)
            {
                await RestoreFromStoredSnapshotAsync(snapshotId.Value, steps, ct);
                statusChanged = false;
            }
            else
            {
                steps.Add("7) DryRun — بازگردانی وضعیت ۳ شبیه‌سازی شد");
            }

            var inHeaderAfter = await ExistsInAccountingDocHeaderAnyAsync(ficheNo, ct);
            bool verifiedRay = false;
            if (!dryRun && soapResult.Success)
            {
                try
                {
                    verifiedRay = await ExistsInRayvarzAnyAsync(
                        ficheNo, DateHelper.ExtractShamsiYear(docDate), ct);
                }
                catch (SqlException ex)
                {
                    steps.Add($"⚠ تأیید incmdocsys: {ex.Message}");
                }
            }

            string? notSent = null;
            if (!dryRun && !inHeaderAfter && !verifiedRay)
            {
                notSent = await _fiches.GetDocNotSentErrorAsync(ficheNo, ct);
                steps.Add(string.IsNullOrWhiteSpace(notSent)
                    ? "8) Accounting_DocNotSent: رکوردی یافت نشد"
                    : $"8) Accounting_DocNotSent: {notSent}");
            }
            else if (inHeaderAfter)
            {
                steps.Add("8) Accounting_DocHeader پس از ارسال پر است");
            }
            else if (verifiedRay)
            {
                steps.Add("8) فیش در incmdocsys تأیید شد");
            }

            var success = dryRun
                ? soapResult.Success
                : soapResult.Success && (inHeaderAfter || verifiedRay);

            if (!dryRun && soapResult.Success && !success)
            {
                soapResult.Success = false;
                soapResult.Message = (soapResult.Message ?? "") + " — SOAP OK ولی در واسط/incmdocsys دیده نشد";
            }

            return new TahatorSendResult
            {
                Success = success,
                FicheNo = ficheNo,
                DryRun = dryRun,
                ExistsInAccountingDocHeaderBefore = false,
                ExistsInAccountingDocHeaderAfter = inHeaderAfter,
                ExistsInRayvarz = verifiedRay,
                Snapshot = snapshot,
                SnapshotId = snapshot?.SnapshotId > 0 ? snapshot.SnapshotId : null,
                TriggerDate = today,
                DocNotSentError = notSent,
                EngineName = built.EngineName,
                DocTyp = fiche.DocTyp,
                Branch = branch,
                Fund = fund,
                PreviewXml = built.Xml,
                SoapResponse = soapResult.SoapResponse,
                PursuitDocNo = soapResult.PursuitDocNo,
                SoapMessage = soapResult.Message,
                Steps = steps,
                Message = dryRun
                    ? "DryRun تهاتر: SOAP ساخته شد؛ برای ارسال واقعی DryRun را false کنید."
                    : success
                        ? "تهاتر: SOAP ارسال و در واسط/رایورز تأیید شد."
                        : string.IsNullOrWhiteSpace(notSent)
                            ? (soapResult.Message ?? "تهاتر ناموفق")
                            : $"تهاتر ناموفق — علت عدم ارسال: {notSent}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tahator send failed for {FicheNo}", ficheNo);
            if (statusChanged)
            {
                try
                {
                    var pending = snapshot?.SnapshotId > 0
                        ? await _snapshots.GetByIdAsync(snapshot.SnapshotId, ct)
                        : await _snapshots.GetPendingAsync(ficheNo, ct);
                    if (pending != null)
                    {
                        await RestoreFromStoredSnapshotAsync(pending.SnapshotId, steps, ct);
                        steps.Add("⚠ پس از خطا وضعیت از snapshot ذخیره‌شده بازگردانی شد");
                    }
                    else if (snapshot != null)
                    {
                        await RestoreSnapshotStatus3Async(snapshot, ct);
                        steps.Add("⚠ پس از خطا وضعیت ۳ از حافظه بازگردانی شد (snapshot DB نبود)");
                    }
                }
                catch (Exception restoreEx)
                {
                    steps.Add($"⚠ بازگردانی وضعیت ۳ ناموفق: {restoreEx.Message}");
                }
            }

            return Fail(ficheNo, dryRun, steps, ex.Message);
        }
    }

    /// <summary>بازگردانی دستی از snapshot Pending ذخیره‌شده (بعد از قطع فرایند).</summary>
    public async Task<TahatorSendResult> RestorePendingAsync(string ficheNo, CancellationToken ct = default)
    {
        var steps = new List<string>();
        if (!_snapshots.IsConfigured)
            return Fail(NormalizeFicheNo(ficheNo), false, steps, "RayvarzRuleEngine تنظیم نشده.");

        var pending = await GetPendingSnapshotAnyAsync(ficheNo, ct);
        if (pending == null)
            return Fail(NormalizeFicheNo(ficheNo), false, steps, "Snapshot Pending برای این فیش یافت نشد.");

        ficheNo = pending.FicheNo;
        steps.Add($"SnapshotId={pending.SnapshotId} از RayvarzRuleEngine خوانده شد (FicheNo={ficheNo})");
        await RestoreFromStoredSnapshotAsync(pending.SnapshotId, steps, ct);
        return new TahatorSendResult
        {
            Success = true,
            FicheNo = ficheNo,
            SnapshotId = pending.SnapshotId,
            Snapshot = pending,
            Steps = steps,
            Message = $"وضعیت فیش از SnapshotId={pending.SnapshotId} به ۳ بازگردانی شد."
        };
    }

    public Task<IReadOnlyList<IncomeFicheTahatorSnapshot>> ListPendingAsync(CancellationToken ct = default) =>
        _snapshots.ListPendingAsync(50, ct);

    private async Task RestoreFromStoredSnapshotAsync(long snapshotId, List<string> steps, CancellationToken ct)
    {
        var stored = await _snapshots.GetByIdAsync(snapshotId, ct)
            ?? throw new InvalidOperationException($"SnapshotId={snapshotId} یافت نشد.");

        await RestoreSnapshotStatus3Async(stored, ct);
        await _snapshots.MarkRestoredAsync(snapshotId, "بازگردانی وضعیت ۳ روی Income_Fiche", ct);
        steps.Add($"7) UPDATE بازگردانی وضعیت ۳ از SnapshotId={snapshotId} (RayvarzRuleEngine) انجام شد");
    }

    /// <summary>مطابق Tahator1 در XmlBody: CI_Bank=4 → DocTyp 14 وگرنه 15.</summary>
    public static void ApplyTahatorDocTyp(FicheHeaderDto fiche)
    {
        var bank = (fiche.BankCode ?? "").Trim();
        fiche.DocTyp = bank == "4" ? 14 : 15;
        fiche.DocDsc = "اسناد تهاتر مبلغ";
        fiche.DocTypDsc = "تهاتر مبلغ";
        fiche.Category = FicheCategory.Income;
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

    private static int ResolveBranch(FicheHeaderDto fiche, int requestBranch)
    {
        // اگر UI صریحاً branch بدهد همان را بفرست؛ وگرنه مثل نمونه‌های رایورز = ۱۰۲
        if (requestBranch > 0) return requestBranch;
        return TahatorRowBuilder.DefaultRayvarzBranch;
    }

    private static string FirstDate(string? fromReq, string? fromFiche)
    {
        if (!string.IsNullOrWhiteSpace(fromReq)) return fromReq.Trim();
        if (!string.IsNullOrWhiteSpace(fromFiche)) return fromFiche.Trim();
        return DateHelper.CurrentShamsiRayvarzDate();
    }

    private static TahatorSendResult Fail(string ficheNo, bool dryRun, List<string> steps, string message) =>
        new()
        {
            Success = false,
            FicheNo = ficheNo,
            DryRun = dryRun,
            Steps = steps,
            Message = message
        };

    private static string NormalizeFicheNo(string ficheNo) =>
        TahatorRowBuilder.NormalizeFicheNo(ficheNo);

    /// <summary>جستجو با واریانت اسلش‌دار / بدون اسلش؛ FicheNo برگشتی همان مقدار ستون Sara است.</summary>
    private async Task<(string FicheNo, FicheHeaderDto? Fiche)> ResolveIncomeFicheAsync(
        string? input, CancellationToken ct)
    {
        string? last = null;
        foreach (var v in TahatorRowBuilder.FicheNoLookupVariants(input))
        {
            last = v;
            var fiche = await _fiches.LoadAsync(IdentifierType.FicheNo, v, ct);
            if (fiche != null && fiche.Category == FicheCategory.Income)
                return (fiche.FicheNo.Trim(), fiche);
        }

        return (last ?? (string.IsNullOrWhiteSpace(input) ? "" : NormalizeFicheNo(input!)), null);
    }

    private async Task<bool> ExistsInAccountingDocHeaderAnyAsync(string ficheNo, CancellationToken ct)
    {
        foreach (var v in TahatorRowBuilder.FicheNoLookupVariants(ficheNo))
        {
            if (await ExistsInAccountingDocHeaderAsync(v, ct))
                return true;
        }

        return false;
    }

    private async Task<bool> ExistsInRayvarzAnyAsync(string ficheNo, int year, CancellationToken ct)
    {
        foreach (var v in TahatorRowBuilder.FicheNoLookupVariants(ficheNo))
        {
            if (await _fiches.ExistsInRayvarzAsync(v, year, ct))
                return true;
        }

        return false;
    }

    private async Task<IncomeFicheTahatorSnapshot?> TryLoadSnapshotAnyAsync(string ficheNo, CancellationToken ct)
    {
        foreach (var v in TahatorRowBuilder.FicheNoLookupVariants(ficheNo))
        {
            var snap = await TryLoadSnapshotAsync(v, ct);
            if (snap != null)
                return snap;
        }

        return null;
    }

    private async Task<IncomeFicheTahatorSnapshot?> GetPendingSnapshotAnyAsync(string ficheNo, CancellationToken ct)
    {
        foreach (var v in TahatorRowBuilder.FicheNoLookupVariants(ficheNo))
        {
            var pending = await _snapshots.GetPendingAsync(v, ct);
            if (pending != null)
                return pending;
        }

        return null;
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
