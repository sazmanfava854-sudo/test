using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Web.RuleEngine;

public enum RayvarzPayloadSourceMode
{
    LegacyCSharp,
    RuleEngineBridge
}

public sealed class RayvarzPayloadBuildResult
{
    public string Xml { get; init; } = "";
    public RayvarzPayloadSourceMode Mode { get; init; }
    public string? Warning { get; init; }
    public RuleEngineMeta? RuleMeta { get; init; }
}

public sealed class RuleEngineMeta
{
    public int NidMember { get; init; }
    public int NidFunction { get; init; }
    public int DbVersion { get; init; }
    public string RuleSource { get; init; } = "";
    public int BodyLength { get; init; }
    public IReadOnlyList<string> Functions { get; init; } = Array.Empty<string>();
    public bool HasNosazi { get; init; }
    public bool HasIncomeRun { get; init; }
}

/// <summary>
/// ساخت XML SaveDocument: پیش‌فرض C# (baseline v16) یا پل Sara که همان Member 1388 را با Info8 اجرا می‌کند.
/// XmlBody شامل VB داخل &lt;Body&gt; است — در .NET بدون موتور شهرسازی اجرا نمی‌شود.
/// </summary>
public sealed class RayvarzPayloadBuilder
{
    private readonly IConfiguration _config;
    private readonly SoapBuilder _soap;
    private readonly MemberRuleRepository _rules;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SaraBridgeStubService? _localStub;

    public RayvarzPayloadBuilder(
        IConfiguration config,
        SoapBuilder soap,
        MemberRuleRepository rules,
        IHttpClientFactory httpClientFactory,
        SaraBridgeStubService? localStub = null)
    {
        _config = config;
        _soap = soap;
        _rules = rules;
        _httpClientFactory = httpClientFactory;
        _localStub = localStub;
    }

    public RayvarzPayloadSourceMode ResolveMode()
    {
        var raw = (_config["Rayvarz:PayloadSource"] ?? "LegacyCSharp").Trim();
        return Enum.TryParse<RayvarzPayloadSourceMode>(raw, ignoreCase: true, out var mode)
            ? mode
            : RayvarzPayloadSourceMode.LegacyCSharp;
    }

    public async Task<RayvarzPayloadBuildResult> BuildAsync(
        FicheHeaderDto fiche,
        int branch,
        int fund,
        string? docDate,
        string? actDate,
        string? dueDate,
        CancellationToken ct = default)
    {
        var nidMember = _config.GetValue("RuleEngine:NidMemberRayvarzRun", 1388);
        MemberRuleRecord? record = null;
        ClsFunctionDocument? parsed = null;
        string? loadError = null;

        try
        {
            record = await _rules.LoadActiveMemberAsync(nidMember, ct: ct);
            if (record != null && !string.IsNullOrWhiteSpace(record.XmlBody))
                parsed = ClsFunctionParser.Parse(record.XmlBody);
        }
        catch (Exception ex)
        {
            loadError = ex.Message;
        }

        var meta = BuildMeta(nidMember, record, parsed);

        if (ResolveMode() == RayvarzPayloadSourceMode.RuleEngineBridge)
        {
            var bridgeUrl = _config["RuleEngine:SaraBridgeUrl"];
            if (!string.IsNullOrWhiteSpace(bridgeUrl))
            {
                try
                {
                    var xml = await CallSaraBridgeAsync(bridgeUrl, fiche, branch, fund, docDate, actDate, dueDate, nidMember, ct);
                    return new RayvarzPayloadBuildResult
                    {
                        Xml = xml,
                        Mode = RayvarzPayloadSourceMode.RuleEngineBridge,
                        RuleMeta = meta
                    };
                }
                catch (Exception ex)
                {
                    return LegacyWithWarning(fiche, branch, fund, docDate, actDate, dueDate, meta,
                        $"SaraBridge خطا داد — fallback به C#: {ex.Message}");
                }
            }

            if (_config.GetValue("RuleEngine:UseLocalBridgeStub", false) && _localStub != null)
            {
                try
                {
                    var stubResult = _localStub.BuildFromFiche(fiche, new SaraBridgeBuildRequest
                    {
                        NidMember = nidMember,
                        NidFiche = fiche.NidFiche,
                        FicheNo = fiche.FicheNo,
                        Category = fiche.Category.ToString(),
                        Branch = branch,
                        Fund = fund,
                        DocDate = docDate,
                        ActDate = actDate,
                        DueDate = dueDate
                    });

                    if (!string.IsNullOrWhiteSpace(stubResult.SoapXml))
                    {
                        return new RayvarzPayloadBuildResult
                        {
                            Xml = stubResult.SoapXml,
                            Mode = RayvarzPayloadSourceMode.RuleEngineBridge,
                            RuleMeta = meta,
                            Warning = stubResult.Warning
                        };
                    }

                    return LegacyWithWarning(fiche, branch, fund, docDate, actDate, dueDate, meta,
                        stubResult.Error ?? "LocalBridgeStub بدون SoapXml برگشت.");
                }
                catch (Exception ex)
                {
                    return LegacyWithWarning(fiche, branch, fund, docDate, actDate, dueDate, meta,
                        $"LocalBridgeStub خطا داد — fallback به C#: {ex.Message}");
                }
            }

            return LegacyWithWarning(fiche, branch, fund, docDate, actDate, dueDate, meta,
                "RuleEngineBridge فعال است ولی RuleEngine:SaraBridgeUrl تنظیم نشده. " +
                "برای تست محلی RuleEngine:UseLocalBridgeStub=true بگذارید. " +
                "XmlBody همان VB داخل ClsFunction است و در این پروژه اجرا نمی‌شود؛ " +
                "روی سرور شهرسازی API بسازید که Run() را صدا بزند و XML برگرداند. " +
                (loadError != null ? $"بارگذاری Member: {loadError}" : $"Member بارگذاری شد (منبع: {record?.Source ?? "—"})."));
        }

        return new RayvarzPayloadBuildResult
        {
            Xml = _soap.Build(fiche, branch, fund, docDate, actDate, dueDate),
            Mode = RayvarzPayloadSourceMode.LegacyCSharp,
            RuleMeta = meta,
            Warning = loadError != null ? $"Member (اختیاری): {loadError}" : null
        };
    }

