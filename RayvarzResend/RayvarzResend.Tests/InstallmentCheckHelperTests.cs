using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class InstallmentCheckHelperTests
{
    [Fact]
    public void BuildCommentPrefix_includes_user_when_provided()
    {
        Assert.Equal("تغییر وضعیت چک به خزانه توسط admin ", InstallmentCheckHelper.BuildCommentPrefix("admin"));
    }

    [Fact]
    public void BuildCommentPrefix_without_user_uses_generic_text()
    {
        Assert.Equal("تغییر وضعیت چک به خزانه", InstallmentCheckHelper.BuildCommentPrefix("").TrimEnd());
    }

    [Fact]
    public void BuildNewComments_appends_existing_without_separator_when_both_present()
    {
        var result = InstallmentCheckHelper.BuildNewComments("karimi", "یادداشت قبلی");
        Assert.Equal("تغییر وضعیت چک به خزانه توسط karimi یادداشت قبلی", result);
    }

    [Fact]
    public void ParseIdentifierList_splits_commas_and_newlines()
    {
        var list = InstallmentCheckHelper.ParseIdentifierList("388749, 515987\n515983");
        Assert.Equal(3, list.Count);
        Assert.Contains("388749", list);
        Assert.Contains("515987", list);
        Assert.Contains("515983", list);
    }

    [Fact]
    public void ParseIdentifierList_deduplicates_case_insensitive()
    {
        var list = InstallmentCheckHelper.ParseIdentifierList("abc, ABC, abc");
        Assert.Single(list);
        Assert.Equal("abc", list[0]);
    }

    [Fact]
    public void Treasury_and_end_state_constants_match_sampa()
    {
        Assert.Equal("28", InstallmentCheckHelper.TreasuryStatus);
        Assert.Equal("عودت", InstallmentCheckHelper.EndStateDescOdooat);
        Assert.Equal("17", InstallmentCheckHelper.EndStateCodeOdooat);
    }
}
