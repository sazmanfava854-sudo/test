using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class FicheBranchResolverTests
{
    [Theory]
    [InlineData(9, 209)]
    [InlineData(80, 218)]
    [InlineData(218, 218)]
    [InlineData(12, 212)]
    [InlineData(209, 209)]
    public void MapRegionToBranch_MapsKnownRegions(int region, int expectedBranch)
    {
        Assert.Equal(expectedBranch, FicheBranchResolver.MapRegionToBranch(region));
    }

    [Fact]
    public void TryResolve_FailsWhenRegionUnknown()
    {
        var fiche = new FicheHeaderDto { IncomeRegion = "99" };
        var ok = FicheBranchResolver.TryResolve(fiche, out var branch, out var fund, out var error);
        Assert.False(ok);
        Assert.Equal(0, branch);
        Assert.Equal(0, fund);
        Assert.Equal(FicheBranchResolver.RegionNotResolvedMessage, error);
    }

    [Fact]
    public void TryResolve_UsesResolvedDistrictBranch()
    {
        var fiche = new FicheHeaderDto
        {
            ResolvedDistrictBranch = 209,
            SuggestedFund = 200209008,
            BankCode = "18"
        };
        Assert.True(FicheBranchResolver.TryResolve(fiche, out var branch, out var fund, out _));
        Assert.Equal(209, branch);
        Assert.Equal(200209008, fund);
    }

    [Fact]
    public void TryResolve_UsesIncomeRegion()
    {
        var fiche = new FicheHeaderDto { IncomeRegion = "7", BankCode = "18" };
        Assert.True(FicheBranchResolver.TryResolve(fiche, out var branch, out var fund, out _));
        Assert.Equal(207, branch);
        Assert.True(fund > 0);
    }
}
