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

    public async Task<BankInquiryApiResult> InquireAsync(
        string billId,
        string payId,
        CancellationToken ct = default)
    {
        billId = (billId ?? "").Trim();
        payId = (payId ?? "").Trim();

        if (string.IsNullOrWhiteSpace(billId) || string.IsNullOrWhiteSpace(payId))
            return BankInquiryApiResult.Failed("شناسه قبض و شناسه پرداخت برای استعلام بانک الزامی است");

        if (!IsConfigured)
            return BankInquiryApiResult.Failed(
                "پیکربندی سرویس استعلام بانک ناقص است (FicheLookupServiceUrl / ServiceUrl / UserName / Password)");

        var userName = _options.UserName.Trim();
        var password = _options.Password;

        var lookupEnvelope = BankInquiryRequestBuilder.BuildBillPayEnvelope(userName, password, billId, payId);
        var lookupStep = await CallAndParseAsync(
            _options.FicheLookupServiceUrl.Trim(),
            lookupEnvelope,
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
                    "Fiche lookup record not found for billId={BillId}, trying online bank. message={Message}",
                    billId, lookupStep.Message);
                break;
            default:
                return lookupStep.ToApiResult(BankInquiryResponseParser.FicheLookupSourceLabel);
        }

        var lookupNote = string.IsNullOrWhiteSpace(lookupStep.Message)
            ? "در استعلام قبوض ثبت نشد"
            : $"استعلام قبوض: {lookupStep.Message}";

        var onlineEnvelope = BankInquiryRequestBuilder.BuildEnvelope(
            userName,
            password,
            billId,
            payId,
            _options.BankCode);

        var onlineUrls = BuildOnlineServiceUrls();
        BankInquiryParsedStep? onlineStep = null;
        foreach (var serviceUrl in onlineUrls)
        {
            onlineStep = await CallAndParseAsync(
                serviceUrl,
                onlineEnvelope,
                billId,
                BankInquiryResponseParser.ParseOnlineBankStep,
                BankInquiryResponseParser.OnlineBankSourceLabel,
                ct);

            if (onlineStep.Kind != BankInquiryStepKind.ServiceError || serviceUrl == onlineUrls[^1])
                break;
        }

        onlineStep ??= ServiceUnavailableStep(
            BankInquiryResponseParser.OnlineBankSourceLabel,
            "سرویس استعلام آنی بانک در دسترس نیست");

        return onlineStep.Kind switch
        {
            BankInquiryStepKind.RecordNotFound => BankInquiryApiResult.NotPaid(
                BankInquiryConfirmHelper.UnpaidFicheMessage,
                BankInquiryResponseParser.OnlineBankSourceLabel),
            BankInquiryStepKind.NotPaid => WithLookupNote(
                onlineStep.ToApiResult(BankInquiryResponseParser.OnlineBankSourceLabel), lookupNote),
            BankInquiryStepKind.Paid => WithLookupNote(
                onlineStep.ToApiResult(BankInquiryResponseParser.OnlineBankSourceLabel), lookupNote),
            _ => WithLookupNote(
                onlineStep.ToApiResult(BankInquiryResponseParser.OnlineBankSourceLabel), lookupNote)
        };
    }

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

                    lastStep = BankInquiryResponseParser.ParseHttpError(statusCode, raw, serviceLabel) switch
                    {
                        var failed => new BankInquiryParsedStep
                        {
                            Kind = BankInquiryStepKind.ServiceError,
                            Message = failed.Message,
                            RawResponse = raw
                        }
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
        if (!string.IsNullOrWhiteSpace(_options.ServiceUrl))
            urls.Add(_options.ServiceUrl.Trim());
        if (!string.IsNullOrWhiteSpace(_options.FallbackServiceUrl)
            && !urls.Contains(_options.FallbackServiceUrl.Trim(), StringComparer.OrdinalIgnoreCase))
            urls.Add(_options.FallbackServiceUrl.Trim());
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
        using var request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("User-Agent", "RayvarzResend/FinancialAssistant");

        var content = new StringContent(json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), "application/json");
        request.Content = content;

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        return ((int)response.StatusCode, raw);
    }

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

        return $"خطا در ارتباط با {serviceName}: {ex.Message}";
    }

    private static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text ?? "";
        return text[..maxLength] + "...";
    }
}
