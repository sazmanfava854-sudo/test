using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

/// <summary>تست‌های پارامتریک بر اساس نمونه‌های واقعی incmdocsys (ارسال 1405/05/20).</summary>
public class AccountingDocProductionFixturesTests
{
    [Theory]
    [MemberData(nameof(AccountingDocTestFixtures.AllFixtures), MemberType = typeof(AccountingDocTestFixtures))]
    public void Incmdocsys_row_vals_sum_to_payable(AccountingDocTestFixtures.FicheFixture fx)
    {
        var sum = fx.Rows.Sum(r => r.Val);
        Assert.Equal(fx.Payable, sum);
    }

    [Theory]
    [MemberData(nameof(AccountingDocTestFixtures.AllFixtures), MemberType = typeof(AccountingDocTestFixtures))]
    public void Built_details_sum_to_payable(AccountingDocTestFixtures.FicheFixture fx)
    {
        var fiche = AccountingDocTestFixtures.ToFicheHeader(fx);
        var details = AccountingDocRowBuilder.BuildDetails(fiche);
        AccountingDocRowBuilder.ReconcileDetailPrices(details, fiche.Payable);

        Assert.Equal(fx.Payable, details.Sum(d => d.Price));
        Assert.Equal(fx.Rows.Length, details.Count);
    }

    [Theory]
    [MemberData(nameof(AccountingDocTestFixtures.AllFixtures), MemberType = typeof(AccountingDocTestFixtures))]
    public void Built_details_match_incmdocsys_vals(AccountingDocTestFixtures.FicheFixture fx)
    {
        var fiche = AccountingDocTestFixtures.ToFicheHeader(fx);
        var details = AccountingDocRowBuilder.BuildDetails(fiche);
        AccountingDocRowBuilder.ReconcileDetailPrices(details, fiche.Payable);

        foreach (var row in fx.Rows)
        {
            var detail = details.Single(d => d.WrapperAccountNo == row.IncmNo);
            Assert.Equal(row.Val, detail.Price);
        }
    }

    [Theory]
    [MemberData(nameof(AccountingDocTestFixtures.AllFixtures), MemberType = typeof(AccountingDocTestFixtures))]
    public void Accounting_no_prefix_matches_category(AccountingDocTestFixtures.FicheFixture fx)
    {
        var fiche = AccountingDocTestFixtures.ToFicheHeader(fx);
        var meta = AccountingDocTestFixtures.ToRayMeta(fx);

        var accountingNo = AccountingDocRowBuilder.BuildAccountingNo(fiche, meta, pursuitDocNo: null);

        Assert.StartsWith($"{fx.AccountingPrefix};", accountingNo);
        Assert.Contains($";{fx.Branch};", accountingNo);
        Assert.Contains($";{fx.Yr};", accountingNo);
        Assert.Contains($";{fx.DocTyp};", accountingNo);
        Assert.EndsWith($";{fx.Doc}", accountingNo);
    }

    [Theory]
    [MemberData(nameof(AccountingDocTestFixtures.AllFixtures), MemberType = typeof(AccountingDocTestFixtures))]
    public void Header_obj_on_price_and_doc_row(AccountingDocTestFixtures.FicheFixture fx)
    {
        var fiche = AccountingDocTestFixtures.ToFicheHeader(fx);
        var meta = AccountingDocTestFixtures.ToRayMeta(fx);
        var (header, _) = AccountingDocRowBuilder.Build(fiche, meta, pursuitDocNo: null);

        Assert.Equal(fx.ExpectedObjOnPrice, header.EumObjOnPrice);
        Assert.Equal(fx.ExpectedDocRow, header.DocRow);
        Assert.Equal(fx.Payable, header.SaraPrice);
        Assert.Equal(fx.FicheNo, header.FicheNo);
    }

    [Theory]
    [MemberData(nameof(AccountingDocTestFixtures.AllFixtures), MemberType = typeof(AccountingDocTestFixtures))]
    public void Detail_account_no_and_bill_payment(AccountingDocTestFixtures.FicheFixture fx)
    {
        if (string.IsNullOrWhiteSpace(fx.BnkAcntNo))
            return;

        var fiche = AccountingDocTestFixtures.ToFicheHeader(fx);
        var details = AccountingDocRowBuilder.BuildDetails(fiche);

        Assert.All(details, d =>
        {
            Assert.Equal(fx.BnkAcntNo, d.AccountNo);
            Assert.Equal(fx.FicheNo, d.FicheNo);
        });

        if (!string.IsNullOrWhiteSpace(fx.BillId))
            Assert.All(details, d => Assert.Equal(fx.BillId, d.BillId));
    }
}
