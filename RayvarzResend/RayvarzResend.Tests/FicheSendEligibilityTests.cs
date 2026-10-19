using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class FicheSendEligibilityTests
{
    [Theory]
    [InlineData(true, false, "رایورز")]
    [InlineData(false, true, "Accounting_DocHeader")]
    [InlineData(true, true, "رایورز و Accounting_DocHeader")]
    public void BuildBlockReason_when_duplicate_in_rayvarz_or_header(bool inRay, bool inHeader, string fragment)
    {
        var reason = FicheSendEligibility.BuildBlockReason(
            new FicheSendEligibility.Result(inRay, inHeader, RayvarzQueryFailed: false));
        Assert.NotNull(reason);
        Assert.Contains(fragment, reason, StringComparison.Ordinal);
        Assert.Contains("ارسال نشد", reason, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildBlockReason_null_when_clean_and_rayvarz_ok()
    {
        Assert.Null(FicheSendEligibility.BuildBlockReason(
            new FicheSendEligibility.Result(false, false, RayvarzQueryFailed: false)));
    }

    [Fact]
    public void BuildBlockReason_null_when_only_rayvarz_query_failed()
    {
        Assert.Null(FicheSendEligibility.BuildBlockReason(
            new FicheSendEligibility.Result(false, false, RayvarzQueryFailed: true)));
    }

    [Fact]
    public void IsBlocked_matches_income_duty_and_tahator_rules()
    {
        Assert.True(FicheSendEligibility.IsBlocked(new FicheSendEligibility.Result(true, false, false)));
        Assert.True(FicheSendEligibility.IsBlocked(new FicheSendEligibility.Result(false, true, false)));
        Assert.False(FicheSendEligibility.IsBlocked(new FicheSendEligibility.Result(false, false, false)));
    }
}
