using System.Text;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class SsoApiSecretHashTests
{
    [Fact]
    public void Sha256AsciiHexLower_matches_rule_engine_style_loop()
    {
        const string password = "secret1700000000";
        using var crypt = System.Security.Cryptography.SHA256.Create();
        var crypto = crypt.ComputeHash(Encoding.ASCII.GetBytes(password));
        var expected = string.Empty;
        foreach (var bit in crypto)
            expected += bit.ToString("x2");

        Assert.Equal(expected, SsoApiSecretHash.Sha256AsciiHexLower(password));
        Assert.Equal(expected, SsoApiSecretHash.ComputeApiSecret("secret", "1700000000", "lower"));
    }
}
