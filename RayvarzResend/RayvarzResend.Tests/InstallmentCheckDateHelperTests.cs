using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class InstallmentCheckDateHelperTests
{
    [Fact]
    public void BuildCommentPrefix_includes_user_and_date()
    {
        var prefix = InstallmentCheckDateHelper.BuildCommentPrefix("op1", "1405/09/25");
        Assert.Contains("1405/09/25", prefix);
        Assert.Contains("op1", prefix);
        Assert.EndsWith(" ", prefix);
    }
}
