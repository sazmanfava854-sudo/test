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
}
