using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.Services;

public sealed class BankInquiryApiClient
{
    public const string HttpClientName = nameof(BankInquiryApiClient);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly BankInquiryConfirmOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BankInquiryApiClient> _logger;

    public BankInquiryApiClient(
        IOptions<BankInquiryConfirmOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<BankInquiryApiClient> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool IsConfigured => _options.IsConfigured;

    /// <summary>هر سرویس را یک‌بار (بدون fallback) فراخوانی می‌کند تا درخواست/پاسخ خام برای رفع مشکل دیده شود.</summary>
    public async Task<BankInquiryDiagnosticsResult> DiagnoseAsync(
        string billId,
        string payId,
        CancellationToken ct = default)
    {
        var result = new BankInquiryDiagnosticsResult
        {
            BillId = BankInquiryConfirmHelper.NormalizeBillOrPayId(billId),
            PayId = BankInquiryConfirmHelper.NormalizeBillOrPayId(payId),
            Configured = IsConfigured,
            UserName = _options.UserName.Trim(),
            PasswordConfigured = !string.IsNullOrWhiteSpace(_options.Password)
        };

        if (string.IsNullOrWhiteSpace(result.BillId) || string.IsNullOrWhiteSpace(result.PayId))
        {
            result.Error = "شناسه قبض و شناسه پرداخت الزامی است";
            return result;
        }

        if (!IsConfigured)
        {
            result.Error = "پیکربندی سرویس استعلام بانک ناقص است (UserName / Password / URL)";
            return result;
        }

        var userName = _options.UserName.Trim();
        var password = _options.Password;

        result.FicheLookup = await DiagnoseOneAsync(
            _options.EffectiveFicheLookupServiceUrl,
            BankInquiryRequestBuilder.BuildBillPayEnvelope(userName, password, result.BillId, result.PayId),
            BankInquiryResponseParser.ParseFicheLookupStep,
            BankInquiryResponseParser.FicheLookupSourceLabel,
            ct);

        result.OnlineBank = await DiagnoseOneAsync(
            _options.EffectiveServiceUrl,
            BankInquiryRequestBuilder.BuildEnvelope(userName, password, result.BillId, result.PayId, _options.BankCode),
            BankInquiryResponseParser.ParseOnlineBankStep,
            BankInquiryResponseParser.OnlineBankSourceLabel,
            ct);

        return result;
    }

    private async Task<BankInquiryDiagnosticsStep> DiagnoseOneAsync(
        string serviceUrl,
        object envelope,
        Func<string?, int, BankInquiryParsedStep> parser,
        string serviceLabel,
        CancellationToken ct)
    {
        var step = new BankInquiryDiagnosticsStep
        {
            Service = serviceLabel,
            Url = serviceUrl,
            RequestBody = RedactPassword(JsonSerializer.Serialize(envelope, JsonOptions))
        };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var (statusCode, raw) = await PostJsonAsync(serviceUrl, envelope, ct);
            step.HttpStatus = statusCode;
            step.RawResponse = Truncate(raw, 4000);
            var parsed = parser(raw, statusCode);
            step.Kind = parsed.Kind.ToString();
            step.Message = parsed.Message;
            step.PaymentDate = parsed.PaymentDate;
        }
        catch (TaskCanceledException)
        {
            step.Kind = BankInquiryStepKind.ServiceError.ToString();
            step.Message = $"زمان انتظار {serviceLabel} به پایان رسید";
        }
        catch (Exception ex)
        {
            step.Kind = BankInquiryStepKind.ServiceError.ToString();
            step.Message = BuildUserErrorMessage(ex, serviceLabel);
            step.Exception = ex.GetType().Name + ": " + ex.Message
                + (ex.InnerException != null ? " | " + ex.InnerException.Message : "");
        }
        finally
        {
            step.ElapsedMs = sw.ElapsedMilliseconds;
        }

        return step;
    }

