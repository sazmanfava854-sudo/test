using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.RuleEngine.Sync;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی و ارسال تکی درآمد شهرسازی / نوسازی·صنفی — تهاتر از مسیر جدا.</summary>
public class FicheSendService
{
    private readonly FicheRepository _repo;
    private readonly RayvarzPayloadBuilder _payload;
    private readonly RayvarzClient _client;
    private readonly AccountingDocWriter _accountingDoc;
    private readonly IConfiguration _config;
    private readonly EpayFichePresenceChecker _epay;
    private readonly RuleSendGate _sendGate;

    public FicheSendService(
        FicheRepository repo,
        RayvarzPayloadBuilder payload,
        RayvarzClient client,
        AccountingDocWriter accountingDoc,
        IConfiguration config,
        EpayFichePresenceChecker epay,
        RuleSendGate sendGate)
    {
        _repo = repo;
        _payload = payload;
        _client = client;
        _accountingDoc = accountingDoc;
        _config = config;
        _epay = epay;
        _sendGate = sendGate;
    }

    public async Task<SendResultDto> SendAsync(SendFicheRequest req, CancellationToken ct = default)
    {
        await _sendGate.EnsureOpenAsync(ct);
        var fiche = req.Fiche;
        var blockReason = ValidateSendable(fiche);
        if (blockReason != null)
            throw new InvalidOperationException(blockReason);

        if (fiche.Payable <= 0)
            throw new InvalidOperationException("مبلغ قابل پرداخت صفر است — ارسال نشد");

        if (!fiche.Rows.Any(r => r.Val != 0))
            throw new InvalidOperationException("ردیف IncmNo یافت نشد — ارسال نشد");

        var incmdocsysYear = ResolveIncmdocsysYear(req);
        var shamsiYear = incmdocsysYear > 0 ? incmdocsysYear : (int?)null;

        string? sendWarning = null;
        var eligibility = await FicheSendEligibility.CheckAsync(_repo, fiche, fiche.FicheNo, shamsiYear, ct);
        if (eligibility.RayvarzQueryFailed && _config.GetValue("Rayvarz:RequireRayvarzDbForSend", true))
            throw new InvalidOperationException("اتصال SQL رایورز (Ray_CityHall) ناموفق — چک تکراری انجام نشد");

        if (eligibility.RayvarzQueryFailed)
            sendWarning = "چک تکراری در Ray_CityHall انجام نشد — ارسال SOAP ادامه یافت";

        var duplicateBlock = FicheSendEligibility.BuildBlockReason(eligibility);
        if (duplicateBlock != null)
            throw new InvalidOperationException(duplicateBlock);

        await _epay.EnsurePresentOrThrowAsync(fiche, ct);

        var built = await _payload.BuildAsync(fiche, req.Branch, req.Fund, req.DocDate, req.ActDate, req.DueDate, ct);
        var dryRun = _config.GetValue<bool>("Rayvarz:DryRun");
        var result = await _client.SendAsync(built.Xml, dryRun, ct);
        result.Warning = CombineWarnings(fiche.Warning, CombineWarnings(sendWarning, built.Warning));

        if (!dryRun && result.Success)
        {
            try
            {
                result.VerifiedInRayvarz = await _repo.ExistsInRayvarzAsync(
                    fiche.FicheNo, incmdocsysYear > 0 ? incmdocsysYear : null, ct);
            }
            catch (SqlException ex)
            {
                result.VerifiedInRayvarz = false;
                result.Warning = CombineWarnings(result.Warning,
                    $"تأیید incmdocsys ممکن نشد (SQL رایورز): {ex.Message}");
            }

            var years = new List<int>();
            if (incmdocsysYear > 0)
                years.Add(incmdocsysYear);
            years.Add(DateHelper.ExtractShamsiYear(req.DocDate));
            years.Add(DateHelper.ExtractShamsiYear(req.ActDate));
            years.Add(DateHelper.CurrentShamsiYear());
            var yearList = years.Where(y => y > 0).Distinct().ToList();

            var accounting = await _accountingDoc.TryWriteAfterSendAsync(
                fiche, result.PursuitDocNo, yearList, ct);
            result.AccountingDocWritten = accounting.Written;
            result.AccountingDocMessage = accounting.Message;
            if (!accounting.Written && !accounting.WasSkipped)
                result.Warning = CombineWarnings(result.Warning, $"واسط Sara: {accounting.Message}");

            if (!result.VerifiedInRayvarz)
            {
                try
                {
                    result.DocNotSentError = await _repo.GetDocNotSentErrorAsync(fiche.FicheNo, ct);
                }
                catch (SqlException ex)
                {
                    result.DocNotSentError = $"Accounting_DocNotSent (Sara): {ex.Message}";
                }

                result.Warning = CombineWarnings(result.Warning,
                    SendResultVerification.BuildUnverifiedWarning(result.Success, result.VerifiedInRayvarz, dryRun));
            }
        }

        return result;
    }

