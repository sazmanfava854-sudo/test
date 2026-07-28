using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RayvarzResend.Web.Services;

internal static class RayvarzSoapResponseParser
{
    private static readonly XNamespace Wcf = "http://schemas.datacontract.org/2004/07/WCFServer";

    private static readonly Regex SuccessRegex = new(
        @"<(?:[\w]+:)?Success>\s*(true|false)\s*</(?:[\w]+:)?Success>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex MessageRegex = new(
        @"<(?:[\w]+:)?Message>([\s\S]*?)</(?:[\w]+:)?Message>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex PursuitRegex = new(
        @"<(?:[\w]+:)?PursuitDocNo[^>]*>([^<]*)</(?:[\w]+:)?PursuitDocNo>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static (bool? Success, string Message, string? PursuitDocNo, string? FaultSummary) Parse(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return (null, "", null, null);

        XDocument? doc = null;
        try
        {
            doc = XDocument.Parse(SanitizeXmlForParse(body));
        }
        catch
        {
            return ParseWithRegexFallback(body);
        }

        XNamespace con = "http://www.bea.com/wli/sb/context";

        var faultCode = doc.Descendants(con + "errorCode").FirstOrDefault()?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultcode")?.Value;
        var faultReason = doc.Descendants(con + "reason").FirstOrDefault()?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "Text" && e.Parent?.Name.LocalName == "Reason")?.Value;

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

        message = FormatRayvarzBusinessMessage(message);

        var pursuit = doc.Descendants(Wcf + "PursuitDocNo").FirstOrDefault()?.Value
            ?? doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "PursuitDocNo")?.Value;

        if (pursuit is not null && pursuit.Contains("nil", StringComparison.OrdinalIgnoreCase))
            pursuit = null;

        return (success, message.Trim(), string.IsNullOrWhiteSpace(pursuit) ? null : pursuit.Trim(), null);
    }

    private static (bool? Success, string Message, string? PursuitDocNo, string? FaultSummary) ParseWithRegexFallback(string body)
    {
        if (body.Contains("s:Fault", StringComparison.OrdinalIgnoreCase)
            || body.Contains(":Fault>", StringComparison.OrdinalIgnoreCase))
        {
            var reasonMatch = MessageRegex.Match(body);
            var faultText = reasonMatch.Success ? reasonMatch.Groups[1].Value : body;
            return (false, FormatRayvarzBusinessMessage(faultText), null, "SOAP Fault (regex fallback)");
        }

        var successMatch = SuccessRegex.Match(body);
        bool? success = successMatch.Success
            ? successMatch.Groups[1].Value.Equals("true", StringComparison.OrdinalIgnoreCase)
            : null;

        var messageMatch = MessageRegex.Match(body);
        var message = messageMatch.Success ? messageMatch.Groups[1].Value : "";
        message = FormatRayvarzBusinessMessage(message);

        var pursuitMatch = PursuitRegex.Match(body);
        var pursuit = pursuitMatch.Success ? pursuitMatch.Groups[1].Value.Trim() : null;
        if (string.IsNullOrWhiteSpace(pursuit) || pursuit.Contains("nil", StringComparison.OrdinalIgnoreCase))
            pursuit = null;

        return (success, message.Trim(), pursuit, null);
    }

    internal static string SanitizeXmlForParse(string body)
    {
        if (string.IsNullOrEmpty(body))
            return body;

        var sb = new System.Text.StringBuilder(body.Length);
        foreach (var ch in body)
        {
            if (ch is '\t' or '\n' or '\r' or >= '\u0020')
                sb.Append(ch);
        }

        return sb.ToString();
    }

    internal static string FormatRayvarzBusinessMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "";

        var decoded = System.Net.WebUtility.HtmlDecode(raw);
        decoded = decoded.Replace('\u001E', '|').Replace('\u001F', ' ').Trim();

        var stackIdx = decoded.IndexOf(" at WCFServer.", StringComparison.Ordinal);
        if (stackIdx > 0)
            decoded = decoded[..stackIdx].Trim();

        var lines = new List<string>();

        if (decoded.Contains("تراکنش تکراری", StringComparison.Ordinal))
            lines.Add("تراکنش تکراری");

        foreach (var part in decoded.Split(new[] { '|', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (part.Contains("تراکنش تکراری", StringComparison.Ordinal))
                continue;
            if (part.Any(c => c >= '\u0600' && c <= '\u06FF'))
                lines.Add(part);
        }

        if (lines.Count > 0)
            return string.Join(" — ", lines.Distinct(StringComparer.Ordinal));

        return decoded.Trim();
    }
}
