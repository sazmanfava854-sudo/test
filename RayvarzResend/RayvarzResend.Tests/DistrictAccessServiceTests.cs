using System.Security.Claims;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class DistrictAccessServiceTests
{
    [Theory]
    [InlineData("2", "2")]
    [InlineData("202", "2")]
    [InlineData("218", "218")]
    [InlineData("80", "218")]
    [InlineData("102", "102")]
    public void NormalizeDistrict_maps_branch_and_region_codes(string input, string expected) =>
        Assert.Equal(expected, DistrictAccessService.NormalizeDistrict(input));

    [Fact]
    public void CanAccessFiche_admin_always_allowed()
    {
        var admin = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(AuthClaimTypes.IsAdmin, "1")
        }, "test"));

        var fiche = new FicheHeaderDto { ResolvedDistrictBranch = 203 };
        Assert.True(DistrictAccessService.CanAccessFiche(admin, fiche));
    }

    [Fact]
    public void CanAccessFiche_regional_user_same_district_allowed()
    {
        var user = RegionalUser("2");
        var fiche = new FicheHeaderDto { ResolvedDistrictBranch = 202 };
        Assert.True(DistrictAccessService.CanAccessFiche(user, fiche));
    }

    [Fact]
    public void CanAccessFiche_regional_user_other_district_denied()
    {
        var user = RegionalUser("2");
        var fiche = new FicheHeaderDto { IncomeRegion = "3" };
        Assert.False(DistrictAccessService.CanAccessFiche(user, fiche));
    }

    [Fact]
    public void CanAccessFiche_center_user_tahator157_allowed()
    {
        var user = RegionalUser("102");
        var fiche = new FicheHeaderDto { IncomeAccountGroup = TahatorRowBuilder.IncomeAccountGroupTahatorAmount };
        Assert.True(DistrictAccessService.CanAccessFiche(user, fiche));
    }

    [Fact]
    public void CanAccessFiche_center_user_regional_fiche_denied()
    {
        var user = RegionalUser("102");
        var fiche = new FicheHeaderDto { ResolvedDistrictBranch = 202 };
        Assert.False(DistrictAccessService.CanAccessFiche(user, fiche));
    }

    private static ClaimsPrincipal RegionalUser(string district) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(AuthClaimTypes.District, district),
            new Claim(AuthClaimTypes.IsAdmin, "0")
        }, "test"));
}
