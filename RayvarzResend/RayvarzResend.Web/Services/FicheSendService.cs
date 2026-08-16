using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;

namespace RayvarzResend.Web.Services;

/// <summary>اعتبارسنجی ارسال و ارسال تکی — مسیر تهاتر جدا از ارسال عادی.</summary>
public class FicheSendService
{
    private readonly FicheRepository _repo;
    private readonly RayvarzPayloadBuilder _payload;
    private readonly RayvarzClient _client;
    private readonly IConfiguration _config;

    public FicheSendService(
        FicheRepository repo,
        RayvarzPayloadBuilder payload,
        RayvarzClient client,
        IConfiguration config)
    {
        _repo = repo;
        _payload = payload;
        _client = client;
        _config = config;
    }

    public async Task<SendResultDto> SendAsync(SendFicheRequest req, CancellationToken ct = default)
    {
        var fiche = req.Fiche;
        var blockReason = ValidateSendable(fiche);
        if (blockReason != null)
            throw new InvalidOperationException(blockReason);

        if (fiche.Payable <= 0)
            throw new InvalidOperationException("مبلغ قابل پرداخت صفر است — ارسال نشد");

        if (!fiche.Rows.Any(r => r.Val != 0))
            throw new InvalidOperationException("ردیف IncmNo یافت نشد — ارسال نشد");

        var incmdocsysYear = ResolveIncmdocsysYear(req);

        bool existsInRayvarz;
        string? sendWarning = null;
        try
        {
            existsInRayvarz = fiche.ExistsInRayvarz
                || await _repo.ExistsInRayvarzAsync(fiche.FicheNo, incmdocsysYear > 0 ? incmdocsysYear : null, ct);
        }
        catch (SqlException ex)
        {
            if (_config.GetValue("Rayvarz:RequireRayvarzDbForSend", true))
                throw new InvalidOperationException($"اتصال SQL رایورز (Ray_CityHall) ناموفق: {ex.Message}", ex);

            existsInRayvarz = fiche.ExistsInRayvarz;
            sendWarning = $"چک تکراری در Ray_CityHall انجام نشد — ارسال SOAP ادامه یافت: {ex.Message}";
        }

        if (existsInRayvarz)
            throw new InvalidOperationException("فیش در رایورز موجود است — ارسال نشد");

        if (req.ResetStatus)
            await _repo.ResetStatusAsync(fiche, ct);

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
        if (fiche.ExistsInRayvarz)
            return "فیش در رایورز موجود است — ارسال نشد";

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
            if (fiche.ExistsInRayvarz)
            {
                fiche.CanSend = false;
                fiche.BlockReason = "فیش در رایورز موجود است — ارسال نشد";
                fiche.StatusMessage = "تکراری — در رایورز موجود است";
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
