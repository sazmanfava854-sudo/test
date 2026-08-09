using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class FicheDateResolverTests
{
    [Theory]
    [InlineData(1, "1404/01/15", "1404/02/20", "14040115")]
    [InlineData(2, "1404/01/15", "1404/02/20", "14040220")]
    public void ResolvePaymentDateByStatus_UsesStatusPriority(int status, string payment, string bank, string expected)
    {
        var result = FicheDateResolver.ResolvePaymentDateByStatus(status, payment, bank);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ResolvePaymentDateByStatus_FallsBackToOtherColumn()
    {
        Assert.Equal("14040220", FicheDateResolver.ResolvePaymentDateByStatus(1, "", "1404/02/20"));
        Assert.Equal("14040115", FicheDateResolver.ResolvePaymentDateByStatus(3, "1404/01/15", ""));
    }
}
