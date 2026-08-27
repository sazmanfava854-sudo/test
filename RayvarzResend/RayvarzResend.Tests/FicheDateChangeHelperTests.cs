using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class FicheDateChangeHelperTests
{
    [Fact]
    public void BuildCommentPrefix_includes_today_and_user()
    {
        var prefix = FicheDateChangeHelper.BuildCommentPrefix("karimi");
        Assert.Contains("karimi", prefix);
        Assert.Contains("تاریخ مهلت پرداخت و تاریخ صدور و وضعیت فیش", prefix);
        Assert.StartsWith("(", prefix);
    }

    [Fact]
    public void NormalizeSlashDate_accepts_slash_and_digits()
    {
        Assert.Equal("1404/01/11", FicheDateChangeHelper.NormalizeSlashDate("1404/01/11"));
        Assert.Equal("1404/01/11", FicheDateChangeHelper.NormalizeSlashDate("14040111"));
    }

    [Fact]
    public void StatusLabel_returns_persian_label()
    {
        Assert.Equal("صدوردایم", FicheDateChangeHelper.StatusLabel(1));
        Assert.Equal("ابطال", FicheDateChangeHelper.StatusLabel(4));
    }

    [Fact]
    public void HasAnySearchFilter_requires_at_least_one_filter()
    {
        Assert.False(FicheDateChangeHelper.HasAnySearchFilter(new FicheDateChangeSearchRequest()));
        Assert.True(FicheDateChangeHelper.HasAnySearchFilter(new FicheDateChangeSearchRequest
        {
            AccountGroupTitle = "شهردار"
        }));
    }

    [Fact]
    public void HasAnyChange_detects_selected_fields()
    {
        Assert.False(FicheDateChangeHelper.HasAnyChange(new FicheDateChangeUpdateRequest
        {
            ApplyEumFicheStatus = false
        }));
        Assert.True(FicheDateChangeHelper.HasAnyChange(new FicheDateChangeUpdateRequest
        {
            ApplyEumFicheStatus = true,
            NewEumFicheStatus = 1
        }));
        Assert.True(FicheDateChangeHelper.HasAnyChange(new FicheDateChangeUpdateRequest
        {
            ApplyPaymentBreakDate = true,
            NewPaymentBreakDate = "1404/01/20"
        }));
    }

    [Fact]
    public void DefaultFicheStatus_is_permanent_export()
    {
        Assert.Equal(1, FicheDateChangeHelper.DefaultFicheStatus);
    }
}
