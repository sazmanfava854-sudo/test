using System.Security.Cryptography;
using System.Text;

namespace RayvarzResend.Web.Services;

public static class SsoApiSecretHash
{
    public static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input ?? ""));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
