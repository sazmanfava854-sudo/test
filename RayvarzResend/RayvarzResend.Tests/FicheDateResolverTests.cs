using Microsoft.Extensions.Configuration;
using RayvarzResend.Web.Models;
using RayvarzResend.Web.Services;
using Xunit;

namespace RayvarzResend.Tests;

public class FicheDateResolverTests
{
  [Theory]
  [InlineData(3, "1404/01/15", "1404/02/20", "14040115")]
  [InlineData(1, "1404/01/15", "1404/02/20", "14040220")]
  [InlineData(2, "1404/01/15", "1404/02/20", "14040220")]
  [InlineData(5, "1404/01/15", "1404/02/20", "14040220")]
  public void ResolvePaymentDateByStatus_UsesStatusPriority(int status, string payment, string bank, string expected)
  {
    var result = FicheDateResolver.ResolvePaymentDateByStatus(status, payment, bank);
    Assert.Equal(expected, result);
  }

  [Fact]
  public void ResolvePaymentDateByStatus_FallsBackToOtherColumn()
  {
    Assert.Equal("14040220", FicheDateResolver.ResolvePaymentDateByStatus(1, "", "1404/02/20"));
    Assert.Equal("14040115", FicheDateResolver.ResolvePaymentDateByStatus(3, "1404/01/15", ""));
  }

  [Fact]
  public void ApplyFromIncomeColumns_tahator_keeps_payment_rowDate()
  {
    var dto = new FicheHeaderDto();
    FicheDateResolver.ApplyFromIncomeColumns(dto, 3, "1404/01/15", "1404/02/20", tahatorFiche: true);
    var today = DateHelper.CurrentShamsiRayvarzDate();

    Assert.Equal("14040115", dto.RayvarzDocDate);
    Assert.Equal(today, dto.RayvarzActDate);
    Assert.Equal("14040115", dto.RowDate);
    Assert.Equal("14040220", dto.RayvarzDueDate);
  }

  [Fact]
  public void ApplyFromIncomeColumns_status1_actDate_is_today()
  {
    var dto = new FicheHeaderDto();
    FicheDateResolver.ApplyFromIncomeColumns(dto, 1, "1404/01/15", "1404/02/20");
    var today = DateHelper.CurrentShamsiRayvarzDate();

    Assert.Equal("14040115", dto.RayvarzDocDate);
    Assert.Equal(today, dto.RayvarzActDate);
    Assert.Equal(today, dto.RowDate);
    Assert.Equal("14040220", dto.RayvarzDueDate);
  }

  [Fact]
  public void ApplyFromIncomeColumns_status5_actDate_is_today()
  {
    var dto = new FicheHeaderDto();
    FicheDateResolver.ApplyFromIncomeColumns(dto, 5, "1404/01/15", "1404/02/20");
    var today = DateHelper.CurrentShamsiRayvarzDate();

    Assert.Equal("14040115", dto.RayvarzDocDate);
    Assert.Equal(today, dto.RayvarzActDate);
    Assert.Equal(today, dto.RowDate);
    Assert.Equal("14040220", dto.RayvarzDueDate);
  }

  [Fact]
  public void Soap_income_actDate_is_today()
  {
    var today = DateHelper.CurrentShamsiRayvarzDate();
    var fiche = new FicheHeaderDto
    {
      Category = FicheCategory.Income,
      FicheNo = "058033501915",
      Payable = 67_531_000m,
      PaymentBranch = "18",
      BnkAcntNo = "80-2-14-11-1-0-2",
      DocTyp = 3,
      DocDsc = "اسناد شهرسازی",
      CurrentStatus = 5,
      RayvarzDocDate = "14040115",
      RayvarzActDate = today,
      RayvarzDueDate = "14040220",
      Rows = { new IncmRowDto { IncmNo = 1262, Val = 67_531_000m, IncmRowDsc = "عوارض بر مشاغل" } }
    };

    var config = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
        ["Rayvarz:ServiceUrl"] = "http://example.local/svc",
        ["Rayvarz:RefRowDocNoInDetail"] = "zero"
      })
      .Build();

    var xml = new SoapBuilder(config).Build(fiche, 218, 200218011, null, null, null);
    Assert.Contains($"<b:ActDate>{today}</b:ActDate>", xml);
    Assert.Contains($"<b:RowDate>{today}</b:RowDate>", xml);
    Assert.DoesNotContain("<b:ActDate>14040220</b:ActDate>", xml);
  }

  [Fact]
  public void ApplyFromDutyColumns_status5_doc_due_from_db_actDate_today()
  {
    var dto = new FicheHeaderDto();
    FicheDateResolver.ApplyFromDutyColumns(dto, 5, "1404/01/15", "1404/02/20", "1404/03/01", "1404/03/05");
    var today = DateHelper.CurrentShamsiRayvarzDate();

    Assert.Equal("14040115", dto.RayvarzDocDate);
    Assert.Equal(today, dto.RayvarzActDate);
    Assert.Equal(today, dto.RowDate);
    Assert.Equal("14040220", dto.RayvarzDueDate);
  }

  [Fact]
  public void DutyNosaziLogic_ApplyRayvarzDates_actDate_is_today()
  {
    var dto = new FicheHeaderDto();
    DutyNosaziLogic.ApplyRayvarzDates(dto, 1, "1404/01/15", "1404/02/20");
    var today = DateHelper.CurrentShamsiRayvarzDate();

    Assert.Equal("14040115", dto.RayvarzDocDate);
    Assert.Equal(today, dto.RayvarzActDate);
    Assert.Equal(today, dto.RowDate);
    Assert.Equal("14040220", dto.RayvarzDueDate);
  }

  [Fact]
  public void Duty_soap_actDate_is_today_doc_from_db()
  {
    var today = DateHelper.CurrentShamsiRayvarzDate();
    var fiche = new FicheHeaderDto
    {
      Category = FicheCategory.DutyNosazi,
      FicheNo = "123456/7890123",
      Payable = 5_000_000m,
      PaymentBranch = "18",
      BankCode = "18",
      CurrentStatus = 5,
      RayvarzDocDate = "14040115",
      RayvarzActDate = today,
      RayvarzDueDate = "14040220",
      BnkAcntNo = "9-8-72-47-1-0-2-0",
      DocTyp = 1,
      DocDsc = "اسناد نوسازی",
      Rows = { new IncmRowDto { IncmNo = 2003, Val = 5_000_000m, IncmRowDsc = "نوسازی" } }
    };

    var config = new ConfigurationBuilder()
      .AddInMemoryCollection(new Dictionary<string, string?>
      {
        ["Rayvarz:SoapAction"] = "http://tempuri.org/IReceiveIncmVchrServices/SaveDocument",
        ["Rayvarz:ServiceUrl"] = "http://example.local/svc",
        ["Rayvarz:RefRowDocNoInDetail"] = "zero"
      })
      .Build();

    var xml = new SoapBuilder(config).Build(fiche, 209, 200209008, null, null, null);
    Assert.Contains("<b:DocDate>14040115</b:DocDate>", xml);
    Assert.Contains($"<b:ActDate>{today}</b:ActDate>", xml);
    Assert.Contains($"<b:RowDate>{today}</b:RowDate>", xml);
    Assert.DoesNotContain($"<b:DocDate>{today}</b:DocDate>", xml);
  }
}
