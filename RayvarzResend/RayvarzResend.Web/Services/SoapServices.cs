using System.Net;
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
        var txnId = fiche.NidFiche.ToString();
        var rows = NormalizeRows(fiche);

        var incmItems = string.Join("\n", rows.Select((r, i) => $@"
              <wcf:DocumentItemIncm>
                <wcf:Due>{docDateRay}</wcf:Due>
                <wcf:Id>{txnId}</wcf:Id>
                <wcf:IncmNo>{r.IncmNo}</wcf:IncmNo>
                <wcf:IncmRow>{i + 1}</wcf:IncmRow>
                <wcf:IncmRowDsc>{Escape(r.IncmRowDsc)}</wcf:IncmRowDsc>
                <wcf:Qty>{fiche.Payable:0}</wcf:Qty>
                <wcf:Ref>{Escape(fiche.FicheNo)}</wcf:Ref>
                <wcf:RefRowDocNo>0</wcf:RefRowDocNo>
                <wcf:SourceId>{txnId}</wcf:SourceId>
                <wcf:Val>{r.Val:0}</wcf:Val>
              </wcf:DocumentItemIncm>"));

        var refRecon = string.IsNullOrWhiteSpace(fiche.RefReconstructionNo)
            ? "" : $"<wcf:RefreconstructionNo>{Escape(fiche.RefReconstructionNo)}</wcf:RefreconstructionNo>";

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
        <wcf:DocTypDsc>عوارض سرا</wcf:DocTypDsc>
        <wcf:Items>
          <wcf:DocumentItem>
            <wcf:ActDate>{docDateRay}</wcf:ActDate>
            <wcf:ActTyp>3</wcf:ActTyp>
            <wcf:Bank>{Escape(fiche.PaymentBranch)}</wcf:Bank>
            <wcf:BnkAcntNo>{Escape(fiche.BnkAcntNo)}</wcf:BnkAcntNo>
            <wcf:DocRow>1</wcf:DocRow>
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
        <wcf:TransactionId>{txnId}</wcf:TransactionId>
      </tem:doc>
    </tem:SaveDocument>
  </soap:Body>
</soap:Envelope>";
    }

    private static List<IncmRowDto> NormalizeRows(FicheHeaderDto fiche)
    {
        var rows = fiche.Rows.Where(r => r.Val != 0).ToList();
        if (rows.Count == 0)
            rows.Add(new IncmRowDto { IncmNo = 0, Val = fiche.Payable, IncmRowDsc = "کل" });

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

        var url = _config["Rayvarz:ServiceUrl"] ?? "";
        var action = _config["Rayvarz:SoapAction"] ?? "";

        using var client = new HttpClient();
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
}
