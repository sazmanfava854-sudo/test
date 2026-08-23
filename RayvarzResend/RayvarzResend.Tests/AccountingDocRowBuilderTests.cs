using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

/// <summary>
/// نمونه واقعی فیش 050533452970 — Accounting_DocDetails پس از ارسال رایورز.
/// </summary>
public class AccountingDocRowBuilderTests
{
  public const string SampleFicheNo = "050533452970";
  public const decimal SamplePayable = 7_511_382_000m;

  public static readonly (int IncmNo, decimal Price, string Desc)[] SampleExpectedDetails =
  [
      (100116, 188_847_878m, "عوارض ناشي از اجراي ماده 9 قانون حمل و نقل ريلي"),
      (1270, 3_776_957_552m, "عوارض زيربنا (مسکوني)"),
      (1278, 113_308_727m, "عوارض آتشنشاني در هنگام صدور پروانه ساختماني"),
      (100070, 4_070_696_911m, "سرانه معابر ، تائسيسات،تجهيزات و خدمات عمومي"),
      (100118, -1_265_280_572m, "پرداختي قبلي 33%"),
      (1281, 626_851_504m, "عوارض ارزش افزوده ناشي از اجراي طرح هاي عمران شهري"),
  ];

  [Fact]
  public void Sample_fiche_details_sum_to_payable()
  {
      var fiche = BuildSampleFiche();
      var details = AccountingDocRowBuilder.BuildDetails(fiche);
      AccountingDocRowBuilder.ReconcileDetailPrices(details, fiche.Payable);

      Assert.Equal(6, details.Count);
      Assert.Equal(SamplePayable, details.Sum(d => d.Price));
  }

  [Fact]
  public void Sample_fiche_detail_prices_match_production_rows()
  {
      var fiche = BuildSampleFiche();
      var details = AccountingDocRowBuilder.BuildDetails(fiche);
      AccountingDocRowBuilder.ReconcileDetailPrices(details, fiche.Payable);

      foreach (var (incmNo, price, desc) in SampleExpectedDetails)
      {
          var row = details.Single(d => d.WrapperAccountNo == incmNo);
          Assert.Equal(price, row.Price);
          Assert.Equal(desc, row.AccountNoComments);
      }
  }

  [Fact]
  public void Sample_fiche_accounting_no_uses_incmdocsys_meta()
  {
      var fiche = BuildSampleFiche();
      var meta = new RayvarzDocMeta { Branch = 205, Yr = 1405, DocTyp = 3, Doc = 9068 };

      var accountingNo = AccountingDocRowBuilder.BuildAccountingNo(fiche, meta, pursuitDocNo: "228");

      Assert.Equal("Incm;205;1405;3;228", accountingNo);
  }

  [Fact]
  public void Sample_fiche_header_fields_match_production_pattern()
  {
      var fiche = BuildSampleFiche();
      var meta = new RayvarzDocMeta { Branch = 205, Yr = 1405, DocTyp = 3, Doc = 9068 };
      var (header, details) = AccountingDocRowBuilder.Build(fiche, meta, pursuitDocNo: "228");

      Assert.Equal(SamplePayable, header.SaraPrice);
      Assert.Equal(SampleFicheNo, header.FicheNo);
      Assert.Equal(AccountingDocRowBuilder.ObjOnPriceIncome, header.EumObjOnPrice);
      Assert.Equal(AccountingDocRowBuilder.ObjInDocumentFiche, header.EumAccountingObjInDocument);
      Assert.Equal(AccountingDocRowBuilder.DocumentingCauseConfirm, header.EumAccountingDocumentingCause);
      Assert.Equal(AccountingDocRowBuilder.PhasTypeRayvarz, header.DocRow);
      Assert.Equal(AccountingDocRowBuilder.PhasTypeRayvarz, header.PhasType);
      Assert.Equal("Rayvarz", header.SubSystem);
      Assert.Equal(6, details.Count);
  }

  [Fact]
  public void Detail_payment_date_uses_bank_payment_date_when_status_not_3()
  {
      var fiche = BuildSampleFiche();
      fiche.CurrentStatus = 5;
      fiche.RayvarzDocDate = "14050326";
      fiche.RayvarzActDate = "14050326";

      var details = AccountingDocRowBuilder.BuildDetails(fiche);
      Assert.All(details, d => Assert.Equal(14050326, d.PaymentDate));
  }

  [Fact]
  public void Reconcile_adjusts_first_row_when_sum_differs()
  {
      var details = new List<AccountingDocRowBuilder.AccountingDocDetailDraft>
      {
          new() { Price = 100m, WrapperAccountNo = 1, IncmRow = 1 },
          new() { Price = 200m, WrapperAccountNo = 2, IncmRow = 2 }
      };

      AccountingDocRowBuilder.ReconcileDetailPrices(details, 305m);

      Assert.Equal(105m, details[0].Price);
      Assert.Equal(200m, details[1].Price);
      Assert.Equal(305m, details.Sum(d => d.Price));
  }

  private static FicheHeaderDto BuildSampleFiche() => new()
  {
      Category = FicheCategory.Income,
      FicheNo = SampleFicheNo,
      NidFiche = Guid.Parse("72A38D51-74DD-41FF-9687-F788F07DE0BD"),
      Payable = SamplePayable,
      BillId = "9000050451561",
      PaymentId = "0751138232590",
      BnkAcntNo = "5-3-22-34-1-0-0",
      PaymentBranch = "18",
      CurrentStatus = 5,
      RayvarzDocDate = "14050326",
      RayvarzActDate = "14050326",
      DocTyp = 3,
      Rows = SampleExpectedDetails
          .Select(e => new IncmRowDto { IncmNo = e.IncmNo, Val = e.Price, IncmRowDsc = e.Desc })
          .ToList()
  };
}
