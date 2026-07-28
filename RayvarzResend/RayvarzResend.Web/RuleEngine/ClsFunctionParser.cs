using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RayvarzResend.Web.RuleEngine;

public static class ClsFunctionParser
{
    private static readonly Regex VbFunctionRegex = new(
        @"<DisplayName\(""([^""]+)""\)>?\s*Public\s+Function\s+(\w+)\s*\(",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static ClsFunctionDocument Parse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new InvalidOperationException("XmlBody خالی است.");

        var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        var root = doc.Root ?? throw new InvalidOperationException("ریشه XML یافت نشد.");

        var bodyRaw = root.Element("Body")?.Value ?? "";
        var body = WebUtility.HtmlDecode(bodyRaw);

        var functions = new List<string>();
        foreach (Match m in VbFunctionRegex.Matches(body))
        {
            if (m.Groups.Count >= 3)
                functions.Add($"{m.Groups[1].Value} ({m.Groups[2].Value})");
        }

        return new ClsFunctionDocument
        {
            NidClass = ParseInt(root.Element("NidClass")?.Value),
            NidFunction = ParseInt(root.Element("NidFunction")?.Value),
            Name = root.Element("Name")?.Value?.Trim() ?? "",
            DisplayText = root.Element("Text")?.Value?.Trim() ?? "",
            BodySource = body,
            IsActive = bool.TryParse(root.Element("IsActive")?.Value, out var active) && active,
            Version = ParseInt(root.Element("Ver")?.Value),
            FormulaVersion = ParseInt(root.Element("FormulaVersion")?.Value),
            FunctionNames = functions
        };
    }

    private static int ParseInt(string? s) => int.TryParse(s, out var n) ? n : 0;
}
