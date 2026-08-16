using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

/// <summary>
/// Golden tests از نمونه‌های واقعی ray.incmdocsys — عوارض بر مشاغل (IncmNo=1262).
/// همان دیتاست IncmdocsysMeshaghelGoldensTests شاخه parity.
/// </summary>
public class IncomeSoapParityTests
{
    public static TheoryData<string, int, int, string, decimal, string, string> StandardSamples =>
        new()
        {
            { "050933509456", 209, 200209008, "9-8-72-47-1-0-2", 133_729_000m, "9001910151966", "0013372932519" },
            { "058033433527", 218, 200218011, "80-4-9-1-1-0-8", 211_041_000m, "9000022152362", "0021104132571" },
            { "051133436812", 211, 200211007, "11-4-105-30-1-0-6", 70_768_000m, "9000057452164", "0007076832523" },
            { "050333503502", 203, 200203013, "3-31-160-16-1-0-2", 390_876_000m, "9000084051368", "0039087632532" },
            { "041033304271", 210, 200210020, "10-5-14-47-1-0-1", 116_819_000m, "9000693552063", "0011681932426" },
            { "050833466808", 208, 200208010, "8-1-6-2-1-0-1", 666_227_000m, "9000124051862", "0066622732506" },
            { "040833325741", 208, 200208010, "8-3-24-28-1-0-2", 619_175_000m, "9000596751868", "0061917532496" },
            { "050833486273", 208, 200208010, "8-2-11-7-1-0-1", 2_400_244_000m, "9000176451869", "0240024432590" },
            { "050833446542", 208, 200208010, "8-3-22-24-1-0-1", 987_973_000m, "9000080251869", "0098797332591" },
            { "058033501915", 218, 200218011, "80-2-14-11-1-0-2", 67_531_000m, "9000162352361", "0006753132532" },
        };

    [Theory]
    [MemberData(nameof(StandardSamples))]
    public void Standard_meshaghel_soap_matches_incmdocsys_pattern(
        string ficheNo, int branch, int fund, string bnkAcntNo,
        decimal payable, string billId, string paymentId)
    {
        var fiche = MakeIncomeFiche(ficheNo, branch, fund, bnkAcntNo, payable, billId, paymentId);
        var xml = BuildSoap(fiche, branch, fund);

        Assert.Contains($"<branch>{branch}</branch>", xml);
        Assert.Contains($"<b:Fund>{fund}</b:Fund>", xml);
        Assert.Contains($"<b:BnkAcntNo>{bnkAcntNo}</b:BnkAcntNo>", xml);
        Assert.Contains("<b:Bank>18</b:Bank>", xml);
        Assert.Contains("<b:IncmMkrTyp>1</b:IncmMkrTyp>", xml);
        Assert.Contains("<b:RefRowDocNo>0</b:RefRowDocNo>", xml);
        Assert.Contains($"<b:Ref>{ficheNo}</b:Ref>", xml);
        Assert.Contains("<b:IncmNo>1262</b:IncmNo>", xml);
        Assert.Contains($"<b:RowDocNo>{ficheNo}</b:RowDocNo>", xml);
        Assert.Contains($"<b:Ref2>{billId}</b:Ref2>", xml);
        Assert.Contains($"<b:Ref3>{paymentId}</b:Ref3>", xml);
        var qty = payable.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        Assert.Contains($"<b:Qty>{qty}</b:Qty>", xml);
        Assert.Contains($"<b:Val>{qty}</b:Val>", xml);
    }

    [Fact]
    public void Income_bank_falls_back_to_payment_bank_when_present()
    {
        var fiche = MakeIncomeFiche("058033501915", 218, 200218011, "80-2-14-11-1-0-2",
            67_531_000m, "9000162352361", "0006753132532");
        fiche.BankCode = "12";

        var xml = BuildSoap(fiche, 218, 200218011);
        Assert.Contains("<b:Bank>12</b:Bank>", xml);
    }

    [Fact]
    public void Config_override_refRowDocNo_headerDocRow_still_works()
    {
        var fiche = MakeIncomeFiche("058033501915", 218, 200218011, "80-2-14-11-1-0-2",
            67_531_000m, "9000162352361", "0006753132532");

        var xml = BuildSoap(fiche, 218, 200218011, refRowMode: "headerDocRow");
        Assert.Contains("<b:RefRowDocNo>1</b:RefRowDocNo>", xml);
    }

    // ---------- Regression: مسیر نوسازی/صنفی نباید تغییر کند ----------

