using System.Security.Cryptography;
using System.Text;

namespace MashhadSso;

/// <summary>
/// Same contract as RuleEngineDocuments PublicHelper.getHashSha256(Secret + requestTime).
/// </summary>
public static class CryptoHelper
{
    public static string GetHashSha256(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var hash = SHA256.HashData(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
