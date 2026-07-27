using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class SoapBuilder
{
    private const string SoapNs = "http://www.w3.org/2003/05/soap-envelope";
    private const string AddressingNs = "http://www.w3.org/2005/08/addressing";
    private const string TempUriNs = "http://tempuri.org/";
    private const string WcfNs = "http://schemas.datacontract.org/2004/07/WCFServer";
    private const string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";

    private readonly IConfiguration _config;

    public SoapBuilder(IConfiguration config) => _config = config;

    public string Build(FicheHeaderDto fiche, int branch, int fund, string docDate)
    {
        var docDateRay = DateHelper.ToRayvarzDate(docDate);
        var rowDateRay = DateHelper.ToRayvarzDate(fiche.RowDate);
        if (fund <= 0)
            fund = FundResolver.Resolve(_config, branch, fiche.PaymentBranch);

        var sourceSystemId = _config["Rayvarz:SourceSystemId"];
        var transactionId = fiche.NidFiche.ToString();
        var action = _config["Rayvarz:SoapAction"] ?? "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument";
        var serviceUrl = ResolveWsAddressingTo();

        const int docRow = 1;
        var rows = NormalizeRows(fiche);
        var phasTyp = ResolveSoapSmallInt(_config["Rayvarz:PhasTyp"], "7", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ptCash"] = "1",
            ["ptDraft"] = "2",
            ["ptCheque"] = "3",
            ["ptChequeDuration"] = "4",
            ["ptDraftRegion"] = "7"
        });
        var vchrTyp = ResolveSoapSmallInt(_config["Rayvarz:VchrTyp"], "0", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pfRecieve"] = "0",
            ["pfReceive"] = "0",
            ["pfPay"] = "1"
        });
        var incmMkrTyp = _config["Rayvarz:IncmMkrTyp"] ?? "0";
        var bank = ResolveBankCode(fiche.BankCode);

        var incmItems = string.Join("\n", rows.Select((r, i) => BuildIncmRow(
            r, i + 1, ResolveDetailRefRowDocNo(fiche.FicheNo), docDateRay, rowDateRay, sourceSystemId)));

        var refRecon = XmlOptionalElement("b", "RefreconstructionNo", fiche.RefReconstructionNo);
        var bodyXml = $@"        <SaveDocument xmlns=""{TempUriNs}"">
          <branch>{branch}</branch>
          <doc xmlns:b=""{WcfNs}""
               xmlns:i=""{XsiNs}"">
            <b:AllowChange>false</b:AllowChange>
            <b:DocDate>{docDateRay}</b:DocDate>
            <b:DocDsc>{Escape(fiche.DocDsc)}</b:DocDsc>
            <b:DocTyp>{fiche.DocTyp}</b:DocTyp>
            <b:DocTypDsc/>
            <b:Items>
              <b:DocumentItem>
                <b:ActDate>{docDateRay}</b:ActDate>
                <b:ActTyp>3</b:ActTyp>
                <b:Bank>{bank}</b:Bank>
                <b:BnkAcntNo>{Escape(fiche.BnkAcntNo)}</b:BnkAcntNo>
                <b:BnkAcntOwnr i:nil=""true""/>
                <b:BnkBrnch i:nil=""true""/>
                <b:Center>0</b:Center>
                <b:Customer i:nil=""true""/>
                <b:CustomerNationalCode i:nil=""true""/>
                <b:DocRow>{docRow}</b:DocRow>
                <b:Fund>{fund}</b:Fund>
                <b:IncmMkr>0</b:IncmMkr>
                <b:IncmMkrTyp>{incmMkrTyp}</b:IncmMkrTyp>
                <b:Incms>{incmItems}
                </b:Incms>
                <b:PhasTyp>{phasTyp}</b:PhasTyp>
                {XmlOptionalElement("b", "Ref2", fiche.BillId, nilIfEmpty: true)}
                {XmlOptionalElement("b", "Ref3", fiche.PaymentId, nilIfEmpty: true)}
                {XmlOptionalElement("b", "RefownrDsc", fiche.FicheNo, nilIfEmpty: true)}
                {refRecon}
                <b:RowDate>{rowDateRay}</b:RowDate>
                <b:RowDocNo>{Escape(fiche.FicheNo)}</b:RowDocNo>
                <b:VchrTyp>{vchrTyp}</b:VchrTyp>
              </b:DocumentItem>
            </b:Items>
            <b:Rcvr>0</b:Rcvr>
            <b:TransactionId>{Escape(transactionId)}</b:TransactionId>
          </doc>
        </SaveDocument>";

        return WrapEnvelope(action, serviceUrl, bodyXml);
    }

    /// <summary>POST خالی — همان تست قبلی.</summary>
    public string BuildPostProbeEnvelope()
    {
        var action = _config["Rayvarz:SoapAction"] ?? "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument";
        var serviceUrl = ResolveWsAddressingTo();
        return WrapEnvelope(action, serviceUrl, "");
    }

    /// <summary>SaveDocument حداقلی — برای تشخیص reset به‌خاطر ساختار/اندازه (ممکن است Fault بدهد؛ سند واقعی ثبت نشود).</summary>
    public string BuildMinimalSaveDocumentProbe(int branch = 207, int fund = 200207009)
    {
        var action = _config["Rayvarz:SoapAction"] ?? "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument";
        var serviceUrl = ResolveWsAddressingTo();
        var phasTyp = ResolveSoapSmallInt(_config["Rayvarz:PhasTyp"], "7", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ptDraftRegion"] = "7"
        });
        var vchrTyp = ResolveSoapSmallInt(_config["Rayvarz:VchrTyp"], "0", new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["pfReceive"] = "0"
        });
        var sourceSystemId = _config["Rayvarz:SourceSystemId"];
        var transactionId = Guid.NewGuid().ToString();
        const string docDateRay = "14000101";
        const string rowDateRay = "14000101";
        const string probeFiche = "000000/0000000";

        var incm = BuildIncmRow(
            new IncmRowDto { IncmNo = 2003, Val = 1, IncmRowDsc = "probe" },
            1,
            ResolveDetailRefRowDocNo(probeFiche),
            docDateRay,
            rowDateRay,
            sourceSystemId);

        var bodyXml = $@"        <SaveDocument xmlns=""{TempUriNs}"">
          <branch>{branch}</branch>
          <doc xmlns:b=""{WcfNs}""
               xmlns:i=""{XsiNs}"">
            <b:AllowChange>false</b:AllowChange>
            <b:DocDate>{docDateRay}</b:DocDate>
            <b:DocDsc>RayvarzResend probe</b:DocDsc>
            <b:DocTyp>1</b:DocTyp>
            <b:DocTypDsc/>
            <b:Items>
              <b:DocumentItem>
                <b:ActDate>{docDateRay}</b:ActDate>
                <b:ActTyp>3</b:ActTyp>
                <b:Bank>0</b:Bank>
                <b:BnkAcntNo>0-0-0-0-0-0-0</b:BnkAcntNo>
                <b:BnkAcntOwnr i:nil=""true""/>
                <b:BnkBrnch i:nil=""true""/>
                <b:Center>0</b:Center>
                <b:Customer i:nil=""true""/>
                <b:CustomerNationalCode i:nil=""true""/>
                <b:DocRow>1</b:DocRow>
                <b:Fund>{fund}</b:Fund>
                <b:IncmMkr>0</b:IncmMkr>
                <b:IncmMkrTyp>0</b:IncmMkrTyp>
                <b:Incms>{incm}
                </b:Incms>
                <b:PhasTyp>{phasTyp}</b:PhasTyp>
                <b:RowDate>{rowDateRay}</b:RowDate>
                <b:RowDocNo>{probeFiche}</b:RowDocNo>
                <b:VchrTyp>{vchrTyp}</b:VchrTyp>
              </b:DocumentItem>
            </b:Items>
            <b:Rcvr>0</b:Rcvr>
            <b:TransactionId>{Escape(transactionId)}</b:TransactionId>
          </doc>
        </SaveDocument>";

        return WrapEnvelope(action, serviceUrl, bodyXml);
    }

    private string ResolveDetailRefRowDocNo(string ficheNo)
    {
        var mode = (_config["Rayvarz:RefRowDocNoInDetail"] ?? "headerDocRow").Trim();
        if (mode.Equals("ficheNo", StringComparison.OrdinalIgnoreCase)
            || mode.Equals("fiche", StringComparison.OrdinalIgnoreCase))
            return ficheNo;
        return "1";
    }

    private (string env, string envNs, bool soap11) ResolveEnvelopeNs()
    {
        var soap11 = RayvarzSoapHttp.ResolveSoapVersion(_config) == RayvarzSoapVersion.Soap11;
        return soap11
            ? ("soap", "http://schemas.xmlsoap.org/soap/envelope/", true)
            : ("s", SoapNs, false);
    }

    private string WrapEnvelope(string action, string serviceUrl, string bodyXml)
    {
        var (env, envNs, soap11) = ResolveEnvelopeNs();
        var headerXml = BuildSoapHeader(action, serviceUrl, env);
        var xmlDecl = soap11 ? "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" : "";
        var bodyInner = string.IsNullOrEmpty(bodyXml) ? "" : bodyXml + "\n";
        return $@"{xmlDecl}<{env}:Envelope xmlns:{env}=""{envNs}""
      xmlns:a=""{AddressingNs}"">
{headerXml}
      <{env}:Body>
{bodyInner}      </{env}:Body>
    </{env}:Envelope>";
    }

    private string ResolveWsAddressingTo() =>
        RayvarzUrlNormalizer.Normalize(_config,
            _config["Rayvarz:WsAddressingTo"]
            ?? _config["Rayvarz:ServiceUrl"]
            ?? "");

    public string ResolveEnvelopeStyle() =>
        RayvarzSoapHttp.ResolveEnvelopeStyle(_config);

    private string BuildSoapHeader(string action, string serviceUrl, string env)
    {
        var style = ResolveEnvelopeStyle();
        if (style is "empty" or "empty-header" or "minimal" or "none")
            return $"      <{env}:Header/>";

        return $@"      <{env}:Header>
        <a:Action {env}:mustUnderstand=""1"">{Escape(action)}</a:Action>
        <a:MessageID>urn:uuid:{Guid.NewGuid()}</a:MessageID>
        <a:ReplyTo>
          <a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>
        </a:ReplyTo>
        <a:To {env}:mustUnderstand=""1"">{Escape(serviceUrl)}</a:To>
      </{env}:Header>";
    }

    /// <summary>کد بانک از PaymentBank / ConfirmBankCode دیتابیس شهرسازی (مثلاً ۱۸) بدون تغییر به رایورز می‌رود.</summary>
    private static int ResolveBankCode(string? bankCode) =>
        int.TryParse(bankCode, out var bank) ? bank : 0;

    private static string FormatRayvarzMoney(decimal val) =>
        val.ToString("0", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>مقادیر SOAP smallint — طبق PDF راهنما (مثلاً PhasTyp=7 برای ptDraftRegion).</summary>
    private static string ResolveSoapSmallInt(string? configured, string defaultValue, IReadOnlyDictionary<string, string> nameToCode)
    {
        var raw = string.IsNullOrWhiteSpace(configured) ? defaultValue : configured.Trim();
        if (int.TryParse(raw, out _))
            return raw;
        return nameToCode.TryGetValue(raw, out var code) ? code : defaultValue;
    }

    private static string BuildIncmRow(
        IncmRowDto row,
        int incmRow,
        string refRowDocNo,
        string docDateRay,
        string rowDateRay,
        string? sourceSystemId)
    {
        var reasonDsc = string.IsNullOrWhiteSpace(row.IncmRowDsc) ? "" : Escape(row.IncmRowDsc);
        var incmNoDsc = string.IsNullOrWhiteSpace(row.IncmRowDsc)
            ? row.IncmNo.ToString()
            : Escape(row.IncmRowDsc);
        var money = FormatRayvarzMoney(row.Val);

        return $@"
              <b:DocumentItemIncm>
                <b:Center1>0</b:Center1>
                <b:Center2>0</b:Center2>
                <b:Center3>0</b:Center3>
                <b:Crncy i:nil=""true""/>
                <b:CrncyDate i:nil=""true""/>
                <b:CrncyPrice>0</b:CrncyPrice>
                <b:CrncyVal>0</b:CrncyVal>
                <b:Due>{docDateRay}</b:Due>
                <b:Id i:nil=""true""/>
                <b:IncmNo>{row.IncmNo}</b:IncmNo>
                <b:IncmNoDsc>{incmNoDsc}</b:IncmNoDsc>
                <b:IncmRow>{incmRow}</b:IncmRow>
                <b:IncmRowDsc i:nil=""true""/>
                <b:Num i:nil=""true""/>
                <b:Qty>{money}</b:Qty>
                <b:Reason>1</b:Reason>
                <b:ReasonDsc>{reasonDsc}</b:ReasonDsc>
                <b:Ref i:nil=""true""/>
                <b:RefRowDate>{rowDateRay}</b:RefRowDate>
                <b:RefRowDocNo>{Escape(refRowDocNo)}</b:RefRowDocNo>
                {XmlOptionalElement("b", "SourceId", sourceSystemId, nilIfEmpty: true)}
                <b:Val>{money}</b:Val>
              </b:DocumentItemIncm>";
    }

    private static string XmlOptionalElement(string prefix, string name, string? value, bool nilIfEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(value))
            return nilIfEmpty ? $"<{prefix}:{name} i:nil=\"true\"/>" : "";

        return $"<{prefix}:{name}>{Escape(value)}</{prefix}:{name}>";
    }

    private static List<IncmRowDto> NormalizeRows(FicheHeaderDto fiche)
    {
        var rows = fiche.Rows.Where(r => r.Val != 0).ToList();
        if (rows.Count == 0)
            rows.Add(new IncmRowDto { IncmNo = 0, Val = fiche.Payable, IncmRowDsc = "کل" });

        if (fiche.Category is FicheCategory.DutyNosazi or FicheCategory.DutySenfi)
            return rows;

        var sum = rows.Sum(r => r.Val);
        if (sum != fiche.Payable && sum != 0)
        {
            var factor = fiche.Payable / sum;
            foreach (var r in rows) r.Val = Math.Round(r.Val * factor, 0);
            var diff = fiche.Payable - rows.Sum(r => r.Val);
            rows[0].Val += diff;
        }

        return rows;
    }

    private static string Escape(string? s) => WebUtility.HtmlEncode(s ?? "");
}

