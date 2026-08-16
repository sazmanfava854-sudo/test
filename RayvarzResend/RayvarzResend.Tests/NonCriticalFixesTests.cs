using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class NonCriticalFixesTests
{
    [Fact]
    public void NumericHelper_decimal_string_truncates_not_concatenates()
    {
        Assert.Equal(123L, NumericHelper.TryParseLegacyLong("123.45"));
        Assert.Equal(123L, NumericHelper.TryParseLegacyLong("123,45"));
        Assert.NotEqual(12345L, NumericHelper.TryParseLegacyLong("123.45"));
    }

    [Fact]
    public void Tahator_amount_null_group_defaults_to_157()
    {
        var fiche = new RayvarzResend.Web.Models.FicheHeaderDto
        {
            Category = RayvarzResend.Web.Models.FicheCategory.Income,
            BankCode = "4",
            Deposit = 1,
            Payable = 10m
        };
        TahatorRowBuilder.ApplyTahatorAmountRows(fiche);
        Assert.Equal(157, fiche.IncomeAccountGroup);
    }

    [Fact]
    public void Program_config_branch_209_matches_bank18_fund()
    {
        Assert.Equal(200209008, DutyDistrictBranchResolver.ResolveFund(209, "18"));
    }
}
