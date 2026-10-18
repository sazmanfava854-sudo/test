using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class CostCenterHelperTests
{
    [Fact]
    public void ExpandSearchFicheKeys_supports_multiple_tokens()
    {
        var keys = CostCenterHelper.ExpandSearchFicheKeys("050533547469, 050533547470");
        Assert.Contains("050533547469", keys);
        Assert.Contains("050533547470", keys);
    }

    [Fact]
    public void TryParseCenterValue_accepts_digits_only()
    {
        Assert.True(CostCenterHelper.TryParseCenterValue("301202530", out var deposit));
        Assert.Equal(301202530L, deposit);
        Assert.False(CostCenterHelper.TryParseCenterValue("abc", out _));
    }
}
