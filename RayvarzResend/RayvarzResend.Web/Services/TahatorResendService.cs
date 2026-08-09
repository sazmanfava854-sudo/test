using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ارسال تهاتر: چک Accounting_DocHeader + نگه‌داشت Income_Fiche + وضعیت ۲
/// + ساخت/ارسال SOAP:
///   گروه ۱۵۷ / Tahator1 → مرکز Branch=102، DocTyp ۱۴/۱۵؛
///   گروه ۱۵۸ / Tahator → منطقه Branch=۲۰۱–۲۱۲، DocTyp ۱۷/۱۸
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
        ficheNo = NormalizeFicheNo(ficheNo);
        var pair = await _fiches.ResolveTahatorPairAsync(ficheNo, ct);
        if (pair == null)
        {
            var lone = await _fiches.LoadAsync(IdentifierType.FicheNo, ficheNo, ct);
            return new TahatorCheckResult
            {
                FicheNo = ficheNo,
                ExistsInIncomeFiche = lone != null,
                Fiche = lone,
                NeedsSend = false,
                Message = lone == null
                    ? "فیش در Income_Fiche یافت نشد."
                    : "جفت تهاتر (۱۵۷ مبلغ + ۱۵۸ درآمد) کامل نیست — هر دو فیش با همان NidIncome لازم است."
            };
        }

        var members = new List<TahatorPairMemberStatus>();
        var anyNeedsSend = false;
        var allInHeader = true;
        var allInRayvarz = true;
        IncomeFicheTahatorSnapshot? inputSnapshot = null;
        IncomeFicheTahatorSnapshot? pendingStored = null;

        foreach (var fiche in new[] { pair.AmountFiche!, pair.IncomeFiche! })
        {
            ApplyTahatorDocTyp(fiche);
            var no = fiche.FicheNo.Trim();
            if (string.Equals(no, ficheNo, StringComparison.Ordinal))
                inputSnapshot ??= await TryLoadSnapshotAsync(no, ct);

            var inHeader = await ExistsInAccountingDocHeaderAsync(no, ct);
            var inRayvarz = false;
            try
            {
                inRayvarz = await _fiches.ExistsInRayvarzAsync(
                    no, DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate), ct);
            }
            catch (SqlException)
            {
                // optional
            }

            var notSent = inHeader ? null : await TryGetDocNotSentAsync(no, ct);
            var needs = !inHeader && !inRayvarz;
            anyNeedsSend |= needs;
            allInHeader &= inHeader;
            allInRayvarz &= inRayvarz;

            members.Add(new TahatorPairMemberStatus
            {
                FicheNo = no,
                IncomeAccountGroup = fiche.IncomeAccountGroup ?? 0,
                DocTyp = fiche.DocTyp,
                Branch = ResolveBranch(fiche, 0),
                Fund = fiche.SuggestedFund ?? (TahatorRowBuilder.IsTahatorIncomeFiche(fiche)
                    ? TahatorRowBuilder.ResolveTahatorIncomeFund(fiche.ResolvedDistrictBranch ?? 0)
                    : TahatorRowBuilder.ResolveTahatorFund(fiche.ResolvedDistrictBranch ?? 0)),
                ExistsInAccountingDocHeader = inHeader,
                ExistsInRayvarz = inRayvarz,
                NeedsSend = needs,
                DocNotSentError = notSent
            });
        }

        try
        {
            if (_snapshots.IsConfigured)
                pendingStored = await _snapshots.GetPendingAsync(ficheNo, ct)
                    ?? await _snapshots.GetPendingAsync(pair.AmountFicheNo, ct)
                    ?? await _snapshots.GetPendingAsync(pair.IncomeFicheNo, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "خواندن snapshot Pending تهاتر ناموفق");
        }

        var primary = string.Equals(ficheNo, pair.AmountFiche!.FicheNo, StringComparison.Ordinal)
            ? pair.AmountFiche
            : string.Equals(ficheNo, pair.IncomeFiche!.FicheNo, StringComparison.Ordinal)
                ? pair.IncomeFiche
                : pair.AmountFiche;
        var msg = allInHeader
            ? "هر دو فیش در Accounting_DocHeader هست — ارسال لازم نیست."
            : allInRayvarz
                ? "هر دو فیش در رایورز (incmdocsys) هست — ارسال لازم نیست."
                : anyNeedsSend
                    ? $"آماده ارسال جفت تهاتر: ۱۵۷={pair.AmountFicheNo} سپس ۱۵۸={pair.IncomeFicheNo}."
                    : "وضعیت جفت تهاتر نامشخص.";
        if (pendingStored != null)
            msg += $" | Snapshot Pending: Id={pendingStored.SnapshotId} — POST /api/tahator/restore";

        return new TahatorCheckResult
        {
            FicheNo = ficheNo,
            ExistsInAccountingDocHeader = allInHeader,
            ExistsInIncomeFiche = true,
            ExistsInRayvarz = allInRayvarz,
            Snapshot = inputSnapshot,
            PendingStoredSnapshot = pendingStored,
            Fiche = primary,
            Pair = pair,
            PairMembers = members,
            DocNotSentError = members.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.DocNotSentError))?.DocNotSentError,
            NeedsSend = anyNeedsSend,
            Message = msg
        };
    }

    public async Task<TahatorSendResult> SendAsync(TahatorFicheRequest req, CancellationToken ct = default)
    {
        var steps = new List<string>();
        var dryRun = IsDryRun;
        var force = req.Force || req.HoldAfterStatus2;
        var ficheNo = NormalizeFicheNo(req.FicheNo);
        steps.Add(
            $"0) جفت تهاتر — FicheNo={ficheNo} | DryRun={dryRun} | force={force}" +
            (req.HoldAfterStatus2 && !req.Force ? " (به‌خاطر holdAfterStatus2)" : "") +
            (req.HoldAfterStatus2 ? " | holdAfterStatus2=true" : ""));
        if (dryRun)
            steps.Add("⚠ DryRun=true — UPDATE وضعیت ۲ روی Sara زده نمی‌شود مگر DryRun=false + Restart.");

        var pair = await _fiches.ResolveTahatorPairAsync(ficheNo, ct);
        if (pair?.AmountFiche == null || pair.IncomeFiche == null)
        {
            return Fail(ficheNo, dryRun, steps,
                "جفت تهاتر (۱۵۷ مبلغ + ۱۵۸ درآمد) یافت نشد — هر دو فیش با همان NidIncome لازم است.");
        }

        steps.Add(
            $"0b) NidIncome={pair.NidIncome} | ۱۵۷={pair.AmountFicheNo} (Tahator1/مرکز) → ۱۵۸={pair.IncomeFicheNo} (Tahator/منطقه)");

        PrepareTahatorFiche(pair.AmountFiche);
        PrepareTahatorFiche(pair.IncomeFiche);

        var ordered = new[] { pair.AmountFiche, pair.IncomeFiche };
        var ficheResults = new List<TahatorFicheSendDetail>();
        var snapshots = new List<(FicheHeaderDto Fiche, IncomeFicheTahatorSnapshot Snap, long? SnapId)>();
        var statusChangedFiches = new List<string>();

        try
        {
            foreach (var fiche in ordered)
            {
                var no = fiche.FicheNo.Trim();
                var inHeader = await ExistsInAccountingDocHeaderAsync(no, ct);
                bool inRayvarz;
                try
                {
                    inRayvarz = await _fiches.ExistsInRayvarzAsync(
                        no, DateHelper.ExtractShamsiYear(fiche.RayvarzDocDate), ct);
                }
                catch (SqlException ex)
                {
                    inRayvarz = false;
                    steps.Add($"⚠ incmdocsys {no}: {ex.Message}");
                }

                var shouldSend = force || (!inHeader && !inRayvarz);
                steps.Add(
                    $"1) {fiche.IncomeAccountGroup} FicheNo={no} DocTyp={fiche.DocTyp} " +
                    $"Header={inHeader} Rayvarz={inRayvarz} → send={shouldSend}");

                if (!shouldSend)
                {
                    ficheResults.Add(new TahatorFicheSendDetail
                    {
                        FicheNo = no,
                        IncomeAccountGroup = fiche.IncomeAccountGroup ?? 0,
                        DocTyp = fiche.DocTyp,
                        Branch = ResolveBranch(fiche, req.Branch),
                        Fund = ResolveFund(fiche, req.Fund),
                        Success = inHeader || inRayvarz,
                        Skipped = true,
                        SkipReason = inHeader ? "InDocHeader" : "InRayvarz",
                        ExistsInAccountingDocHeaderAfter = inHeader,
                        ExistsInRayvarz = inRayvarz
                    });
                    continue;
                }

                if (fiche.Rows.Count == 0 || fiche.Payable <= 0)
                    return Fail(ficheNo, dryRun, steps, $"فیش {no} ردیف/مبلغ معتبر ندارد.");

                var snap = await TryLoadSnapshotAsync(no, ct);
                if (snap == null)
                    return Fail(ficheNo, dryRun, steps, $"Snapshot Income_Fiche برای {no} خوانده نشد.");

                snapshots.Add((fiche, snap, null));
            }

            if (snapshots.Count == 0)
            {
                var allSkipped = ficheResults.All(r => r.Skipped);
                return new TahatorSendResult
                {
                    Success = allSkipped && ficheResults.All(r => r.Success),
                    Skipped = true,
                    FicheNo = ficheNo,
                    DryRun = dryRun,
                    Pair = pair,
                    FicheResults = ficheResults,
                    Steps = steps,
                    SkipReason = "BothInDocHeaderOrRayvarz",
                    Message = "ارسال نشد: هر دو فیش قبلاً در واسط یا رایورز هستند. force=true برای ارسال مجدد."
                };
            }

            var today = DateHelper.CurrentShamsiSlashDate();
            var todayRay = DateHelper.CurrentShamsiRayvarzDate();
            var docDate = FirstDateOrToday(req.DocDate, todayRay);
            var actDate = FirstDateOrToday(req.ActDate, todayRay);
            var dueDate = FirstDateOrToday(req.DueDate, todayRay);
            steps.Add($"2) تاریخ SOAP: DocDate/ActDate/Due={docDate} (امروز مگر override)");

            if (dryRun && req.HoldAfterStatus2)
                return Fail(ficheNo, dryRun, steps,
                    "HoldAfterStatus2 با DryRun ممکن نیست — Rayvarz:DryRun=false + Restart.");

            if (!dryRun)
            {
                if (!_snapshots.IsConfigured)
                    return Fail(ficheNo, dryRun, steps,
                        "ConnectionStrings:RayvarzRuleEngine برای snapshot تهاتر تنظیم نشده.");

                for (var i = 0; i < snapshots.Count; i++)
                {
                    var (fiche, snap, _) = snapshots[i];
                    var no = fiche.FicheNo.Trim();
                    var snapId = await _snapshots.InsertPendingAsync(
                        snap, today, $"قبل از UPDATE وضعیت ۲ — ارسال تهاتر {fiche.IncomeAccountGroup}", ct);
                    snap.SnapshotId = snapId;
                    snap.PersistStatus = TahatorSnapshotStore.StatusPending;
                    snapshots[i] = (fiche, snap, snapId);
                    steps.Add($"3) Snapshot {no}: Id={snapId}");

                    await ApplyTriggerStatus2Async(no, today, ct);
                    statusChangedFiches.Add(no);
                    var after = await TryLoadSnapshotAsync(no, ct);
                    steps.Add(
                        $"4) UPDATE وضعیت ۲ {no}: Status={after?.EumFicheStatus}, " +
                        $"Export={after?.ExportPermanentDate}, Break={after?.PaymentBreakDate}, Pay='{after?.PaymentDate ?? ""}'");
                }
            }
            else
            {
                steps.Add("3-4) DryRun — UPDATE وضعیت ۲ روی Sara زده نشد.");
            }

            if (!dryRun && req.HoldAfterStatus2)
            {
                return new TahatorSendResult
                {
                    Success = true,
                    FicheNo = ficheNo,
                    DryRun = false,
                    Pair = pair,
                    Snapshot = snapshots[0].Snap,
                    SnapshotId = snapshots[0].SnapId,
                    TriggerDate = today,
                    Steps = steps,
                    Message =
                        $"وضعیت ۲ روی {snapshots.Count} فیش اعمال شد (Export/Break={today}). " +
                        "POST /api/tahator/restore برای بازگردانی."
                };
            }

            for (var i = 0; i < snapshots.Count; i++)
            {
                var (fiche, _, _) = snapshots[i];
                var no = fiche.FicheNo.Trim();
                var branch = ResolveBranch(fiche, req.Branch);
                var fund = ResolveFund(fiche, req.Fund);
                steps.Add($"5) SOAP {fiche.IncomeAccountGroup} FicheNo={no} Branch={branch} Fund={fund}");

                var built = await _payload.BuildAsync(fiche, branch, fund, docDate, actDate, dueDate, ct);
                steps.Add($"   engine={built.EngineName}, bytes={built.Xml.Length}");
                if (!string.IsNullOrWhiteSpace(built.Warning))
                    steps.Add($"   warning: {built.Warning}");

                var soapResult = await _client.SendAsync(built.Xml, dryRun, ct);
                steps.Add(dryRun
                    ? $"6) DryRun — SOAP {no} POST نشد"
                    : soapResult.Success
                        ? $"6) SOAP {no} OK — {soapResult.Message}"
                        : $"6) SOAP {no} FAIL — {soapResult.Message}");

                var inHeaderAfter = await ExistsInAccountingDocHeaderAsync(no, ct);
                var verifiedRay = false;
                if (!dryRun && soapResult.Success)
                {
                    try
                    {
                        verifiedRay = await _fiches.ExistsInRayvarzAsync(
                            no, DateHelper.ExtractShamsiYear(docDate), ct);
                    }
                    catch (SqlException ex)
                    {
                        steps.Add($"⚠ تأیید incmdocsys {no}: {ex.Message}");
                    }
                }

                string? notSent = null;
                if (!dryRun && !inHeaderAfter && !verifiedRay)
                {
                    notSent = await TryGetDocNotSentAsync(no, ct);
                    if (!string.IsNullOrWhiteSpace(notSent))
                        steps.Add($"7) DocNotSent {no}: {notSent}");
                }

                var oneOk = dryRun
                    ? soapResult.Success
                    : soapResult.Success && (inHeaderAfter || verifiedRay);

                ficheResults.Add(new TahatorFicheSendDetail
                {
                    FicheNo = no,
                    IncomeAccountGroup = fiche.IncomeAccountGroup ?? 0,
                    DocTyp = fiche.DocTyp,
                    Branch = branch,
                    Fund = fund,
                    Success = oneOk,
                    Skipped = false,
                    ExistsInAccountingDocHeaderAfter = inHeaderAfter,
                    ExistsInRayvarz = verifiedRay,
                    SoapMessage = soapResult.Message,
                    PursuitDocNo = soapResult.PursuitDocNo,
                    PreviewXml = built.Xml,
                    DocNotSentError = notSent
                });
            }

            if (!dryRun)
            {
                for (var i = 0; i < snapshots.Count; i++)
                {
                    var (_, snap, snapId) = snapshots[i];
                    if (snapId is not > 0) continue;
                    // پس از SOAP (موفق یا ناموفق): بازگردانی کامل از snapshot — شامل Export/Break
                    await RestoreFromStoredSnapshotAsync(snapId.Value, steps, ct);
                    statusChangedFiches.Remove(snap.FicheNo);
                }
            }
            else
            {
                steps.Add("8) DryRun — بازگردانی وضعیت ۳ شبیه‌سازی شد");
            }

            var primaryDetail = ficheResults.FirstOrDefault(r =>
                                    string.Equals(r.FicheNo, pair.AmountFicheNo, StringComparison.Ordinal))
                                ?? ficheResults.FirstOrDefault();
            var success = ficheResults.Count > 0 && ficheResults.All(r => r.Skipped || r.Success);

            return new TahatorSendResult
            {
                Success = success,
                Skipped = ficheResults.All(r => r.Skipped),
                FicheNo = ficheNo,
                DryRun = dryRun,
                Pair = pair,
                FicheResults = ficheResults,
                ExistsInAccountingDocHeaderAfter = ficheResults.All(r => r.ExistsInAccountingDocHeaderAfter || r.Skipped),
                ExistsInRayvarz = ficheResults.All(r => r.ExistsInRayvarz || r.Skipped),
                Snapshot = snapshots.FirstOrDefault().Snap,
                SnapshotId = snapshots.FirstOrDefault().SnapId,
                TriggerDate = today,
                EngineName = primaryDetail != null ? "Active" : null,
                DocTyp = primaryDetail?.DocTyp ?? 0,
                Branch = primaryDetail?.Branch ?? 0,
                Fund = primaryDetail?.Fund ?? 0,
                PreviewXml = ficheResults.LastOrDefault(r => r.PreviewXml != null)?.PreviewXml,
                SoapMessage = string.Join(" | ", ficheResults.Select(r => $"{r.FicheNo}:{r.SoapMessage ?? r.SkipReason ?? (r.Success ? "OK" : "FAIL")}")),
                DocNotSentError = ficheResults.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.DocNotSentError))?.DocNotSentError,
                Steps = steps,
                Message = dryRun
                    ? "DryRun جفت تهاتر: SOAP ساخته شد؛ UPDATE Sara زده نشد."
                    : success
                        ? $"جفت تهاتر ارسال شد: ۱۵۷={pair.AmountFicheNo}، ۱۵۸={pair.IncomeFicheNo}. تاریخ‌های Sara از snapshot اصلی بازگردانی شد."
                        : "ارسال جفت تهاتر ناموفق — جزئیات در ficheResults."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tahator pair send failed for {FicheNo}", ficheNo);
            foreach (var no in statusChangedFiches.ToList())
            {
                try
                {
                    var pending = await _snapshots.GetPendingAsync(no, ct);
                    if (pending != null)
                    {
                        await RestoreFromStoredSnapshotAsync(pending.SnapshotId, steps, ct);
                        steps.Add($"⚠ پس از خطا {no} از snapshot بازگردانی شد");
                    }
                }
                catch (Exception restoreEx)
                {
                    steps.Add($"⚠ بازگردانی {no} ناموفق: {restoreEx.Message}");
                }
            }

            return Fail(ficheNo, dryRun, steps, ex.Message);
        }
    }

    private static void PrepareTahatorFiche(FicheHeaderDto fiche)
    {
        if (!TahatorRowBuilder.IsTahatorFiche(fiche))
            TahatorRowBuilder.ApplyTahatorRows(fiche);
        else if (TahatorRowBuilder.IsTahatorAmountFiche(fiche)
                 && (fiche.Rows.Count != 1
                     || fiche.Rows[0].IncmNo is not (TahatorRowBuilder.IncmNoBank4 or TahatorRowBuilder.IncmNoOther)))
            TahatorRowBuilder.ApplyTahatorAmountRows(fiche);
        else if (TahatorRowBuilder.IsTahatorIncomeFiche(fiche))
            TahatorRowBuilder.ApplyTahatorIncomeRows(fiche);
        else
            ApplyTahatorDocTyp(fiche);
    }

    private static int ResolveFund(FicheHeaderDto fiche, int requestFund)
    {
        if (requestFund > 0) return requestFund;
        if (fiche.SuggestedFund is > 0) return fiche.SuggestedFund.Value;
        var district = fiche.ResolvedDistrictBranch ?? 0;
        return TahatorRowBuilder.IsTahatorIncomeFiche(fiche)
            ? TahatorRowBuilder.ResolveTahatorIncomeFund(district)
            : TahatorRowBuilder.ResolveTahatorFund(district);
    }

    /// <summary>بازگردانی دستی از snapshot Pending — هر دو فیش جفت در صورت وجود.</summary>
    public async Task<TahatorSendResult> RestorePendingAsync(string ficheNo, CancellationToken ct = default)
    {
        ficheNo = NormalizeFicheNo(ficheNo);
        var steps = new List<string>();
        if (!_snapshots.IsConfigured)
            return Fail(ficheNo, false, steps, "RayvarzRuleEngine تنظیم نشده.");

        var pair = await _fiches.ResolveTahatorPairAsync(ficheNo, ct);
        var ficheNos = pair != null
            ? new[] { pair.AmountFicheNo, pair.IncomeFicheNo }
            : new[] { ficheNo };

        var restored = 0;
        foreach (var no in ficheNos.Distinct(StringComparer.Ordinal))
        {
            var pending = await _snapshots.GetPendingAsync(no, ct);
            if (pending == null) continue;
            steps.Add($"SnapshotId={pending.SnapshotId} ({pending.FicheNo})");
            await RestoreFromStoredSnapshotAsync(pending.SnapshotId, steps, ct);
            restored++;
        }

        if (restored == 0)
            return Fail(ficheNo, false, steps, "Snapshot Pending برای این فیش/جفت یافت نشد.");

        return new TahatorSendResult
        {
            Success = true,
            FicheNo = ficheNo,
            Pair = pair,
            Steps = steps,
            Message = $"وضعیت {restored} فیش از snapshot به ۳ بازگردانی شد."
        };
    }

    public Task<IReadOnlyList<IncomeFicheTahatorSnapshot>> ListPendingAsync(CancellationToken ct = default) =>
        _snapshots.ListPendingAsync(50, ct);

    private async Task RestoreFromStoredSnapshotAsync(
        long snapshotId,
        List<string> steps,
        CancellationToken ct,
        string? keepExportBreakSlash = null)
    {
        var stored = await _snapshots.GetByIdAsync(snapshotId, ct)
            ?? throw new InvalidOperationException($"SnapshotId={snapshotId} یافت نشد.");

        await RestoreSnapshotStatus3Async(stored, ct, keepExportBreakSlash);
        await _snapshots.MarkRestoredAsync(snapshotId, "بازگردانی وضعیت ۳ روی Income_Fiche", ct);
        var export = keepExportBreakSlash ?? stored.ExportPermanentDate;
        var brk = keepExportBreakSlash ?? stored.PaymentBreakDate;
        steps.Add(
            $"7) وضعیت ۳ از SnapshotId={snapshotId}: " +
            $"Export={export}, Break={brk}" +
            (keepExportBreakSlash != null ? " (تاریخ روز — نگه داشته شد)" : " (مقادیر اصلی snapshot)") +
            $", PaymentDate='{stored.PaymentDate ?? ""}'");
    }

    /// <summary>DocTyp تهاتر — تفویض به <see cref="TahatorRowBuilder.ApplyTahatorDocTyp"/>.</summary>
    public static void ApplyTahatorDocTyp(FicheHeaderDto fiche) =>
        TahatorRowBuilder.ApplyTahatorDocTyp(fiche);

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
        // PaymentDate عمداً خالی می‌شود (تریگر واسط) — مقدار اصلی قبل از این در snapshot است
        const string sql = @"