    [Fact]
    public void Duty_nosazi_soap_unchanged()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutyNosazi,
            FicheNo = "123456/7890123",
            Payable = 5_000_000m,
            PaymentBranch = "18",
            BankCode = "18",
            BnkAcntNo = "9-8-72-47-1-0-2-0",
            DocTyp = 1,
            DocDsc = "اسناد نوسازی",
            BillIdRaw = "9000010151966",
            PaymentIdRaw = "0000500032519",
            BillId = "9000010151966",
            PaymentId = "0000500032519",
            RayvarzDocDate = "14050422",
            RayvarzActDate = "14050101",
            RayvarzDueDate = "14050422",
            Rows =
            {
                new IncmRowDto { IncmNo = 2003, Val = 4_000_000m, IncmRowDsc = "نوسازی" },
                new IncmRowDto { IncmNo = 100002, Val = 600_000m, IncmRowDsc = "آتش نشانی" },
                new IncmRowDto { IncmNo = 100003, Val = 400_000m, IncmRowDsc = "پسماند" }
            }
        };

        var xml = BuildSoap(fiche, 209, 200209008);

        Assert.Contains("<b:Bank>18</b:Bank>", xml);
        Assert.Contains("<b:IncmMkrTyp>1</b:IncmMkrTyp>", xml);
        Assert.Contains("<b:RefRowDocNo>0</b:RefRowDocNo>", xml);
        Assert.Contains("<b:Ref>123456/7890123</b:Ref>", xml);
        Assert.Contains("<b:IncmNo>2003</b:IncmNo>", xml);
        Assert.Contains("<b:IncmNo>100002</b:IncmNo>", xml);
        Assert.Contains("<b:IncmNo>100003</b:IncmNo>", xml);
        // Qty نوسازی = کل Payable در هر ردیف
        Assert.Contains("<b:Qty>5000000</b:Qty>", xml);
        Assert.Contains("<b:Val>4000000</b:Val>", xml);
        Assert.Contains("<b:DocTypDsc>عوارض سرا</b:DocTypDsc>", xml);
    }

    [Fact]
    public void Duty_senfi_bank_from_confirm_bank_code_unchanged()
    {
        var fiche = new FicheHeaderDto
        {
            Category = FicheCategory.DutySenfi,
            FicheNo = "654321/1234567",
            Payable = 3_000_000m,
            PaymentBranch = "1",
            BankCode = "1",
            BnkAcntNo = "7-14-55-1-1-0-1",
            DocTyp = 2,
            DocDsc = "اسناد صنفی",
            RayvarzDocDate = "14050422",
            RayvarzActDate = "14050422",
            RayvarzDueDate = "14050422",
            Rows = { new IncmRowDto { IncmNo = 100062, Val = 3_000_000m, IncmRowDsc = "صنفي" } }
        };

        var xml = BuildSoap(fiche, 207, 200207006);
        Assert.Contains("<b:Bank>1</b:Bank>", xml);
        Assert.Contains("<b:IncmMkrTyp>1</b:IncmMkrTyp>", xml);
        Assert.Contains("<b:DocTypDsc>صنفی</b:DocTypDsc>", xml);
    }

    [Fact]
    public void Income_rows_scaled_to_payable_unchanged()
    {
        var fiche = MakeIncomeFiche("058033501915", 218, 200218011, "80-2-14-11-1-0-2",
            67_531_000m, "9000162352361", "0006753132532");
        fiche.Rows.Clear();
        fiche.Rows.Add(new IncmRowDto { IncmNo = 1262, Val = 90_493_682m, IncmRowDsc = "عوارض بر مشاغل" });

        var xml = BuildSoap(fiche, 218, 200218011);
        Assert.Contains("<b:Val>67531000</b:Val>", xml);
        Assert.DoesNotContain("<b:Val>90493682</b:Val>", xml);
    }

    [Fact]
    public void Empty_rows_throws_instead_of_incmNo_zero_fallback()
    {
        var fiche = MakeIncomeFiche("058033501915", 218, 200218011, "80-2-14-11-1-0-2",
            67_531_000m, "9000162352361", "0006753132532");
        fiche.Rows.Clear();

        var ex = Assert.Throws<InvalidOperationException>(() => BuildSoap(fiche, 218, 200218011));
        Assert.Contains("ردیف IncmNo یافت نشد", ex.Message);
    }

    [Fact]
    public void All_zero_val_rows_throws_instead_of_incmNo_zero_fallback()
    {
        var fiche = MakeIncomeFiche("058033501915", 218, 200218011, "80-2-14-11-1-0-2",
            67_531_000m, "9000162352361", "0006753132532");
        fiche.Rows.Clear();
        fiche.Rows.Add(new IncmRowDto { IncmNo = 1262, Val = 0, IncmRowDsc = "صفر" });

        var ex = Assert.Throws<InvalidOperationException>(() => BuildSoap(fiche, 218, 200218011));
        Assert.Contains("ردیف IncmNo یافت نشد", ex.Message);
    }

    [Fact]
    public void Valid_income_never_emits_incmNo_zero()
    {
        var fiche = MakeIncomeFiche("058033501915", 218, 200218011, "80-2-14-11-1-0-2",
            67_531_000m, "9000162352361", "0006753132532");
        var xml = BuildSoap(fiche, 218, 200218011);
        Assert.DoesNotContain("<b:IncmNo>0</b:IncmNo>", xml);
    }

    private static FicheHeaderDto MakeIncomeFiche(
        string ficheNo, int branch, int fund, string bnkAcntNo,
        decimal payable, string billId, string paymentId) =>
        new()
        {
            Category = FicheCategory.Income,
            IncomeAccountGroup = 162,
            FicheNo = ficheNo,
            Payable = payable,
            PaymentBranch = "18",
            BnkAcntNo = bnkAcntNo,
            DocTyp = 3,
            DocDsc = "اسناد شهرسازی",
            BillId = billId,
            PaymentId = paymentId,
            ResolvedDistrictBranch = branch,
            SuggestedFund = fund,
            RayvarzDocDate = "14050422",
            RayvarzActDate = "14050422",
            RayvarzDueDate = "14050422",
            Rows = { new IncmRowDto { IncmNo = 1262, Val = payable, IncmRowDsc = "عوارض بر مشاغل" } }
        };

    private static string BuildSoap(FicheHeaderDto fiche, int branch, int fund, string? refRowMode = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
            ["Rayvarz:ServiceUrl"] = "http://example.local/svc",
            ["Rayvarz:SourceSystemId"] = "RAYVARZ-RESEND"
        };
        if (refRowMode != null)
            settings["Rayvarz:RefRowDocNoInDetail"] = refRowMode;

        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new SoapBuilder(config).Build(fiche, branch, fund, null, null, null);
    }
}
