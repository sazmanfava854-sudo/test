using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class RuleEngineBridgeStubTests
{
    [Fact]
    public void SaraBridgeBuildRequest_matches_contract_fields()
    {
        var req = new SaraBridgeBuildRequest
        {
            NidMember = 1388,
            NidFiche = Guid.NewGuid(),
            FicheNo = "050933509456",
            Category = "Income",
            Branch = 209,
            Fund = 200209008
        };
        Assert.Equal(1388, req.NidMember);
        Assert.Equal("050933509456", req.FicheNo);
    }

    [Fact]
    public void Local_stub_builds_soap_from_in_memory_fiche()
    {
        var config = StubConfig();
        var stub = new SaraBridgeStubService(
            new FicheRepository(config), new SoapBuilder(config), config);

        var result = stub.BuildFromFiche(SampleIncomeFiche(), new SaraBridgeBuildRequest
        {
            FicheNo = "050933509456",
            Branch = 209,
            Fund = 200209008
        });

        Assert.Null(result.Error);
        Assert.NotNull(result.SoapXml);
        Assert.Contains("<SaveDocument", result.SoapXml);
        Assert.Contains("<branch>209</branch>", result.SoapXml);
        Assert.Contains("050933509456", result.SoapXml);
        Assert.Equal("LocalStub/LegacyCSharp", result.Source);
    }

    [Fact]
    public async Task PayloadBuilder_uses_local_stub_when_RuleEngineBridge_and_UseLocalBridgeStub()
    {
        var config = StubConfig(ruleEngineBridge: true, localStub: true);
        var soap = new SoapBuilder(config);
        var stub = new SaraBridgeStubService(new FicheRepository(config), soap, config);
        var builder = new RayvarzPayloadBuilder(
            config, soap, new MemberRuleRepository(config), new StubHttpClientFactory(), stub);

        var built = await builder.BuildAsync(
            SampleIncomeFiche(), 209, 200209008, "14050323", "14050323", "14050323");

        Assert.Equal(RayvarzPayloadSourceMode.RuleEngineBridge, built.Mode);
        Assert.Contains("<SaveDocument", built.Xml);
        Assert.Contains("050933509456", built.Xml);
    }

    [Fact]
    public async Task PayloadBuilder_falls_back_with_warning_when_bridge_url_missing_and_stub_disabled()
    {
        var config = StubConfig(ruleEngineBridge: true, localStub: false);
        var soap = new SoapBuilder(config);
        var builder = new RayvarzPayloadBuilder(
            config, soap, new MemberRuleRepository(config), new StubHttpClientFactory());

        var built = await builder.BuildAsync(
            SampleIncomeFiche(), 209, 200209008, "14050323", "14050323", "14050323");

        Assert.Equal(RayvarzPayloadSourceMode.LegacyCSharp, built.Mode);
        Assert.Contains("SaraBridgeUrl", built.Warning, StringComparison.Ordinal);
    }

    [Fact]
    public void Bridge_health_contract_documented_in_Program()
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "Program.cs"));
        var cs = File.ReadAllText(path);
        Assert.Contains("/api/rule/bridge/health", cs);
        Assert.Contains("/api/rule/bridge/build-save-document", cs);
    }

    private static IConfiguration StubConfig(bool ruleEngineBridge = false, bool localStub = false) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Sara"] = "Server=.;Database=stub;",
                ["ConnectionStrings:Rayvarz"] = "Server=.;Database=stub;",
                ["Rayvarz:PayloadSource"] = ruleEngineBridge ? "RuleEngineBridge" : "LegacyCSharp",
                ["RuleEngine:UseLocalBridgeStub"] = localStub ? "true" : "false",
                ["RuleEngine:NidMemberRayvarzRun"] = "1388",
                ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
                ["Rayvarz:RefRowDocNoInDetail"] = "zero"
            })
            .Build();

    private static FicheHeaderDto SampleIncomeFiche() => new()
    {
        Category = FicheCategory.Income,
        FicheNo = "050933509456",
        BillId = "9001910151966",
        PaymentId = "0013372932519",
        Payable = 133_729_000m,
        BnkAcntNo = "9-8-72-47-1-0-2",
        BankCode = "18",
        PaymentBranch = "18",
        ResolvedDistrictBranch = 209,
        SuggestedFund = 200209008,
        DocTyp = 3,
        DocDsc = "اسناد شهرسازی",
        Rows = { new IncmRowDto { IncmNo = 1262, Val = 133_729_000m, IncmRowDsc = "عوارض" } }
    };

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new HttpClient();
    }
}
