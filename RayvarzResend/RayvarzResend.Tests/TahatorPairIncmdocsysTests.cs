using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.RuleEngine.Executor;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

/// <summary>
/// جفت نمونه ray.incmdocsys: 050833533236 (مبلغ ۱۵۷) + 050833533237 (درآمد ۱۵۸).
/// </summary>
public class TahatorPairIncmdocsysTests
{
    public const string AmountFicheNo = "050833533236";
    public const string IncomeFicheNo = "050833533237";
    public const decimal PairPayable = 2_350_000_000m;

    [Fact]
    public void Amount_157_is_single_negative_200098_row()
    {
        var fiche = BuildAmountFiche();
        TahatorRowBuilder.ApplyTahatorAmountRows(fiche);

        Assert.Equal(157, fiche.IncomeAccountGroup);
        Assert.Equal(14, fiche.DocTyp);
        Assert.Single(fiche.Rows);
        Assert.Equal(TahatorRowBuilder.IncmNoBank4, fiche.Rows[0].IncmNo);
        Assert.Equal(-PairPayable, fiche.Rows[0].Val);
        Assert.Equal(310_206_569L, fiche.Rows[0].Center1);
        Assert.Equal(TahatorRowBuilder.Center3Default, fiche.Rows[0].Center3);
        Assert.Equal("10865999", fiche.Rows[0].Ref);
        Assert.Equal("4", fiche.Rows[0].Num);
    }

    [Fact]
    public void Income_158_has_six_dtl_rows_not_200098()
    {
        var fiche = BuildIncomeFicheWithGrossRows();
        Member1388IncomeRowProfiles.ApplyTahatorIncome(fiche);

        Assert.Equal(158, fiche.IncomeAccountGroup);
        Assert.Equal(17, fiche.DocTyp);
        Assert.Equal(6, fiche.Rows.Count);
        Assert.DoesNotContain(fiche.Rows, r => r.IncmNo is TahatorRowBuilder.IncmNoBank4 or TahatorRowBuilder.IncmNoOther);
        Assert.Equal(PairPayable, fiche.Rows.Sum(r => r.Val));
    }

    [Fact]
    public void Amount_soap_qty_is_payable_val_is_negative_payable()
    {
        var fiche = BuildAmountFiche();
        TahatorRowBuilder.ApplyTahatorAmountRows(fiche);
        var xml = BuildSoap(fiche, TahatorRowBuilder.DefaultRayvarzBranch, 58);

        Assert.Contains("<b:DocTyp>14</b:DocTyp>", xml);
        Assert.Contains("<b:IncmNo>200098</b:IncmNo>", xml);
        Assert.DoesNotContain("<b:IncmNo>1025</b:IncmNo>", xml);
        var qty = PairPayable.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        Assert.Contains($"<b:Qty>{qty}</b:Qty>", xml);
        Assert.Contains($"<b:Val>-{qty}</b:Val>", xml);
    }

    [Fact]
    public void Income_soap_has_six_lines_each_qty_payable()
    {
        var fiche = BuildIncomeFicheWithGrossRows();
        Member1388IncomeRowProfiles.ApplyTahatorIncome(fiche);
        var xml = BuildSoap(fiche, 208, 38);

        Assert.Contains("<b:DocTyp>17</b:DocTyp>", xml);
        var qty = PairPayable.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(6, System.Text.RegularExpressions.Regex.Matches(xml, $"<b:Qty>{qty}</b:Qty>").Count);
        Assert.Contains("<b:IncmNo>1271</b:IncmNo>", xml);
        Assert.Contains("<b:Ref>4</b:Ref>", xml);
        Assert.Contains("<b:Num>10865999</b:Num>", xml);
    }

    private static FicheHeaderDto BuildAmountFiche() => new()
    {
        Category = FicheCategory.Income,
        IncomeAccountGroup = 157,
        FicheNo = AmountFicheNo,
        Payable = PairPayable,
        BankCode = "4",
        Deposit = 310_206_569,
        DepositId = 10865999,
        BnkAcntNo = "8-4-49-2-1-0-0",
        ResolvedDistrictBranch = 208,
        RayvarzDocDate = "14050629",
        RayvarzActDate = "14050629",
        RayvarzDueDate = "14050629"
    };

    private static FicheHeaderDto BuildIncomeFicheWithGrossRows()
    {
        var gross = new (int IncmNo, decimal Val)[]
        {
            (1025, 2_001_734_525m),
            (1275, 159_795_492m),
            (1267, 4_027_447m),
            (1288, 9_674_041m),
            (100116, 16_123_402m),
            (1271, 158_645_093m)
        };

        return new FicheHeaderDto
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 158,
            FicheNo = IncomeFicheNo,
            Payable = PairPayable,
            BankCode = "4",
            DepositId = 10865999,
            BnkAcntNo = "8-4-49-2-1-0-0",
            ResolvedDistrictBranch = 208,
            DocTyp = 17,
            DocDsc = "اسناد تهاتر درامد",
            RayvarzDocDate = "14050726",
            RayvarzActDate = "14050726",
            RayvarzDueDate = "14050726",
            Rows = gross.Select(g => new IncmRowDto { IncmNo = g.IncmNo, Val = g.Val }).ToList()
        };
    }

    private static string BuildSoap(FicheHeaderDto fiche, int branch, int fund)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
                ["Rayvarz:ServiceUrl"] = "http://example.local/svc",
                ["Rayvarz:SourceSystemId"] = "FinancialAssistant"
            })
            .Build();
        return new SoapBuilder(config).Build(fiche, branch, fund, null, null, null);
    }
}
