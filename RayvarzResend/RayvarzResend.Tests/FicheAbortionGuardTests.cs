using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class FicheAbortionGuardTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("1405/07/15", true)]
    public void IsAborted_uses_canceled_date_only(string? date, bool expected) =>
        Assert.Equal(expected, FicheAbortionGuard.IsAborted(date));

    [Fact]
    public void ValidateSendable_blocks_aborted_fiche()
    {
        var fiche = new FicheHeaderDto
        {
            FicheNo = "051205/20581975",
            Payable = 1000,
            CanceledDate = "1405/07/15",
            Rows = [new IncmRowDto { Val = 1000 }]
        };

        Assert.Equal(FicheAbortionGuard.SingleSendBlockMessage, FicheSendService.ValidateSendable(fiche));
    }

    [Fact]
    public void DescribeMiss_says_aborted_not_cancelled()
    {
        var text = UnsentBillPayLookupHelper.DescribeMiss(
            new BillPayMissDiagnostic
            {
                Found = true,
                FicheNo = "051205/20581975",
                Cancelled = true
            },
            "9000359151562",
            "0000333222596");

        Assert.Contains("ابطال شده", text);
        Assert.DoesNotContain("لغو شده", text);
    }

    [Fact]
    public void Repository_sql_uses_canceled_date_not_status_only()
    {
        var repo = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "Services", "FicheRepository.cs")));

        Assert.Contains("FicheAbortionGuard.DutyNotAbortedSql", repo);
        Assert.Contains("FicheAbortionGuard.IncomeNotAbortedSql", repo);
        Assert.DoesNotContain("EumDutyFicheStatus <> 2", repo);
        Assert.DoesNotContain("EumFicheStatus <> 4", repo);
        Assert.DoesNotContain("EumDutyFicheStatus = 2 THEN 1", repo);
        Assert.DoesNotContain("EumFicheStatus = 4 THEN 1", repo);
    }
}
