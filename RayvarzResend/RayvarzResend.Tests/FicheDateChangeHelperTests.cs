using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class FicheDateChangeHelperTests
{
    [Fact]
    public void BuildCommentPrefix_status_only_shows_from_to_labels()
    {
        var today = DateHelper.CurrentShamsiSlashDate();
        var prefix = FicheDateChangeHelper.BuildCommentPrefix(
            "شادی امینی",
            applyExportPermanentDate: false,
            newExportPermanentDate: null,
            applyExportTemporaryDate: false,
            newExportTemporaryDate: null,
            applyPaymentBreakDate: false,
            newPaymentBreakDate: null,
            applyEumFicheStatus: true,
            previousEumFicheStatus: 5,
            newEumFicheStatus: 4);

        Assert.Contains("تغییر وضعیت از تایید بانک به ابطال", prefix);
        Assert.Contains("شادی امینی", prefix);
        Assert.Contains($"مورخ {today}", prefix);
        Assert.DoesNotContain("صدور دایم", prefix);
        Assert.DoesNotContain("مهلت پرداخت", prefix);
    }

    [Fact]
    public void BuildCommentPrefix_permanent_date_only()
    {
        var prefix = FicheDateChangeHelper.BuildCommentPrefix(
            "karimi",
            applyExportPermanentDate: true,
            newExportPermanentDate: "1405/08/01",
            applyExportTemporaryDate: false,
            newExportTemporaryDate: null,
            applyPaymentBreakDate: false,
            newPaymentBreakDate: null,
            applyEumFicheStatus: false,
            previousEumFicheStatus: null,
            newEumFicheStatus: null);

        Assert.Contains("تغییر تاریخ صدور دایم به 1405/08/01", prefix);
        Assert.Contains("karimi", prefix);
        Assert.DoesNotContain("وضعیت", prefix);
    }

    [Fact]
    public void NormalizeSlashDate_accepts_only_slash_format()
    {
        Assert.Equal("1404/01/11", FicheDateChangeHelper.NormalizeSlashDate("1404/01/11"));
        Assert.Equal("1404/01/11", FicheDateChangeHelper.NormalizeSlashDate("۱۴۰۴/۰۱/۱۱"));
        Assert.Equal("", FicheDateChangeHelper.NormalizeSlashDate("14040111"));
        Assert.Equal("", FicheDateChangeHelper.NormalizeSlashDate("051205/20601482"));
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
        Assert.True(FicheDateChangeHelper.HasAnySearchFilter(new FicheDateChangeSearchRequest
        {
            AccountGroupTitles = ["شهردار", "معاون"]
        }));
    }

    [Fact]
    public void HasAnySearchFilter_accepts_identifier_value()
    {
        Assert.True(FicheDateChangeHelper.HasAnySearchFilter(new FicheDateChangeSearchRequest
        {
            IdentifierValue = "101104/9881711"
        }));
    }

    [Fact]
    public void BuildIdentifierFilter_uses_fiche_no_or_bill_payment()
    {
        var fiche = FicheDateChangeHelper.BuildIdentifierFilter("101104/9881711");
        Assert.NotNull(fiche);
        Assert.Contains("FicheNo", fiche.Value.Clause);

        var bill = FicheDateChangeHelper.BuildIdentifierFilter("12345678901234567890123456");
        Assert.NotNull(bill);
        Assert.Contains("BillID", bill.Value.Clause);
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
