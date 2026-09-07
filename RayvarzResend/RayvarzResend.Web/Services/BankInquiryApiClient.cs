using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace RayvarzResend.Web.Services;

public sealed class BankInquiryApiClient
{
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

        var payload = new
        {
            userName = _options.UserName.Trim(),
            password = _options.Password,
            billId,
            payId,
            bankCode = _options.BankCode
        };

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(BankInquiryApiClient));
            client.Timeout = TimeSpan.FromSeconds(60);

            using var response = await client.PostAsJsonAsync(_options.ServiceUrl.Trim(), payload, JsonOptions, ct);
            var raw = await response.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("Bank inquiry response HTTP {Status}: {Body}", (int)response.StatusCode, raw);

            var parsed = BankInquiryResponseParser.Parse(raw, (int)response.StatusCode);
            parsed.RawResponse ??= raw;
            return parsed;
        }
        catch (TaskCanceledException)
        {
            return BankInquiryApiResult.Failed("زمان انتظار سرویس استعلام بانک به پایان رسید");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bank inquiry call failed for billId={BillId}", billId);
            return BankInquiryApiResult.Failed($"خطا در ارتباط با سرویس استعلام بانک: {ex.Message}");
        }
    }
}
