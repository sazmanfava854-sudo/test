using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class FicheSendServiceTests
{
    [Fact]
    public void ApplySendStatus_SetsBlockReasonWhenRegionUnknown()
    {
        var fiche = new FicheHeaderDto { IncomeRegion = "99", Payable = 1_000_000m, Rows = { new IncmRowDto { IncmNo = 1, Val = 1_000_000m } } };
        FicheSendService.ApplySendStatus(fiche);
        Assert.False(fiche.CanSend);
        Assert.Equal(FicheBranchResolver.RegionNotResolvedMessage, fiche.BlockReason);
        Assert.Equal(fiche.BlockReason, fiche.StatusMessage);
    }

    [Fact]
    public void ApplySendStatus_ReadyWhenValid()
    {
        var fiche = new FicheHeaderDto
        {
            IncomeRegion = "7",
            BankCode = "18",
            Payable = 500_000m,
            Rows = { new IncmRowDto { IncmNo = 1, Val = 500_000m } }
        };
        FicheSendService.ApplySendStatus(fiche);
        Assert.True(fiche.CanSend);
        Assert.Null(fiche.BlockReason);
        Assert.Equal("آماده ارسال", fiche.StatusMessage);
    }

    [Fact]
    public void ApplySendStatus_BlocksDuplicateInRayvarz()
    {
        var fiche = new FicheHeaderDto
        {
            ExistsInRayvarz = true,
            IncomeRegion = "7",
            BankCode = "18",
            Payable = 500_000m,
            Rows = { new IncmRowDto { IncmNo = 1, Val = 500_000m } }
        };
        FicheSendService.ApplySendStatus(fiche);
        Assert.False(fiche.CanSend);
        Assert.Contains("رایورز", fiche.BlockReason, StringComparison.Ordinal);
    }
}
