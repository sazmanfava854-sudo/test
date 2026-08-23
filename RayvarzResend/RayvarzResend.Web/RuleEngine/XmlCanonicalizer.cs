using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace RayvarzResend.Web.RuleEngine;

public static class XmlCanonicalizer
{
    public static string Normalize(string xml)
    {
        var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        return doc.ToString(SaveOptions.DisableFormatting);
    }
}

public static class RuleHashService
{
    public static string ComputeSha256Hex(string canonicalXml)
    {
        var bytes = Encoding.UTF8.GetBytes(canonicalXml);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
