using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class AppUserDomainNormalizerTests
{
    [Theory]
    [InlineData("hoseine-sh", "hoseine-sh")]
    [InlineData(@"MASHHAD\hoseine-sh", "hoseine-sh")]
    [InlineData("hoseine-sh@mashhad.ir", "hoseine-sh")]
    [InlineData("  hoseine-sh  ", "hoseine-sh")]
    public void Normalize_strips_prefix_and_suffix(string input, string expected)
    {
        Assert.Equal(expected, AppUserDomainNormalizer.Normalize(input));
    }

    [Fact]
    public void IsValid_accepts_sample_domain()
    {
        Assert.True(AppUserDomainNormalizer.IsValid("hoseine-sh"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("bad domain")]
    public void IsValid_rejects_empty_or_invalid(string input)
    {
        Assert.False(AppUserDomainNormalizer.IsValid(input));
    }
}
