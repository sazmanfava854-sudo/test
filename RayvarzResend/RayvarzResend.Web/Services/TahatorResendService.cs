using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ارسال تهاتر: خواندن از Sara + چک Accounting_DocHeader / incmdocsys
/// + ساخت/ارسال SOAP:
///   گروه ۱۵۷ / Tahator1 → مرکز Branch=102، DocTyp ۱۴/۱۵؛
///   گروه ۱۵۸ / Tahator → منطقه Branch=۲۰۱–۲۱۲، DocTyp ۱۷/۱۸.
/// تغییر وضعیت Income_Fiche پس از ارسال در Sara (تایید فیش دستی) انجام می‌شود — نه در این سرویس.
/// </summary>
public sealed class TahatorResendService
{
    private readonly string _saraCs;
    private readonly IConfiguration _config;
    private readonly ILogger<TahatorResendService> _logger;
    private readonly FicheRepository _fiches;
    private readonly RayvarzPayloadBuilder _payload;
    private readonly RayvarzClient _client;
    private readonly AccountingDocWriter _accountingDoc;

    public TahatorResendService(
        IConfiguration config,
        ILogger<TahatorResendService> logger,
        FicheRepository fiches,
        RayvarzPayloadBuilder payload,
        RayvarzClient client,
        AccountingDocWriter accountingDoc)
    {
        _config = config;
        _logger = logger;
        _fiches = fiches;
        _payload = payload;
        _client = client;
        _accountingDoc = accountingDoc;
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
        var rayvarzCheckWarnings = new List<string>();
        IncomeFicheTahatorSnapshot? inputSnapshot = null;

        foreach (var fiche in new[] { pair.AmountFiche!, pair.IncomeFiche! })
        {
            ApplyTahatorDocTyp(fiche);
            var no = fiche.FicheNo.Trim();
            if (string.Equals(no, ficheNo, StringComparison.Ordinal))
                inputSnapshot ??= await TryLoadIncomeFicheStateAsync(no, ct);

            var inHeader = await ExistsInAccountingDocHeaderAsync(no, ct);
            var inRayvarz = false;
            var rayvarzCheckFailed = false;
            try
            {
                inRayvarz = await TryExistsTahatorInRayvarzAsync(fiche, reqDates: null, ct);
            }
            catch (SqlException ex)
            {
                inRayvarz = false;
                rayvarzCheckFailed = true;
                rayvarzCheckWarnings.Add($"incmdocsys {no}: {ex.Message}");
            }

            var notSent = !inRayvarz && !rayvarzCheckFailed ? await TryGetDocNotSentAsync(no, ct) : null;
            var needs = TahatorSendPolicy.NeedsSend(inRayvarz, rayvarzCheckFailed);
            anyNeedsSend |= needs;
            allInHeader &= inHeader;
            allInRayvarz &= inRayvarz;

            members.Add(new TahatorPairMemberStatus
            {
                FicheNo = no,
                IncomeAccountGroup = fiche.IncomeAccountGroup ?? 0,
                DocTyp = fiche.DocTyp,
                Branch = TahatorRowBuilder.ResolveSendBranch(fiche, 0),
                Fund = fiche.SuggestedFund ?? (TahatorRowBuilder.IsTahatorIncomeFiche(fiche)
                    ? TahatorRowBuilder.ResolveTahatorIncomeFund(fiche.ResolvedDistrictBranch ?? 0)
                    : TahatorRowBuilder.ResolveTahatorFund(fiche.ResolvedDistrictBranch ?? 0)),
                ExistsInAccountingDocHeader = inHeader,
                ExistsInRayvarz = inRayvarz,
                NeedsSend = needs,
                DocNotSentError = notSent
            });
        }

        var primary = string.Equals(ficheNo, pair.AmountFiche!.FicheNo, StringComparison.Ordinal)
            ? pair.AmountFiche
            : string.Equals(ficheNo, pair.IncomeFiche!.FicheNo, StringComparison.Ordinal)
                ? pair.IncomeFiche
                : pair.AmountFiche;
        var rayvarzCount = members.Count(m => m.ExistsInRayvarz);
        var headerOnlyCount = members.Count(m => m.ExistsInAccountingDocHeader && !m.ExistsInRayvarz);
        var msg = allInRayvarz
            ? "هر دو فیش در رایورز (incmdocsys) هست — ارسال لازم نیست."
            : rayvarzCount == 1
                ? $"یکی از دو فیش جفت در رایورز است ({rayvarzCount}/۲) — فقط فیش دیگر ارسال می‌شود."
                : anyNeedsSend
                    ? headerOnlyCount > 0
                        ? $"آماده ارسال مجدد: {headerOnlyCount} فیش در واسط است ولی در رایورز نیست — ۱۵۷={pair.AmountFicheNo} سپس ۱۵۸={pair.IncomeFicheNo}."
                        : $"آماده ارسال جفت تهاتر: ۱۵۷={pair.AmountFicheNo} سپس ۱۵۸={pair.IncomeFicheNo}."
                    : "وضعیت جفت تهاتر نامشخص.";
        var warning = rayvarzCheckWarnings.Count > 0
            ? string.Join(" | ", rayvarzCheckWarnings)
            : null;
        if (!string.IsNullOrWhiteSpace(warning))
            msg += $" | ⚠ {warning}";

        return new TahatorCheckResult
        {
            FicheNo = ficheNo,
            ExistsInAccountingDocHeader = allInHeader,
            ExistsInIncomeFiche = true,
            ExistsInRayvarz = allInRayvarz,
            Snapshot = inputSnapshot,
            Fiche = primary,
            Pair = pair,
            PairMembers = members,
            DocNotSentError = members.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m.DocNotSentError))?.DocNotSentError,
            NeedsSend = anyNeedsSend,
            Warning = warning,
            Message = msg
        };
    }

    public async Task<TahatorSendResult> SendAsync(TahatorFicheRequest req, CancellationToken ct = default)
    {
        var steps = new List<string>();
        var dryRun = IsDryRun;
        var force = req.Force;
        var ficheNo = NormalizeFicheNo(req.FicheNo);
        steps.Add($"0) جفت تهاتر — FicheNo={ficheNo} | DryRun={dryRun} | force={force}");

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
        var toSend = new List<(FicheHeaderDto Fiche, IncomeFicheTahatorSnapshot State)>();

        try
        {
            foreach (var fiche in ordered)
            {
                var no = fiche.FicheNo.Trim();
                var inHeader = await ExistsInAccountingDocHeaderAsync(no, ct);
                bool inRayvarz;
                try
                {
                    inRayvarz = await TryExistsTahatorInRayvarzAsync(fiche, req, ct);
                }
                catch (SqlException ex)
                {
                    inRayvarz = false;
                    steps.Add($"⚠ incmdocsys {no}: {ex.Message}");
                }

                var shouldSend = TahatorSendPolicy.ShouldSendMember(force, inRayvarz);
                if (inHeader && !inRayvarz)
                    steps.Add($"⚠ {no}: در واسط (Accounting_DocHeader) هست ولی در رایورز نیست — ارسال مجدد");
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
                        Branch = TahatorRowBuilder.ResolveSendBranch(fiche, req.Branch),
                        Fund = ResolveFund(fiche, req.Fund),
                        Success = inHeader || inRayvarz,
                        Skipped = true,
                        SkipReason = "InRayvarz",
                        ExistsInAccountingDocHeaderAfter = inHeader,
                        ExistsInRayvarz = inRayvarz
                    });
                    continue;
                }

                if (fiche.Rows.Count == 0 || fiche.Payable <= 0)
                    return Fail(ficheNo, dryRun, steps, $"فیش {no} ردیف/مبلغ معتبر ندارد.");

                var state = await TryLoadIncomeFicheStateAsync(no, ct);
                if (state == null)
                    return Fail(ficheNo, dryRun, steps, $"فیش {no} در Income_Fiche یافت نشد.");

                toSend.Add((fiche, state));
            }

            if (toSend.Count == 0)
            {
                var allSkipped = ficheResults.All(r => r.Skipped);
                var detail = string.Join(" | ", ficheResults.Select(r =>
                    $"{r.FicheNo}:{r.SkipReason ?? (r.Success ? "OK" : "FAIL")}"));
                return new TahatorSendResult
                {
                    Success = allSkipped && ficheResults.All(r => r.Success),
                    Skipped = true,
                    FicheNo = ficheNo,
                    DryRun = dryRun,
                    Pair = pair,
                    FicheResults = ficheResults,
                    Steps = steps,
                    SkipReason = "AllInRayvarz",
                    Message = dryRun
                        ? $"ارسال نشد (DryRun): همه فیش‌ها در رایورز هستند — {detail}. برای تست واقعی: Rayvarz:DryRun=false و Restart."
                        : $"ارسال نشد: همه فیش‌ها در رایورز (incmdocsys) هستند — {detail}. force=true برای ارسال اجباری."
                };
            }

            var todayRay = DateHelper.CurrentShamsiRayvarzDate();
            var docDate = FirstDateOrToday(req.DocDate, todayRay);
            var actDate = FirstDateOrToday(req.ActDate, todayRay);
            var dueDate = FirstDateOrToday(req.DueDate, todayRay);
            steps.Add($"2) تاریخ SOAP: DocDate/ActDate/Due={docDate} (امروز مگر override)");

            for (var i = 0; i < toSend.Count; i++)
            {
                var (fiche, state) = toSend[i];
                var no = fiche.FicheNo.Trim();
                var branch = TahatorRowBuilder.ResolveSendBranch(fiche, req.Branch);
                var fund = ResolveFund(fiche, req.Fund);
                steps.Add(
                    $"3) SOAP {fiche.IncomeAccountGroup} FicheNo={no} Branch={branch} Fund={fund} " +
                    $"(Sara Status={state.EumFicheStatus} — بدون UPDATE)");

                var built = await _payload.BuildAsync(fiche, branch, fund, docDate, actDate, dueDate, ct);
                steps.Add($"   engine={built.Mode}, bytes={built.Xml.Length}");
                if (!string.IsNullOrWhiteSpace(built.Warning))
                    steps.Add($"   warning: {built.Warning}");

                var soapResult = await _client.SendAsync(built.Xml, dryRun, ct);
                steps.Add(dryRun
                    ? $"4) DryRun — SOAP {no} POST نشد"
                    : soapResult.Success
                        ? $"4) SOAP {no} OK — {soapResult.Message}"
                        : $"4) SOAP {no} FAIL — {soapResult.Message}");

                var inHeaderAfter = await ExistsInAccountingDocHeaderAsync(no, ct);
                var verifiedRay = false;
                if (!dryRun && soapResult.Success)
                {
                    try
                    {
                        verifiedRay = await TryExistsTahatorInRayvarzAsync(
                            fiche,
                            new TahatorFicheRequest
                            {
                                DocDate = docDate,
                                ActDate = actDate,
                                DueDate = dueDate
                            },
                            ct);
                    }
                    catch (SqlException ex)
                    {
                        steps.Add($"⚠ تأیید incmdocsys {no}: {ex.Message}");
                    }
                }

                var accountingMessage = (string?)null;
                if (!dryRun && soapResult.Success && verifiedRay && !inHeaderAfter)
                {
                    var accounting = await _accountingDoc.TryWriteAfterSendAsync(fiche, soapResult.PursuitDocNo, ct);
                    inHeaderAfter = accounting.Written || await ExistsInAccountingDocHeaderAsync(no, ct);
                    accountingMessage = accounting.Message;
                    steps.Add(accounting.Written
                        ? $"5) واسط Sara {no}: {accounting.Message}"
                        : accounting.WasSkipped
                            ? $"5) واسط Sara {no}: {accounting.Message}"
                            : $"5) ⚠ واسط Sara {no}: {accounting.Message}");
                }

                string? notSent = null;
                if (!dryRun && !inHeaderAfter && !verifiedRay)
                {
                    notSent = await TryGetDocNotSentAsync(no, ct);
                    if (!string.IsNullOrWhiteSpace(notSent))
                        steps.Add($"5) DocNotSent {no}: {notSent}");
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
                    DocNotSentError = notSent,
                    AccountingDocMessage = accountingMessage
                });

                if (!dryRun && !oneOk)
                {
                    steps.Add($"⚠ ارسال متوقف شد — {no} ناموفق؛ فیش‌های بعدی جفت ارسال نمی‌شوند.");
                    for (var j = i + 1; j < toSend.Count; j++)
                    {
                        var aborted = toSend[j].Fiche;
                        var abortedNo = aborted.FicheNo.Trim();
                        ficheResults.Add(new TahatorFicheSendDetail
                        {
                            FicheNo = abortedNo,
                            IncomeAccountGroup = aborted.IncomeAccountGroup ?? 0,
                            DocTyp = aborted.DocTyp,
                            Branch = aborted.ResolvedDistrictBranch is > 0
                                ? aborted.ResolvedDistrictBranch.Value
                                : TahatorRowBuilder.IsTahatorAmountFiche(aborted)
                                    ? TahatorRowBuilder.DefaultRayvarzBranch
                                    : 0,
                            Fund = ResolveFund(aborted, req.Fund),
                            Success = false,
                            Skipped = true,
                            SkipReason = "PairAborted",
                            SoapMessage = $"به‌دلیل شکست ارسال {no} ارسال نشد"
                        });
                    }

                    break;
                }
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
                Snapshot = toSend.FirstOrDefault().State,
                EngineName = primaryDetail != null ? "Active" : null,
                DocTyp = primaryDetail?.DocTyp ?? 0,
                Branch = primaryDetail?.Branch ?? 0,
                Fund = primaryDetail?.Fund ?? 0,
                PreviewXml = ficheResults.LastOrDefault(r => r.PreviewXml != null)?.PreviewXml,
                SoapMessage = string.Join(" | ", ficheResults.Select(r => $"{r.FicheNo}:{r.SoapMessage ?? r.SkipReason ?? (r.Success ? "OK" : "FAIL")}")),
                DocNotSentError = ficheResults.FirstOrDefault(r => !string.IsNullOrWhiteSpace(r.DocNotSentError))?.DocNotSentError,
                Steps = steps,
                Message = dryRun
                    ? "DryRun جفت تهاتر: SOAP ساخته شد؛ Sara تغییر نکرد."
                    : success
                        ? $"جفت تهاتر ارسال شد: ۱۵۷={pair.AmountFicheNo}، ۱۵۸={pair.IncomeFicheNo}. تایید وضعیت در Sara (تایید فیش دستی)."
                        : "ارسال جفت تهاتر ناموفق — جزئیات در ficheResults."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tahator pair send failed for {FicheNo}", ficheNo);
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

    public async Task<IncomeFicheTahatorSnapshot?> TryLoadIncomeFicheStateAsync(string ficheNo, CancellationToken ct = default)
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

    private async Task<bool> TryExistsTahatorInRayvarzAsync(
        FicheHeaderDto fiche,
        TahatorFicheRequest? reqDates,
        CancellationToken ct)
    {
        var years = RayvarzYearResolver.CollectCandidates(
            reqDates?.DocDate,
            reqDates?.ActDate,
            reqDates?.DueDate,
            fiche.RayvarzDocDate,
            fiche.RayvarzActDate,
            fiche.RayvarzDueDate);
        var isAmount = TahatorRowBuilder.IsTahatorAmountFiche(fiche);
        return await _fiches.ExistsTahatorDocumentInRayvarzRobustAsync(
            fiche.FicheNo.Trim(), isAmount, years, ct);
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
        if (s.Length == 0) return "";
        return s.Contains('/') ? s : DateHelper.ToShamsiSlashDate(s);
    }
}
