using Microsoft.Data.SqlClient;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;

namespace RayvarzResend.Web.Services;

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
            if (_config.GetValue<bool>("Rayvarz:RequireRayvarzDbForSend"))
            {
                throw new InvalidOperationException($"اتصال SQL رایورز (Ray_CityHall) ناموفق: {ex.Message}", ex);
            }

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
        result.Warning = CombineWarnings(sendWarning, built.Warning);

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
                result.Message = (result.Message ?? "") + $" | تأیید incmdocsys ممکن نشد (SQL رایورز): {ex.Message}";
            }
        }

        if (!dryRun && !result.VerifiedInRayvarz)
        {
            try
            {
                result.DocNotSentError = await _repo.GetDocNotSentErrorAsync(fiche.FicheNo, ct);
            }
            catch (SqlException ex)
            {
                result.DocNotSentError = $"Accounting_DocNotSent (Sara): {ex.Message}";
            }

            if (result.Success)
            {
                result.Success = false;
                result.Message = string.IsNullOrWhiteSpace(result.Message)
                    ? "SOAP موفق گزارش شد ولی فیش در incmdocsys ثبت نشد"
                    : result.Message + " — ولی فیش در incmdocsys ثبت نشده";
            }
        }

        return result;
    }

    public static string? ValidateSendable(FicheHeaderDto fiche)
    {
        if (TahatorRowBuilder.IsTahatorFiche(fiche))
            return "فیش تهاتر — از تب تهاتر ارسال کنید";

        if (fiche.Payable <= 0)
            return "مبلغ قابل پرداخت صفر است";

        if (fiche.Rows.Count == 0)
            return "ردیف IncmNo یافت نشد";

        if (!TahatorRowBuilder.IsTahatorFiche(fiche)
            && !FicheBranchResolver.TryResolve(fiche, out _, out _, out var branchError))
            return branchError;

        return null;
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
