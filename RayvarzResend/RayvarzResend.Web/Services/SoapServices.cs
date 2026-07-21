using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Xml.Linq;
using RayvarzResend.Web.Models;

namespace RayvarzResend.Web.Services;

public class SoapBuilder
{
    private readonly IConfiguration _config;

    public SoapBuilder(IConfiguration config) => _config = config;

    public string Build(FicheHeaderDto fiche, int branch, int fund, string docDate)
    {
        var docDateRay = DateHelper.ToRayvarzDate(docDate);
        var rowDateRay = DateHelper.ToRayvarzDate(fiche.RowDate);
        if (fund <= 0)
            fund = FundResolver.Resolve(_config, branch, fiche.PaymentBranch);

        // Id سامانه مبدا — کد ثابت سامانه (مثلاً 11111)
        var sourceSystemId = _config["Rayvarz:SourceSystemId"] ?? "11111";
        // شناسه یکتای تراکنش/فیش — GUID فیش
        var transactionId = fiche.NidFiche.ToString();

        const int docRow = 1;
        var rows = NormalizeRows(fiche);
        var docTypDsc = ResolveDocTypDsc(fiche.DocTyp);

        var incmItems = string.Join("\n", rows.Select((r, i) => BuildIncmRow(
            r, i + 1, docRow, docDateRay, fiche.FicheNo, fiche.Payable, transactionId, sourceSystemId)));

        var refRecon = XmlOptional("RefreconstructionNo", fiche.RefReconstructionNo);
        var bankXml = XmlOptional("Bank", fiche.BankCode);

        return $@"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
  xmlns:tem=""http://tempuri.org/""
  xmlns:wcf=""http://schemas.datacontract.org/2004/07/WCFServer""
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <soap:Header/>
  <soap:Body>
    <tem:SaveDocument>
      <tem:branch>{branch}</tem:branch>
      <tem:doc>
        <wcf:AllowChange>false</wcf:AllowChange>
        <wcf:DocDate>{docDateRay}</wcf:DocDate>
        <wcf:DocDsc>{Escape(fiche.DocDsc)}</wcf:DocDsc>
        <wcf:DocTyp>{fiche.DocTyp}</wcf:DocTyp>
        <wcf:DocTypDsc>{Escape(docTypDsc)}</wcf:DocTypDsc>
        <wcf:Items>
          <wcf:DocumentItem>
            <wcf:ActDate>{docDateRay}</wcf:ActDate>
            <wcf:ActTyp>3</wcf:ActTyp>
            {bankXml}
            <wcf:BnkAcntNo>{Escape(fiche.BnkAcntNo)}</wcf:BnkAcntNo>
            <wcf:DocRow>{docRow}</wcf:DocRow>
            <wcf:DocTypDsc>{Escape(docTypDsc)}</wcf:DocTypDsc>
            <wcf:Fund>{fund}</wcf:Fund>
            <wcf:IncmMkrTyp>1</wcf:IncmMkrTyp>
            <wcf:Incms>{incmItems}
            </wcf:Incms>
            <wcf:PhasTyp>7</wcf:PhasTyp>
            <wcf:Ref2>{Escape(fiche.BillId)}</wcf:Ref2>
            <wcf:Ref3>{Escape(fiche.PaymentId)}</wcf:Ref3>
            <wcf:RefownrDsc>{Escape(fiche.FicheNo)}</wcf:RefownrDsc>
            {refRecon}
            <wcf:RowDate>{rowDateRay}</wcf:RowDate>
            <wcf:RowDocNo>{Escape(fiche.FicheNo)}</wcf:RowDocNo>
            <wcf:VchrTyp>0</wcf:VchrTyp>
          </wcf:DocumentItem>
        </wcf:Items>
        <wcf:TransactionId>{Escape(transactionId)}</wcf:TransactionId>
      </tem:doc>
    </tem:SaveDocument>
  </soap:Body>
</soap:Envelope>";
    }

    private static string BuildIncmRow(
        IncmRowDto row,
        int incmRow,
        int parentDocRow,
        string docDateRay,
        string ficheNo,
        decimal payable,
        string transactionId,
        string sourceSystemId) => $@"
              <wcf:DocumentItemIncm>
                <wcf:Due>{docDateRay}</wcf:Due>
                <wcf:Id>{Escape(transactionId)}</wcf:Id>
                <wcf:IncmNo>{row.IncmNo}</wcf:IncmNo>
                <wcf:IncmRow>{incmRow}</wcf:IncmRow>
                <wcf:IncmRowDsc>{Escape(row.IncmRowDsc)}</wcf:IncmRowDsc>
                <wcf:Qty>{payable:0}</wcf:Qty>
                <wcf:Ref>{Escape(ficheNo)}</wcf:Ref>
                <wcf:RefRowDocNo>{parentDocRow}</wcf:RefRowDocNo>
                <wcf:SourceId>{Escape(sourceSystemId)}</wcf:SourceId>
                <wcf:Val>{row.Val:0}</wcf:Val>
              </wcf:DocumentItemIncm>";

    private static string ResolveDocTypDsc(int docTyp) => docTyp switch
    {
        1 => "اسناد نوسازی",
        2 => "اسناد صنفی",
        3 or 11 => "اسناد شهرسازی",
        _ => "اسناد شهرسازی"
    };

    private static string XmlOptional(string name, string? value) =>
        string.IsNullOrWhiteSpace(value) ? "" : $"<wcf:{name}>{Escape(value)}</wcf:{name}>";

    private static List<IncmRowDto> NormalizeRows(FicheHeaderDto fiche)
    {
        var rows = fiche.Rows.Where(r => r.Val != 0).ToList();
        if (rows.Count == 0)
            rows.Add(new IncmRowDto { IncmNo = 0, Val = fiche.Payable, IncmRowDsc = "کل" });

        // فیش نوسازی/صنفی: مبالغ از Duty_FicheSub خوانده می‌شوند؛ نرمال‌سازی آن‌ها را خراب می‌کند (مثلاً جایزه)
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

    public string ResolveServiceUrl()
    {
        var useTest = _config.GetValue<bool>("Rayvarz:UseTestUrl");
        return useTest
            ? (_config["Rayvarz:ServiceUrlTest"] ?? "")
            : (_config["Rayvarz:ServiceUrl"] ?? "");
    }

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
                result.Success = doc.Descendants(wcf + "Success").FirstOrDefault()?.Value == "true";
                result.Message = doc.Descendants(wcf + "Message").FirstOrDefault()?.Value ?? "";
                result.PursuitDocNo = doc.Descendants(wcf + "PursuitDocNo").FirstOrDefault()?.Value;
            }
            catch
            {
                result.Success = response.IsSuccessStatusCode;
                result.Message = response.IsSuccessStatusCode ? "پاسخ دریافت شد" : $"HTTP {(int)response.StatusCode}";
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
        if (msg.Contains("ssl") || msg.Contains("certificate") || msg.Contains("tls")
            || msg.Contains("connection was closed") || msg.Contains("unexpected error occurred on a send"))
            return "شبکه: از همان سروری که سامانه شهرسازی ارسال می‌کند اجرا کنید؛ VPN؛ AllowInvalidSsl=true؛ یا ProxyUrl در appsettings.";
        return "شبکه/فایروال را با IT چک کنید.";
    }
}
