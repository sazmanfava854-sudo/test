using System.Xml.Linq;

namespace RayvarzResend.Web.Services;

internal static class RayvarzSoapResponseParser
{
    private static readonly XNamespace Wcf = "http://schemas.datacontract.org/2004/07/WCFServer";

    public static (bool? Success, string Message, string? PursuitDocNo, string? FaultSummary) Parse(string body)
    {
        var doc = XDocument.Parse(body);
        XNamespace con = "http://www.bea.com/wli/sb/context";

        var faultCode = doc.Descendants(con + "errorCode").FirstOrDefault()?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultcode")?.Value;
        var faultReason = doc.Descendants(con + "reason").FirstOrDefault()?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value;

        if (!string.IsNullOrWhiteSpace(faultCode) || !string.IsNullOrWhiteSpace(faultReason))
        {
            var fault = $"SOAP Fault: {faultCode} — {faultReason}".Trim(' ', '—');
            return (false, fault, null, fault);
        }

        var successEl = doc.Descendants(Wcf + "Success").FirstOrDefault()
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Success");
        bool? success = successEl?.Value switch
        {
            "true" => true,
            "false" => false,
            _ => null
        };

        var message = doc.Descendants(Wcf + "Message").FirstOrDefault()?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Message" && e.Parent?.Name.LocalName != "Header")?.Value
            ?? "";

        if (string.IsNullOrWhiteSpace(message))
        {
            message = doc.Descendants()
                .Where(e => e.Name.LocalName is "Detail" or "InnerException" or "ExceptionMessage")
                .Select(e => e.Value?.Trim())
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? "";
        }

        var pursuit = doc.Descendants(Wcf + "PursuitDocNo").FirstOrDefault()?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "PursuitDocNo")?.Value;

        return (success, message.Trim(), pursuit, null);
    }
}
