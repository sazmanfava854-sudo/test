namespace RayvarzResend.Web.RuleEngine.Parser;

public sealed class XmlEnvelope
{
    public ClsFunctionDocument Document { get; init; } = new();
    public string CanonicalXml { get; init; } = "";
    public string XmlHash { get; init; } = "";
    public string Source { get; init; } = "";
}

/// <summary>استخراج Body و متادیتا از XmlBody Member/History.</summary>
public static class XmlEnvelopeReader
{
    public static XmlEnvelope Read(string xml, string source = "xml")
    {
        if (string.IsNullOrWhiteSpace(xml))
            throw new InvalidOperationException("XmlBody خالی است.");

        var canonical = XmlCanonicalizer.Normalize(xml);
        var hash = RuleHashService.ComputeSha256Hex(canonical);
        var document = ClsFunctionParser.Parse(xml);

        return new XmlEnvelope
        {
            Document = document,
            CanonicalXml = canonical,
            XmlHash = hash,
            Source = source
        };
    }
}
