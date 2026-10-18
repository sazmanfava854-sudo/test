using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Abcdefg1")] // no special
    [InlineData("abcdefg@")] // no upper
    [InlineData("ABCDEFG@")] // no lower
    [InlineData("Ab@1")] // too short
    public void Rejects_weak_passwords(string password)
    {
        Assert.NotNull(PasswordPolicy.Validate(password));
    }

    [Theory]
    [InlineData("Abcdefg@1")]
    [InlineData("Secret@123")]
    public void Accepts_valid_passwords(string password)
    {
        Assert.Null(PasswordPolicy.Validate(password));
    }
}
