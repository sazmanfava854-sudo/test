using System.Security.Cryptography;
using System.Text;

namespace RayvarzResend.Web.Services;

public static class SsoApiSecretHash
{
    /// <summary>همان الگوی RuleEngine: SHA256 + Encoding.ASCII + hex با x2.</summary>
    public static string Sha256AsciiHexLower(string input)
    {
        var password = input ?? "";
        var crypto = SHA256.HashData(Encoding.ASCII.GetBytes(password));
        var hash = string.Empty;
        foreach (var bit in crypto)
            hash += bit.ToString("x2");
        return hash;
    }

    public static string Sha256AsciiHexUpper(string input)
    {
        var password = input ?? "";
        var crypto = SHA256.HashData(Encoding.ASCII.GetBytes(password));
        var hash = string.Empty;
        foreach (var bit in crypto)
            hash += bit.ToString("X2");
        return hash;
    }

    /// <summary>apiSecret سند SSO: SHA256(AppSecretKey + requestTime) به hex (ASCII).</summary>
    public static string ComputeApiSecret(string secret, string requestTime, string? encoding)
    {
        var raw = (secret ?? "") + (requestTime ?? "");
        return string.Equals(encoding, "upper", StringComparison.OrdinalIgnoreCase)
            ? Sha256AsciiHexUpper(raw)
            : Sha256AsciiHexLower(raw);
    }

    /// <summary>فرمول‌های محتمل هدر apiSecret — برای تشخیص وقتی SSO 403 می‌دهد.</summary>
    public static IReadOnlyList<(string Name, Func<string, string, string> Compute)> ProbeVariants { get; } =
    [
        ("sha256(secret+time) ASCII hex lower", (s, t) => Sha256AsciiHexLower(s + t)),
        ("sha256(secret+time) ASCII hex upper", (s, t) => Sha256AsciiHexUpper(s + t)),
        ("sha256(secret+time) UTF8 hex lower", (s, t) => Sha256Utf8HexLower(s + t)),
        ("sha256(time+secret) ASCII hex lower", (s, t) => Sha256AsciiHexLower(t + s)),
        ("sha256(secret+time) base64 UTF8", (s, t) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(s + t)))),
        ("md5(secret+time) ASCII hex lower", (s, t) => Convert.ToHexString(MD5.HashData(Encoding.ASCII.GetBytes(s + t))).ToLowerInvariant()),
        ("sha256(secret) ASCII hex lower (بدون time)", (s, _) => Sha256AsciiHexLower(s)),
        ("raw secret (بدون هش)", (s, _) => s)
    ];

    private static string Sha256Utf8HexLower(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input ?? ""))).ToLowerInvariant();
}
