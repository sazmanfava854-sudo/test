using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class ShahkarSearchHelperTests
{
    [Fact]
    public void Validate_requires_at_least_one_field()
    {
        Assert.NotNull(ShahkarSearchHelper.ValidateSearchRequest(new ShahkarSearchRequest()));
        Assert.Null(ShahkarSearchHelper.ValidateSearchRequest(new ShahkarSearchRequest { UserName = "a-b" }));
    }

    [Fact]
    public void BuildSearch_where_includes_username_and_national_code()
    {
        var (where, parameters) = ShahkarSearchHelper.BuildSearchWhere(new ShahkarSearchRequest
        {
            UserName = "ghobasefidi-s",
            NationalCode = "0922808074"
        });
        Assert.Contains("UserName", where);
        Assert.Contains("NationalCode", where);
        Assert.Equal("ghobasefidi-s", parameters["userName"]);
        Assert.Equal("0922808074", parameters["nationalCode"]);
    }
}
