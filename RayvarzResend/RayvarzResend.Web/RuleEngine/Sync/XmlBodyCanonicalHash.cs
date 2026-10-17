using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RayvarzResend.Web.RuleEngine.Sync;

/// <summary>هش پایدار XmlBody برای مقایسه نسخه‌ها (فاز ۲ — canonical ساده).</summary>
public static partial class XmlBodyCanonicalHash
{
    public static string Compute(string? xmlBody)
    {
        if (string.IsNullOrWhiteSpace(xmlBody))
            return "";

        var normalized = Normalize(xmlBody);
        var bytes = Encoding.UTF8.GetBytes(normalized);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static string Normalize(string xmlBody)
    {
        var s = xmlBody.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Trim();
        s = WhitespaceCollapse().Replace(s, " ");
        return s;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceCollapse();
}
