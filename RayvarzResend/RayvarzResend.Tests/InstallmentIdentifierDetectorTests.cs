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
    public void WillApplyEndState_follows_checkbox_for_both_kinds()
    {
        Assert.False(InstallmentIdentifierDetector.WillApplyEndState(InstallmentLookupKind.NoDocument, false));
        Assert.True(InstallmentIdentifierDetector.WillApplyEndState(InstallmentLookupKind.NoDocument, true));
        Assert.False(InstallmentIdentifierDetector.WillApplyEndState(InstallmentLookupKind.TrackingNo, false));
        Assert.True(InstallmentIdentifierDetector.WillApplyEndState(InstallmentLookupKind.TrackingNo, true));
    }

    [Theory]
    [InlineData("0502090614002610", "0502090614002610", true)]
    [InlineData("502090614002610", "0502090614002610", true)]
    [InlineData("0502090614002610", "502090614002610", true)]
    [InlineData("502090614002610", "502090614002611", false)]
    public void TrackingNoDigitsMatch_handles_missing_leading_zero(string a, string b, bool expected)
    {
        Assert.Equal(expected, InstallmentIdentifierDetector.TrackingNoDigitsMatch(a, b));
    }

    [Fact]
    public void BuildTrackingNoWhereClause_is_balanced_sql()
    {
        var clause = InstallmentIdentifierDetector.BuildTrackingNoWhereClause("il.TrackingNo");
        Assert.Contains("CONCAT('0', @v)", clause);
        Assert.Equal(clause.Count(c => c == '('), clause.Count(c => c == ')'));
    }
}
