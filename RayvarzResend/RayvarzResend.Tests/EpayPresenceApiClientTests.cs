using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class EpayPresenceApiClientTests
{
    [Fact]
    public async Task Presence_paid_or_unpaid_is_found()
    {
        var paid = await CheckAsync("""[{"intResult":0,"strResult":"یافت شد","billID":"1","payID":"2","isPay":1}]""");
        var unpaid = await CheckAsync("""[{"intResult":0,"strResult":"پرداخت نشده","billID":"1","payID":"2","isPay":0}]""");

        Assert.True(EpayFichePresenceChecker.FromLookupStep(paid).Exists);
        Assert.True(EpayFichePresenceChecker.FromLookupStep(unpaid).Exists);
    }

    [Fact]
    public async Task Presence_empty_array_is_not_found()
    {
        var step = await CheckAsync("[]");
        var result = EpayFichePresenceChecker.FromLookupStep(step);

        Assert.Equal(BankInquiryStepKind.RecordNotFound, step.Kind);
        Assert.False(result.Exists);
        Assert.Equal(EpayFichePresenceChecker.NotFoundMessage, result.UserMessage);
    }

    [Fact]
    public async Task Presence_http_error_is_service_error()
    {
        var step = await CheckAsync("bad gateway", HttpStatusCode.BadGateway);
        var result = EpayFichePresenceChecker.FromLookupStep(step);

        Assert.Equal(BankInquiryStepKind.ServiceError, step.Kind);
        Assert.False(result.Exists);
        Assert.Contains("502", result.UserMessage);
    }

    [Fact]
    public async Task EnsurePresentOrThrow_uses_operator_message()
    {
        var checker = CreateChecker("[]");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            checker.EnsurePresentOrThrowAsync("60510574", "123"));
        Assert.Equal(EpayFichePresenceChecker.NotFoundMessage, ex.Message);
    }

    private static Task<BankInquiryParsedStep> CheckAsync(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        var client = CreateClient(body, status);
        return client.CheckFichePresenceAsync("60510574", "123");
    }

    private static EpayFichePresenceChecker CreateChecker(string body)
    {
        return new EpayFichePresenceChecker(
            CreateClient(body),
            NullLogger<EpayFichePresenceChecker>.Instance);
    }

    private static BankInquiryApiClient CreateClient(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        var options = Options.Create(new BankInquiryConfirmOptions
        {
            UserName = "FinancialAssistant",
            Password = "secret",
            FicheLookupServiceUrl = "http://127.0.0.1/api/Proxy/epay_FindFichesByBillIDPayID",
            ServiceUrl = "http://127.0.0.1/api/Proxy/epay_EstelamOnLineBank",
            RetryCount = 0
        });
        return new BankInquiryApiClient(
            options,
            new StubFactory(new StubHandler(status, body)),
            NullLogger<BankInquiryApiClient>.Instance);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _body;

        public StubHandler(HttpStatusCode status, string body)
        {
            _status = status;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
    }

    private sealed class StubFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }
}
