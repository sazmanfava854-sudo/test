using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Xml.Linq;
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
        var phasTyp = _config["Rayvarz:PhasTyp"] ?? "ptDraftRegion";
        var vchrTyp = _config["Rayvarz:VchrTyp"] ?? "pfRecieve";
        var incmMkrTyp = _config["Rayvarz:IncmMkrTyp"] ?? "0";
        var bank = ResolveBankCode(fiche.BankCode);

        var incmItems = string.Join("\n", rows.Select((r, i) => BuildIncmRow(
            r, i + 1, docRow, docDateRay, rowDateRay, sourceSystemId)));

        var refRecon = XmlOptionalElement("b", "RefreconstructionNo", fiche.RefReconstructionNo);

        return $@"<s:Envelope xmlns:s=""{SoapNs}""
      xmlns:a=""{AddressingNs}"">

      <s:Header>
        <a:Action s:mustUnderstand=""1"">{Escape(action)}</a:Action>
        <a:MessageID>urn:uuid:{Guid.NewGuid()}</a:MessageID>
        <a:ReplyTo>
          <a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address>
        </a:ReplyTo>
        <a:To s:mustUnderstand=""1"">{Escape(serviceUrl)}</a:To>
      </s:Header>
      <s:Body>
        <SaveDocument xmlns=""{TempUriNs}"">
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
                <b:PhasTyp>{Escape(phasTyp)}</b:PhasTyp>
                {XmlOptionalElement("b", "Ref2", fiche.BillId, nilIfEmpty: true)}
                {XmlOptionalElement("b", "Ref3", fiche.PaymentId, nilIfEmpty: true)}
                {XmlOptionalElement("b", "RefownrDsc", fiche.FicheNo, nilIfEmpty: true)}
                {refRecon}
                <b:RowDate>{rowDateRay}</b:RowDate>
                <b:RowDocNo>{Escape(fiche.FicheNo)}</b:RowDocNo>
                <b:VchrTyp>{Escape(vchrTyp)}</b:VchrTyp>
              </b:DocumentItem>
            </b:Items>
            <b:Rcvr>0</b:Rcvr>
            <b:TransactionId>{Escape(transactionId)}</b:TransactionId>
          </doc>
        </SaveDocument>
      </s:Body>
    </s:Envelope>";
    }

    private string ResolveWsAddressingTo() =>
        _config["Rayvarz:WsAddressingTo"]
        ?? _config["Rayvarz:ServiceUrl"]
        ?? "";

    private static int ResolveBankCode(string? bankCode) =>
        int.TryParse(bankCode, out var bank) ? bank : 0;

    private static string BuildIncmRow(
        IncmRowDto row,
        int incmRow,
        int parentDocRow,
        string docDateRay,
        string rowDateRay,
        string? sourceSystemId)
    {
        var reasonDsc = string.IsNullOrWhiteSpace(row.IncmRowDsc) ? "" : Escape(row.IncmRowDsc);
        var incmNoDsc = string.IsNullOrWhiteSpace(row.IncmRowDsc)
            ? row.IncmNo.ToString()
            : Escape(row.IncmRowDsc);

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
                <b:Qty>{row.Val:0}</b:Qty>
                <b:Reason>1</b:Reason>
                <b:ReasonDsc>{reasonDsc}</b:ReasonDsc>
                <b:Ref i:nil=""true""/>
                <b:RefRowDate>{rowDateRay}</b:RefRowDate>
                <b:RefRowDocNo>{parentDocRow}</b:RefRowDocNo>
                {XmlOptionalElement("b", "SourceId", sourceSystemId, nilIfEmpty: true)}
                <b:Val>{row.Val:0}</b:Val>
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

    public RayvarzClient(IConfiguration config) => _config = config;

    public string ResolveServiceUrl() =>
        _config["Rayvarz:ServiceUrl"] ?? "";

    public async Task<object> PingAsync(CancellationToken ct = default)
    {
        var baseUrl = ResolveServiceUrl().TrimEnd('/');
        var wsdlUrl = baseUrl.Contains('?') ? baseUrl : baseUrl + "?wsdl";
        var allowInvalidSsl = _config.GetValue<bool>("Rayvarz:AllowInvalidSsl");
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var client = CreateHttpClient(allowInvalidSsl);
            using var response = await client.GetAsync(wsdlUrl, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            sw.Stop();
            return new
            {
                ok = response.IsSuccessStatusCode,
                url = wsdlUrl,
                statusCode = (int)response.StatusCode,
                elapsedMs = sw.ElapsedMilliseconds,
                bodyPreview = body.Length > 200 ? body[..200] : body,
                allowInvalidSsl
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new
            {
                ok = false,
                url = wsdlUrl,
                elapsedMs = sw.ElapsedMilliseconds,
                error = ex.Message,
                inner = ex.InnerException?.Message,
                allowInvalidSsl,
                hint = BuildNetworkHint(ex)
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
        var sendDelayMs = _config.GetValue<int>("Rayvarz:SendDelayMs");
        if (sendDelayMs > 0)
            await Task.Delay(sendDelayMs, ct);

        try
        {
            using var client = CreateHttpClient(allowInvalidSsl);
            using var content = new StringContent(soapXml, Encoding.UTF8, "application/soap+xml");
            content.Headers.ContentType!.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("action", $"\"{action}\""));

            var response = await client.PostAsync(url, content, ct);
            var body = await response.Content.ReadAsStringAsync(ct);

            var result = new SendResultDto
            {
                SoapResponse = body,
                PreviewXml = soapXml,
                DryRun = false
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
                    return result;
                }

                result.Success = doc.Descendants(wcf + "Success").FirstOrDefault()?.Value == "true";
                result.Message = doc.Descendants(wcf + "Message").FirstOrDefault()?.Value ?? "";
                result.PursuitDocNo = doc.Descendants(wcf + "PursuitDocNo").FirstOrDefault()?.Value;

                if (!result.Success && string.IsNullOrWhiteSpace(result.Message))
                    result.Message = "پاسخ رایورز Success=false — جزئیات در SoapResponse";
            }
            catch
            {
                result.Success = false;
                result.Message = response.IsSuccessStatusCode
                    ? "پاسخ HTTP موفق بود ولی SOAP معتبر نبود — در رایورز ثبت نشده"
                    : $"HTTP {(int)response.StatusCode}";
            }

            return result;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message;
            return new SendResultDto
            {
                Success = false,
                DryRun = false,
                PreviewXml = soapXml,
                Message = inner != null
                    ? $"{ex.Message} | Inner: {inner} | {BuildNetworkHint(ex)}"
                    : $"{ex.Message} | {BuildNetworkHint(ex)}"
            };
        }
    }

    private HttpClient CreateHttpClient(bool allowInvalidSsl)
    {
        var proxyUrl = _config["Rayvarz:ProxyUrl"];
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

        if (allowInvalidSsl)
            handler.SslOptions.RemoteCertificateValidationCallback = static (_, _, _, _) => true;

        return new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(120) };
    }

    private static string BuildNetworkHint(Exception ex)
    {
        var msg = (ex.Message + " " + (ex.InnerException?.Message ?? "")).ToLowerInvariant();
        if (msg.Contains("forcibly closed") || msg.Contains("copying content to a stream"))
            return "شبکه: ابتدا GET /api/rayvarz-ping؛ اگر ping OK و send خطا دارد XML را با مستندات مقایسه کنید؛ WsAddressingTo را در appsettings تنظیم کنید.";
        if (msg.Contains("ssl") || msg.Contains("certificate") || msg.Contains("tls")
            || msg.Contains("connection was closed") || msg.Contains("unexpected error occurred on a send"))
            return "شبکه: از همان سروری که سامانه شهرسازی ارسال می‌کند اجرا کنید؛ VPN؛ AllowInvalidSsl=true؛ یا ProxyUrl در appsettings.";
        return "شبکه/فایروال را با IT چک کنید.";
    }
}
