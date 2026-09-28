using System.Security.Cryptography;
using System.Text;

namespace RayvarzResend.Web.Services;

public static class SsoApiSecretHash
{
    public static string Sha256HexLower(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input ?? ""))).ToLowerInvariant();

    public static string Sha256HexUpper(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input ?? "")));

    public static string ComputeApiSecret(string secret, string requestTime, string? encoding)
    {
        var raw = (secret ?? "") + (requestTime ?? "");
        return string.Equals(encoding, "upper", StringComparison.OrdinalIgnoreCase)
            ? Sha256HexUpper(raw)
            : Sha256HexLower(raw);
    }

    /// <summary>فرمول‌های محتمل هدر apiSecret — برای پیدا کردن فرمول درست وقتی SSO 403 می‌دهد.</summary>
    public static IReadOnlyList<(string Name, Func<string, string, string> Compute)> ProbeVariants { get; } =
    [
        ("sha256(secret+time) hex lower", (s, t) => Sha256HexLower(s + t)),
        ("sha256(secret+time) hex upper", (s, t) => Sha256HexUpper(s + t)),
        ("sha256(time+secret) hex lower", (s, t) => Sha256HexLower(t + s)),
        ("sha256(time+secret) hex upper", (s, t) => Sha256HexUpper(t + s)),
        ("sha256(secret+time) base64", (s, t) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(s + t)))),
        ("md5(secret+time) hex lower", (s, t) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(s + t))).ToLowerInvariant()),
        ("md5(secret+time) hex upper", (s, t) => Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(s + t)))),
        ("sha256(secret) hex lower (بدون time)", (s, _) => Sha256HexLower(s)),
        ("raw secret (بدون هش)", (s, _) => s)
    ];
}
