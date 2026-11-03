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
    public void HasCanceledDate_detects_non_empty(string? date, bool expected) =>
        Assert.Equal(expected, FicheAbortionGuard.HasCanceledDate(date));

    [Fact]
    public void Duty_status_2_is_aborted_even_without_canceled_date()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "051205/20581975",
            CurrentStatus = FicheAbortionGuard.DutyAbortedStatus,
            Payable = 1000,
            Rows = [new IncmRowDto { Val = 1000 }]
        };

        Assert.True(FicheAbortionGuard.IsFicheAborted(fiche));
        Assert.Equal(FicheAbortionGuard.SingleSendBlockMessage, FicheSendService.ValidateSendable(fiche));
    }

    [Fact]
    public void Duty_sendable_when_status_not_2_and_no_canceled_date()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutySenfi,
            FicheNo = "101104/9881711",
            CurrentStatus = 4,
            Payable = 1000,
            BnkAcntNo = "5-8-26-2-2-0-1",
            Rows = [new IncmRowDto { Val = 1000 }]
        };

        Assert.False(FicheAbortionGuard.IsFicheAborted(fiche));
    }

    [Fact]
    public void ValidateSendable_blocks_when_canceled_date_set()
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
    public void Repository_sql_uses_duty_status_2_and_canceled_date()
    {
        FicheAbortionGuard.Configure(new FicheSchemaOptions
        {
            IncomeFicheCanceledDateColumn = "",
            DutyFicheCanceledDateColumn = "CanceledDate"
        });

        var repo = File.ReadAllText(Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..",
            "RayvarzResend.Web", "Services", "FicheRepository.cs")));

        Assert.Contains("FicheAbortionGuard.DutyNotAbortedSql", repo);
        Assert.Contains("FicheAbortionGuard.IncomeNotAbortedSql", repo);
        Assert.Contains("FicheAbortionGuard.SelectCanceledDateSql", repo);
        Assert.Contains("EumDutyFicheStatus <> 2", FicheAbortionGuard.DutyNotAbortedSql);
        Assert.Contains("EumFicheStatus <> 4", FicheAbortionGuard.IncomeNotAbortedSql);
        Assert.Contains("EumDutyFicheStatus = 2", FicheAbortionGuard.DutyAbortedCaseSql);
        Assert.Contains("CanceledDate", FicheAbortionGuard.DutyNotAbortedSql);
        Assert.DoesNotContain("CanceledDate", FicheAbortionGuard.IncomeNotAbortedSql);
    }

    [Fact]
    public void Income_aborted_sql_omits_missing_canceled_date_column()
    {
        FicheAbortionGuard.Configure(new FicheSchemaOptions { IncomeFicheCanceledDateColumn = "" });
        Assert.DoesNotContain("CanceledDate", FicheAbortionGuard.IncomeAbortedCaseSqlAliasX);
        Assert.Equal("CAST(NULL AS nvarchar(30)) AS CanceledDate",
            FicheAbortionGuard.SelectCanceledDateSql("f", isDuty: false));
    }
}
