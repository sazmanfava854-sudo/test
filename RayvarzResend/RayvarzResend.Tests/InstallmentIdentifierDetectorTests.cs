using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class InstallmentIdentifierDetectorTests
{
    [Theory]
    [InlineData("809552", InstallmentLookupKind.NoDocument)]
    [InlineData("388749", InstallmentLookupKind.NoDocument)]
    [InlineData("0212280614002187", InstallmentLookupKind.TrackingNo)]
    public void Detect_uses_digit_length(string raw, InstallmentLookupKind expected)
    {
        Assert.Equal(expected, InstallmentIdentifierDetector.Detect(raw));
    }

    [Fact]
    public void WillApplyEndState_no_document_always_true()
    {
        Assert.True(InstallmentIdentifierDetector.WillApplyEndState(InstallmentLookupKind.NoDocument, false));
    }

    [Fact]
    public void WillApplyEndState_tracking_no_follows_checkbox()
    {
        Assert.False(InstallmentIdentifierDetector.WillApplyEndState(InstallmentLookupKind.TrackingNo, false));
        Assert.True(InstallmentIdentifierDetector.WillApplyEndState(InstallmentLookupKind.TrackingNo, true));
    }
}