public class RayvarzClient
{
    private readonly IConfiguration _config;
    private readonly ILogger<RayvarzClient> _logger;
    private readonly SoapBuilder _soapBuilder;

    public RayvarzClient(IConfiguration config, ILogger<RayvarzClient> logger, SoapBuilder soapBuilder)
    {
        _config = config;
        _logger = logger;
        _soapBuilder = soapBuilder;
    }

    public string ResolveServiceUrl() =>
        RayvarzUrlNormalizer.Normalize(_config, _config["Rayvarz:ServiceUrl"] ?? "");

    public async Task<RayvarzPingResultDto> PingAsync(CancellationToken ct = default)
    {
        var baseUrl = ResolveServiceUrl().TrimEnd('/');
        var wsdlUrl = baseUrl.Contains('?') ? baseUrl : baseUrl + "?wsdl";
        var allowInvalidSsl = _config.GetValue<bool>("Rayvarz:AllowInvalidSsl");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var diagnostics = new RayvarzTransportDiagnostics
        {
            PostUrl = wsdlUrl,
            EnvelopeStyle = "(ping — GET ?wsdl)"
        };

        try
        {
            _logger.LogInformation("Rayvarz ping شروع — {Url} AllowInvalidSsl={AllowInvalidSsl}", wsdlUrl, allowInvalidSsl);
            using var client = CreateHttpClient(allowInvalidSsl);
            using var response = await client.GetAsync(wsdlUrl, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            sw.Stop();

            var status = (int)response.StatusCode;
            var wsdlOk = response.IsSuccessStatusCode;
            var httpReached = wsdlOk || IsHttpReachable(status, body);

            diagnostics = RayvarzDiagnosticsHelper.ForSuccess(sw.ElapsedMilliseconds, diagnostics, status, body.Length);
            diagnostics.Stage = "GetWsdl";

            string? warning = null;
            if (httpReached && !wsdlOk)
            {
                diagnostics.Category = "PingWsdlDegraded";
                diagnostics.LikelyCause =
                    $"پاسخ HTTP {status} از MSB/پروکسی دریافت شد — مسیر شبکه باز است، ولی ?wsdl خطا داد (در این محیط طبیعی است).";
                diagnostics.Hint = "معیار اصلی: «تست POST (بدون ثبت)» و ارسال SaveDocument — نه WSDL.";
                warning = $"WSDL: HTTP {status} — اتصال برقرار؛ برای ارسال فیش به POST Test تکیه کنید.";
            }
            else if (wsdlOk)
            {
                diagnostics.Category = "PingOk";
            }
            else
            {
                diagnostics.Category = "PingHttpError";
                diagnostics.LikelyCause = $"WSDL با HTTP {status} و بدنه خالی/کوتاه — مسیر نامشخص.";
                diagnostics.Hint = "VPN و ServiceUrl را چک کنید؛ سپس POST Test.";
            }

            _logger.LogInformation(
                "Rayvarz ping پایان — Reachable={Reachable} WsdlOk={WsdlOk} Status={Status} ElapsedMs={ElapsedMs}",
                httpReached, wsdlOk, status, sw.ElapsedMilliseconds);

            return new RayvarzPingResultDto
            {
                Ok = httpReached,
                Url = wsdlUrl,
                StatusCode = status,
                ElapsedMs = sw.ElapsedMilliseconds,
                BodyPreview = body.Length > 200 ? body[..200] : body,
                AllowInvalidSsl = allowInvalidSsl,
                Warning = warning,
                Hint = diagnostics.Hint,
                Diagnostics = diagnostics
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics = RayvarzDiagnosticsHelper.ClassifyFailure(ex, "GetWsdl", sw.ElapsedMilliseconds, diagnostics);
            _logger.LogWarning(ex,
                "Rayvarz ping خطا — Category={Category} ElapsedMs={ElapsedMs} Hint={Hint}",
                diagnostics.Category, sw.ElapsedMilliseconds, diagnostics.Hint);

            return new RayvarzPingResultDto
            {
                Ok = false,
                Url = wsdlUrl,
                ElapsedMs = sw.ElapsedMilliseconds,
                Error = ex.Message,
                Inner = ex.InnerException?.Message,
                AllowInvalidSsl = allowInvalidSsl,
                Hint = diagnostics.Hint ?? BuildNetworkHint(ex),
                Diagnostics = diagnostics
            };
        }
    }

    /// <summary>پاسخ HTTP از پروکسی (حتی 502) یعنی مسیر شبکه باز است؛ برعکس connection reset.</summary>
    private static bool IsHttpReachable(int statusCode, string body) =>
        body.Length > 50 && statusCode is 502 or 405 or 401 or 403 or 500 or 400;

    public async Task<RayvarzPingResultDto> PostProbeAsync(CancellationToken ct = default) =>
        await PostSoapDiagnosticAsync(
            _soapBuilder.BuildPostProbeEnvelope(),
            "PostProbe",
            "(post-probe — Body خالی، سندی ثبت نمی‌شود)",
            reachedCause:
                "POST تا MSB رسید و پاسخ HTTP گرفت (حتی اگر Fault باشد طبیعی است چون Body خالی بود). یعنی مسیر POST باز است — اگر Send واقعی reset می‌شود مشکل از محتوا/اندازه XML است.",
            resetCause:
                "حتی POST کوچک با Body خالی هم قطع شد — یعنی مسیر POST به MSB بسته است (فایروال/WAF/مجوز IP)، ربطی به محتوای فیش ندارد.",
            resetHint:
                "با IT: از IP این سرور، POST به MSB (همان مسیر پروکسی) باید مجاز شود — GET/WSDL باز است ولی POST reset می‌شود.",
            ct);

    public async Task<RayvarzPingResultDto> PostMinimalSaveDocumentAsync(CancellationToken ct = default) =>
        await PostSoapDiagnosticAsync(
            _soapBuilder.BuildMinimalSaveDocumentProbe(),
            "PostMinimalSaveDocument",
            "(minimal SaveDocument — یک ردیف تست، ممکن است Fault)",
            reachedCause:
                "POST با ساختار SaveDocument (حداقلی) تا MSB رسید — اگر ارسال فیش واقعی reset می‌شود، احتمالاً WAF/اندازه/کاراکتر فارسی یا فیلد خاص فیش است نه مسیر POST.",
            resetCause:
                "SaveDocument حداقلی هم قطع شد — MSB/WAF احتمالاً هر بدنهٔ SaveDocument را می‌بندد یا نسخه SOAP/هدر با سرویس نمی‌خواند؛ SoapVersion=soap11 و SoapEnvelopeStyle=empty-header را امتحان کنید.",
            resetHint:
                "appsettings: SoapVersion=soap11؛ SoapEnvelopeStyle=empty-header؛ UseSystemProxy=true اگر شهرسازی از پروکسی سیستم استفاده می‌کند.",
            ct);

    private async Task<RayvarzPingResultDto> PostSoapDiagnosticAsync(
        string probeXml,
        string stage,
        string envelopeLabel,
        string reachedCause,
        string resetCause,
        string resetHint,
        CancellationToken ct)
    {
        var url = ResolveServiceUrl();
        var action = _config["Rayvarz:SoapAction"] ?? "";
        var allowInvalidSsl = _config.GetValue<bool>("Rayvarz:AllowInvalidSsl");
        var envelopeStyle = _soapBuilder.ResolveEnvelopeStyle();
        var soapVersion = RayvarzSoapHttp.ResolveSoapVersion(_config);

        var diagnostics = new RayvarzTransportDiagnostics
        {
            PostUrl = url,
            SoapAction = action,
            EnvelopeStyle = $"{envelopeStyle}+{RayvarzSoapHttp.SoapVersionLabel(soapVersion)} ({envelopeLabel})"
        };
        RayvarzDiagnosticsHelper.ApplySoapRequestMeta(diagnostics, probeXml, diagnostics.EnvelopeStyle);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("Rayvarz {Stage} شروع — {Url} Bytes={Bytes}", stage, url, diagnostics.RequestBodyBytes);
            using var client = CreateHttpClient(allowInvalidSsl);
            using var response = await RayvarzSoapHttp.PostAsync(client, url, probeXml, action, soapVersion, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            sw.Stop();

            diagnostics.HttpStatusCode = (int)response.StatusCode;
            diagnostics.ResponseBodyBytes = body.Length;
            diagnostics.ElapsedMs = sw.ElapsedMilliseconds;
            diagnostics.Stage = stage;
            diagnostics.Category = stage + "Reached";
            diagnostics.LikelyCause = reachedCause;
            diagnostics.ContentType = response.Content.Headers.ContentType?.ToString();

            _logger.LogInformation("Rayvarz {Stage} پایان — Status={Status} ElapsedMs={Elapsed}", stage, (int)response.StatusCode, sw.ElapsedMilliseconds);

            return new RayvarzPingResultDto
            {
                Ok = true,
                Url = url,
                StatusCode = (int)response.StatusCode,
                ElapsedMs = sw.ElapsedMilliseconds,
                BodyPreview = body.Length > 400 ? body[..400] : body,
                AllowInvalidSsl = allowInvalidSsl,
                Diagnostics = diagnostics
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics = RayvarzDiagnosticsHelper.ClassifyFailure(ex, stage, sw.ElapsedMilliseconds, diagnostics);
            diagnostics.LikelyCause = resetCause;
            diagnostics.Hint = resetHint;
            _logger.LogWarning(ex, "Rayvarz {Stage} خطا — Category={Category}", stage, diagnostics.Category);

            return new RayvarzPingResultDto
            {
                Ok = false,
                Url = url,
                ElapsedMs = sw.ElapsedMilliseconds,
                Error = ex.Message,
                Inner = ex.InnerException?.Message,
                AllowInvalidSsl = allowInvalidSsl,
                Hint = diagnostics.Hint,
                Diagnostics = diagnostics
            };
        }
    }

    public async Task<SendResultDto> SendAsync(string soapXml, bool dryRun, CancellationToken ct = default)
    {
        if (dryRun)
        {
            return new SendResultDto
            {
                Success = true,
                DryRun = true,
                Message = "حالت DryRun — XML ساخته شد ولی ارسال نشد",
                PreviewXml = soapXml
            };
        }

        var url = ResolveServiceUrl();
        var action = _config["Rayvarz:SoapAction"] ?? "";
        var allowInvalidSsl = _config.GetValue<bool>("Rayvarz:AllowInvalidSsl");
        var envelopeStyle = RayvarzSoapHttp.ResolveEnvelopeStyle(_config);
        var soapVersion = RayvarzSoapHttp.ResolveSoapVersion(_config);
        var sendDelayMs = _config.GetValue<int>("Rayvarz:SendDelayMs");
        if (sendDelayMs > 0)
            await Task.Delay(sendDelayMs, ct);

        var diagnostics = new RayvarzTransportDiagnostics
        {
            PostUrl = url,
            SoapAction = action,
            EnvelopeStyle = $"{envelopeStyle}+{RayvarzSoapHttp.SoapVersionLabel(soapVersion)}"
        };
        RayvarzDiagnosticsHelper.ApplySoapRequestMeta(diagnostics, soapXml, diagnostics.EnvelopeStyle);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Rayvarz SendDocument شروع — Url={Url} Bytes={Bytes} HasWsAddressing={HasWs} To={To} Style={Style}",
                url, diagnostics.RequestBodyBytes, diagnostics.HasWsAddressingHeader,
                diagnostics.WsAddressingTo ?? "(empty-header)", envelopeStyle);

            using var client = CreateHttpClient(allowInvalidSsl);
            using var response = await RayvarzSoapHttp.PostAsync(client, url, soapXml, action, soapVersion, ct);
            diagnostics.ContentType = response.RequestMessage?.Content?.Headers.ContentType?.ToString();

            string body;
            try
            {
                body = await response.Content.ReadAsStringAsync(ct);
            }
            catch (Exception ex)
            {
                sw.Stop();
                diagnostics.HttpStatusCode = (int)response.StatusCode;
                diagnostics = RayvarzDiagnosticsHelper.ClassifyFailure(ex, "ReadResponseBody", sw.ElapsedMilliseconds, diagnostics);
                _logger.LogWarning(ex,
                    "Rayvarz SendDocument خطا در خواندن پاسخ — HttpStatus={Status} Category={Category}",
                    (int)response.StatusCode, diagnostics.Category);

                return new SendResultDto
                {
                    Success = false,
                    DryRun = false,
                    PreviewXml = soapXml,
                    Diagnostics = diagnostics,
                    Message = FormatUserMessage(ex, diagnostics)
                };
            }

            sw.Stop();
            diagnostics.ResponseBodyBytes = body.Length;
            diagnostics.HttpStatusCode = (int)response.StatusCode;

            var result = new SendResultDto
            {
                SoapResponse = body,
                PreviewXml = soapXml,
                DryRun = false,
                Diagnostics = diagnostics
            };

            try
            {
                var doc = XDocument.Parse(body);
                XNamespace wcf = "http://schemas.datacontract.org/2004/07/WCFServer";
                XNamespace con = "http://www.bea.com/wli/sb/context";

                var faultCode = doc.Descendants(con + "errorCode").FirstOrDefault()?.Value
                    ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultcode")?.Value;
                var faultReason = doc.Descendants(con + "reason").FirstOrDefault()?.Value
                    ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value;

                if (!string.IsNullOrWhiteSpace(faultCode) || !string.IsNullOrWhiteSpace(faultReason))
                {
                    result.Success = false;
                    result.Message = $"SOAP Fault: {faultCode} — {faultReason}".Trim(' ', '—');
                    result.Diagnostics = RayvarzDiagnosticsHelper.ForSoapFault(sw.ElapsedMilliseconds, diagnostics, (int)response.StatusCode);
                    _logger.LogWarning("Rayvarz SOAP Fault — {FaultCode} {FaultReason}", faultCode, faultReason);
                    return result;
                }

                result.Success = doc.Descendants(wcf + "Success").FirstOrDefault()?.Value == "true";
                result.Message = doc.Descendants(wcf + "Message").FirstOrDefault()?.Value ?? "";
                result.PursuitDocNo = doc.Descendants(wcf + "PursuitDocNo").FirstOrDefault()?.Value;

                if (!result.Success && string.IsNullOrWhiteSpace(result.Message))
                    result.Message = "پاسخ رایورز Success=false — جزئیات در SoapResponse";

                result.Diagnostics = result.Success
                    ? RayvarzDiagnosticsHelper.ForSuccess(sw.ElapsedMilliseconds, diagnostics, (int)response.StatusCode, body.Length)
                    : diagnostics;
                result.Diagnostics.Category = result.Success ? "SoapSuccess" : "SoapBusinessError";
                result.Diagnostics.ElapsedMs = sw.ElapsedMilliseconds;
                if (!result.Success)
                {
                    result.Diagnostics.LikelyCause = "رایورز Success=false — معمولاً فیلد Body (Fund، IncmNo، تاریخ، …).";
                    result.Diagnostics.Hint = "Message و SoapResponse را ببینید.";
                }

                _logger.LogInformation(
                    "Rayvarz SendDocument پایان — Success={Success} Http={Http} ElapsedMs={ElapsedMs}",
                    result.Success, (int)response.StatusCode, sw.ElapsedMilliseconds);
            }
            catch (Exception parseEx)
            {
                result.Success = false;
                result.Message = response.IsSuccessStatusCode
                    ? "پاسخ HTTP موفق بود ولی SOAP معتبر نبود — در رایورز ثبت نشده"
                    : $"HTTP {(int)response.StatusCode}";
                result.Diagnostics!.Category = "InvalidSoapResponse";
                result.Diagnostics.Stage = "ParseResponse";
                result.Diagnostics.LikelyCause = "پاسخ HTTP دریافت شد ولی XML SOAP قابل parse نبود.";
                result.Diagnostics.ExceptionChain = new List<string> { $"{parseEx.GetType().Name}: {parseEx.Message}" };
                _logger.LogWarning(parseEx, "Rayvarz پاسخ غیرقابل parse — HttpStatus={Status}", (int)response.StatusCode);
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            diagnostics = RayvarzDiagnosticsHelper.ClassifyFailure(ex, "PostSoap", sw.ElapsedMilliseconds, diagnostics);
            _logger.LogWarning(ex,
                "Rayvarz SendDocument خطا در POST — Category={Category} Chain={Chain}",
                diagnostics.Category, string.Join(" | ", diagnostics.ExceptionChain));
            return new SendResultDto
            {
                Success = false,
                DryRun = false,
                PreviewXml = soapXml,
                Diagnostics = diagnostics,
                Message = FormatUserMessage(ex, diagnostics)
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            diagnostics = RayvarzDiagnosticsHelper.ClassifyFailure(ex, "SendUnhandled", sw.ElapsedMilliseconds, diagnostics);
            _logger.LogError(ex, "Rayvarz SendDocument خطای پیش‌بینی‌نشده");
            return new SendResultDto
            {
                Success = false,
                DryRun = false,
                PreviewXml = soapXml,
                Diagnostics = diagnostics,
                Message = FormatUserMessage(ex, diagnostics)
            };
        }
    }

    private static string FormatUserMessage(Exception ex, RayvarzTransportDiagnostics d)
    {
        var inner = ex.InnerException?.Message;
        var core = inner != null ? $"{ex.Message} | Inner: {inner}" : ex.Message;
        var extra = d.LikelyCause;
        var hint = d.Hint;
        return string.Join(" | ", new[] { core, extra, hint }.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private HttpClient CreateHttpClient(bool allowInvalidSsl)
    {
        var proxyUrl = _config["Rayvarz:ProxyUrl"];
        var useSystemProxy = _config.GetValue<bool>("Rayvarz:UseSystemProxy");
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            }
        };

        if (!string.IsNullOrWhiteSpace(proxyUrl))
        {
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;
        }
        else if (useSystemProxy)
        {
            handler.Proxy = HttpClient.DefaultProxy;
            handler.UseProxy = true;
            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
        }

        if (allowInvalidSsl)
            handler.SslOptions.RemoteCertificateValidationCallback = static (_, _, _, _) => true;

        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(120) };
    }

    private static string BuildNetworkHint(Exception ex)
    {
        var msg = (ex.Message + " " + (ex.InnerException?.Message ?? "")).ToLowerInvariant();
        if (msg.Contains("ssl connection could not be established"))
            return "MSB/TLS: از همان سرور/شبکه شهرسازی اجرا کنید؛ VPN؛ ProxyUrl یا UseSystemProxy=true؛ با IT دسترسی به https://msb.mashhad.ir.";
        if (msg.Contains("forcibly closed") || msg.Contains("copying content to a stream"))
            return "شبکه: Ping باید OK شود قبل از Send؛ SoapEnvelopeStyle فقط بعد از Ping موفق.";
        if (msg.Contains("ssl") || msg.Contains("certificate") || msg.Contains("tls")
            || msg.Contains("connection was closed") || msg.Contains("unexpected error occurred on a send"))
            return "شبکه: از همان سروری که سامانه شهرسازی ارسال می‌کند اجرا کنید؛ VPN؛ AllowInvalidSsl=true؛ یا ProxyUrl در appsettings.";
        return "شبکه/فایروال را با IT چک کنید.";
    }
}
