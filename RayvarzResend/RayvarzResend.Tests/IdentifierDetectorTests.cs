using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class IdentifierDetectorTests
{
    [Theory]
    [InlineData("101104/9881711", IdentifierType.FicheNo)]
    [InlineData("040933318150", IdentifierType.FicheNo)]
    [InlineData("90000273516650048062122597", IdentifierType.BillPaymentKey)]
    public void Detect_ClassifiesInput(string input, IdentifierType expected)
    {
        Assert.Equal(expected, IdentifierDetector.Detect(input));
    }

    [Fact]
    public void Detect_EmptyDefaultsToFicheNo()
    {
        Assert.Equal(IdentifierType.FicheNo, IdentifierDetector.Detect(""));
        Assert.Equal(IdentifierType.FicheNo, IdentifierDetector.Detect("   "));
    }
}
