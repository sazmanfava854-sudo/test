using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AuthTests
{
    [Fact]
    public void PasswordHasher_hash_and_verify_roundtrip()
    {
        var hash = PasswordHasherUtil.Hash("Secret@123");
        Assert.True(PasswordHasherUtil.Verify("Secret@123", hash));
        Assert.False(PasswordHasherUtil.Verify("wrong", hash));
    }

    [Fact]
    public void PasswordHasher_rejects_invalid_stored_format()
    {
        Assert.False(PasswordHasherUtil.Verify("x", ""));
        Assert.False(PasswordHasherUtil.Verify("x", "not-valid"));
        Assert.False(PasswordHasherUtil.IsStoredHashFormat("not-valid"));
    }

    [Fact]
    public void PasswordHasher_recognizes_app_hash_format()
    {
        var hash = PasswordHasherUtil.Hash("x");
        Assert.True(PasswordHasherUtil.IsStoredHashFormat(hash));
    }
}