UPDATE dbo.Income_Fiche
SET EumFicheStatus = 2,
    ExportPermanentDate = @today,
    PaymentBreakDate = @today,
    PaymentDate = @emptyPay
WHERE FicheNo = @f";

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", ficheNo);
        cmd.Parameters.AddWithValue("@today", todaySlash);
        cmd.Parameters.Add("@emptyPay", System.Data.SqlDbType.NVarChar, 30).Value = "";
        var n = await cmd.ExecuteNonQueryAsync(ct);
        if (n == 0)
            throw new InvalidOperationException($"UPDATE وضعیت ۲ برای فیش {ficheNo} هیچ ردیفی را تغییر نداد.");
        _logger.LogInformation("Tahator trigger status=2 FicheNo={FicheNo} Today={Today}", ficheNo, todaySlash);
    }

    private async Task RestoreSnapshotStatus3Async(
        IncomeFicheTahatorSnapshot snap,
        CancellationToken ct,
        string? keepExportBreakSlash = null)
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

        var export = keepExportBreakSlash ?? snap.ExportPermanentDate;
        var brk = keepExportBreakSlash ?? snap.PaymentBreakDate;

        await using var conn = new SqlConnection(_saraCs);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@f", snap.FicheNo);
        AddSlashDateParam(cmd, "@export", export);
        AddSlashDateParam(cmd, "@brk", brk);
        // PaymentDate اصلی (مثلاً 1404/12/11) باید عین همان برگردد — نه NULL
        AddSlashDateParam(cmd, "@pay", snap.PaymentDate);
        AddSlashDateParam(cmd, "@ucDate", snap.UserConfirmDate);
        cmd.Parameters.AddWithValue("@ucName", (object?)NullIfEmpty(snap.UsernameUserConfirm) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ucNid", (object?)snap.NidUserUserConfirm ?? DBNull.Value);
        var n = await cmd.ExecuteNonQueryAsync(ct);
        if (n == 0)
            throw new InvalidOperationException($"بازگردانی وضعیت ۳ برای فیش {snap.FicheNo} انجام نشد.");
        _logger.LogInformation(
            "Tahator restore status=3 FicheNo={FicheNo} Export={Export} Break={Break} PaymentDate={Pay}",
            snap.FicheNo, export ?? "", brk ?? "", snap.PaymentDate ?? "");
    }

    /// <summary>ستون‌های تاریخ شمسی Income_Fiche معمولاً NVARCHAR هستند — مقدار اسلش‌دار را عین خودش بنویس.</summary>
    private static void AddSlashDateParam(SqlCommand cmd, string name, string? value)
    {
        var p = cmd.Parameters.Add(name, System.Data.SqlDbType.NVarChar, 30);
        p.Value = string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
    }

    private static int ResolveBranch(FicheHeaderDto fiche, int requestBranch)
    {
        if (requestBranch > 0) return requestBranch;
        // درآمدی تهاتر (۱۵۸): ارسال به منطقه — Branch = DistrickBranch
        if (TahatorRowBuilder.IsTahatorIncomeFiche(fiche)
            && fiche.ResolvedDistrictBranch is > 0)
            return fiche.ResolvedDistrictBranch.Value;
        // مبلغ تهاتر (۱۵۷): ارسال به مرکز — Branch = ۱۰۲
        return TahatorRowBuilder.DefaultRayvarzBranch;
    }

    private static string FirstDateOrToday(string? fromReq, string todayRayvarz)
    {
        if (!string.IsNullOrWhiteSpace(fromReq)) return fromReq.Trim();
        return todayRayvarz;
    }

    private async Task<string?> TryGetDocNotSentAsync(string ficheNo, CancellationToken ct)
    {
        try
        {
            return await _fiches.GetDocNotSentErrorAsync(ficheNo, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Accounting_DocNotSent read failed for {FicheNo}", ficheNo);
            return $"Accounting_DocNotSent: {ex.Message}";
        }
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
            // بعضی ستون‌های Sara تاریخ شمسی را به‌صورت DateTime با سال ۱۳xx–۱۴xx نگه می‌دارند
            if (dt.Year is >= 1300 and <= 1500)
                return dt.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);
            return DateHelper.ToShamsiSlashDate(DateHelper.FromDatabaseDateValue(dt));
        }

        var s = value.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(s)) return null;
        // "" بعد از تریگر وضعیت ۲ — خالی معتبر است
        if (s.Length == 0) return "";
        return s.Contains('/') ? s : DateHelper.ToShamsiSlashDate(s);
    }
}