    public async Task<BankInquiryApiResult> InquireAsync(
        string billId,
        string payId,
        CancellationToken ct = default)
    {
        billId = BankInquiryConfirmHelper.NormalizeBillOrPayId(billId);
        payId = BankInquiryConfirmHelper.NormalizeBillOrPayId(payId);

        if (string.IsNullOrWhiteSpace(billId) || string.IsNullOrWhiteSpace(payId))
            return BankInquiryApiResult.Failed("شناسه قبض و شناسه پرداخت برای استعلام بانک الزامی است");

        if (!IsConfigured)
            return BankInquiryApiResult.Failed(
                "پیکربندی سرویس استعلام بانک ناقص است (FicheLookupServiceUrl / ServiceUrl / UserName / Password)");

        var userName = _options.UserName.Trim();
        var password = _options.Password;

        var lookupStep = await CallWithJsonFormatFallbackAsync(
            usePascalCase => BankInquiryRequestBuilder.BuildBillPayEnvelope(userName, password, billId, payId, usePascalCase),
            _options.EffectiveFicheLookupServiceUrl,
            billId,
            BankInquiryResponseParser.ParseFicheLookupStep,
            BankInquiryResponseParser.FicheLookupSourceLabel,
            ct);

        switch (lookupStep.Kind)
        {
            case BankInquiryStepKind.Paid:
            case BankInquiryStepKind.NotPaid:
                return lookupStep.ToApiResult(BankInquiryResponseParser.FicheLookupSourceLabel);
            case BankInquiryStepKind.ServiceError:
                return lookupStep.ToApiResult(BankInquiryResponseParser.FicheLookupSourceLabel);
            case BankInquiryStepKind.RecordNotFound:
                _logger.LogInformation(
                    "Fiche lookup record not found for billId={BillId} payId={PayId}, trying online bank. message={Message}",
                    billId, payId, lookupStep.Message);
                break;
            default:
                return lookupStep.ToApiResult(BankInquiryResponseParser.FicheLookupSourceLabel);
        }

        var lookupNote = string.IsNullOrWhiteSpace(lookupStep.Message)
            ? "در استعلام قبوض ثبت نشد"
            : $"استعلام قبوض: {lookupStep.Message}";

        var onlineUrls = BuildOnlineServiceUrls();
        BankInquiryParsedStep? onlineStep = null;
        foreach (var serviceUrl in onlineUrls)
        {
            onlineStep = await CallWithJsonFormatFallbackAsync(
                usePascalCase => BankInquiryRequestBuilder.BuildEnvelope(
                    userName, password, billId, payId, _options.BankCode, usePascalCase),
                serviceUrl,
                billId,
                BankInquiryResponseParser.ParseOnlineBankStep,
                BankInquiryResponseParser.OnlineBankSourceLabel,
                ct);

            if (onlineStep.Kind == BankInquiryStepKind.Paid)
                break;

            if (onlineStep.Kind != BankInquiryStepKind.ServiceError || serviceUrl == onlineUrls[^1])
                break;
        }

        onlineStep ??= ServiceUnavailableStep(
            BankInquiryResponseParser.OnlineBankSourceLabel,
            "سرویس استعلام آنی بانک در دسترس نیست");

        return onlineStep.Kind switch
        {
            BankInquiryStepKind.Paid => WithLookupNote(
                onlineStep.ToApiResult(BankInquiryResponseParser.OnlineBankSourceLabel), lookupNote),
            BankInquiryStepKind.NotPaid when BankInquiryResponseParser.LooksLikeServiceFailure(onlineStep.Message) =>
                BankInquiryApiResult.Failed(
                    $"{lookupNote} → {onlineStep.Message}",
                    onlineStep.RawResponse,
                    BankInquiryResponseParser.OnlineBankSourceLabel),
            BankInquiryStepKind.ServiceError => BankInquiryApiResult.Failed(
                $"{lookupNote} → {onlineStep.Message}",
                onlineStep.RawResponse,
                BankInquiryResponseParser.OnlineBankSourceLabel),
            BankInquiryStepKind.RecordNotFound => BankInquiryApiResult.NotPaid(
                BankInquiryConfirmHelper.UnpaidFicheMessage,
                BankInquiryResponseParser.OnlineBankSourceLabel),
            BankInquiryStepKind.NotPaid => WithLookupNote(
                onlineStep.ToApiResult(BankInquiryResponseParser.OnlineBankSourceLabel), lookupNote),
            _ => WithLookupNote(
                onlineStep.ToApiResult(BankInquiryResponseParser.OnlineBankSourceLabel), lookupNote)
        };
    }

