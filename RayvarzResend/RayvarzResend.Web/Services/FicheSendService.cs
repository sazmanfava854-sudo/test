using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.RuleEngine.Engines;
using RayvarzResend.Web.Validation;

namespace RayvarzResend.Web.Services;

/// <summary>
/// ساخت payload مالی (Preview) و ارسال SOAP (Send) — جدا از هم.
/// اگر خطای blocking وجود داشته باشد SOAP اصلاً POST نمی‌شود.
/// </summary>
public class FicheSendService
{
    private readonly FicheRepository _repo;
    private readonly RayvarzPayloadBuilder _payload;
    private readonly RayvarzClient _client;
    private readonly RayvarzSoapPayloadValidator _validator;
    private readonly RuleEngineFactory _engineFactory;
    private readonly IConfiguration _config;

    public FicheSendService(
        FicheRepository repo,
        RayvarzPayloadBuilder payload,
        RayvarzClient client,
        RayvarzSoapPayloadValidator validator,
        RuleEngineFactory engineFactory,
        IConfiguration config)
    {
        _repo = repo;
        _payload = payload;
        _client = client;
        _validator = validator;
        _engineFactory = engineFactory;
        _config = config;
    }

    /// <summary>Preview / DryRun — فقط تولید XML + اعتبارسنجی؛ بدون POST.</summary>
    public async Task<FichePreviewResultDto> PreviewAsync(SendFicheRequest req, CancellationToken ct = default)
    {
        try
        {
            var blockReason = LegacyValidateSendable(req.Fiche);
            if (blockReason != null)
            {
                return new FichePreviewResultDto
                {
                    Success = false,
                    ErrorMessage = blockReason,
                    CanSend = false,
                    Validation = new RayvarzValidationResultDto()
                };
            }

            var exists = await TryExistsInRayvarzAsync(req, ct);
            var built = await _payload.BuildAsync(
                req.Fiche, req.Branch, req.Fund, req.DocDate, req.ActDate, req.DueDate, ct);

            var validation = await ValidateBuiltPayloadAsync(req, built.Xml, exists, ct);

            return new FichePreviewResultDto
            {
                Success = true,
                Xml = built.Xml,
                CanSend = validation.CanSend,
                PayloadMode = built.Mode.ToString(),
                EngineName = built.EngineName,
                Warning = built.Warning,
                Validation = validation.ToDto()
            };
        }
        catch (Exception ex)
        {
            return new FichePreviewResultDto
            {
                Success = false,
                ErrorMessage = ex.Message,
                CanSend = false,
                Validation = new RayvarzValidationResultDto()
            };
        }
    }

    /// <summary>Send — فقط پس از Preview موفق و بدون blocking؛ در صورت خطای حیاتی SOAP POST نمی‌شود.</summary>
    public async Task<SendResultDto> SendAsync(SendFicheRequest req, CancellationToken ct = default)
    {
        var preview = await PreviewAsync(req, ct);
        if (!preview.Success)
            throw new InvalidOperationException(preview.ErrorMessage ?? "Preview ناموفق بود");

        if (!preview.CanSend)
        {
            var msg = preview.Validation.BlockingIssues.Count > 0
                ? string.Join("; ", preview.Validation.BlockingIssues.Select(i => $"[{i.Code}] {i.Message}"))
                : "اعتبارسنجی payload — ارسال مجاز نیست";
            throw new InvalidOperationException(msg);
        }

        var fiche = req.Fiche;
        if (req.ResetStatus)
            await _repo.ResetStatusAsync(fiche, ct);

        var dryRun = _config.GetValue<bool>("Rayvarz:DryRun");
        var result = await _client.SendAsync(preview.Xml, dryRun, ct);
        result.PreviewXml = preview.Xml;
        result.Validation = preview.Validation;
        result.Warning = CombineWarnings(preview.Warning, result.Warning);

        if (!dryRun && result.Success)
            await TryVerifyAfterSendAsync(req, result, ct);

        return result;
    }

    public static string? ValidateSendable(FicheHeaderDto fiche) =>
        LegacyValidateSendable(fiche);

    /// <summary>نگهداری سازگاری — منطق اصلی در RayvarzSoapPayloadValidator.</summary>
    public static string? LegacyValidateSendable(FicheHeaderDto fiche)
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

        var blockReason = LegacyValidateSendable(fiche);
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

    private async Task<RayvarzValidationResult> ValidateBuiltPayloadAsync(
        SendFicheRequest req,
        string soapXml,
        bool existsInRayvarz,
        CancellationToken ct)
    {
        IReadOnlyList<string> compatibilityWarnings = Array.Empty<string>();
        IReadOnlyList<string> preSoapErrors = Array.Empty<string>();

        try
        {
            var evaluated = await _engineFactory.EvaluateWithFallbackAsync(new FicheRuleContext
            {
                Fiche = req.Fiche,
                Branch = req.Branch,
                Fund = req.Fund,
                DocDate = req.DocDate,
                ActDate = req.ActDate,
                DueDate = req.DueDate
            }, buildSoap: false, ct);

            if (evaluated is { } ev && !ev.Success)
                preSoapErrors = [ev.ErrorMessage ?? "ارزیابی موتور ناموفق"];
        }
        catch
        {
            // Preview از XML ساخته‌شده استفاده می‌کند؛ خطای موتور در warning می‌آید
        }

        return _validator.Validate(new RayvarzValidationInput
        {
            Fiche = req.Fiche,
            SoapXml = soapXml,
            Branch = req.Branch,
            Fund = req.Fund,
            DocDate = req.DocDate,
            ActDate = req.ActDate,
            DueDate = req.DueDate,
            ExistsInRayvarz = existsInRayvarz,
            CompatibilityWarnings = compatibilityWarnings,
            PreSoapRuleErrors = preSoapErrors
        });
    }

    private async Task<bool> TryExistsInRayvarzAsync(SendFicheRequest req, CancellationToken ct)
    {
        var fiche = req.Fiche;
        if (fiche.ExistsInRayvarz)
            return true;

        var year = ResolveIncmdocsysYear(req);
        try
        {
            return await _repo.ExistsInRayvarzAsync(
                fiche.FicheNo, year > 0 ? year : null, ct);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            if (_config.GetValue<bool>("Rayvarz:RequireRayvarzDbForSend"))
                throw new InvalidOperationException($"اتصال SQL رایورز (Ray_CityHall) ناموفق: {ex.Message}", ex);
            return fiche.ExistsInRayvarz;
        }
    }

    private async Task TryVerifyAfterSendAsync(SendFicheRequest req, SendResultDto result, CancellationToken ct)
    {
        var year = ResolveIncmdocsysYear(req);
        try
        {
            result.VerifiedInRayvarz = await _repo.ExistsInRayvarzAsync(
                req.Fiche.FicheNo, year > 0 ? year : null, ct);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            result.VerifiedInRayvarz = false;
            result.Message = (result.Message ?? "") + $" | تأیید incmdocsys ممکن نشد (SQL رایورز): {ex.Message}";
        }

        if (!result.VerifiedInRayvarz)
        {
            try
            {
                result.DocNotSentError = await _repo.GetDocNotSentErrorAsync(req.Fiche.FicheNo, ct);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
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