    public static string? ValidateSendable(FicheHeaderDto fiche)
    {
        var duplicateBlock = FicheSendEligibility.BuildBlockReason(new FicheSendEligibility.Result(
            fiche.ExistsInRayvarz,
            fiche.ExistsInAccountingDocHeader,
            false));
        if (duplicateBlock != null)
            return duplicateBlock;

        if (TahatorRowBuilder.IsTahatorFiche(fiche))
            return "فیش تهاتر — از مسیر تهاتر ارسال کنید";

        if (fiche.Payable <= 0)
            return "مبلغ قابل پرداخت صفر است";

        if (fiche.Rows.Count == 0)
            return "ردیف IncmNo یافت نشد";

        if (!TahatorRowBuilder.IsTahatorFiche(fiche)
            && !FicheBranchResolver.TryResolve(fiche, out _, out _, out var branchError))
            return branchError;

        return null;
    }

    /// <summary>وضعیت ارسال را روی DTO فیش می‌گذارد (بارگذاری تکی / نمایش UI).</summary>
    public static void ApplySendStatus(FicheHeaderDto fiche)
    {
        if (TahatorRowBuilder.IsTahatorFiche(fiche))
        {
            var duplicateBlock = FicheSendEligibility.BuildBlockReason(new FicheSendEligibility.Result(
                fiche.ExistsInRayvarz,
                fiche.ExistsInAccountingDocHeader,
                false));
            if (duplicateBlock != null)
            {
                fiche.CanSend = false;
                fiche.BlockReason = duplicateBlock;
                fiche.StatusMessage = fiche.ExistsInRayvarz && fiche.ExistsInAccountingDocHeader
                    ? "تکراری — در رایورز و Accounting_DocHeader"
                    : fiche.ExistsInRayvarz
                        ? "تکراری — در رایورز موجود است"
                        : "تکراری — در Accounting_DocHeader موجود است";
                return;
            }

            fiche.CanSend = true;
            fiche.BlockReason = null;
            fiche.StatusMessage = "تهاتر — آماده ارسال از مسیر تهاتر";
            return;
        }

        var blockReason = ValidateSendable(fiche);
        if (blockReason != null)
        {
            fiche.CanSend = false;
            fiche.BlockReason = blockReason;
            fiche.StatusMessage = blockReason;
            return;
        }

        fiche.CanSend = true;
        fiche.BlockReason = null;
        fiche.StatusMessage = "آماده ارسال";
    }

    private static int ResolveIncmdocsysYear(SendFicheRequest req)
    {
        foreach (var d in new[] { req.DocDate, req.ActDate, req.DueDate })
        {
            var y = DateHelper.ExtractShamsiYear(d);
            if (y > 0) return y;
        }

        foreach (var d in new[] { req.Fiche.RayvarzDocDate, req.Fiche.RayvarzActDate, req.Fiche.RayvarzDueDate })
        {
            var y = DateHelper.ExtractShamsiYear(d);
            if (y > 0) return y;
        }

        return 0;
    }

    private static string? CombineWarnings(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a)) return b;
        if (string.IsNullOrWhiteSpace(b)) return a;
        return a + " | " + b;
    }
}
