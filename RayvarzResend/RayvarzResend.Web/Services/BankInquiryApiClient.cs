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
            return BankInquiryApiResult.Failed("پیکربندی سرویس استعلام بانک ناقص است (ServiceUrl / UserName / Password)");

        var envelope = BankInquiryRequestBuilder.BuildEnvelope(
            _options.UserName.Trim(),
            _options.Password,
            billId,
            payId,
            _options.BankCode);

        var urls = BuildServiceUrls();
        var maxAttempts = Math.Max(1, _options.RetryCount + 1);
        BankInquiryApiResult? lastResult = null;

        for (var urlIndex = 0; urlIndex < urls.Count; urlIndex++)
        {
            var serviceUrl = urls[urlIndex];
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var (statusCode, raw) = await PostJsonAsync(serviceUrl, envelope, ct);
                    _logger.LogInformation(
                        "Bank inquiry HTTP {Status} url={Url} attempt={Attempt}/{MaxAttempts} billId={BillId}",
                        statusCode, serviceUrl, attempt, maxAttempts, billId);

                    if (statusCode is 502 or 503 or 504)
                    {
                        _logger.LogWarning(
                            "Bank inquiry gateway error {Status} url={Url} attempt={Attempt} body={Body}",
                            statusCode, serviceUrl, attempt, Truncate(raw, 500));

                        lastResult = BankInquiryResponseParser.ParseHttpError(statusCode, raw);
                        if (attempt < maxAttempts && _options.RetryCount > 0)
                        {
                            await Task.Delay(_options.RetryDelayMs, ct);
                            continue;
                        }

                        if (urlIndex < urls.Count - 1)
                            break;

                        return lastResult;
                    }

                    var parsed = BankInquiryResponseParser.Parse(raw, statusCode);
                    parsed.RawResponse ??= raw;
                    return parsed;
                }
                catch (TaskCanceledException)
                {
                    return BankInquiryApiResult.Failed("زمان انتظار سرویس استعلام بانک به پایان رسید");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Bank inquiry call failed for billId={BillId} url={Url}", billId, serviceUrl);
                    lastResult = BankInquiryApiResult.Failed(BuildUserErrorMessage(ex));
                    if (attempt < maxAttempts && _options.RetryCount > 0)
                    {
                        await Task.Delay(_options.RetryDelayMs, ct);
                        continue;
                    }

                    if (urlIndex < urls.Count - 1)
                        break;

                    return lastResult;
                }
            }
        }

        return lastResult ?? BankInquiryApiResult.Failed("خطا در ارتباط با سرویس استعلام بانک");
    }

    private List<string> BuildServiceUrls()
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

        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Content = content;

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);
        return ((int)response.StatusCode, raw);
    }

    public static string BuildUserErrorMessage(Exception ex)
    {
        var msg = (ex.Message + " " + (ex.InnerException?.Message ?? "")).ToLowerInvariant();
        if (msg.Contains("ssl connection could not be established")
            || msg.Contains("forcibly closed")
            || msg.Contains("certificate")
            || msg.Contains("tls"))
        {
            return "خطا در ارتباط SSL با سرویس استعلام بانک. برنامه را از همان سرور/شبکه سازمان اجرا کنید؛ VPN را فعال کنید؛ "
                   + "در appsettings مقدار BankInquiryConfirm:UseSystemProxy=true یا ProxyUrl را تنظیم کنید؛ "
                   + "در صورت نیاز BankInquiryConfirm:AllowInvalidSsl=true (فقط برای تست).";
        }

        return $"خطا در ارتباط با سرویس استعلام بانک: {ex.Message}";
    }

    private static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text ?? "";
        return text[..maxLength] + "...";
    }
}
