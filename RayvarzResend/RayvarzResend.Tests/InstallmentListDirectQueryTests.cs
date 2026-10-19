using RayvarzResend.Web.Services;

namespace RayvarzResend.Tests;

public class InstallmentListDirectQueryTests
{
    [Fact]
    public void Search_sql_uses_installment_list_and_tracking_no()
    {
        var sql = InstallmentListDirectQuery.BuildSearchByTrackingNoSql();
        Assert.Contains("FROM dbo.Installment_List il", sql);
        Assert.Contains("il.TrackingNo", sql);
        Assert.Contains("NidInstallmentList", sql);
        Assert.Contains("PaymentDate", sql);
        Assert.DoesNotContain("Income_Fiche", sql);
    }
}