    private async Task<BankInquiryParsedStep> CallWithJsonFormatFallbackAsync(
        Func<bool, object> buildEnvelope,
        string serviceUrl,
        string billId,
        Func<string?, int, BankInquiryParsedStep> parser,
        string serviceLabel,
        CancellationToken ct)
    {
        BankInquiryParsedStep? lastStep = null;

        foreach (var usePascalCase in new[] { false, true })
        {
            var step = await CallAndParseAsync(
                serviceUrl,
                buildEnvelope(usePascalCase),
                billId,
                parser,
                serviceLabel,
                ct);
            lastStep = step;

            if (step.Kind is BankInquiryStepKind.Paid or BankInquiryStepKind.NotPaid)
                return step;

            if (!ShouldTryAlternateJsonFormat(step, usePascalCase))
                return step;
        }

        return lastStep ?? ServiceUnavailableStep(serviceLabel, $"خطا در ارتباط با {serviceLabel}");
    }

    private static bool ShouldTryAlternateJsonFormat(BankInquiryParsedStep step, bool usedPascalCase) =>
        !usedPascalCase
        && (step.Kind == BankInquiryStepKind.RecordNotFound
            || (step.Kind == BankInquiryStepKind.ServiceError
                && (step.Message.Contains("400", StringComparison.Ordinal)
                    || step.Message.Contains("PayId", StringComparison.OrdinalIgnoreCase)
                    || step.Message.Contains("نامعتبر", StringComparison.Ordinal))));

    private static BankInquiryApiResult WithLookupNote(BankInquiryApiResult result, string lookupNote)
    {
        if (string.IsNullOrWhiteSpace(lookupNote))
            return result;

        result.Message = $"{lookupNote} → {result.Message}";
        return result;
    }

    private async Task<BankInquiryParsedStep> CallAndParseAsync(
        string serviceUrl,
        object envelope,
        string billId,
        Func<string?, int, BankInquiryParsedStep> parser,
        string serviceLabel,
        CancellationToken ct)
    {
        var maxAttempts = Math.Max(1, _options.RetryCount + 1);
        BankInquiryParsedStep? lastStep = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var (statusCode, raw) = await PostJsonAsync(serviceUrl, envelope, ct);
                _logger.LogInformation(
                    "Bank inquiry HTTP {Status} service={Service} url={Url} attempt={Attempt}/{MaxAttempts} billId={BillId}",
                    statusCode, serviceLabel, serviceUrl, attempt, maxAttempts, billId);

                if (statusCode is 502 or 503 or 504)
                {
                    _logger.LogWarning(
                        "Bank inquiry gateway error {Status} service={Service} url={Url} attempt={Attempt} body={Body}",
                        statusCode, serviceLabel, serviceUrl, attempt, Truncate(raw, 500));

                    lastStep = new BankInquiryParsedStep
                    {
                        Kind = BankInquiryStepKind.ServiceError,
                        Message = BankInquiryResponseParser.ParseHttpError(statusCode, raw, serviceLabel).Message,
                        RawResponse = raw
                    };

                    if (attempt < maxAttempts && _options.RetryCount > 0)
                    {
                        await Task.Delay(_options.RetryDelayMs, ct);
                        continue;
                    }

                    return lastStep;
                }

                var parsed = parser(raw, statusCode);
                if (parsed.RawResponse == null)
                {
                    return new BankInquiryParsedStep
                    {
                        Kind = parsed.Kind,
                        PaymentDate = parsed.PaymentDate,
                        Message = parsed.Message,
                        RawResponse = raw
                    };
                }

                return parsed;
            }
            catch (TaskCanceledException)
            {
                return ServiceUnavailableStep(serviceLabel, $"زمان انتظار {serviceLabel} به پایان رسید");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Bank inquiry call failed service={Service} billId={BillId} url={Url}",
                    serviceLabel, billId, serviceUrl);