    private RayvarzPayloadBuildResult LegacyWithWarning(
        FicheHeaderDto fiche, int branch, int fund,
        string? docDate, string? actDate, string? dueDate,
        RuleEngineMeta? meta, string warning) =>
        new()
        {
            Xml = _soap.Build(fiche, branch, fund, docDate, actDate, dueDate),
            Mode = RayvarzPayloadSourceMode.LegacyCSharp,
            RuleMeta = meta,
            Warning = warning
        };

    private static RuleEngineMeta? BuildMeta(int nidMember, MemberRuleRecord? record, ClsFunctionDocument? parsed)
    {
        if (record == null && parsed == null)
            return null;

        return new RuleEngineMeta
        {
            NidMember = nidMember,
            NidFunction = parsed?.NidFunction ?? 0,
            DbVersion = record?.Version ?? parsed?.Version ?? 0,
            RuleSource = record?.Source ?? "parse-only",
            BodyLength = parsed?.BodySource.Length ?? record?.XmlBody.Length ?? 0,
            Functions = parsed?.FunctionNames ?? Array.Empty<string>(),
            HasNosazi = parsed?.ContainsFunction("نوسازی") == true || parsed?.ContainsFunction("Nosazi") == true,
            HasIncomeRun = parsed?.ContainsFunction("iNcOME") == true || parsed?.Name == "Run"
        };
    }

    private async Task<string> CallSaraBridgeAsync(
        string bridgeUrl,
        FicheHeaderDto fiche,
        int branch,
        int fund,
        string? docDate,
        string? actDate,
        string? dueDate,
        int nidMember,
        CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("SaraBridge");
        var payload = new SaraBridgeBuildRequest
        {
            NidMember = nidMember,
            NidFiche = fiche.NidFiche,
            FicheNo = fiche.FicheNo,
            Category = fiche.Category.ToString(),
            Branch = branch,
            Fund = fund,
            DocDate = docDate,
            ActDate = actDate,
            DueDate = dueDate
        };

        using var response = await client.PostAsJsonAsync(bridgeUrl.TrimEnd('/') + "/rayvarz/build-save-document", payload, ct);
        response.EnsureSuccessStatusCode();
        var doc = await response.Content.ReadFromJsonAsync<SaraBridgeBuildResponse>(cancellationToken: ct)
                  ?? throw new InvalidOperationException("پاسخ SaraBridge خالی است.");
        if (string.IsNullOrWhiteSpace(doc.SoapXml))
            throw new InvalidOperationException(doc.Error ?? "SoapXml در پاسخ نیست.");
        return doc.SoapXml;
    }
}