                lastStep = ServiceUnavailableStep(serviceLabel, BuildUserErrorMessage(ex, serviceLabel));
                if (attempt < maxAttempts && _options.RetryCount > 0)
                {
                    await Task.Delay(_options.RetryDelayMs, ct);
                    continue;
                }

                return lastStep;
            }
        }

        return lastStep ?? ServiceUnavailableStep(serviceLabel, $"خطا در ارتباط با {serviceLabel}");
    }

    private static BankInquiryParsedStep ServiceUnavailableStep(string serviceLabel, string message) => new()
    {
        Kind = BankInquiryStepKind.ServiceError,
        Message = message,
        RawResponse = null
    };

    private List<string> BuildOnlineServiceUrls()
    {
        var urls = new List<string>();
        var primary = _options.EffectiveServiceUrl;
        if (!string.IsNullOrWhiteSpace(primary))
            urls.Add(primary);
        var fallback = _options.EffectiveFallbackServiceUrl;
        if (!string.IsNullOrWhiteSpace(fallback)
            && !urls.Contains(fallback, StringComparer.OrdinalIgnoreCase))
            urls.Add(fallback);
        return urls;
    }

    private async Task<(int StatusCode, string Raw)> PostJsonAsync(
        string serviceUrl,
        object envelope,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        client.Timeout = TimeSpan.FromSeconds(Math.Max(10, _options.TimeoutSeconds));

        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        _logger.LogDebug("Bank inquiry request url={Url} body={Body}", serviceUrl, RedactPassword(json));

        using var request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", "RayvarzResend/FinancialAssistant");

        var content = new StringContent(json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), "application/json");
        request.Content = content;

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        return ((int)response.StatusCode, raw);
    }

    private static string RedactPassword(string json) =>
        System.Text.RegularExpressions.Regex.Replace(
            json,
            """"(password|Password)"\s*:\s*"[^"]*"""",
            """"$1":"***"""",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    public static string BuildUserErrorMessage(Exception ex, string? serviceLabel = null)
    {
        var serviceName = string.IsNullOrWhiteSpace(serviceLabel) ? "سرویس استعلام بانک" : serviceLabel.Trim();
        var msg = (ex.Message + " " + (ex.InnerException?.Message ?? "")).ToLowerInvariant();
        if (msg.Contains("ssl connection could not be established")
            || msg.Contains("forcibly closed")
            || msg.Contains("certificate")
            || msg.Contains("tls"))
        {
            return $"خطا در ارتباط SSL با {serviceName}. برنامه را از همان سرور/شبکه سازمان اجرا کنید؛ VPN را فعال کنید؛ "
                   + "در appsettings مقدار BankInquiryConfirm:UseSystemProxy=true یا ProxyUrl را تنظیم کنید؛ "
                   + "در صورت نیاز BankInquiryConfirm:AllowInvalidSsl=true (فقط برای تست).";
        }

        if (BankInquiryResponseParser.LooksLikeServiceFailure(ex.Message))
            return $"خطا در ارتباط با {serviceName}: {ex.Message}";

        return $"خطا در ارتباط با {serviceName}: {ex.Message}";
    }

    private static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text ?? "";
        return text[..maxLength] + "...";
    }
}
